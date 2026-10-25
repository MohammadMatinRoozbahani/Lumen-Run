using UnityEngine;

public enum InteractableKind
{
    Ground,
    Air,
    Coin
}

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(SpriteRenderer))]
public abstract class Interactable : MonoBehaviour
{
    public abstract InteractableKind Kind { get; }
    protected GameController Game;
    protected SpriteRenderer Visual;
    protected BoxCollider2D Hitbox;
    protected Rigidbody2D Body;
    protected float Age;
    protected float BaseY;
    WorldSpawner owner;
    bool consumed;

    public void Activate(WorldSpawner source, Vector2 position, int variant)
    {
        owner = source;
        Game = source.Game;
        Visual = GetComponent<SpriteRenderer>();
        Hitbox = GetComponent<BoxCollider2D>();
        Body = GetComponent<Rigidbody2D>();
        Body.bodyType = RigidbodyType2D.Kinematic;
        Body.interpolation = RigidbodyInterpolation2D.Interpolate;
        Body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        Body.gravityScale = 0;
        Body.linearVelocity = Vector2.zero;
        Body.angularVelocity = 0;
        Hitbox.isTrigger = true;
        Visual.sortingOrder = 4;
        Visual.color = Color.white;
        Age = 0;
        consumed = false;
        BaseY = position.y;
        transform.localScale = Vector3.one;
        transform.position = position;
        Body.position = position;
        Configure(variant);
        gameObject.SetActive(true);
    }

    protected abstract void Configure(int variant);
    protected abstract void Apply(DinoMotor player);
    protected virtual float Height() { return BaseY; }
    protected virtual void Animate() { }

    void FixedUpdate()
    {
        if (Game == null || !Game.Running) return;
        Age += Time.fixedDeltaTime;
        Body.MovePosition(new Vector2(Body.position.x - Game.WorldSpeed * Time.fixedDeltaTime, Height()));
        Animate();
        if (Body.position.x < -13f) Recycle();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (consumed || Game == null || !Game.Running) return;
        var player = other.GetComponentInParent<DinoMotor>();
        if (player == null) return;
        consumed = true;
        Apply(player);
        Recycle();
    }

    public void Recycle()
    {
        if (owner != null) owner.Recycle(this);
        else gameObject.SetActive(false);
    }
}

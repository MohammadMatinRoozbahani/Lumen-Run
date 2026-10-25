using UnityEngine;

public sealed class AirObstacle : Interactable
{
    public override InteractableKind Kind { get { return InteractableKind.Air; } }

    protected override void Configure(int variant)
    {
        Visual.sprite = Game.Art.Frame(10);
        transform.localScale = Vector3.one * .65f;
        Hitbox.size = new Vector2(1.45f, .65f);
        Hitbox.offset = Vector2.zero;
    }

    protected override float Height() { return BaseY + Mathf.Sin(Age * 3.3f) * .12f; }
    protected override void Animate() { Visual.sprite = Game.Art.Frame(10 + (int)(Age * 10f) % 2); }
    protected override void Apply(DinoMotor player) { player.Hit(); }
}

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public sealed class DinoMotor : MonoBehaviour
{
    public const float GroundY = -3f;
    public const float DashDuration = .32f;
    public const float DashRecovery = 2f;
    public GameController Game;
    public Animator Animator;
    public SpriteRenderer Visual;
    public Transform Shadow;
    public bool Dashing { get { return boost > 0; } }
    public float Cooldown { get { return cooldown; } }
    public bool Grounded { get; private set; }
    Rigidbody2D body;
    float boost;
    float cooldown;
    float immunity;
    float jumpBuffer;
    float ghostTimer;
    float coyote;
    static readonly Vector2 Home = new Vector2(-5.8f, -2.24f);

    void Awake() { body = GetComponent<Rigidbody2D>(); }

    public void ResetRun()
    {
        if (body == null) body = GetComponent<Rigidbody2D>();
        body.simulated = true;
        body.position = Home;
        body.linearVelocity = Vector2.zero;
        boost = cooldown = immunity = jumpBuffer = ghostTimer = coyote = 0;
        Grounded = true;
        Visual.transform.localScale = Vector3.one;
        Visual.color = Color.white;
        Animator.Rebind();
        Animator.Update(0);
        Animator.SetBool("Menu", false);
        Animator.SetBool("Dead", false);
        Animator.SetBool("Airborne", false);
        Animator.SetBool("Dashing", false);
        Animator.ResetTrigger("Hit");
        Animator.Play("Run", 0, 0);
    }

    public void PresentMenu()
    {
        if (body == null) body = GetComponent<Rigidbody2D>();
        body.linearVelocity = Vector2.zero;
        body.simulated = false;
        transform.position = new Vector3(4.5f, -2.24f, 0);
        Visual.transform.localScale = Vector3.one * 2.2f;
        Visual.color = Color.white;
        boost = cooldown = immunity = 0;
        Animator.Rebind();
        Animator.Update(0);
        Animator.SetBool("Menu", true);
        Animator.Play("Idle", 0, 0);
    }

    void Update()
    {
        if (Game == null) return;
        if (Shadow != null)
        {
            float altitude = Mathf.Max(0, body.position.y - Home.y);
            Shadow.position = new Vector3(transform.position.x + .1f, GroundY - .015f, 0);
            Shadow.localScale = new Vector3(Mathf.Lerp(1.65f, .85f, altitude / 3f), .16f, 1);
        }
        if (!Game.Running) return;
        float dt = Time.deltaTime;
        boost = Mathf.Max(0, boost - dt);
        cooldown = Mathf.Max(0, cooldown - dt);
        immunity = Mathf.Max(0, immunity - dt);
        jumpBuffer = Mathf.Max(0, jumpBuffer - dt);
        Grounded = Physics2D.BoxCast(body.position + new Vector2(.1f, -.69f),
            new Vector2(.5f, .06f), 0, Vector2.down, .12f, 1 << 6).collider != null
            && body.linearVelocity.y <= .2f;
        coyote = Grounded ? .075f : Mathf.Max(0, coyote - dt);
        if (Game.Input.JumpPressed) RequestJump();
        if (Game.Input.DashPressed) TryDash();
        Animator.SetBool("Airborne", !Grounded);
        Animator.SetBool("Dashing", Dashing);
        Visual.color = immunity > 0 ? new Color(1f, .3f, .35f, Mathf.Sin(immunity * 42f) > 0 ? .45f : 1f)
            : Dashing ? new Color(.6f, 1f, 1f) : Color.white;
        if (Dashing && Game.MotionEnabled)
        {
            ghostTimer -= dt;
            if (ghostTimer <= 0)
            {
                Game.Effects.Ghost(Visual);
                ghostTimer = .045f;
            }
        }
    }

    void FixedUpdate()
    {
        if (Game == null || !Game.Running || jumpBuffer <= 0 || coyote <= 0) return;
        body.linearVelocity = new Vector2(0, 11.4f);
        jumpBuffer = coyote = 0;
        Grounded = false;
        Animator.SetBool("Airborne", true);
        Game.Audio.Play("jump");
        Game.Effects.Burst(new Vector3(body.position.x, GroundY, 0), new Color(.25f, .9f, .75f), 5);
    }

    public bool Hit()
    {
        if (Game == null || !Game.Running || immunity > 0) return false;
        immunity = .9f;
        Animator.SetTrigger("Hit");
        Game.Damage();
        return true;
    }

    public void RequestJump()
    {
        if (Game != null && Game.Running) jumpBuffer = .11f;
    }

    public bool TryDash()
    {
        if (Game == null || !Game.Running || cooldown > 0) return false;
        boost = DashDuration;
        cooldown = DashRecovery;
        ghostTimer = 0;
        Game.Audio.Play("dash");
        Game.Effects.Burst(transform.position, new Color(.2f, 1f, .9f), 5);
        return true;
    }

    public void Die()
    {
        boost = jumpBuffer = coyote = 0;
        body.linearVelocity = Vector2.zero;
        body.simulated = true;
        Animator.ResetTrigger("Hit");
        Animator.SetBool("Dead", true);
        Visual.color = Color.white;
    }
}

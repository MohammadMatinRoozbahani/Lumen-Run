using UnityEngine;

public sealed class GroundObstacle : Interactable
{
    public override InteractableKind Kind { get { return InteractableKind.Ground; } }

    protected override void Configure(int variant)
    {
        Visual.sprite = Game.Art.Frame(8 + variant % 2);
        transform.localScale = Vector3.one * .7f;
        Hitbox.size = variant % 2 == 0 ? new Vector2(.95f, 1.9f) : new Vector2(1.35f, 1.5f);
        Hitbox.offset = new Vector2(0, Hitbox.size.y * .5f);
    }

    protected override void Apply(DinoMotor player) { player.Hit(); }
}

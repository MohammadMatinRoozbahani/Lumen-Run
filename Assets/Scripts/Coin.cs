using UnityEngine;

public sealed class Coin : Interactable
{
    public override InteractableKind Kind { get { return InteractableKind.Coin; } }

    protected override void Configure(int variant)
    {
        Visual.sprite = Game.Art.Frame(12);
        transform.localScale = Vector3.one * .48f;
        Hitbox.size = new Vector2(1.15f, 1.15f);
        Hitbox.offset = Vector2.zero;
    }

    protected override void Animate()
    {
        int frame = (int)(Age * 10f) % 4;
        Visual.sprite = Game.Art.Frame(12 + (frame < 3 ? frame : 1));
    }

    protected override void Apply(DinoMotor player) { Game.CollectCoin(transform.position); }
}

using System.Collections.Generic;
using UnityEngine;

public sealed class LumenEffects : MonoBehaviour
{
    public GameController Game;
    public Camera Camera;
    sealed class Bit
    {
        public SpriteRenderer View;
        public Vector3 Velocity;
        public Color Color;
        public float Life;
        public float Maximum;
        public float Size;
        public bool Ghost;
    }
    readonly List<Bit> bits = new List<Bit>();
    readonly List<SpriteRenderer> fireflies = new List<SpriteRenderer>();
    float shake;
    float shakeStrength;
    int next;

    void Start()
    {
        for (int i = 0; i < 96; i++)
        {
            var view = new GameObject("Particle_" + i).AddComponent<SpriteRenderer>();
            view.transform.SetParent(transform, false);
            view.sortingOrder = 7;
            view.enabled = false;
            bits.Add(new Bit { View = view });
        }
        for (int i = 0; i < 24; i++)
        {
            var fly = new GameObject("Firefly_" + i).AddComponent<SpriteRenderer>();
            fly.transform.SetParent(transform, false);
            fly.sprite = Game.Art.Disc;
            fly.sortingOrder = -4;
            fly.transform.position = new Vector3(Random.Range(-11f, 11f), Random.Range(-2.7f, 4f), 0);
            fly.transform.localScale = Vector3.one * Random.Range(.025f, .065f);
            fireflies.Add(fly);
        }
    }

    Bit Allocate(Sprite sprite, Vector3 position, float size, float life, Color color, bool ghost)
    {
        if (bits.Count == 0) return null;
        var bit = bits[next++ % bits.Count];
        bit.View.sprite = sprite;
        bit.View.enabled = true;
        bit.View.sortingOrder = ghost ? 3 : 7;
        bit.View.transform.position = position;
        bit.View.transform.rotation = Quaternion.identity;
        bit.View.transform.localScale = Vector3.one * size;
        bit.Size = size;
        bit.Life = bit.Maximum = life;
        bit.Color = color;
        bit.Velocity = Vector3.zero;
        bit.Ghost = ghost;
        return bit;
    }

    public void Burst(Vector3 position, Color color, int count)
    {
        if (Game == null || !Game.MotionEnabled) return;
        for (int i = 0; i < count; i++)
        {
            var bit = Allocate(i == 0 ? Game.Art.Frame(15) : Game.Art.Disc, position,
                i == 0 ? .38f : Random.Range(.05f, .13f), Random.Range(.25f, .6f), color, false);
            if (bit != null) bit.Velocity = (Vector3)Random.insideUnitCircle * Random.Range(1.3f, 3f);
        }
    }

    public void Ghost(SpriteRenderer source)
    {
        var bit = Allocate(source.sprite, source.transform.position, source.transform.lossyScale.x,
            .22f, new Color(.15f, 1f, 1f, .28f), true);
        if (bit != null) bit.Velocity = Vector3.left * 2f;
    }

    public void Impact(Vector3 position, Color color, float strength)
    {
        Burst(position, color, 12);
        if (!Game.MotionEnabled) return;
        shake = .25f;
        shakeStrength = strength;
    }

    void Update()
    {
        if (Game == null) return;
        float dt = Time.deltaTime;
        for (int i = 0; i < fireflies.Count; i++)
        {
            var fly = fireflies[i];
            fly.enabled = Game.MotionEnabled;
            if (dt <= 0) continue;
            fly.transform.position += new Vector3(-Game.ScenerySpeed * .055f, Mathf.Sin(Time.time * .8f + i) * .025f, 0) * dt;
            if (fly.transform.position.x < -11) fly.transform.position += Vector3.right * 22f;
            fly.color = new Color(.6f, 1f, .72f, .24f + .32f * (.5f + .5f * Mathf.Sin(Time.time * 1.8f + i * 2)));
        }
        foreach (var bit in bits)
        {
            if (bit.Life <= 0) continue;
            bit.Life -= dt;
            if (bit.Life <= 0 || !Game.MotionEnabled) { bit.View.enabled = false; continue; }
            float ratio = bit.Life / bit.Maximum;
            if (!bit.Ghost) bit.Velocity += Vector3.down * 3f * dt;
            bit.View.transform.position += bit.Velocity * dt;
            bit.View.transform.localScale = Vector3.one * bit.Size * (bit.Ghost ? 1f : .4f + .6f * ratio);
            bit.View.color = new Color(bit.Color.r, bit.Color.g, bit.Color.b, bit.Color.a * ratio);
        }
    }

    void LateUpdate()
    {
        if (Camera == null || Game == null || Time.deltaTime <= 0) return;
        shake = Mathf.Max(0, shake - Time.deltaTime);
        Vector2 offset = Game.MotionEnabled && shake > 0 ? Random.insideUnitCircle * shakeStrength * shake / .25f : Vector2.zero;
        Camera.transform.position = new Vector3(offset.x, offset.y, -10);
        float target = Game.MotionEnabled && Game.Player.Dashing && Game.Running ? 5.4f : 5.2f;
        Camera.orthographicSize = Mathf.Lerp(Camera.orthographicSize, target, Time.deltaTime * 8f);
    }

    public void Clear()
    {
        foreach (var bit in bits) { bit.Life = 0; bit.View.enabled = false; }
        shake = 0;
        if (Camera != null)
        {
            Camera.transform.position = new Vector3(0, 0, -10);
            Camera.orthographicSize = 5.2f;
        }
    }
}

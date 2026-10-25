using System.Collections.Generic;
using UnityEngine;

public sealed class WorldSpawner : MonoBehaviour
{
    public GameController Game;
    readonly List<Interactable> active = new List<Interactable>();
    readonly Stack<Interactable>[] pools =
    {
        new Stack<Interactable>(),
        new Stack<Interactable>(),
        new Stack<Interactable>()
    };
    float remaining;
    public int ActiveCount { get { return active.Count; } }

    public void ResetRun() { remaining = 10f; }

    public void Tick(float deltaTime)
    {
        if (!Game.Running) return;
        remaining -= Game.WorldSpeed * deltaTime;
        if (remaining > 0) return;
        int pattern = Random.Range(0, 5);
        if (pattern == 0)
            Spawn(InteractableKind.Air, new Vector2(12.6f, -.6f), 0);
        else if (pattern != 1)
            Spawn(InteractableKind.Ground, new Vector2(12.6f, DinoMotor.GroundY), Random.Range(0, 2));
        float start = pattern == 1 ? 12.6f : 15f;
        int count = pattern == 1 ? 5 : 3;
        for (int i = 0; i < count; i++)
        {
            float t = count == 1 ? 0 : i / (float)(count - 1);
            float y = -1.7f + Mathf.Sin(t * Mathf.PI) * (pattern == 1 ? 1.05f : .65f);
            Spawn(InteractableKind.Coin, new Vector2(start + i * 1.1f, y), 0);
        }
        remaining += Mathf.Max(15f, Game.Session.BaseSpeed * 1.3f) + Random.Range(0f, 5f);
    }

    public Interactable Spawn(InteractableKind kind, Vector2 position, int variant)
    {
        var pool = pools[(int)kind];
        Interactable item = pool.Count > 0 ? pool.Pop() : CreateItem(kind);
        item.Activate(this, position, variant);
        active.Add(item);
        return item;
    }

    Interactable CreateItem(InteractableKind kind)
    {
        var item = new GameObject(kind.ToString(), typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D));
        item.transform.SetParent(transform, false);
        item.SetActive(false);
        if (kind == InteractableKind.Ground) return item.AddComponent<GroundObstacle>();
        if (kind == InteractableKind.Air) return item.AddComponent<AirObstacle>();
        return item.AddComponent<Coin>();
    }

    public void Recycle(Interactable item)
    {
        if (!active.Remove(item)) return;
        item.gameObject.SetActive(false);
        pools[(int)item.Kind].Push(item);
    }

    public void Clear()
    {
        for (int i = active.Count - 1; i >= 0; i--) Recycle(active[i]);
    }
}

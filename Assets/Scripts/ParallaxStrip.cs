using UnityEngine;

public sealed class ParallaxStrip : MonoBehaviour
{
    public GameController Game;
    public float Factor;
    public float Step;
    public int TileCount = 3;
    public float LeftBoundary;

    void Update()
    {
        if (Game == null || Step <= 0) return;
        transform.position += Vector3.left * Game.ScenerySpeed * Factor * Time.deltaTime;
        float boundary = LeftBoundary == 0 ? -Step * 1.5f : LeftBoundary;
        if (transform.position.x < boundary)
            transform.position += Vector3.right * Step * TileCount;
    }
}

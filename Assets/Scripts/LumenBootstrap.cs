using UnityEngine;

public sealed class LumenBootstrap : MonoBehaviour
{
    void Awake()
    {
        if (FindAnyObjectByType<GameController>() != null) return;
        var art = Resources.Load<LumenArt>("LumenArt");
        string missing = art == null ? "LumenArt" : art.MissingAsset();
        if (missing != null)
        {
            Debug.LogError("Lumen Run setup is incomplete: " + missing
                + ". Stop Play and choose Lumen > Prepare Project. Check Console for the first preparation error.");
            return;
        }
        GameWorldBuilder.Build(art);
        Destroy(gameObject);
    }
}

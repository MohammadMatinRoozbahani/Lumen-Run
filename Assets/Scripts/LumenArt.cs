using UnityEngine;

public sealed class LumenArt : ScriptableObject
{
    public Sprite[] Frames;
    public Sprite Environment;
    public Sprite Foliage;
    public Sprite Rounded;
    public Sprite Disc;
    public Sprite White;
    public Font BodyFont;
    public Font DisplayFont;
    public Material PanoramaMaterial;
    public RuntimeAnimatorController Animator;

    public string MissingAsset()
    {
        if (Frames == null || Frames.Length != 16) return "animation frames";
        for (int i = 0; i < Frames.Length; i++)
            if (Frames[i] == null) return "animation frame " + i;
        if (Environment == null) return "Environment";
        if (Foliage == null) return "Foliage";
        if (Rounded == null) return "Rounded";
        if (Disc == null) return "Disc";
        if (White == null) return "White";
        if (BodyFont == null) return "BodyFont";
        if (DisplayFont == null) return "DisplayFont";
        if (PanoramaMaterial == null || PanoramaMaterial.shader == null) return "PanoramaMaterial";
        if (Animator == null) return "Animator";
        return null;
    }

    public Sprite Frame(int index)
    {
        return Frames[Mathf.Clamp(index, 0, Frames.Length - 1)];
    }
}

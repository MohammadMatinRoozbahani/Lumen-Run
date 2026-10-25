using System.Collections.Generic;
using UnityEngine;

public sealed class LumenAudio : MonoBehaviour
{
    public GameController Game;
    public bool Enabled { get; private set; }
    readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
    AudioSource effects;
    AudioSource ambience;

    void Awake()
    {
        Enabled = PlayerPrefs.GetInt("LumenRun.Sound", 1) != 0;
        effects = gameObject.AddComponent<AudioSource>();
        effects.playOnAwake = false;
        effects.volume = .55f;
        var music = new GameObject("Ambience");
        music.transform.SetParent(transform, false);
        ambience = music.AddComponent<AudioSource>();
        ambience.playOnAwake = false;
        ambience.loop = true;
        ambience.volume = .14f;
        foreach (string name in new[] { "jump", "coin", "dash", "hit", "death", "click", "start" })
            clips[name] = Resources.Load<AudioClip>("Audio/" + name);
        effects.mute = ambience.mute = !Enabled;
    }

    public void StartAmbience()
    {
        ambience.clip = Resources.Load<AudioClip>("Audio/ambience");
        if (ambience.clip != null) ambience.Play();
    }

    void Update()
    {
        if (ambience == null || Game == null) return;
        float target = Game.Phase == RunPhase.Paused ? .05f : .14f;
        ambience.volume = Mathf.Lerp(ambience.volume, target, Time.unscaledDeltaTime * 5f);
    }

    public void Play(string name)
    {
        AudioClip clip;
        if (Enabled && effects != null && clips.TryGetValue(name, out clip) && clip != null)
            effects.PlayOneShot(clip);
    }

    public void Toggle()
    {
        Enabled = !Enabled;
        effects.mute = ambience.mute = !Enabled;
        PlayerPrefs.SetInt("LumenRun.Sound", Enabled ? 1 : 0);
        PlayerPrefs.Save();
        if (Enabled) Play("click");
    }
}

using UnityEngine;

public enum RunPhase
{
    Menu,
    Playing,
    Paused,
    Dying,
    GameOver
}

[DefaultExecutionOrder(-100)]
public sealed class GameController : MonoBehaviour
{
    public LumenArt Art;
    public RunnerInput Input;
    public DinoMotor Player;
    public WorldSpawner Spawner;
    public LumenEffects Effects;
    public LumenAudio Audio;
    public LumenHud Hud;
    public RunSession Session { get; private set; }
    public RunPhase Phase { get; private set; }
    public int Best { get; private set; }
    public bool Running { get { return Phase == RunPhase.Playing; } }
    public bool MotionEnabled { get; private set; }
    public float WorldSpeed { get { return Running ? Session.BaseSpeed * (Player.Dashing ? 1.6f : 1f) : 0; } }
    public float ScenerySpeed { get { return Phase == RunPhase.Menu ? 1.2f : WorldSpeed; } }
    float deathDelay;
    float saveTimer;

    void Awake()
    {
        Time.timeScale = 1;
        Best = PlayerPrefs.GetInt("LumenRun.Best", 0);
        MotionEnabled = PlayerPrefs.GetInt("LumenRun.Motion", 1) != 0;
        Session = new RunSession();
        Phase = RunPhase.Menu;
    }

    void Start()
    {
        Hud.Build(this);
        MainMenu();
        Audio.StartAmbience();
    }

    void Update()
    {
        if (Input.CapturePressed) Capture();
        if (Input.PausePressed)
        {
            if (Hud.HelpOpen) Hud.CloseHelp();
            else if (Phase == RunPhase.Playing) Pause();
            else if (Phase == RunPhase.Paused) Resume();
        }
        if (Running)
        {
            Session.Step(Time.deltaTime, WorldSpeed);
            Best = Mathf.Max(Best, Session.Record);
            Spawner.Tick(Time.deltaTime);
            saveTimer += Time.deltaTime;
            if (saveTimer >= 2f) { Save(); saveTimer = 0; }
        }
        if (Phase == RunPhase.Dying)
        {
            deathDelay -= Time.deltaTime;
            if (deathDelay <= 0) { Phase = RunPhase.GameOver; Hud.ShowGameOver(); }
        }
    }

    public void Begin()
    {
        Time.timeScale = 1;
        Spawner.Clear();
        Effects.Clear();
        Session.Begin(Best);
        saveTimer = 0;
        Phase = RunPhase.Playing;
        Player.ResetRun();
        Spawner.ResetRun();
        Hud.ShowRun();
        Audio.Play("start");
    }

    public void MainMenu()
    {
        Save();
        Time.timeScale = 1;
        Phase = RunPhase.Menu;
        Session.Stop();
        Spawner.Clear();
        Effects.Clear();
        Player.PresentMenu();
        Hud.ShowMainMenu();
    }

    public void Pause()
    {
        if (!Running) return;
        Phase = RunPhase.Paused;
        Time.timeScale = 0;
        Save();
        Hud.ShowPause();
        Audio.Play("click");
    }

    public void Resume()
    {
        if (Phase != RunPhase.Paused) return;
        Time.timeScale = 1;
        Phase = RunPhase.Playing;
        Hud.ShowRun();
        Audio.Play("click");
    }

    public void Damage()
    {
        if (!Running || !Session.Damage()) return;
        Effects.Impact(Player.transform.position, new Color(1f, .3f, .35f), .18f);
        Audio.Play(Session.Lives == 0 ? "death" : "hit");
        Hud.Flash();
        if (Session.Lives != 0) return;
        Player.Die();
        Phase = RunPhase.Dying;
        Session.Stop();
        deathDelay = .85f;
        Save();
    }

    public void CollectCoin(Vector3 position)
    {
        if (!Running) return;
        Session.CollectCoin();
        Best = Mathf.Max(Best, Session.Record);
        Audio.Play("coin");
        Effects.Burst(position, new Color(1f, .76f, .25f), 7);
        Hud.CoinPopup();
    }

    public void ToggleMotion()
    {
        MotionEnabled = !MotionEnabled;
        PlayerPrefs.SetInt("LumenRun.Motion", MotionEnabled ? 1 : 0);
        PlayerPrefs.Save();
        Hud.RefreshSettings();
    }

    public void ToggleAudio()
    {
        Audio.Toggle();
        Hud.RefreshSettings();
    }

    void Save()
    {
        if (Session == null) return;
        Best = Mathf.Max(Best, Session.Record);
        PlayerPrefs.SetInt("LumenRun.Best", Best);
        PlayerPrefs.Save();
    }

    void Capture()
    {
        string directory = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../Docs/Screenshots"));
        System.IO.Directory.CreateDirectory(directory);
        string file = System.IO.Path.Combine(directory, Phase + "_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".png");
        ScreenCapture.CaptureScreenshot(file);
        Debug.Log("Screenshot saved: " + file);
    }

    void OnApplicationFocus(bool focused)
    {
        if (!focused && Running) Pause();
    }

    void OnApplicationPause(bool paused)
    {
        if (!paused) return;
        Save();
        if (Running) Pause();
    }

    void OnApplicationQuit() { Save(); }
    void OnDestroy() { Save(); Time.timeScale = 1; }
}

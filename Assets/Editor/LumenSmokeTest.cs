using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;

[InitializeOnLoad]
public static class LumenSmokeTest
{
    const string Flag = "Lumen.Smoke.Active";
    const string Result = "Lumen.Smoke.Result";
    static int step;
    static double changed;
    static double started;
    static float score;
    static int coinCount;
    static Vector3 position;

    static LumenSmokeTest()
    {
        EditorApplication.playModeStateChanged += ModeChanged;
        EditorApplication.update += Tick;
    }

    [MenuItem("Lumen/Run Automated Play Check")]
    public static void Start()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play before starting the automated check.");
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        LumenProjectSetup.Validate();
        foreach (string key in new[] { "LumenRun.Best", "LumenRun.Sound", "LumenRun.Motion" })
        {
            SessionState.SetBool("Lumen.Smoke.Has." + key, PlayerPrefs.HasKey(key));
            SessionState.SetInt("Lumen.Smoke.Pref." + key, PlayerPrefs.GetInt(key));
        }
        SessionState.SetBool(Flag, true);
        SessionState.SetString(Result, "FAIL: The check did not finish.");
        EditorSceneManager.OpenScene(LumenProjectSetup.ScenePath);
        EditorApplication.isPlaying = true;
    }

    static void ModeChanged(PlayModeStateChange change)
    {
        if (!SessionState.GetBool(Flag, false)) return;
        if (change == PlayModeStateChange.EnteredPlayMode)
        {
            step = 0;
            changed = started = EditorApplication.timeSinceStartup;
        }
        if (change != PlayModeStateChange.EnteredEditMode) return;
        foreach (string key in new[] { "LumenRun.Best", "LumenRun.Sound", "LumenRun.Motion" })
        {
            if (SessionState.GetBool("Lumen.Smoke.Has." + key, false))
                PlayerPrefs.SetInt(key, SessionState.GetInt("Lumen.Smoke.Pref." + key, 0));
            else PlayerPrefs.DeleteKey(key);
        }
        PlayerPrefs.Save();
        SessionState.SetBool(Flag, false);
        string result = SessionState.GetString(Result, "FAIL");
        Directory.CreateDirectory("TestResults");
        File.WriteAllText("TestResults/PlaySmokeTest.txt", DateTime.UtcNow.ToString("O") + "\n" + result
            + "\nKeyboard mappings, visual polish and standalone build still need manual review.");
        if (result.StartsWith("PASS")) Debug.Log(result);
        else Debug.LogError(result);
    }

    static void Require(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
    }

    static void Next() { step++; changed = EditorApplication.timeSinceStartup; }
    static double Elapsed { get { return EditorApplication.timeSinceStartup - changed; } }

    static void Finish(string result)
    {
        SessionState.SetString(Result, result);
        EditorApplication.isPlaying = false;
    }

    static void Tick()
    {
        if (!SessionState.GetBool(Flag, false) || !EditorApplication.isPlaying ||
            EditorApplication.isCompiling || EditorApplication.isPaused) return;
        if (EditorApplication.timeSinceStartup - started > 45)
        {
            Finish("FAIL: Automated Play check timed out.");
            return;
        }
        try
        {
            var game = UnityEngine.Object.FindAnyObjectByType<GameController>();
            if (game == null || game.Session == null) return;
            if (step == 0 && Elapsed > .25)
            {
                Require(game.Phase == RunPhase.Menu, "Main menu did not open.");
                Require(UnityEngine.Object.FindObjectsByType<Canvas>().Length == 1, "Expected one Canvas.");
                Require(UnityEngine.Object.FindAnyObjectByType<InputSystemUIInputModule>() != null, "New Input System UI module is missing.");
                game.Begin();
                Next();
            }
            else if (step == 1 && Elapsed > .3)
            {
                game.Spawner.Clear();
                Require(game.Running && game.Session.Lives == 3, "Run did not start with three lives.");
                position = game.Player.transform.position;
                game.Player.RequestJump();
                Next();
            }
            else if (step == 2 && Elapsed > .12)
            {
                Require(game.Player.transform.position.y > position.y + .12f, "Physics jump did not move upward.");
                Next();
            }
            else if (step == 3 && Elapsed > .85)
            {
                Require(game.Player.Grounded, "Dinosaur did not land.");
                Require(game.Player.TryDash(), "Dash did not start.");
                Require(!game.Player.TryDash(), "Dash cooldown did not block a repeated dash.");
                Require(game.Player.Dashing, "Dash state is inactive.");
                Next();
            }
            else if (step == 4 && Elapsed > .45)
            {
                Require(!game.Player.Dashing && game.Player.Cooldown > 0, "Dash duration or cooldown is incorrect.");
                game.Spawner.Clear();
                game.Pause();
                score = game.Session.Score;
                position = game.Player.transform.position;
                Next();
            }
            else if (step == 5 && Elapsed > .3)
            {
                Require(game.Phase == RunPhase.Paused && Time.timeScale == 0, "Pause state is incorrect.");
                Require(game.Session.Score == score && (game.Player.transform.position - position).sqrMagnitude < .0001f, "Pause did not freeze the run.");
                game.Resume();
                Require(game.Running && Time.timeScale == 1, "Resume failed.");
                game.Spawner.Clear();
                score = game.Session.Score;
                coinCount = game.Session.Coins;
                game.Spawner.Spawn(InteractableKind.Coin,
                    (Vector2)game.Player.transform.position + Vector2.right * .25f, 0);
                Next();
            }
            else if (step == 6 && Elapsed > .25)
            {
                Require(game.Session.Coins == coinCount + 1 && game.Session.Score >= score + 100,
                    "The physical coin trigger did not award its score.");
                game.Spawner.Clear();
                game.Spawner.Spawn(InteractableKind.Ground,
                    new Vector2(game.Player.transform.position.x + .25f, DinoMotor.GroundY), 0);
                Next();
            }
            else if (step == 7 && Elapsed > .25)
            {
                Require(game.Session.Lives == 2, "The ground obstacle did not remove one life.");
                game.Player.Hit();
                Require(game.Session.Lives == 2, "Post-hit grace did not prevent duplicate damage.");
                Next();
            }
            else if (step == 8 && Elapsed > 1.05)
            {
                game.Spawner.Clear();
                game.Spawner.Spawn(InteractableKind.Air,
                    (Vector2)game.Player.transform.position + Vector2.right * .25f, 0);
                Next();
            }
            else if (step == 9 && Elapsed > .25)
            {
                Require(game.Session.Lives == 1, "The air obstacle did not remove one life.");
                Next();
            }
            else if (step == 10 && Elapsed > 1.05)
            {
                game.Spawner.Clear();
                game.Spawner.Spawn(InteractableKind.Ground,
                    new Vector2(game.Player.transform.position.x + .25f, DinoMotor.GroundY), 1);
                Next();
            }
            else if (step == 11 && Elapsed > .25)
            {
                Require(game.Session.Lives == 0 && game.Phase == RunPhase.Dying, "Third hit did not start death.");
                Next();
            }
            else if (step == 12 && Elapsed > 1.05)
            {
                Require(game.Phase == RunPhase.GameOver, "Game Over did not open.");
                Require(game.Player.Animator.GetCurrentAnimatorStateInfo(0).IsName("Death"), "Death Animator state did not play.");
                Require(PlayerPrefs.GetInt("LumenRun.Best", -1) == game.Best, "Record was not saved.");
                game.Begin();
                Require(game.Session.Lives == 3 && game.Session.IntegerScore == 0 && game.Spawner.ActiveCount == 0, "Restart did not reset the run.");
                game.MainMenu();
                Require(game.Phase == RunPhase.Menu && Time.timeScale == 1, "Return to main menu failed.");
                Finish("PASS: runtime menu, Canvas, new UI input, physics jump, landing, dash, cooldown, pause, resume, physical coin trigger, ground and air collision triggers, three-hit damage, death Animator, Game Over, saved record and restart.");
            }
        }
        catch (Exception error) { Finish("FAIL: " + error.Message); }
    }
}

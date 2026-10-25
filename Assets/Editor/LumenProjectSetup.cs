using System;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class LumenProjectSetup
{
    public const string ScenePath = "Assets/Scenes/Game.unity";
    public const string ArtPath = "Assets/Resources/LumenArt.asset";
    const string AnimatorPath = "Assets/Resources/Dinosaur.controller";
    static bool queued;
    static bool building;

    [InitializeOnLoadMethod]
    static void Initialize() { Schedule(); }

    public static void Schedule()
    {
        if (queued || building) return;
        queued = true;
        EditorApplication.update += AutoPrepare;
    }

    static void AutoPrepare()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating ||
            EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.update -= AutoPrepare;
        queued = false;
        if (building || ArtIsReady()) return;
        if (!File.Exists(Path.Combine(Application.dataPath, "Resources/Art/atlas.png"))) return;
        try
        {
            GenerateAssets();
            if (!UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty) GenerateScene();
            Debug.Log("Lumen Run is ready. Open Assets/Scenes/Game.unity and press Play.");
        }
        catch (Exception error) { Debug.LogException(error); }
    }

    [MenuItem("Lumen/Prepare Project")]
    public static void Prepare()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play before preparing the project.");
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        GenerateAssets();
        GenerateScene();
        Validate();
    }

    static string ArtFile(string name)
    {
        string path = Path.Combine(Application.dataPath, "Resources", "Art", name + ".png");
        if (!File.Exists(path))
            throw new FileNotFoundException("Source image is missing. Copy Assets/Resources/Art from the complete ZIP. Expected: " + path, path);
        return path;
    }

    static Texture2D Texture(string name)
    {
        string path = ArtFile(name);
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        try
        {
            if (!ImageConversion.LoadImage(texture, File.ReadAllBytes(path), false))
                throw new InvalidDataException("Could not decode the PNG image: " + path);
            texture.name = "Lumen_" + name;
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            return SaveGenerated(texture, "Assets/Generated/Textures/" + name + ".asset");
        }
        catch
        {
            if (texture != null && !EditorUtility.IsPersistent(texture)) UnityEngine.Object.DestroyImmediate(texture);
            throw;
        }
    }

    static bool ArtIsReady()
    {
        var art = AssetDatabase.LoadAssetAtPath<LumenArt>(ArtPath);
        return art != null && art.MissingAsset() == null;
    }

    static T SaveGenerated<T>(T value, string path) where T : UnityEngine.Object
    {
        if (File.Exists(path)) AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(value, path);
        EditorUtility.SetDirty(value);
        AssetDatabase.SaveAssetIfDirty(value);
        var saved = AssetDatabase.LoadAssetAtPath<T>(path);
        if (saved == null) throw new InvalidOperationException("Could not save generated asset: " + path);
        return saved;
    }

    static Sprite AddSprite(Texture2D texture, string name, Rect rect,
        Vector2 pivot, float pixelsPerUnit, Vector4 border)
    {
        var sprite = Sprite.Create(texture, rect, pivot, pixelsPerUnit, 0, SpriteMeshType.FullRect, border);
        if (sprite == null) throw new InvalidOperationException("Could not create sprite: " + name);
        sprite.name = name;
        return SaveGenerated(sprite, "Assets/Generated/Sprites/" + name + ".asset");
    }

    static float BasePivot(Color32[] pixels, int textureWidth, Rect rect)
    {
        int minY = (int)rect.height;
        for (int y = 0; y < (int)rect.height; y++)
        {
            bool visible = false;
            for (int x = 1; x < (int)rect.width - 1; x++)
            {
                int index = ((int)rect.y + y) * textureWidth + (int)rect.x + x;
                if (pixels[index].a > 180) { visible = true; break; }
            }
            if (visible) { minY = y; break; }
        }
        return Mathf.Clamp01(minY / rect.height);
    }

    static void GenerateAssets()
    {
        if (building) return;
        building = true;
        string stage = "checking the source PNG files";
        LumenArt bank = null;
        try
        {
            Directory.CreateDirectory("Assets/Resources");
            Directory.CreateDirectory("Assets/Scenes");
            Directory.CreateDirectory("Assets/Generated");
            Directory.CreateDirectory("Assets/Generated/Sprites");
            Directory.CreateDirectory("Assets/Generated/UI");
            Directory.CreateDirectory("Assets/Generated/Textures");
            foreach (string name in new[] { "atlas", "environment", "foliage" }) ArtFile(name);
            AssetDatabase.Refresh();
            if (File.Exists(AnimatorPath)) AssetDatabase.DeleteAsset(AnimatorPath);
            foreach (string name in new[] { "Idle", "Run", "Jump", "Hit", "Death", "Dash" })
            {
                string clipPath = "Assets/Generated/" + name + ".anim";
                if (File.Exists(clipPath)) AssetDatabase.DeleteAsset(clipPath);
            }
            string materialPath = "Assets/Generated/Panorama.mat";
            if (File.Exists(materialPath)) AssetDatabase.DeleteAsset(materialPath);
            stage = "creating animation sprites";
            bank = ScriptableObject.CreateInstance<LumenArt>();
            bank.name = "LumenArt";
            var atlas = Texture("atlas");
            var atlasPixels = atlas.GetPixels32();
            bank.Frames = new Sprite[16];
            string[] names =
            {
                "Run_A", "Run_B", "Run_C", "Run_D", "Jump", "Hit", "Death", "Dash",
                "Crystal_Tall", "Crystal_Low", "Beetle_A", "Beetle_B",
                "Coin_Front", "Coin_Turn", "Coin_Edge", "Spark"
            };
            float pixelsPerUnit = atlas.width / 4f / 2.5f;
            for (int i = 0; i < 16; i++)
            {
                int column = i % 4;
                int row = i / 4;
                int x0 = Mathf.RoundToInt(column * atlas.width / 4f);
                int x1 = Mathf.RoundToInt((column + 1) * atlas.width / 4f);
                int y0 = Mathf.RoundToInt((3 - row) * atlas.height / 4f);
                int y1 = Mathf.RoundToInt((4 - row) * atlas.height / 4f);
                var rect = new Rect(x0, y0, x1 - x0, y1 - y0);
                var pivot = new Vector2(.5f, i <= 9 ? BasePivot(atlasPixels, atlas.width, rect) : .5f);
                bank.Frames[i] = AddSprite(atlas, names[i], rect, pivot, pixelsPerUnit, Vector4.zero);
            }
            stage = "creating the environment sprites";
            var panorama = Texture("environment");
            bank.Environment = AddSprite(panorama, "MoonlitRuins", new Rect(0, 0, panorama.width, panorama.height),
                new Vector2(.5f, .5f), 100, Vector4.zero);
            var foliage = Texture("foliage");
            bank.Foliage = AddSprite(foliage, "JungleFerns", new Rect(0, 0, foliage.width, Mathf.CeilToInt(foliage.height * .34f)),
                new Vector2(.5f, 0), 100, Vector4.zero);
            stage = "creating the interface assets";
            bank.Rounded = UiSprite("Rounded", 64, 0);
            bank.Disc = UiSprite("Disc", 64, 1);
            bank.White = UiSprite("White", 4, 2);
            bank.BodyFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Fonts/Body.ttf");
            bank.DisplayFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/Fonts/Display.ttf");
            var shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/Shaders/SeamBlend.shader");
            if (shader == null) throw new InvalidOperationException("Missing panorama shader.");
            stage = "creating the panorama material";
            var material = new Material(shader) { name = "PanoramaSeamBlend" };
            material.SetFloat("_EdgeFade", .08f);
            AssetDatabase.CreateAsset(material, materialPath);
            bank.PanoramaMaterial = material;
            stage = "creating the Animator";
            bank.Animator = CreateAnimator(bank);
            string missing = bank.MissingAsset();
            if (missing != null) throw new InvalidOperationException("Missing generated asset: " + missing);
            stage = "saving the complete art catalog";
            var existing = AssetDatabase.LoadAssetAtPath<LumenArt>(ArtPath);
            if (existing == null)
            {
                if (File.Exists(ArtPath)) AssetDatabase.DeleteAsset(ArtPath);
                AssetDatabase.CreateAsset(bank, ArtPath);
                existing = bank;
                bank = null;
            }
            else
            {
                EditorUtility.CopySerialized(bank, existing);
            }
            EditorUtility.SetDirty(existing);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(ArtPath, ImportAssetOptions.ForceSynchronousImport);
            var reloaded = AssetDatabase.LoadAssetAtPath<LumenArt>(ArtPath);
            if (reloaded == null || reloaded.MissingAsset() != null)
                throw new InvalidOperationException("The saved art catalog is incomplete after import.");
        }
        catch (Exception error)
        {
            var failure = new InvalidOperationException("Lumen preparation failed while " + stage
                + ". See TestResults/SetupFailure.txt for details.", error);
            Directory.CreateDirectory("TestResults");
            File.WriteAllText("TestResults/SetupFailure.txt", DateTime.UtcNow.ToString("O") + "\n" + failure);
            throw failure;
        }
        finally
        {
            if (bank != null && !EditorUtility.IsPersistent(bank)) UnityEngine.Object.DestroyImmediate(bank);
            building = false;
        }
    }

    static Sprite UiSprite(string name, int size, int kind)
    {
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = "UI_" + name;
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        var pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float alpha = 1f;
                if (kind == 0)
                {
                    float qx = Mathf.Abs(x + .5f - size * .5f) - (size * .5f - 16);
                    float qy = Mathf.Abs(y + .5f - size * .5f) - (size * .5f - 16);
                    float d = new Vector2(Mathf.Max(qx, 0), Mathf.Max(qy, 0)).magnitude
                        + Mathf.Min(Mathf.Max(qx, qy), 0) - 16;
                    alpha = Mathf.Clamp01(.5f - d);
                }
                else if (kind == 1)
                {
                    float d = Vector2.Distance(new Vector2(x + .5f, y + .5f), Vector2.one * size * .5f) / (size * .5f);
                    alpha = Mathf.Pow(Mathf.Clamp01(1 - d), 1.7f);
                }
                pixels[y * size + x] = new Color(1, 1, 1, alpha);
            }
        }
        texture.SetPixels32(pixels);
        texture.Apply(false, false);
        texture = SaveGenerated(texture, "Assets/Generated/UI/" + name + "Texture.asset");
        return AddSprite(texture, name, new Rect(0, 0, size, size), new Vector2(.5f, .5f),
            size, kind == 0 ? Vector4.one * 16 : Vector4.zero);
    }

    static AnimationClip Clip(LumenArt bank, string name, int[] frames, float step, bool loop)
    {
        var clip = new AnimationClip { name = name, frameRate = 12 };
        var keys = new ObjectReferenceKeyframe[frames.Length];
        for (int i = 0; i < frames.Length; i++)
            keys[i] = new ObjectReferenceKeyframe { time = i * step, value = bank.Frame(frames[i]) };
        AnimationUtility.SetObjectReferenceCurve(clip,
            new EditorCurveBinding { path = "", type = typeof(SpriteRenderer), propertyName = "m_Sprite" }, keys);
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        var y = name == "Idle"
            ? new AnimationCurve(new Keyframe(0, -.76f), new Keyframe(.4f, -.70f), new Keyframe(.8f, -.76f))
            : AnimationCurve.Constant(0, Mathf.Max(.1f, (frames.Length - 1) * step), -.76f);
        clip.SetCurve("", typeof(Transform), "m_LocalPosition.y", y);
        AssetDatabase.CreateAsset(clip, "Assets/Generated/" + name + ".anim");
        return clip;
    }

    static AnimatorStateTransition Transition(AnimatorState from, AnimatorState to, bool exit, params string[] conditions)
    {
        var transition = from.AddTransition(to);
        transition.hasExitTime = exit;
        transition.exitTime = 1;
        transition.duration = 0;
        foreach (var condition in conditions)
            transition.AddCondition(condition[0] == '!' ? AnimatorConditionMode.IfNot : AnimatorConditionMode.If,
                0, condition.TrimStart('!'));
        return transition;
    }

    static RuntimeAnimatorController CreateAnimator(LumenArt bank)
    {
        var controller = AnimatorController.CreateAnimatorControllerAtPath(AnimatorPath);
        foreach (string name in new[] { "Menu", "Airborne", "Dead", "Dashing" })
            controller.AddParameter(name, AnimatorControllerParameterType.Bool);
        controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
        var machine = controller.layers[0].stateMachine;
        var idle = machine.AddState("Idle", new Vector3(0, 0));
        var run = machine.AddState("Run", new Vector3(260, 0));
        var jump = machine.AddState("Jump", new Vector3(520, 0));
        var hit = machine.AddState("Hit", new Vector3(260, 140));
        var death = machine.AddState("Death", new Vector3(520, 140));
        var dash = machine.AddState("Dash", new Vector3(0, 140));
        idle.motion = Clip(bank, "Idle", new[] { 0, 0 }, .8f, true);
        run.motion = Clip(bank, "Run", new[] { 0, 1, 2, 3, 0 }, .08f, true);
        jump.motion = Clip(bank, "Jump", new[] { 4, 4 }, .3f, false);
        hit.motion = Clip(bank, "Hit", new[] { 5, 5 }, .23f, false);
        death.motion = Clip(bank, "Death", new[] { 6, 6 }, .7f, false);
        dash.motion = Clip(bank, "Dash", new[] { 7, 7 }, .32f, false);
        machine.defaultState = idle;
        var dead = machine.AddAnyStateTransition(death);
        dead.hasExitTime = false; dead.duration = 0; dead.canTransitionToSelf = false;
        dead.AddCondition(AnimatorConditionMode.If, 0, "Dead");
        dead.AddCondition(AnimatorConditionMode.IfNot, 0, "Menu");
        var hurt = machine.AddAnyStateTransition(hit);
        hurt.hasExitTime = false; hurt.duration = 0; hurt.canTransitionToSelf = false;
        hurt.AddCondition(AnimatorConditionMode.If, 0, "Hit");
        hurt.AddCondition(AnimatorConditionMode.IfNot, 0, "Dead");
        hurt.AddCondition(AnimatorConditionMode.IfNot, 0, "Menu");
        var home = machine.AddAnyStateTransition(idle);
        home.hasExitTime = false; home.duration = 0; home.canTransitionToSelf = false;
        home.AddCondition(AnimatorConditionMode.If, 0, "Menu");
        Transition(idle, run, false, "!Menu", "!Dead");
        Transition(run, jump, false, "Airborne", "!Dead");
        Transition(run, dash, false, "Dashing", "!Airborne", "!Dead");
        Transition(jump, run, false, "!Airborne", "!Dashing", "!Dead");
        Transition(jump, dash, false, "!Airborne", "Dashing", "!Dead");
        Transition(dash, run, false, "!Dashing", "!Airborne", "!Dead");
        Transition(dash, jump, false, "Airborne", "!Dead");
        Transition(hit, jump, true, "Airborne", "!Dead");
        Transition(hit, dash, true, "!Airborne", "Dashing", "!Dead");
        Transition(hit, run, true, "!Airborne", "!Dashing", "!Dead");
        return controller;
    }

    static void GenerateScene()
    {
        var art = AssetDatabase.LoadAssetAtPath<LumenArt>(ArtPath);
        if (art == null || art.MissingAsset() != null)
            throw new InvalidOperationException("Prepare the complete art assets before creating the scene.");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameWorldBuilder.Build(art);
        PlayerSettings.companyName = "Student";
        PlayerSettings.productName = "Lumen Run";
        PlayerSettings.defaultScreenWidth = 1600;
        PlayerSettings.defaultScreenHeight = 900;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.runInBackground = true;
        if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new InvalidOperationException("Could not save the game scene.");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Lumen/Validate Project")]
    public static void Validate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play before validating the project.");
        if (!ArtIsReady()) { GenerateAssets(); GenerateScene(); }
        var art = AssetDatabase.LoadAssetAtPath<LumenArt>(ArtPath);
        if (art == null || art.MissingAsset() != null)
            throw new InvalidOperationException("One or more required art assets are missing after preparation.");
        if (ShaderUtil.ShaderHasError(art.PanoramaMaterial.shader))
            throw new InvalidOperationException("The panorama shader has compilation errors.");
        foreach (string name in new[] { "jump", "coin", "dash", "hit", "death", "click", "start", "ambience" })
            if (AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/Audio/" + name + ".wav") == null)
                throw new InvalidOperationException("Missing audio clip: " + name);
        var controller = art.Animator as AnimatorController;
        if (controller == null) throw new InvalidOperationException("Animator controller is invalid.");
        foreach (string name in new[] { "Run", "Jump", "Hit", "Death" })
        {
            bool found = false;
            foreach (var state in controller.layers[0].stateMachine.states)
                if (state.state.name == name && state.state.motion != null) found = true;
            if (!found) throw new InvalidOperationException("Missing Animator state: " + name);
        }
        var settings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
        if (settings.Length == 0) throw new InvalidOperationException("Project settings could not be loaded.");
        var input = new SerializedObject(settings[0]).FindProperty("activeInputHandler");
        if (input == null || input.intValue != 1) throw new InvalidOperationException("Active Input Handling must be Input System Package (New).");
        string result = RunSessionChecks.Run();
        Directory.CreateDirectory("TestResults");
        File.WriteAllText("TestResults/EditorValidation.txt", DateTime.UtcNow.ToString("O") + "\n" + result
            + "\nAssets, audio, shader, input settings and required Animator states passed.\nPlay testing is a separate check.");
        Debug.Log("Lumen validation passed. " + result);
    }

    [MenuItem("Lumen/Build Windows x64")]
    public static void BuildWindows()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Validate();
        if (!File.Exists(ScenePath)) GenerateScene();
        Directory.CreateDirectory("Builds/Windows");
        var result = BuildPipeline.BuildPlayer(new[] { ScenePath }, "Builds/Windows/LumenRun.exe",
            BuildTarget.StandaloneWindows64, BuildOptions.None);
        if (result.summary.result != BuildResult.Succeeded)
            throw new InvalidOperationException("Windows build failed: " + result.summary.result);
        Debug.Log("Lumen Run Windows build is ready in Builds/Windows.");
    }
}

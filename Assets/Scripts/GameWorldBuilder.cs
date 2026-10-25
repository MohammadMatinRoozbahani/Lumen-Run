using UnityEngine;

public static class GameWorldBuilder
{
    public static GameController Build(LumenArt art)
    {
        string missing = art == null ? "LumenArt" : art.MissingAsset();
        if (missing != null)
            throw new System.InvalidOperationException("Lumen Run setup is incomplete: " + missing
                + ". Stop Play and choose Lumen > Prepare Project.");
        var root = new GameObject("LumenWorld");
        root.SetActive(false);
        var game = root.AddComponent<GameController>();
        game.Art = art;
        game.Input = root.AddComponent<RunnerInput>();
        var camera = Child("Main Camera", root.transform).AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.transform.position = new Vector3(0, 0, -10);
        camera.orthographic = true;
        camera.orthographicSize = 5.2f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.018f, .055f, .1f);
        camera.gameObject.AddComponent<AudioListener>();
        var environment = Child("Environment", root.transform).transform;
        Strip(game, environment, "Moonlit Ruins", art.Environment, 12f, -1f, .045f, -30, Color.white);
        Strip(game, environment, "Distant Growth", art.Foliage, 2.7f, -5.2f, .25f, -12, new Color(.2f, .5f, .5f, .48f));
        Strip(game, environment, "Foreground Ferns", art.Foliage, 1.75f, -5.25f, 1.05f, 12, Color.white);
        var floor = Child("Ground", environment);
        floor.layer = 6;
        floor.transform.position = new Vector3(0, -3.1f, 0);
        floor.AddComponent<BoxCollider2D>().size = new Vector2(80f, .2f);
        Rectangle(environment, "Runway Shade", art.White, new Vector3(0, -3.31f, 0), new Vector2(40, .6f),
            new Color(.015f, .065f, .09f, .3f), -2);
        Rectangle(environment, "Runway Edge", art.White, new Vector3(0, -3.025f, 0), new Vector2(40, .025f),
            new Color(.29f, .9f, .77f, .6f), 0);
        for (int i = 0; i < 9; i++)
        {
            var marker = Rectangle(environment, "Luminous Paver " + i, art.White, new Vector3(-12f + i * 3f, -3.11f, 0),
                new Vector2(1.55f, .026f), new Color(.39f, .86f, .75f, .38f), 0);
            var scrolling = marker.gameObject.AddComponent<ParallaxStrip>();
            scrolling.Game = game;
            scrolling.Factor = 1;
            scrolling.Step = 3;
            scrolling.TileCount = 9;
            scrolling.LeftBoundary = -13;
        }
        var player = Child("Dinosaur", root.transform);
        player.transform.position = new Vector3(-5.8f, -2.24f, 0);
        var body = player.AddComponent<Rigidbody2D>();
        body.gravityScale = 3.1f;
        body.linearDamping = 0;
        body.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        var collider = player.AddComponent<CapsuleCollider2D>();
        collider.direction = CapsuleDirection2D.Vertical;
        collider.size = new Vector2(.75f, 1.52f);
        collider.offset = new Vector2(.1f, 0);
        var visual = Child("Visual", player.transform);
        visual.transform.localPosition = new Vector3(0, -.76f, 0);
        var renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = art.Frame(0);
        renderer.sortingOrder = 5;
        var animator = visual.AddComponent<Animator>();
        animator.runtimeAnimatorController = art.Animator;
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        var motor = player.AddComponent<DinoMotor>();
        motor.Game = game;
        motor.Animator = animator;
        motor.Visual = renderer;
        var shadow = Rectangle(root.transform, "Dinosaur Shadow", art.Disc, new Vector3(-5.8f, -3.015f, 0),
            new Vector2(1.65f, .16f), new Color(0, .02f, .04f, .5f), 1);
        motor.Shadow = shadow.transform;
        game.Player = motor;
        var spawner = Child("Interactables", root.transform).AddComponent<WorldSpawner>();
        spawner.Game = game;
        game.Spawner = spawner;
        var effects = Child("Visual Effects", root.transform).AddComponent<LumenEffects>();
        effects.Game = game;
        effects.Camera = camera;
        game.Effects = effects;
        var audio = Child("Audio", root.transform).AddComponent<LumenAudio>();
        audio.Game = game;
        game.Audio = audio;
        game.Hud = Child("Interface", root.transform).AddComponent<LumenHud>();
        root.SetActive(true);
        return game;
    }

    static GameObject Child(string name, Transform parent)
    {
        var node = new GameObject(name);
        node.transform.SetParent(parent, false);
        return node;
    }

    static SpriteRenderer Rectangle(Transform parent, string name, Sprite sprite, Vector3 position,
        Vector2 size, Color color, int order)
    {
        var view = Child(name, parent).AddComponent<SpriteRenderer>();
        view.sprite = sprite;
        view.color = color;
        view.sortingOrder = order;
        view.transform.position = position;
        view.transform.localScale = new Vector3(size.x / sprite.bounds.size.x, size.y / sprite.bounds.size.y, 1);
        return view;
    }

    static void Strip(GameController game, Transform parent, string name, Sprite sprite,
        float height, float y, float factor, int order, Color color)
    {
        float scale = height / sprite.bounds.size.y;
        float width = sprite.bounds.size.x * scale;
        float step = width * .92f;
        for (int i = -1; i <= 1; i++)
        {
            var view = Child(name + " " + (i + 1), parent).AddComponent<SpriteRenderer>();
            view.sprite = sprite;
            view.color = color;
            view.sharedMaterial = game.Art.PanoramaMaterial;
            view.sortingOrder = order;
            view.transform.localScale = Vector3.one * scale;
            view.transform.position = new Vector3(i * step, y, 0);
            var scrolling = view.gameObject.AddComponent<ParallaxStrip>();
            scrolling.Game = game;
            scrolling.Factor = factor;
            scrolling.Step = step;
        }
    }
}

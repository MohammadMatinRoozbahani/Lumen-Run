using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Events;

public sealed class LumenHud : MonoBehaviour
{
    GameController game;
    Transform canvas;
    GameObject hudRoot;
    GameObject mainRoot;
    GameObject pauseRoot;
    GameObject overRoot;
    GameObject helpRoot;
    Text score;
    Text record;
    Text lives;
    Text distance;
    Text coins;
    Text level;
    Text dashStatus;
    Text menuRecord;
    Text overStats;
    Text overRecord;
    Text audioLabel;
    Text motionLabel;
    Text coinPopup;
    Image health;
    Image dashFill;
    Image flash;
    Button startButton;
    Button resumeButton;
    Button retryButton;
    Button helpBackButton;
    Button pauseButton;
    float flashTime;
    float popupTime;
    int cachedScore = -1;
    int cachedBest = -1;
    int cachedLives = -1;
    int cachedCoins = -1;
    int cachedLevel = -1;
    int cachedDistance = -1;
    public bool HelpOpen { get { return helpRoot != null && helpRoot.activeSelf; } }
    static readonly Color Ink = new Color(.022f, .065f, .095f, .94f);
    static readonly Color Mint = new Color(.36f, .98f, .83f);
    static readonly Color Gold = new Color(1f, .75f, .29f);
    static readonly Color Soft = new Color(.64f, .77f, .8f);

    public void Build(GameController owner)
    {
        game = owner;
        var system = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        system.transform.SetParent(transform, false);
        var surface = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        surface.transform.SetParent(transform, false);
        surface.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        surface.GetComponent<Canvas>().sortingOrder = 100;
        var scaler = surface.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600, 900);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        canvas = surface.transform;
        BuildRunHud();
        BuildMainMenu();
        BuildPauseMenu();
        BuildGameOver();
        BuildHelp();
        flash = FullScreen("ImpactFlash", canvas, new Color(1f, .12f, .2f, 0)).GetComponent<Image>();
        flash.raycastTarget = false;
        RefreshSettings();
    }

    RectTransform Node(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var node = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        node.SetParent(parent, false);
        node.sizeDelta = size;
        node.anchoredPosition = position;
        return node;
    }

    Image Panel(string name, Transform parent, Vector2 position, Vector2 size, Color color)
    {
        var node = Node(name, parent, position, size);
        var image = node.gameObject.AddComponent<Image>();
        image.sprite = game.Art.Rounded;
        image.type = Image.Type.Sliced;
        image.color = color;
        return image;
    }

    Text Label(string name, Transform parent, string value, Vector2 position, Vector2 size,
        int fontSize, Color color, bool bold = false, TextAnchor anchor = TextAnchor.MiddleLeft)
    {
        var text = Node(name, parent, position, size).gameObject.AddComponent<Text>();
        text.font = bold ? game.Art.DisplayFont : game.Art.BodyFont;
        text.fontSize = fontSize;
        text.alignment = anchor;
        text.color = color;
        text.text = value;
        text.lineSpacing = .94f;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    Button ActionButton(string name, Transform parent, string value, Vector2 position, Vector2 size,
        Color color, UnityAction action, int fontSize = 22)
    {
        Panel(name + "Shadow", parent, position + Vector2.down * 5, size, new Color(0, 0, 0, .23f)).raycastTarget = false;
        var image = Panel(name, parent, position, size, color);
        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        var palette = button.colors;
        palette.normalColor = Color.white;
        palette.highlightedColor = new Color(1.08f, 1.08f, 1.08f);
        palette.pressedColor = new Color(.82f, .88f, .9f);
        palette.selectedColor = new Color(1.08f, 1.08f, 1.08f);
        palette.fadeDuration = .1f;
        button.colors = palette;
        button.onClick.AddListener(() => { game.Audio.Play("click"); action(); });
        Label("Label", image.transform, value, Vector2.zero, size - new Vector2(16, 4), fontSize,
            color == Gold ? new Color(.06f, .12f, .14f) : Color.white, true, TextAnchor.MiddleCenter);
        return button;
    }

    GameObject FullScreen(string name, Transform parent, Color color)
    {
        var root = Node(name, parent, Vector2.zero, Vector2.zero);
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = root.offsetMax = Vector2.zero;
        root.gameObject.AddComponent<Image>().color = color;
        return root.gameObject;
    }

    void BuildRunHud()
    {
        hudRoot = Node("RunHUD", canvas, Vector2.zero, new Vector2(1600, 900)).gameObject;
        var header = Panel("Header", hudRoot.transform, new Vector2(0, 371), new Vector2(1480, 112), Ink);
        Label("LifeTitle", header.transform, "VITALITY", new Vector2(-545, 29), new Vector2(240, 24), 15, Soft, true);
        var track = Panel("HealthTrack", header.transform, new Vector2(-548, -3), new Vector2(238, 17), new Color(.12f, .22f, .25f));
        health = Panel("HealthFill", track.transform, Vector2.zero, new Vector2(238, 17), Mint);
        health.sprite = game.Art.White;
        health.type = Image.Type.Filled;
        health.fillMethod = Image.FillMethod.Horizontal;
        health.fillOrigin = 0;
        lives = Label("LifeValue", header.transform, "3 / 3", new Vector2(-385, -3), new Vector2(62, 28), 18, Color.white, true);
        Label("ScoreTitle", header.transform, "SCORE", new Vector2(-166, 29), new Vector2(220, 24), 15, Soft, true);
        score = Label("CurrentScore", header.transform, "000000", new Vector2(-166, -10), new Vector2(220, 46), 35, Color.white, true);
        Label("BestTitle", header.transform, "BEST RUN", new Vector2(81, 29), new Vector2(230, 24), 15, Soft, true);
        record = Label("HighScore", header.transform, "000000", new Vector2(81, -10), new Vector2(230, 46), 35, Gold, true);
        coins = Label("Coins", header.transform, "0  COINS", new Vector2(340, -7), new Vector2(210, 36), 22, Gold, true);
        level = Label("Stage", header.transform, "GARDEN 01", new Vector2(340, 27), new Vector2(210, 24), 14, Soft, true);
        pauseButton = ActionButton("Pause", header.transform, "II", new Vector2(661, 0), new Vector2(58, 58),
            new Color(.13f, .27f, .3f), game.Pause, 25);
        var footer = Panel("ControlHint", hudRoot.transform, new Vector2(-419, -372), new Vector2(682, 68), Ink);
        Label("Keys", footer.transform, "SPACE  JUMP   /   SHIFT  DASH   /   ESC  PAUSE", Vector2.zero,
            new Vector2(638, 36), 17, new Color(.75f, .86f, .87f), true);
        var dash = Panel("DashCard", hudRoot.transform, new Vector2(549, -361), new Vector2(422, 92), Ink);
        dashStatus = Label("DashStatus", dash.transform, "DASH READY", new Vector2(0, 15), new Vector2(366, 30), 19, Mint, true);
        var dashTrack = Panel("DashTrack", dash.transform, new Vector2(0, -17), new Vector2(366, 9), new Color(.12f, .22f, .25f));
        dashFill = Panel("DashFill", dashTrack.transform, Vector2.zero, new Vector2(366, 9), Mint);
        dashFill.sprite = game.Art.White;
        dashFill.type = Image.Type.Filled;
        dashFill.fillMethod = Image.FillMethod.Horizontal;
        dashFill.fillOrigin = 0;
        distance = Label("Distance", hudRoot.transform, "0 m", new Vector2(660, 284), new Vector2(200, 28),
            16, new Color(.72f, .86f, .88f), false, TextAnchor.MiddleRight);
        coinPopup = Label("CoinPopup", hudRoot.transform, "+100", new Vector2(-485, -85), new Vector2(200, 44),
            30, new Color(1f, .8f, .3f, 0), true, TextAnchor.MiddleCenter);
    }

    void BuildMainMenu()
    {
        mainRoot = FullScreen("MainMenu", canvas, Color.clear);
        Label("Brand", mainRoot.transform, "LR  /  LUMEN RUN", new Vector2(-500, 382), new Vector2(495, 45), 24, Color.white, true);
        Label("Chapter", mainRoot.transform, "THE LOST GARDEN     /     CHAPTER 01", new Vector2(-420, -388),
            new Vector2(650, 26), 14, Soft, true);
        var card = Panel("MainCard", mainRoot.transform, new Vector2(-352, -7), new Vector2(650, 664), new Color(.02f, .065f, .095f, .94f));
        Panel("Accent", card.transform, new Vector2(-246, 300), new Vector2(36, 4), Mint).raycastTarget = false;
        Label("Eyebrow", card.transform, "A SMALL HERO. A WILD WORLD.", new Vector2(0, 268), new Vector2(526, 28), 16, Mint, true);
        Label("Title", card.transform, "LUMEN\nRUN", new Vector2(0, 144), new Vector2(526, 220), 98, Color.white, true);
        Label("Description", card.transform, "Find your rhythm in a luminous world.\nJump. Dash. Go further.", new Vector2(0, 7),
            new Vector2(526, 62), 20, new Color(.69f, .83f, .85f));
        startButton = ActionButton("StartRun", card.transform, "START RUN    >",
            new Vector2(0, -83), new Vector2(526, 70), Gold, game.Begin, 24);
        ActionButton("HowToPlay", card.transform, "HOW TO PLAY", new Vector2(0, -158), new Vector2(526, 49),
            new Color(.1f, .22f, .25f), OpenHelp, 18);
        var best = Panel("BestTile", card.transform, new Vector2(-137, -230), new Vector2(252, 67), new Color(.07f, .15f, .19f));
        Label("BestCaption", best.transform, "PERSONAL BEST", new Vector2(0, 16), new Vector2(214, 20), 12, Soft, true);
        menuRecord = Label("BestValue", best.transform, "000000", new Vector2(0, -12), new Vector2(214, 30), 24, Gold, true);
        var play = Panel("LifeTile", card.transform, new Vector2(137, -230), new Vector2(252, 67), new Color(.07f, .15f, .19f));
        Label("ThreeLives", play.transform, "3 LIVES", new Vector2(0, 12), new Vector2(214, 28), 23, Mint, true);
        Label("TryAgain", play.transform, "ONE MORE ADVENTURE", new Vector2(0, -15), new Vector2(214, 22), 11, Soft, true);
        var audio = ActionButton("Audio", card.transform, "", new Vector2(-137, -295), new Vector2(252, 43),
            new Color(.085f, .18f, .21f), game.ToggleAudio, 14);
        audioLabel = audio.GetComponentInChildren<Text>();
        var motion = ActionButton("Motion", card.transform, "", new Vector2(137, -295), new Vector2(252, 43),
            new Color(.085f, .18f, .21f), game.ToggleMotion, 14);
        motionLabel = motion.GetComponentInChildren<Text>();
        Label("HeroCaption", mainRoot.transform, "READY FOR THE NEXT RUN?", new Vector2(384, -336),
            new Vector2(580, 30), 18, new Color(.71f, .94f, .88f), true, TextAnchor.MiddleCenter);
        ActionButton("Quit", mainRoot.transform, "QUIT", new Vector2(693, 382), new Vector2(112, 44),
            new Color(.025f, .075f, .11f, .9f), Quit, 15);
    }

    void BuildPauseMenu()
    {
        pauseRoot = FullScreen("PauseMenu", canvas, new Color(.012f, .035f, .06f, .72f));
        var card = Panel("PauseCard", pauseRoot.transform, Vector2.zero, new Vector2(620, 448), Ink);
        Label("PauseEyebrow", card.transform, "THE GARDEN CAN WAIT", new Vector2(0, 157), new Vector2(530, 28), 15, Mint, true, TextAnchor.MiddleCenter);
        Label("Paused", card.transform, "PAUSED", new Vector2(0, 72), new Vector2(530, 100), 70, Color.white, true, TextAnchor.MiddleCenter);
        Label("PauseHint", card.transform, "Take a breath. Your run is right here.", new Vector2(0, -2), new Vector2(530, 40), 20, Soft, false, TextAnchor.MiddleCenter);
        resumeButton = ActionButton("Resume", card.transform, "CONTINUE RUN", new Vector2(0, -82), new Vector2(490, 66), Gold, game.Resume, 23);
        ActionButton("BackToMain", card.transform, "MAIN MENU", new Vector2(0, -157), new Vector2(490, 48),
            new Color(.1f, .22f, .25f), game.MainMenu, 18);
    }

    void BuildGameOver()
    {
        overRoot = FullScreen("GameOver", canvas, new Color(.012f, .035f, .06f, .76f));
        var card = Panel("GameOverCard", overRoot.transform, new Vector2(0, -10), new Vector2(680, 552), Ink);
        overRecord = Label("RecordMessage", card.transform, "THERE IS ALWAYS ANOTHER RUN", new Vector2(0, 207),
            new Vector2(600, 30), 16, Mint, true, TextAnchor.MiddleCenter);
        Label("GameOverTitle", card.transform, "GAME OVER", new Vector2(0, 123), new Vector2(600, 98), 69, Color.white, true, TextAnchor.MiddleCenter);
        overStats = Label("RunStats", card.transform, "", new Vector2(0, 8), new Vector2(590, 114), 24, Gold, true, TextAnchor.MiddleCenter);
        retryButton = ActionButton("TryAgain", card.transform, "ONE MORE RUN", new Vector2(0, -122),
            new Vector2(530, 66), Gold, game.Begin, 23);
        ActionButton("Menu", card.transform, "MAIN MENU", new Vector2(0, -198), new Vector2(530, 48),
            new Color(.1f, .22f, .25f), game.MainMenu, 18);
    }

    void BuildHelp()
    {
        helpRoot = FullScreen("HowToPlay", canvas, new Color(.012f, .035f, .06f, .86f));
        var card = Panel("HelpCard", helpRoot.transform, Vector2.zero, new Vector2(820, 654), Ink);
        Label("HelpTitle", card.transform, "FIND YOUR RHYTHM", new Vector2(0, 242), new Vector2(724, 65), 40, Color.white, true, TextAnchor.MiddleCenter);
        HelpRow(card.transform, 125, "SPACE", "JUMP", "Clear the crystals. Time your landing.");
        HelpRow(card.transform, 12, "SHIFT", "DASH", "A burst of speed. Watch the next obstacle.");
        HelpRow(card.transform, -101, "ESC", "PAUSE", "Take a break and continue your run.");
        Label("HelpRules", card.transform, "3 lives   /   +100 per coin   /   Your best is saved", new Vector2(0, -204),
            new Vector2(724, 36), 18, Mint, true, TextAnchor.MiddleCenter);
        helpBackButton = ActionButton("HelpBack", card.transform, "GOT IT", new Vector2(0, -268), new Vector2(724, 54), Gold, CloseHelp, 19);
    }

    void HelpRow(Transform parent, float y, string key, string title, string detail)
    {
        var keycard = Panel("Key_" + key, parent, new Vector2(-259, y), new Vector2(180, 72), new Color(.11f, .24f, .27f));
        Label("Key", keycard.transform, key, Vector2.zero, new Vector2(160, 40), 23, Mint, true, TextAnchor.MiddleCenter);
        Label("Action", parent, title, new Vector2(115, y + 16), new Vector2(500, 35), 25, Color.white, true);
        Label("Detail", parent, detail, new Vector2(115, y - 21), new Vector2(500, 39), 17, Soft);
    }

    void HideScreens()
    {
        mainRoot.SetActive(false);
        pauseRoot.SetActive(false);
        overRoot.SetActive(false);
        helpRoot.SetActive(false);
    }

    void Select(Button button)
    {
        if (EventSystem.current == null) return;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button.gameObject);
    }

    public void ShowMainMenu()
    {
        HideScreens();
        hudRoot.SetActive(false);
        mainRoot.SetActive(true);
        menuRecord.text = game.Best.ToString("000000");
        RefreshSettings();
        Select(startButton);
    }

    public void ShowRun()
    {
        HideScreens();
        hudRoot.SetActive(true);
        flashTime = popupTime = 0;
        cachedScore = cachedBest = cachedLives = cachedCoins = cachedLevel = cachedDistance = -1;
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    public void ShowPause()
    {
        HideScreens();
        pauseRoot.SetActive(true);
        Select(resumeButton);
    }

    public void ShowGameOver()
    {
        HideScreens();
        overRoot.SetActive(true);
        overRecord.text = game.Session.NewRecord ? "A NEW PERSONAL BEST!" : "THERE IS ALWAYS ANOTHER RUN";
        overStats.text = "SCORE   " + game.Session.IntegerScore.ToString("000000")
            + "      BEST   " + game.Best.ToString("000000")
            + "\n\n" + (int)game.Session.Distance + " m explored      " + game.Session.Coins + " coins collected";
        Select(retryButton);
    }

    public void OpenHelp()
    {
        mainRoot.SetActive(false);
        helpRoot.SetActive(true);
        Select(helpBackButton);
    }

    public void CloseHelp()
    {
        helpRoot.SetActive(false);
        if (game.Phase == RunPhase.Menu) mainRoot.SetActive(true);
        Select(game.Phase == RunPhase.Paused ? resumeButton : startButton);
    }

    public void RefreshSettings()
    {
        if (audioLabel != null) audioLabel.text = "AUDIO  " + (game.Audio.Enabled ? "ON" : "OFF");
        if (motionLabel != null) motionLabel.text = "VISUAL FX  " + (game.MotionEnabled ? "ON" : "OFF");
    }

    public void Flash() { flashTime = .2f; }
    public void CoinPopup() { popupTime = .65f; }

    void Update()
    {
        if (game == null || game.Session == null || flash == null) return;
        var session = game.Session;
        if (cachedScore != session.IntegerScore) { cachedScore = session.IntegerScore; score.text = cachedScore.ToString("000000"); }
        if (cachedBest != game.Best) { cachedBest = game.Best; record.text = cachedBest.ToString("000000"); }
        if (cachedLives != session.Lives)
        {
            cachedLives = session.Lives;
            lives.text = cachedLives + " / 3";
            health.fillAmount = cachedLives / 3f;
        }
        if (cachedCoins != session.Coins) { cachedCoins = session.Coins; coins.text = cachedCoins + "  COINS"; }
        if (cachedLevel != session.Level) { cachedLevel = session.Level; level.text = "GARDEN " + cachedLevel.ToString("00"); }
        if (cachedDistance != (int)session.Distance) { cachedDistance = (int)session.Distance; distance.text = cachedDistance + " m"; }
        dashFill.fillAmount = 1f - game.Player.Cooldown / DinoMotor.DashRecovery;
        dashStatus.text = game.Player.Dashing ? "DASH ACTIVE" : game.Player.Cooldown <= 0 ? "DASH READY" : "DASH   " + game.Player.Cooldown.ToString("0.0") + "s";
        pauseButton.interactable = game.Running;
        flashTime = Mathf.Max(0, flashTime - Time.unscaledDeltaTime);
        flash.color = new Color(1f, .13f, .2f, flashTime / .2f * .13f);
        popupTime = Mathf.Max(0, popupTime - Time.deltaTime);
        coinPopup.color = new Color(1f, .8f, .3f, Mathf.Clamp01(popupTime * 4f));
        coinPopup.rectTransform.anchoredPosition = new Vector2(-485, -85 + (.65f - popupTime) * 55f);
    }

    void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

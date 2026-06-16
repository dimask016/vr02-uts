using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System;

public class EvacuationSceneBuilder : EditorWindow
{
    [MenuItem("Tools/Build Evacuation Scene")]
    public static void BuildScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "UTS_Dimas_EvacuationScene";

        // ========== 0. LIGHTING ==========
        GameObject lightGO = new GameObject("Directional Light");
        Light sunLight = lightGO.AddComponent<Light>();
        sunLight.type = LightType.Directional;
        sunLight.transform.rotation = Quaternion.Euler(50, -30, 0);
        sunLight.intensity = 1.5f;
        RenderSettings.ambientIntensity = 1.2f;

        // ========== 1. ENVIRONMENT ==========
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(50, 1, 50);
        SetMaterialColor(ground, new Color(0.4f, 0.6f, 0.3f));

        Color wallColor = new Color(0.85f, 0.85f, 0.85f);
        CreateWall("Wall_North", new Vector3(0, 2.5f, 25), new Vector3(50, 5, 0.5f), wallColor);
        CreateWall("Wall_South", new Vector3(0, 2.5f, -25), new Vector3(50, 5, 0.5f), wallColor);
        CreateWall("Wall_East", new Vector3(25, 2.5f, 0), new Vector3(0.5f, 5, 50), wallColor);
        CreateWall("Wall_West", new Vector3(-25, 2.5f, 0), new Vector3(0.5f, 5, 50), wallColor);
        CreateWall("CorridorLeft", new Vector3(-10, 2f, 0), new Vector3(0.5f, 4, 20), wallColor);
        CreateWall("CorridorRight", new Vector3(10, 2f, 0), new Vector3(0.5f, 4, 20), wallColor);
        CreateWall("CenterDivider", new Vector3(0, 2f, 0), new Vector3(2, 4, 0.5f), wallColor);

        // ========== 2. STATIONS (kecil) ==========
        Vector3[] stationPositions = new Vector3[]
        {
            new Vector3(-18, 0.4f, -15),
            new Vector3(-18, 0.4f, 15),
            new Vector3(18, 0.4f, 15),
            new Vector3(18, 0.4f, -15)
        };
        Color[] stationColors = { Color.blue, Color.red, Color.green, Color.yellow };
        string[] stationNames = { "IDCard_Station", "Alarm_Station", "Door_Station", "Exit_Station" };
        string[] stationLabels = { "ID CARD", "ALARM", "EMERGENCY DOOR", "EXIT" };

        for (int i = 0; i < 4; i++)
        {
            CreateStation(stationNames[i], stationPositions[i], stationColors[i], stationLabels[i]);
        }

        // Door
        GameObject doorObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        doorObj.name = "DoorObject";
        doorObj.transform.position = new Vector3(18, 0.5f, 12);
        doorObj.transform.localScale = new Vector3(1.0f, 1.8f, 0.2f);
        SetMaterialColor(doorObj, new Color(0.6f, 0.4f, 0.2f));
        doorObj.AddComponent<DoorController>();

        // ========== 3. UI OVERLAY (kecil) ==========
        GameObject canvasGO = new GameObject("MainUI");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Left panel
        GameObject leftPanel = CreateUIPanel(canvasGO.transform, "LeftPanel", new Vector2(12, -12), new Vector2(240, 360), new Vector2(0, 1));
        leftPanel.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
        VerticalLayoutGroup vlg = leftPanel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.spacing = 10;
        vlg.childAlignment = TextAnchor.UpperCenter;

        // Title
        GameObject title = new GameObject("Title");
        title.transform.SetParent(leftPanel.transform, false);
        TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
        titleText.text = "EVACUATION CHECKLIST";
        titleText.fontSize = 18;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;
        title.GetComponent<RectTransform>().sizeDelta = new Vector2(220, 24);

        // Minimap
        GameObject minimapContainer = new GameObject("Minimap");
        minimapContainer.transform.SetParent(leftPanel.transform, false);
        HorizontalLayoutGroup hlgMinimap = minimapContainer.AddComponent<HorizontalLayoutGroup>();
        hlgMinimap.childAlignment = TextAnchor.MiddleCenter;
        hlgMinimap.spacing = 12;
        minimapContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(220, 30);

        Image[] lights = new Image[4];
        string[] lightNames = { "Light_ID", "Light_Alarm", "Light_Door", "Light_Exit" };
        for (int i = 0; i < 4; i++)
        {
            GameObject lightObj = new GameObject(lightNames[i]);
            lightObj.transform.SetParent(minimapContainer.transform, false);
            Image img = lightObj.AddComponent<Image>();
            img.color = Color.gray;
            img.rectTransform.sizeDelta = new Vector2(24, 24);
            lights[i] = img;
        }

        // Checklist
        GameObject checklistContainer = new GameObject("ChecklistItems");
        checklistContainer.transform.SetParent(leftPanel.transform, false);
        VerticalLayoutGroup vlgCheck = checklistContainer.AddComponent<VerticalLayoutGroup>();
        vlgCheck.childAlignment = TextAnchor.UpperLeft;
        vlgCheck.spacing = 6;
        checklistContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(220, 150);

        TextMeshProUGUI[] checklistTexts = new TextMeshProUGUI[4];
        string[] checklistNames = { "Txt_ID", "Txt_Alarm", "Txt_Door", "Txt_Exit" };
        string[] checklistStrings = { "☐ 1. Take ID Card", "☐ 2. Activate Alarm", "☐ 3. Open Emergency Door", "☐ 4. Go To Exit" };
        for (int i = 0; i < 4; i++)
        {
            GameObject txtObj = new GameObject(checklistNames[i]);
            txtObj.transform.SetParent(checklistContainer.transform, false);
            TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
            tmp.text = checklistStrings[i];
            tmp.fontSize = 16;
            tmp.color = Color.white;
            tmp.rectTransform.pivot = new Vector2(0, 0.5f);
            tmp.rectTransform.anchorMin = new Vector2(0, 0.5f);
            tmp.rectTransform.anchorMax = new Vector2(1, 0.5f);
            checklistTexts[i] = tmp;
        }

        // Timer
        GameObject timerPanel = CreateUIPanel(canvasGO.transform, "TimerPanel", new Vector2(-12, -12), new Vector2(140, 60), new Vector2(1, 1));
        timerPanel.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
        GameObject timerTextObj = new GameObject("TimerText");
        timerTextObj.transform.SetParent(timerPanel.transform, false);
        TextMeshProUGUI timerText = timerTextObj.AddComponent<TextMeshProUGUI>();
        timerText.text = "02:00";
        timerText.fontSize = 32;
        timerText.alignment = TextAlignmentOptions.Center;
        timerText.rectTransform.anchorMin = Vector2.zero;
        timerText.rectTransform.anchorMax = Vector2.one;
        timerText.rectTransform.sizeDelta = Vector2.zero;

        // Status bar
        GameObject statusBar = CreateUIPanel(canvasGO.transform, "StatusBar", new Vector2(0, 30), new Vector2(600, 40), new Vector2(0.5f, 0));
        statusBar.GetComponent<Image>().color = new Color(0, 0, 0, 0.9f);
        GameObject statusTextObj = new GameObject("StatusText");
        statusTextObj.transform.SetParent(statusBar.transform, false);
        TextMeshProUGUI statusText = statusTextObj.AddComponent<TextMeshProUGUI>();
        statusText.text = "🔵 Go to BLUE station and click 'Take ID Card'!";
        statusText.fontSize = 20;
        statusText.alignment = TextAlignmentOptions.Center;
        statusText.rectTransform.anchorMin = Vector2.zero;
        statusText.rectTransform.anchorMax = Vector2.one;
        statusText.rectTransform.sizeDelta = Vector2.zero;

        // Warning flash & popup
        GameObject warningFlash = new GameObject("WarningFlash");
        warningFlash.transform.SetParent(canvasGO.transform, false);
        Image flashImg = warningFlash.AddComponent<Image>();
        flashImg.color = new Color(1, 0, 0, 0);
        flashImg.rectTransform.anchorMin = Vector2.zero;
        flashImg.rectTransform.anchorMax = Vector2.one;
        flashImg.rectTransform.sizeDelta = Vector2.zero;

        GameObject popupPanel = CreateUIPanel(canvasGO.transform, "WarningPopupPanel", Vector2.zero, new Vector2(350, 100), new Vector2(0.5f, 0.5f));
        popupPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.95f);
        CanvasGroup cg = popupPanel.AddComponent<CanvasGroup>();
        cg.alpha = 0;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        GameObject popupTextObj = new GameObject("PopupText");
        popupTextObj.transform.SetParent(popupPanel.transform, false);
        TextMeshProUGUI popupText = popupTextObj.AddComponent<TextMeshProUGUI>();
        popupText.fontSize = 20;
        popupText.alignment = TextAlignmentOptions.Center;
        popupText.rectTransform.anchorMin = Vector2.zero;
        popupText.rectTransform.anchorMax = Vector2.one;
        popupText.rectTransform.sizeDelta = Vector2.zero;

        // Crosshair
        GameObject crosshair = new GameObject("Crosshair");
        crosshair.transform.SetParent(canvasGO.transform, false);
        Image crosshairImg = crosshair.AddComponent<Image>();
        crosshairImg.color = Color.white;
        crosshairImg.rectTransform.sizeDelta = new Vector2(6, 6);
        crosshairImg.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        crosshairImg.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        crosshairImg.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        crosshairImg.rectTransform.anchoredPosition = Vector2.zero;

        // ========== 4. MANAGERS ==========
        GameObject managerObj = new GameObject("EvacuationManager");
        EvacuationManager evac = managerObj.AddComponent<EvacuationManager>();

        evac.txtObjectiveID = checklistTexts[0];
        evac.txtObjectiveAlarm = checklistTexts[1];
        evac.txtObjectiveDoor = checklistTexts[2];
        evac.txtObjectiveExit = checklistTexts[3];
        evac.txtTimer = timerText;
        evac.txtFeedback = statusText;
        evac.warningCanvasGroup = cg;
        evac.warningPopupText = popupText;
        evac.flashImage = flashImg;
        evac.mapIDCard = lights[0];
        evac.mapAlarm = lights[1];
        evac.mapDoor = lights[2];
        evac.mapExit = lights[3];
        evac.stationIDCard = GameObject.Find("IDCard_Station");
        evac.stationAlarm = GameObject.Find("Alarm_Station");
        evac.stationDoor = GameObject.Find("Door_Station");
        evac.stationExit = GameObject.Find("Exit_Station");

        GameObject timerManagerGO = new GameObject("TimerManager");
        TimerManager timerMgr = timerManagerGO.AddComponent<TimerManager>();
        timerMgr.timerText = timerText;
        timerMgr.maxTime = 120;
        timerMgr.OnTimerExpiredEvent += evac.OnTimerExpired;

        // ========== 5. WORLD SPACE BUTTONS ==========
        string[] buttonLabels = { "Take ID Card", "Activate Alarm", "Open Door", "Exit Building" };
        string[] methodNames = { "InteractIDCardStation", "InteractAlarmStation", "InteractEmergencyDoorStation", "InteractExitStation" };
        Vector3[] buttonPositions = new Vector3[]
        {
            new Vector3(-18, 1.0f, -13),
            new Vector3(-18, 1.0f, 13),
            new Vector3(18, 1.0f, 13),
            new Vector3(18, 1.0f, -13)
        };

        for (int i = 0; i < 4; i++)
        {
            CreateWorldSpaceButton(methodNames[i], buttonPositions[i], buttonLabels[i], evac);
        }

        // ========== 6. PLAYER ==========
        GameObject player = new GameObject("Player");
        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 1.8f;
        controller.center = new Vector3(0, 0.9f, 0);
        player.transform.position = new Vector3(0, 1, 0);

        GameObject camObj = new GameObject("MainCamera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.tag = "MainCamera";
        cam.fieldOfView = 75;
        camObj.transform.SetParent(player.transform);
        camObj.transform.localPosition = new Vector3(0, 0.6f, 0);
        camObj.AddComponent<AudioListener>();
        camObj.AddComponent<PhysicsRaycaster>();

        SimpleFirstPersonController fps = player.AddComponent<SimpleFirstPersonController>();
        fps.playerCamera = cam.transform;
        fps.characterController = controller;

        // Spacebar handler
        camObj.AddComponent<SpacebarClickHandler>();

        // ========== 7. EVENT SYSTEM ==========
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject evSys = new GameObject("EventSystem");
            evSys.AddComponent<EventSystem>();
            evSys.AddComponent<StandaloneInputModule>();
        }

        // ========== 8. SAVE SCENE ==========
        string scenePath = "Assets/Scenes/UTS_Dimas_EvacuationScene.unity";
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");
        EditorSceneManager.SaveScene(scene, scenePath);
        AssetDatabase.Refresh();

        Debug.Log("Scene built successfully!");
        EditorUtility.DisplayDialog("Success", "Scene created. Press Play and click the blue station's button.", "OK");
    }

    static void CreateWorldSpaceButton(string methodName, Vector3 position, string buttonText, EvacuationManager evac)
    {
        GameObject canvasObj = new GameObject("Btn_" + methodName + "_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasObj.AddComponent<GraphicRaycaster>();
        canvasObj.transform.position = position;

        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(240, 80);
        canvasRect.localScale = new Vector3(0.08f, 0.08f, 0.08f);

        Vector3 lookTarget = new Vector3(0, 1.0f, 0);
        Vector3 direction = (lookTarget - position).normalized;
        canvasObj.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

        GameObject buttonObj = new GameObject("Btn_" + methodName);
        buttonObj.transform.SetParent(canvasObj.transform, false);
        RectTransform btnRect = buttonObj.AddComponent<RectTransform>();
        btnRect.anchorMin = Vector2.zero;
        btnRect.anchorMax = Vector2.one;
        btnRect.sizeDelta = Vector2.zero;
        btnRect.anchoredPosition = Vector2.zero;

        Button btn = buttonObj.AddComponent<Button>();
        Image btnImage = buttonObj.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.6f, 0.9f);
        btnImage.raycastTarget = true;

        buttonObj.AddComponent<ButtonClickEffect>();
        buttonObj.AddComponent<ButtonPulseEffect>();

        Outline outline = buttonObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, -1);

        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.2f, 0.6f, 0.9f);
        colors.highlightedColor = new Color(0.4f, 0.8f, 1f);
        colors.pressedColor = new Color(0.1f, 0.4f, 0.7f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.1f;
        btn.colors = colors;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = buttonText + "\n<size=16><i>(Click)</i></size>";
        tmp.fontSize = 20;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        // Pasang listener
        if (evac != null)
        {
            if (methodName == "InteractIDCardStation")
                btn.onClick.AddListener(() => { Debug.Log(">>> ID Card button clicked!"); evac.InteractIDCardStation(); });
            else if (methodName == "InteractAlarmStation")
                btn.onClick.AddListener(() => { Debug.Log(">>> Alarm button clicked!"); evac.InteractAlarmStation(); });
            else if (methodName == "InteractEmergencyDoorStation")
                btn.onClick.AddListener(() => { Debug.Log(">>> Door button clicked!"); evac.InteractEmergencyDoorStation(); });
            else if (methodName == "InteractExitStation")
                btn.onClick.AddListener(() => { Debug.Log(">>> Exit button clicked!"); evac.InteractExitStation(); });
        }
    }

    // ========== HELPER ==========
    static void SetMaterialColor(GameObject go, Color color)
    {
        Renderer rend = go.GetComponent<Renderer>();
        if (rend != null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material mat = new Material(shader);
            mat.color = color;
            rend.material = mat;
        }
    }

    static void CreateWall(string name, Vector3 pos, Vector3 scale, Color color)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = pos;
        wall.transform.localScale = scale;
        SetMaterialColor(wall, color);
    }

    static void CreateStation(string name, Vector3 pos, Color color, string label)
    {
        GameObject station = GameObject.CreatePrimitive(PrimitiveType.Cube);
        station.name = name;
        station.transform.position = pos;
        station.transform.localScale = new Vector3(0.7f, 0.5f, 0.7f);
        SetMaterialColor(station, color);
        station.AddComponent<StationClickEffect>();

        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(station.transform);
        TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = label;
        tmp.fontSize = 10;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        textObj.transform.localPosition = new Vector3(0, 0.4f, 0);
        textObj.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
    }

    static GameObject CreateUIPanel(Transform parent, string name, Vector2 anchoredPos, Vector2 size, Vector2 anchor)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        Image img = panel.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.8f);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;
        return panel;
    }
}
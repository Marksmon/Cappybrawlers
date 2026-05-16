#if UNITY_EDITOR
using Capybrawlers.Battle;
using Capybrawlers.Creatures;
using Capybrawlers.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Layout (portrait 1080×1920):
//
//   ┌─────────────────────────────┐  y = 1.00
//   │ Turn N  Overlord vs Opp   │  HUD bar          y 0.96–1.00
//   │[Stg0..Stg9]               │  staging strip    y 0.90–0.96  (touches HUD, thinner)
//   │[P_Mid]          [E_Mid]   │  mid brawlers     y 0.70–0.88  (same size as all brawlers)
//   │[P_Front]      [E_Front]   │  front brawlers   y 0.50–0.68  (same size)
//   │[P_Back]         [E_Back]  │  back brawlers    y 0.30–0.48  (same size)
//   │[C0..C17]                  │  hand (18 cards)  y 0.07–0.27
//   │[Stam] [End Turn]   [Deck] │  controls         y 0.00–0.07
//   └─────────────────────────────┘  y = 0.00
//
//   Overlays (hidden by default):
//     BrawlerInfoPanel  y 0.08–0.85   — tap any brawler
//     CardDetailPopup   y 0.20–0.70   — tap equipment in info panel (on top)

public static class BattleSceneBuilder
{
    private const string ScenePath = "Assets/_Game/Scenes/Battle.unity";

    [MenuItem("Capybrawlers/Build Battle Scene")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── Camera ─────────────────────────────────────────────────────────────
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.clearFlags      = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.07f, 0.14f, 0.07f);
        cam.orthographic    = true;
        cam.depth           = -1;

        // ── EventSystem ────────────────────────────────────────────────────────
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();
        esGO.AddComponent<StandaloneInputModule>();

        // ── Canvas ─────────────────────────────────────────────────────────────
        var canvasGO = new GameObject("Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasGO.AddComponent<GraphicRaycaster>();
        var ct = canvasGO.transform;

        // ── HUD bar ────────────────────────────────────────────────────────────
        var hudPanel = MakePanel("HUDBar", ct, 0f, 0.96f, 1f, 1f);
        AddPanelBg(hudPanel, new Color(0f, 0f, 0f, 0.55f));
        var turnText  = MakeTMP(hudPanel.transform, "TurnText",  "Turn 1", 22);
        PositionRect(turnText.rectTransform,  0.02f, 0f, 0.22f, 1f);
        turnText.alignment = TextAlignmentOptions.MidlineLeft;
        var matchupText = MakeTMP(hudPanel.transform, "MatchupText", "Overlord vs Opponent", 20);
        PositionRect(matchupText.rectTransform, 0.22f, 0f, 0.78f, 1f);
        matchupText.alignment = TextAlignmentOptions.Center;
        matchupText.color     = new Color(1f, 0.90f, 0.65f);
        var timerText = MakeTMP(hudPanel.transform, "TimerText", "",       22);
        PositionRect(timerText.rectTransform, 0.78f, 0f, 0.98f, 1f);
        timerText.alignment = TextAlignmentOptions.MidlineRight;
        timerText.color     = new Color(1f, 0.85f, 0.2f);

        // ── Brawler views ──────────────────────────────────────────────────────
        // All panels: anchor width 0.22 (≈ 238px at 1080 ref) — no x-scale so text stays sharp.
        // Mid + Front pinned to outer edge; Back shifted toward center so the two face each other.
        // Formation (player left, opponent right, perfectly mirrored):
        //   P_Mid  (upper-left)   P_Front (center-left)  P_Back (lower-left)
        //   E_Mid  (upper-right)  E_Front (center-right) E_Back (lower-right)
        var plyViews = new BrawlerView[3];
        plyViews[0] = MakeBrawlerView("P_Front", ct, 0.26f, 0.49f, 0.48f, 0.67f);
        plyViews[1] = MakeBrawlerView("P_Mid",   ct, 0.02f, 0.67f, 0.24f, 0.85f);
        plyViews[2] = MakeBrawlerView("P_Back",  ct, 0.02f, 0.32f, 0.24f, 0.50f);

        var oppViews = new BrawlerView[3];
        oppViews[0] = MakeBrawlerView("E_Front", ct, 0.52f, 0.49f, 0.74f, 0.67f);
        oppViews[1] = MakeBrawlerView("E_Mid",   ct, 0.76f, 0.67f, 0.98f, 0.85f);
        oppViews[2] = MakeBrawlerView("E_Back",  ct, 0.76f, 0.31f, 0.98f, 0.49f);

        // ── Staging strip (10 slots, y 0.90–0.96, directly below HUD) ────────
        var stagedPanel = MakePanel("StagedPanel", ct, 0f, 0.90f, 1f, 0.96f);
        AddPanelBg(stagedPanel, new Color(0.14f, 0.09f, 0.04f, 0.90f));
        var stagedViews = MakeStagedCardViews(stagedPanel.transform, 10);

        // ── Hand panel (18 cards, y 0.07–0.27) ────────────────────────────────
        var handPanel = MakePanel("HandPanel", ct, 0f, 0.07f, 1f, 0.27f);
        var cardViews = MakeCardViews(handPanel.transform, 18);
        var handView  = handPanel.AddComponent<HandView>();
        SetArray(handView, "_cardViews",       cardViews);
        SetArray(handView, "_stagedCardViews", stagedViews);

        // ── Controls row (y 0–0.07): [STAM] | End Turn | [DECK] ─────────────────
        var ctrlBg = MakePanel("ControlsRowBg", ct, 0f, 0f, 1f, 0.07f);
        AddPanelBg(ctrlBg, new Color(0.06f, 0.04f, 0.02f));

        // Stamina — left side of controls row
        var staminaGO = MakePanel("StaminaView", ct, 0f, 0f, 0.16f, 0.07f);
        AddPanelBg(staminaGO, new Color(0.10f, 0.07f, 0.03f));
        var stamLbl = MakeTMP(staminaGO.transform, "StamLabel", "STAM", 8);
        PositionRect(stamLbl.rectTransform, 0.05f, 0.60f, 0.95f, 0.95f);
        stamLbl.color = new Color(0.55f, 0.55f, 0.55f);
        var staminaTxt = MakeTMP(staminaGO.transform, "StaminaNum", "3", 16);
        PositionRect(staminaTxt.rectTransform, 0.05f, 0.05f, 0.95f, 0.65f);
        staminaTxt.color = new Color(0.4f, 0.9f, 1f);
        var staminaView = staminaGO.AddComponent<StaminaView>();
        Set(staminaView, "_staminaText", staminaTxt);

        // End Turn — centered with space on both sides
        var endTurnGO  = MakePanel("EndTurnButton", ct, 0.20f, 0.008f, 0.80f, 0.062f);
        var endTurnImg = endTurnGO.AddComponent<Image>();
        endTurnImg.color = new Color(0.48f, 0.29f, 0.13f);
        var endTurnBtn  = endTurnGO.AddComponent<Button>();
        MakeTMP(endTurnGO.transform, "Label", "End Turn", 22);
        var confirmQBtn = endTurnGO.AddComponent<QueueConfirmButton>();
        Set(confirmQBtn, "_endTurnButton", endTurnBtn);

        // Deck count — right side of controls row
        var deckGO = MakePanel("DeckCount", ct, 0.84f, 0f, 1f, 0.07f);
        AddPanelBg(deckGO, new Color(0.10f, 0.07f, 0.03f));
        var deckLbl = MakeTMP(deckGO.transform, "DeckLabel", "DECK", 8);
        PositionRect(deckLbl.rectTransform, 0.05f, 0.60f, 0.95f, 0.95f);
        deckLbl.color = new Color(0.55f, 0.55f, 0.55f);
        var deckCountTxt = MakeTMP(deckGO.transform, "DeckCountNum", "18", 16);
        PositionRect(deckCountTxt.rectTransform, 0.05f, 0.05f, 0.95f, 0.65f);
        deckCountTxt.color = new Color(1f, 0.85f, 0.50f);
        Set(handView, "_deckCountText", deckCountTxt);

        // ── End screen (starts inactive) ───────────────────────────────────────
        var endGO   = MakePanel("EndScreen", ct, 0.1f, 0.2f, 0.9f, 0.8f);
        AddPanelBg(endGO, new Color(0.09f, 0.06f, 0.02f, 0.93f));
        var resultTxt = MakeTMP(endGO.transform, "ResultText", "Victory!", 54);
        var bpTxt     = MakeTMP(endGO.transform, "BPText",     "0 BP (+25)", 36);
        var rankTxt   = MakeTMP(endGO.transform, "RankText",   "Pup", 36);
        PositionRect(resultTxt.rectTransform, 0f, 0.65f, 1f, 0.85f);
        PositionRect(bpTxt.rectTransform,     0f, 0.45f, 1f, 0.65f);
        PositionRect(rankTxt.rectTransform,   0f, 0.25f, 1f, 0.45f);
        var returnBtnGO  = MakePanel("ReturnButton", endGO.transform, 0.2f, 0.05f, 0.8f, 0.22f);
        var returnBtnImg = returnBtnGO.AddComponent<Image>();
        returnBtnImg.color = new Color(0.8f, 0.2f, 0.2f);
        var returnBtn    = returnBtnGO.AddComponent<Button>();
        MakeTMP(returnBtnGO.transform, "Label", "Return to Menu", 24);
        var endCtrl = endGO.AddComponent<BattleEndController>();
        Set(endCtrl, "_resultText",   resultTxt);
        Set(endCtrl, "_bpText",       bpTxt);
        Set(endCtrl, "_rankText",     rankTxt);
        Set(endCtrl, "_returnButton", returnBtn);
        endGO.SetActive(false);

        // ── Brawler info panel (overlay, starts inactive) ──────────────────────
        var infoPanel = MakeBrawlerInfoPanel(ct, out var cardDetailPopup);

        // ── Non-canvas objects ─────────────────────────────────────────────────
        var battleMgrGO = new GameObject("BattleManager");
        var battleMgr   = battleMgrGO.AddComponent<BattleManager>();

        var sceneCtrlGO = new GameObject("BattleSceneController");
        var sceneCtrl   = sceneCtrlGO.AddComponent<BattleSceneController>();
        Set(sceneCtrl, "_battleManager", battleMgr);

        const string LibsDir = "Assets/_Game/ScriptableObjects/Libraries";
        var natLib   = AssetDatabase.LoadAssetAtPath<NatureLibrary>($"{LibsDir}/NatureLibrary.asset");
        var equipLib = AssetDatabase.LoadAssetAtPath<EquipmentLibrary>($"{LibsDir}/EquipmentLibrary.asset");
        if (natLib  != null) Set(sceneCtrl, "_natures",   natLib);
        if (equipLib != null) Set(sceneCtrl, "_equipment", equipLib);
        if (natLib == null || equipLib == null)
            Debug.LogWarning("[Capybrawlers] Library SOs not found — run Generate SO Assets to wire them.");

        // ── Battle intro screen (rendered on top) ──────────────────────────────
        var introCtrl = MakeBattleIntroPanel(ct);
        Set(sceneCtrl, "_introController", introCtrl);

        var uiCtrlGO = new GameObject("BattleUIController");
        var uiCtrl   = uiCtrlGO.AddComponent<BattleUIController>();
        SetArray(uiCtrl, "_playerBrawlerViews",   plyViews);
        SetArray(uiCtrl, "_opponentBrawlerViews",  oppViews);
        Set(uiCtrl, "_handView",          handView);
        Set(uiCtrl, "_staminaView",       staminaView);
        Set(uiCtrl, "_confirmButton",     confirmQBtn);
        Set(uiCtrl, "_endScreen",         endCtrl);
        Set(uiCtrl, "_turnText",          turnText);
        Set(uiCtrl, "_matchupText",       matchupText);
        Set(uiCtrl, "_timerText",         timerText);
        Set(uiCtrl, "_brawlerInfoPanel",  infoPanel);
        Set(sceneCtrl, "_ui", uiCtrl);

        var resAnimGO = new GameObject("ResolutionAnimator");
        var resAnim   = resAnimGO.AddComponent<ResolutionAnimator>();
        SetArray(resAnim, "_playerViews",   plyViews);
        SetArray(resAnim, "_opponentViews", oppViews);
        Set(resAnim, "_popupContainer", canvasGO.GetComponent<RectTransform>());

        // ── Save ───────────────────────────────────────────────────────────────
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddToBuildSettings(ScenePath);
        AssetDatabase.Refresh();
        Debug.Log("[Capybrawlers] Battle scene rebuilt → " + ScenePath);
        Debug.Log("[Capybrawlers] Run Generate SO Assets to re-wire libraries.");
    }

    // ── Brawler view ──────────────────────────────────────────────────────────

    private static BrawlerView MakeBrawlerView(string name, Transform parent,
        float x0, float y0, float x1, float y1)
    {
        var go = MakePanel(name, parent, x0, y0, x1, y1);
        AddPanelBg(go, new Color(0.23f, 0.16f, 0.07f, 0.80f));

        var sliderGO = MakePanel("HPBar", go.transform, 0.04f, 0.04f, 0.96f, 0.20f);
        var bg       = sliderGO.AddComponent<Image>();
        bg.color     = new Color(0.12f, 0.12f, 0.12f);
        var slider   = sliderGO.AddComponent<Slider>();
        var fillArea = MakePanel("Fill", sliderGO.transform, 0f, 0f, 1f, 1f);
        var fill     = fillArea.AddComponent<Image>();
        fill.color   = new Color(0.2f, 0.85f, 0.25f);
        slider.fillRect      = fill.rectTransform;
        slider.targetGraphic = fill;
        slider.value         = 1f;
        slider.interactable  = false;

        var hpTxt = MakeTMP(go.transform, "HPText", "100/100", 13);
        PositionRect(hpTxt.rectTransform, 0.04f, 0.20f, 0.96f, 0.38f);

        var natureTxt = MakeTMP(go.transform, "NatureText", "Rock", 12);
        PositionRect(natureTxt.rectTransform, 0.04f, 0.37f, 0.96f, 0.55f);
        natureTxt.color = new Color(1f, 0.86f, 0.50f);

        // Badge row — 6 small indicator slots at y 0.55–0.72
        const int badgeCount = 6;
        var badgeBgs  = new Image[badgeCount];
        var badgeTxts = new TextMeshProUGUI[badgeCount];
        float bw = 1f / badgeCount;
        for (int b = 0; b < badgeCount; b++)
        {
            var badgeGO  = MakePanel($"Badge_{b}", go.transform,
                bw * b + 0.004f, 0.55f, bw * (b + 1) - 0.004f, 0.72f);
            var badgeImg = badgeGO.AddComponent<Image>();
            badgeImg.color   = new Color(0.2f, 0.7f, 0.3f, 0.92f);
            badgeImg.enabled = false;
            var badgeTxt = MakeTMP(badgeGO.transform, "BadgeLbl", "", 9);
            badgeTxt.enabled = false;
            badgeBgs[b]  = badgeImg;
            badgeTxts[b] = badgeTxt;
        }

        var hlGO  = MakePanel("TargetHighlight", go.transform, 0f, 0f, 1f, 1f);
        var hlImg = hlGO.AddComponent<Image>();
        hlImg.color         = new Color(1f, 0.9f, 0f, 0.22f);
        hlImg.enabled       = false;
        hlImg.raycastTarget = false;

        var koGO  = MakePanel("KOOverlay", go.transform, 0f, 0f, 1f, 1f);
        var koImg = koGO.AddComponent<Image>();
        koImg.color         = new Color(0f, 0f, 0f, 0.65f);
        koImg.raycastTarget = false;
        koGO.SetActive(false);

        var bv = go.AddComponent<BrawlerView>();
        Set(bv, "_hpBar",             slider);
        Set(bv, "_hpText",            hpTxt);
        Set(bv, "_natureText",        natureTxt);
        Set(bv, "_targetHighlight",   hlImg);
        Set(bv, "_knockedOutOverlay", koGO);
        SetArray(bv, "_badgeBackgrounds", badgeBgs);
        SetArray(bv, "_badgeLabels",      badgeTxts);
        return bv;
    }

    // ── Brawler info panel + card detail popup ────────────────────────────────

    private static BrawlerInfoPanel MakeBrawlerInfoPanel(Transform ct, out CardDetailPopup cardDetail)
    {
        var infoGO = MakePanel("BrawlerInfoPanel", ct, 0f, 0.08f, 1f, 0.85f);
        AddPanelBg(infoGO, new Color(0.96f, 0.91f, 0.78f, 0.97f));

        var inkBrown = new Color(0.16f, 0.10f, 0.04f);

        var nameTxt  = MakeTMP(infoGO.transform, "NameText",  "Rock Capybrawler", 28);
        PositionRect(nameTxt.rectTransform,  0.03f, 0.93f, 0.80f, 1.00f);
        nameTxt.alignment = TextAlignmentOptions.MidlineLeft;
        nameTxt.color     = inkBrown;

        var statsTxt = MakeTMP(infoGO.transform, "StatsText", "HP 100/100  ATK 20  DEF 10  SPD 8", 20);
        PositionRect(statsTxt.rectTransform, 0.03f, 0.85f, 0.97f, 0.93f);
        statsTxt.alignment = TextAlignmentOptions.MidlineLeft;
        statsTxt.color     = inkBrown;

        var closeBtnGO  = MakePanel("CloseButton", infoGO.transform, 0.82f, 0.93f, 0.98f, 1.00f);
        var closeBtnImg = closeBtnGO.AddComponent<Image>();
        closeBtnImg.color = new Color(0.7f, 0.15f, 0.15f);
        var closeBtn    = closeBtnGO.AddComponent<Button>();
        MakeTMP(closeBtnGO.transform, "Label", "X", 22);

        float[] equipY0 = { 0.57f, 0.35f, 0.13f };
        float[] equipY1 = { 0.78f, 0.57f, 0.36f };
        var equipBtns   = new Button[3];
        var equipLabels = new TextMeshProUGUI[3];
        string[] slotNames = { "Helm", "Armor", "Weapon" };

        for (int i = 0; i < 3; i++)
        {
            var rowGO  = MakePanel($"EquipRow_{i}", infoGO.transform, 0.03f, equipY0[i], 0.97f, equipY1[i]);
            var rowImg = rowGO.AddComponent<Image>();
            rowImg.color = new Color(0.78f, 0.62f, 0.38f, 0.80f);
            var rowBtn  = rowGO.AddComponent<Button>();
            var lbl     = MakeTMP(rowGO.transform, "EquipLabel", $"[{slotNames[i]}] —", 20);
            PositionRect(lbl.rectTransform, 0.04f, 0.1f, 0.96f, 0.9f);
            lbl.alignment = TextAlignmentOptions.MidlineLeft;
            lbl.color     = inkBrown;
            equipBtns[i]   = rowBtn;
            equipLabels[i] = lbl;
        }

        var infoPanel = infoGO.AddComponent<BrawlerInfoPanel>();
        Set(infoPanel, "_nameText",    nameTxt);
        Set(infoPanel, "_statsText",   statsTxt);
        Set(infoPanel, "_closeButton", closeBtn);
        SetArray(infoPanel, "_equipButtons",    equipBtns);
        SetArray(infoPanel, "_equipNameLabels", equipLabels);

        cardDetail = MakeCardDetailPopup(ct);
        Set(infoPanel, "_cardDetail", cardDetail);

        infoGO.SetActive(false);
        return infoPanel;
    }

    private static CardDetailPopup MakeCardDetailPopup(Transform ct)
    {
        var go = MakePanel("CardDetailPopup", ct, 0.04f, 0.20f, 0.96f, 0.70f);
        AddPanelBg(go, new Color(0.09f, 0.06f, 0.02f, 0.97f));

        var nameTxt = MakeTMP(go.transform, "CardName", "Card Name", 26);
        PositionRect(nameTxt.rectTransform, 0.05f, 0.86f, 0.72f, 1.00f);
        nameTxt.alignment = TextAlignmentOptions.MidlineLeft;

        var costTxt = MakeTMP(go.transform, "CostText", "0 Stamina", 22);
        PositionRect(costTxt.rectTransform, 0.72f, 0.86f, 0.97f, 1.00f);
        costTxt.color     = new Color(0.4f, 0.9f, 1f);
        costTxt.alignment = TextAlignmentOptions.MidlineRight;

        var descTxt = MakeTMP(go.transform, "DescText", "Effect description", 18);
        PositionRect(descTxt.rectTransform, 0.05f, 0.22f, 0.95f, 0.84f);
        descTxt.textWrappingMode = TMPro.TextWrappingModes.Normal;
        descTxt.alignment          = TextAlignmentOptions.TopLeft;

        var closeBtnGO  = MakePanel("CloseButton", go.transform, 0.25f, 0.03f, 0.75f, 0.19f);
        var closeBtnImg = closeBtnGO.AddComponent<Image>();
        closeBtnImg.color = new Color(0.7f, 0.15f, 0.15f);
        var closeBtn    = closeBtnGO.AddComponent<Button>();
        MakeTMP(closeBtnGO.transform, "Label", "Close", 20);

        var popup = go.AddComponent<CardDetailPopup>();
        Set(popup, "_nameText",    nameTxt);
        Set(popup, "_costText",    costTxt);
        Set(popup, "_descText",    descTxt);
        Set(popup, "_closeButton", closeBtn);
        go.SetActive(false);
        return popup;
    }

    // ── Staged card views — cards auto-resize to fill available width ──────────

    private static CardView[] MakeStagedCardViews(Transform parent, int count)
    {
        var hlg = parent.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment         = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth  = true;
        hlg.childForceExpandHeight = true;
        hlg.spacing = 3f;
        hlg.padding = new RectOffset(4, 4, 4, 4);

        var views = new CardView[count];
        for (int i = 0; i < count; i++)
        {
            var go = new GameObject($"StagedView_{i}");
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();

            AddPanelBg(go, new Color(0.20f, 0.45f, 0.20f, 0.9f));

            var nameTxt = MakeTMP(go.transform, "NameText", "Card", 14);
            PositionRect(nameTxt.rectTransform, 0.04f, 0.58f, 0.78f, 0.94f);

            var costTxt = MakeTMP(go.transform, "CostText", "1", 14);
            PositionRect(costTxt.rectTransform, 0.78f, 0.58f, 0.97f, 0.94f);
            costTxt.color = new Color(0.4f, 0.9f, 1f);

            var descTxt = MakeTMP(go.transform, "DescText", "", 10);
            PositionRect(descTxt.rectTransform, 0.04f, 0.35f, 0.95f, 0.58f);
            descTxt.textWrappingMode = TMPro.TextWrappingModes.Normal;

            var removeBtnGO  = MakePanel("AssignButton", go.transform, 0.04f, 0.02f, 0.96f, 0.33f);
            var removeBtnImg = removeBtnGO.AddComponent<Image>();
            removeBtnImg.color = new Color(0.75f, 0.20f, 0.20f);
            var removeBtn    = removeBtnGO.AddComponent<Button>();
            MakeTMP(removeBtnGO.transform, "Label", "X", 13);

            var assignedGO  = MakePanel("AssignedIndicator", go.transform, 0f, 0f, 1f, 1f);
            var assignedImg = assignedGO.AddComponent<Image>();
            assignedImg.color   = new Color(0f, 1f, 0f, 0f);
            assignedImg.enabled = false;

            var cv = go.AddComponent<CardView>();
            Set(cv, "_nameText",          nameTxt);
            Set(cv, "_costText",          costTxt);
            Set(cv, "_descText",          descTxt);
            Set(cv, "_assignButton",      removeBtn);
            Set(cv, "_assignedIndicator", assignedImg);
            go.SetActive(false);
            views[i] = cv;
        }
        return views;
    }

    // ── Card views (hand) — cards auto-resize to fill available width ─────────

    private static CardView[] MakeCardViews(Transform parent, int count)
    {
        var hlg = parent.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment         = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth  = true;
        hlg.childForceExpandHeight = true;
        hlg.spacing = 3f;
        hlg.padding = new RectOffset(4, 4, 4, 4);

        var views = new CardView[count];
        for (int i = 0; i < count; i++)
        {
            var go = new GameObject($"CardView_{i}");
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();

            AddPanelBg(go, new Color(0.23f, 0.16f, 0.07f));

            var nameTxt = MakeTMP(go.transform, "NameText", "Card Name", 14);
            PositionRect(nameTxt.rectTransform, 0.04f, 0.78f, 0.96f, 0.97f);
            nameTxt.textWrappingMode = TMPro.TextWrappingModes.Normal;

            var costTxt = MakeTMP(go.transform, "CostText", "1", 15);
            PositionRect(costTxt.rectTransform, 0.70f, 0.60f, 0.97f, 0.78f);
            costTxt.color = new Color(0.4f, 0.9f, 1f);

            var descTxt = MakeTMP(go.transform, "DescText", "Effect", 11);
            PositionRect(descTxt.rectTransform, 0.04f, 0.26f, 0.96f, 0.60f);
            descTxt.textWrappingMode = TMPro.TextWrappingModes.Normal;

            var assignBtnGO  = MakePanel("AssignButton", go.transform, 0.04f, 0.03f, 0.96f, 0.24f);
            var assignBtnImg = assignBtnGO.AddComponent<Image>();
            assignBtnImg.color = new Color(0.48f, 0.29f, 0.13f);
            var assignBtn    = assignBtnGO.AddComponent<Button>();
            MakeTMP(assignBtnGO.transform, "Label", "Select", 13);

            var assignedGO  = MakePanel("AssignedIndicator", go.transform, 0f, 0f, 1f, 1f);
            var assignedImg = assignedGO.AddComponent<Image>();
            assignedImg.color   = new Color(0f, 1f, 0f, 0.18f);
            assignedImg.enabled = false;

            go.AddComponent<CanvasGroup>();
            go.SetActive(false);

            var cv = go.AddComponent<CardView>();
            Set(cv, "_nameText",          nameTxt);
            Set(cv, "_costText",          costTxt);
            Set(cv, "_descText",          descTxt);
            Set(cv, "_assignButton",      assignBtn);
            Set(cv, "_assignedIndicator", assignedImg);
            views[i] = cv;
        }
        return views;
    }

    // ── Battle intro screen ───────────────────────────────────────────────────

    private static BattleIntroController MakeBattleIntroPanel(Transform ct)
    {
        // Full-screen overlay — rendered on top of everything
        var introGO = MakePanel("BattleIntroScreen", ct, 0f, 0f, 1f, 1f);
        AddPanelBg(introGO, new Color(0.04f, 0.06f, 0.04f, 0.97f));

        // ── Opponent band (top 47%) ────────────────────────────────────────
        var oppBand = MakePanel("OpponentBand", introGO.transform, 0f, 0.52f, 1f, 1.0f);
        AddPanelBg(oppBand, new Color(0.18f, 0.04f, 0.04f, 0.70f));

        var oppNameTxt = MakeTMP(oppBand.transform, "OpponentName", "OPPONENT", 24);
        PositionRect(oppNameTxt.rectTransform, 0.03f, 0.86f, 0.97f, 1.00f);
        oppNameTxt.alignment = TextAlignmentOptions.MidlineLeft;
        oppNameTxt.color     = new Color(0.95f, 0.75f, 0.45f);

        var oppSlices = new BrawlerIntroSlice[3];
        for (int i = 0; i < 3; i++)
        {
            float x0 = i / 3f + 0.004f;
            float x1 = (i + 1) / 3f - 0.004f;
            oppSlices[i] = MakeIntroSlice($"OppSlice_{i}", oppBand.transform, x0, 0.02f, x1, 0.84f);
        }

        // ── VS badge ──────────────────────────────────────────────────────
        var vsBadgeGO  = MakePanel("VSBadge", introGO.transform, 0.35f, 0.46f, 0.65f, 0.55f);
        var vsBadgeImg = vsBadgeGO.AddComponent<Image>();
        vsBadgeImg.color = new Color(0.55f, 0.38f, 0.08f);
        var vsTxt  = MakeTMP(vsBadgeGO.transform, "VSText", "VS", 46);
        StretchRect(vsTxt.rectTransform);
        vsTxt.fontStyle = FontStyles.Bold;
        vsTxt.color     = new Color(1f, 0.92f, 0.55f);

        // ── Divider lines flanking VS ──────────────────────────────────────
        var leftLine  = MakePanel("DivLeft",  introGO.transform, 0f,    0.497f, 0.34f, 0.503f);
        var rightLine = MakePanel("DivRight", introGO.transform, 0.66f, 0.497f, 1.0f,  0.503f);
        AddPanelBg(leftLine,  new Color(0.78f, 0.56f, 0.16f, 0.8f));
        AddPanelBg(rightLine, new Color(0.78f, 0.56f, 0.16f, 0.8f));

        // ── Player band (bottom 47%) ───────────────────────────────────────
        var plyBand = MakePanel("PlayerBand", introGO.transform, 0f, 0f, 1f, 0.47f);
        AddPanelBg(plyBand, new Color(0.04f, 0.18f, 0.04f, 0.70f));

        var plyNameTxt = MakeTMP(plyBand.transform, "PlayerName", "YOU", 24);
        PositionRect(plyNameTxt.rectTransform, 0.03f, 0.0f, 0.97f, 0.14f);
        plyNameTxt.alignment = TextAlignmentOptions.MidlineLeft;
        plyNameTxt.color     = new Color(0.95f, 0.75f, 0.45f);

        var plySlices = new BrawlerIntroSlice[3];
        for (int i = 0; i < 3; i++)
        {
            float x0 = i / 3f + 0.004f;
            float x1 = (i + 1) / 3f - 0.004f;
            plySlices[i] = MakeIntroSlice($"PlySlice_{i}", plyBand.transform, x0, 0.16f, x1, 0.98f);
        }

        // ── Countdown ──────────────────────────────────────────────────────
        var countdownTxt = MakeTMP(introGO.transform, "Countdown", "4", 36);
        PositionRect(countdownTxt.rectTransform, 0.42f, 0.003f, 0.58f, 0.06f);
        countdownTxt.color     = new Color(0.95f, 0.78f, 0.30f);
        countdownTxt.fontStyle = FontStyles.Bold;

        // ── Wire controller ────────────────────────────────────────────────
        var ctrl = introGO.AddComponent<BattleIntroController>();
        Set(ctrl, "_playerNameText",   plyNameTxt);
        Set(ctrl, "_opponentNameText", oppNameTxt);
        Set(ctrl, "_countdownText",    countdownTxt);
        SetArray(ctrl, "_playerSlices",   plySlices);
        SetArray(ctrl, "_opponentSlices", oppSlices);

        introGO.SetActive(false);
        return ctrl;
    }

    private static BrawlerIntroSlice MakeIntroSlice(string name, Transform parent,
        float x0, float y0, float x1, float y1)
    {
        var go = MakePanel(name, parent, x0, y0, x1, y1);
        var bg = go.AddComponent<Image>();
        bg.color = new Color(0.3f, 0.3f, 0.3f, 0.85f); // overwritten by Bind()

        // Thin darker border overlay
        var borderGO  = MakePanel("Border", go.transform, 0f, 0f, 1f, 1f);
        var borderImg = borderGO.AddComponent<Image>();
        borderImg.color         = new Color(0f, 0f, 0f, 0.35f);
        borderImg.raycastTarget = false;

        // Creature sprite — fills most of the slice
        var creGO  = MakePanel("Creature", go.transform, 0.05f, 0.22f, 0.95f, 0.88f);
        var creImg = creGO.AddComponent<Image>();
        creImg.preserveAspect = true;
        creImg.color          = Color.white;
        creImg.raycastTarget  = false;

        // Nature name — rotated to read bottom-to-top along the slice
        var natGO = new GameObject("NatureLabel");
        natGO.transform.SetParent(go.transform, false);
        var natRT = natGO.AddComponent<RectTransform>();
        PositionRect(natRT, 0f, 0.1f, 1f, 0.9f);
        natGO.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
        var natTxt = natGO.AddComponent<TextMeshProUGUI>();
        natTxt.text           = "NATURE";
        natTxt.fontSize       = 20;
        natTxt.fontStyle      = FontStyles.Bold;
        natTxt.alignment      = TextAlignmentOptions.Center;
        natTxt.color          = new Color(1f, 1f, 1f, 0.25f);
        natTxt.raycastTarget  = false;

        // Stats text — bottom strip
        var statsTxt = MakeTMP(go.transform, "Stats", "HP 80", 12);
        PositionRect(statsTxt.rectTransform, 0.04f, 0.01f, 0.96f, 0.20f);
        statsTxt.color       = new Color(1f, 1f, 1f, 0.90f);
        statsTxt.alignment   = TextAlignmentOptions.Center;
        statsTxt.enableWordWrapping = false;

        var slice = go.AddComponent<BrawlerIntroSlice>();
        Set(slice, "_bg",            bg);
        Set(slice, "_creatureImage", creImg);
        Set(slice, "_natureText",    natTxt);
        Set(slice, "_statsText",     statsTxt);
        return slice;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static GameObject MakePanel(string name, Transform parent,
        float x0, float y0, float x1, float y1)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        PositionRect(go.AddComponent<RectTransform>(), x0, y0, x1, y1);
        return go;
    }

    private static void AddPanelBg(GameObject go, Color color)
    {
        var img = go.AddComponent<Image>();
        img.color = color;
    }

    private static TextMeshProUGUI MakeTMP(Transform parent, string name, string text, int size)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent, false);
        StretchRect(go.AddComponent<RectTransform>());
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
        return tmp;
    }

    private static void PositionRect(RectTransform rt, float x0, float y0, float x1, float y1)
    {
        rt.anchorMin = new Vector2(x0, y0);
        rt.anchorMax = new Vector2(x1, y1);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void StretchRect(RectTransform rt) => PositionRect(rt, 0, 0, 1, 1);

    private static void Set(Component comp, string field, Object value)
    {
        var so   = new SerializedObject(comp);
        var prop = so.FindProperty(field);
        if (prop != null) { prop.objectReferenceValue = value; so.ApplyModifiedProperties(); }
        else Debug.LogWarning($"Field '{field}' not found on {comp.GetType().Name}");
    }

    private static void SetArray<T>(Component comp, string field, T[] values) where T : Object
    {
        var so   = new SerializedObject(comp);
        var prop = so.FindProperty(field);
        if (prop == null) { Debug.LogWarning($"Field '{field}' not found on {comp.GetType().Name}"); return; }
        prop.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        so.ApplyModifiedProperties();
    }

    private static void AddToBuildSettings(string path)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var s in scenes)
            if (s.path == path) return;
        scenes.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

}
#endif

using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using EcoSort.UI;

namespace EcoSort.Editor
{
    [InitializeOnLoad]
    public static class EcoSortUIBuilder
    {
        private const string PrefabDir = "Assets/Prefabs";
        private const string ScenesDir = "Assets/Scenes/ScenesOfTheProject";
        private const string SpritesDir = "Assets/UI/Sprites";
        private const string FontPath = "Assets/VRTemplateAssets/Fonts/Inter/Inter-Regular SDF.asset";

        private static TMP_FontAsset _interFont;

        static EcoSortUIBuilder()
        {
            EditorApplication.delayCall += () =>
            {
                // Only run automatically once if the prefabs don't exist yet
                if (!File.Exists(Path.Combine(PrefabDir, "LevelCompletedCanvas.prefab")))
                {
                    Debug.Log("[EcoSortUIBuilder] Initial build of EcoSort UI prefabs and scenes...");
                    BuildAll();
                }
            };
        }

        [MenuItem("EcoSort/Build All UI (MainMenu & LevelCompleted)")]
        public static void BuildAll()
        {
            EnsureDirectories();
            LoadFont();

            BuildMainMenuSceneAndPrefab();
            BuildLevelCompletedPrefabAndScene();

            RegisterScenesInBuild();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EcoSortUIBuilder] Successfully built all EcoSort UI prefabs and scenes!");
        }

        private static void EnsureDirectories()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");

            if (!AssetDatabase.IsValidFolder("Assets/Scenes/ScenesOfTheProject"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                    AssetDatabase.CreateFolder("Assets", "Scenes");
                AssetDatabase.CreateFolder("Assets/Scenes", "ScenesOfTheProject");
            }
        }

        private static void LoadFont()
        {
            _interFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if (_interFont == null)
            {
                _interFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            }
        }

        private static Sprite LoadSprite(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>($"{SpritesDir}/{name}.png");
        }

        #region Main Menu Builder

        [MenuItem("EcoSort/Build Main Menu Scene & Prefab")]
        public static void BuildMainMenuSceneAndPrefab()
        {
            LoadFont();
            EnsureDirectories();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Lighting & Camera Rig
            SetupEnvironmentAndVRRig(scene);

            // Create Canvas
            GameObject canvasGO = CreateWorldSpaceCanvas("MainMenuCanvas", new Vector2(1280, 720));
            canvasGO.transform.position = new Vector3(0f, 1.4f, 2.5f);

            // Main Background Panel
            GameObject bgGO = CreatePanel(canvasGO.transform, "BackgroundPanel", new Vector2(1240, 680), new Color(0.93f, 0.96f, 0.94f, 1f)); // #EDF5EF
            Image bgImg = bgGO.GetComponent<Image>();
            bgImg.sprite = LoadSprite("CardBackgroundWhite");
            bgImg.type = Image.Type.Sliced;

            // MainMenuUI Component
            MainMenuUI menuUI = canvasGO.AddComponent<MainMenuUI>();

            // --- Top Bar ---
            // Subtitle: "Aventura de reciclaje"
            GameObject subheaderGO = CreateTextMesh(bgGO.transform, "TopSubtitle", "Aventura de reciclaje", 28, new Color(0.12f, 0.42f, 0.25f, 1f), TextAlignmentOptions.Left);
            RectTransform subRect = subheaderGO.GetComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0f, 1f);
            subRect.anchorMax = new Vector2(0f, 1f);
            subRect.pivot = new Vector2(0f, 1f);
            subRect.anchoredPosition = new Vector2(60f, -50f);
            subRect.sizeDelta = new Vector2(400f, 40f);
            LocalizedText topSubLoc = subheaderGO.AddComponent<LocalizedText>();
            topSubLoc.SetTexts("Aventura de reciclaje", "Recycling Adventure");

            // Player Badge (Top Right)
            GameObject badgeGO = CreateImage(bgGO.transform, "PlayerBadge", new Vector2(280f, 52f), LoadSprite("PillBadgeWhite"), Color.white);
            RectTransform badgeRect = badgeGO.GetComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(1f, 1f);
            badgeRect.anchorMax = new Vector2(1f, 1f);
            badgeRect.pivot = new Vector2(1f, 1f);
            badgeRect.anchoredPosition = new Vector2(-60f, -44f);
            Image badgeImg = badgeGO.GetComponent<Image>();
            badgeImg.type = Image.Type.Sliced;

            GameObject badgeTextGO = CreateTextMesh(badgeGO.transform, "BadgeText", "Jugando como: Colibrí", 22, new Color(0.08f, 0.23f, 0.16f, 1f), TextAlignmentOptions.Center);
            RectTransform badgeTextRect = badgeTextGO.GetComponent<RectTransform>();
            badgeTextRect.anchorMin = Vector2.zero;
            badgeTextRect.anchorMax = Vector2.one;
            badgeTextRect.sizeDelta = Vector2.zero;
            LocalizedText badgeLoc = badgeTextGO.AddComponent<LocalizedText>();
            badgeLoc.SetTexts("Jugando como: Colibrí", "Playing as: Colibrí");

            // --- Main Left Column ---
            // Big Title "EcoSort VR"
            GameObject titleGO = CreateTextMesh(bgGO.transform, "MainTitle", "EcoSort VR", 76, new Color(0.07f, 0.23f, 0.16f, 1f), TextAlignmentOptions.Left);
            RectTransform titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(0f, 1f);
            titleRect.pivot = new Vector2(0f, 1f);
            titleRect.anchoredPosition = new Vector2(60f, -145f);
            titleRect.sizeDelta = new Vector2(500f, 90f);
            titleGO.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

            // Tagline: "¡Aprende a separar la basura jugando!"
            GameObject taglineGO = CreateTextMesh(bgGO.transform, "Tagline", "¡Aprende a separar la basura jugando!", 26, new Color(0.11f, 0.35f, 0.23f, 1f), TextAlignmentOptions.Left);
            RectTransform tagRect = taglineGO.GetComponent<RectTransform>();
            tagRect.anchorMin = new Vector2(0f, 1f);
            tagRect.anchorMax = new Vector2(0f, 1f);
            tagRect.pivot = new Vector2(0f, 1f);
            tagRect.anchoredPosition = new Vector2(60f, -245f);
            tagRect.sizeDelta = new Vector2(560f, 40f);
            taglineGO.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
            LocalizedText tagLoc = taglineGO.AddComponent<LocalizedText>();
            tagLoc.SetTexts("¡Aprende a separar la basura jugando!", "Learn to sort trash while playing!");

            // Buttons Container
            // Button 1: "Jugar"
            GameObject playBtnGO = CreateButton(bgGO.transform, "PlayButton", new Vector2(250f, 74f), LoadSprite("PillButtonGreen"), Color.white, "Jugar", 34, Color.white);
            RectTransform playRect = playBtnGO.GetComponent<RectTransform>();
            playRect.anchorMin = new Vector2(0f, 1f);
            playRect.anchorMax = new Vector2(0f, 1f);
            playRect.pivot = new Vector2(0f, 1f);
            playRect.anchoredPosition = new Vector2(60f, -325f);
            LocalizedText playLoc = playBtnGO.GetComponentInChildren<TextMeshProUGUI>().gameObject.AddComponent<LocalizedText>();
            playLoc.SetTexts("Jugar", "Play");
            Button playBtn = playBtnGO.GetComponent<Button>();

            // Button 2: "Mi progreso"
            GameObject progressBtnGO = CreateButton(bgGO.transform, "ProgressButton", new Vector2(230f, 62f), LoadSprite("PillButtonOutline"), Color.white, "Mi progreso", 24, new Color(0.14f, 0.45f, 0.28f, 1f));
            RectTransform progRect = progressBtnGO.GetComponent<RectTransform>();
            progRect.anchorMin = new Vector2(0f, 1f);
            progRect.anchorMax = new Vector2(0f, 1f);
            progRect.pivot = new Vector2(0f, 1f);
            progRect.anchoredPosition = new Vector2(60f, -425f);
            LocalizedText progLoc = progressBtnGO.GetComponentInChildren<TextMeshProUGUI>().gameObject.AddComponent<LocalizedText>();
            progLoc.SetTexts("Mi progreso", "My Progress");
            Button progBtn = progressBtnGO.GetComponent<Button>();

            // (Note: "Comodidad" button is completely removed as requested!)

            // Instruction text at bottom: "Apunta con el control y presiona el gatillo para elegir."
            GameObject instGO = CreateTextMesh(bgGO.transform, "Instructions", "Apunta con el control y presiona el gatillo para elegir.", 20, new Color(0.24f, 0.38f, 0.31f, 1f), TextAlignmentOptions.Left);
            RectTransform instRect = instGO.GetComponent<RectTransform>();
            instRect.anchorMin = new Vector2(0f, 0f);
            instRect.anchorMax = new Vector2(0f, 0f);
            instRect.pivot = new Vector2(0f, 0f);
            instRect.anchoredPosition = new Vector2(60f, 45f);
            instRect.sizeDelta = new Vector2(620f, 36f);
            LocalizedText instLoc = instGO.AddComponent<LocalizedText>();
            instLoc.SetTexts("Apunta con el control y presiona el gatillo para elegir.", "Aim with the controller and pull the trigger to select.");

            // --- Language Toggle Button (Bottom Right) ---
            // User request: "y pon un boton en la esquina derecha inferior para cambiar el idioma a Ingles y viceversa"
            GameObject langBtnGO = CreateButton(bgGO.transform, "LanguageButton", new Vector2(210f, 54f), LoadSprite("PillButtonOutline"), Color.white, "Idioma: English", 20, new Color(0.14f, 0.45f, 0.28f, 1f));
            RectTransform langRect = langBtnGO.GetComponent<RectTransform>();
            langRect.anchorMin = new Vector2(1f, 0f);
            langRect.anchorMax = new Vector2(1f, 0f);
            langRect.pivot = new Vector2(1f, 0f);
            langRect.anchoredPosition = new Vector2(-60f, 40f);
            Button langBtn = langBtnGO.GetComponent<Button>();
            TextMeshProUGUI langBtnText = langBtnGO.GetComponentInChildren<TextMeshProUGUI>();

            // Wire MainMenuUI
            SetPrivateField(menuUI, "playButton", playBtn);
            SetPrivateField(menuUI, "progressButton", progBtn);
            SetPrivateField(menuUI, "languageToggleButton", langBtn);
            SetPrivateField(menuUI, "languageButtonText", langBtnText);
            SetPrivateField(menuUI, "playSceneName", "TutorialLevel");

            // --- Right Side: 6 Recycling Bins Grid ---
            GameObject binsContainer = new GameObject("RecyclingBinsGrid");
            binsContainer.transform.SetParent(bgGO.transform, false);
            RectTransform binsRect = binsContainer.AddComponent<RectTransform>();
            binsRect.anchorMin = new Vector2(1f, 0.5f);
            binsRect.anchorMax = new Vector2(1f, 0.5f);
            binsRect.pivot = new Vector2(1f, 0.5f);
            binsRect.anchoredPosition = new Vector2(-60f, 10f);
            binsRect.sizeDelta = new Vector2(460f, 380f);

            // Bins setup: 2 rows of 3 columns
            // Row 1 (y = 80): Green, Grey, Red
            // Row 2 (y = -85): Blue, Yellow, Orange
            string[] binNamesRow1 = { "BinGreen", "BinGrey", "BinRed" };
            string[] binNamesRow2 = { "BinBlue", "BinYellow", "BinOrange" };
            float[] xOffsets = { -300f, -150f, 0f };

            for (int i = 0; i < 3; i++)
            {
                CreateBinImage(binsContainer.transform, binNamesRow1[i], LoadSprite(binNamesRow1[i]), new Vector2(xOffsets[i], 85f));
            }
            for (int i = 0; i < 3; i++)
            {
                CreateBinImage(binsContainer.transform, binNamesRow2[i], LoadSprite(binNamesRow2[i]), new Vector2(xOffsets[i], -85f));
            }

            // Save Prefab
            string prefabPath = $"{PrefabDir}/MainMenuCanvas.prefab";
            PrefabUtility.SaveAsPrefabAsset(canvasGO, prefabPath);

            // Save Scene
            string scenePath = $"{ScenesDir}/MainMenu.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[EcoSortUIBuilder] Main Menu Scene and Prefab saved: {scenePath}");
        }

        private static void CreateBinImage(Transform parent, string name, Sprite sprite, Vector2 localPos)
        {
            GameObject binGO = new GameObject(name);
            binGO.transform.SetParent(parent, false);
            RectTransform rt = binGO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 0.5f);
            rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = localPos;
            rt.sizeDelta = new Vector2(125f, 145f);

            Image img = binGO.AddComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
        }

        #endregion

        #region Level Completed Builder

        [MenuItem("EcoSort/Build Level Completed Prefab & Scene")]
        public static void BuildLevelCompletedPrefabAndScene()
        {
            LoadFont();
            EnsureDirectories();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SetupEnvironmentAndVRRig(scene);

            // Create Canvas
            GameObject canvasGO = CreateWorldSpaceCanvas("LevelCompletedCanvas", new Vector2(1280, 720));
            canvasGO.transform.position = new Vector3(0f, 1.4f, 2.5f);

            // Main Background Panel
            GameObject bgGO = CreatePanel(canvasGO.transform, "BackgroundPanel", new Vector2(1240, 680), new Color(0.93f, 0.96f, 0.94f, 1f));
            Image bgImg = bgGO.GetComponent<Image>();
            bgImg.sprite = LoadSprite("CardBackgroundWhite");
            bgImg.type = Image.Type.Sliced;

            LevelCompletedUI lvlUI = canvasGO.AddComponent<LevelCompletedUI>();

            // ================= LEFT SECTION =================
            GameObject leftSection = new GameObject("LeftSection");
            leftSection.transform.SetParent(bgGO.transform, false);
            RectTransform leftRect = leftSection.AddComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0f, 0f);
            leftRect.anchorMax = new Vector2(0.58f, 1f);
            leftRect.pivot = new Vector2(0f, 0.5f);
            leftRect.anchoredPosition = Vector2.zero;
            leftRect.sizeDelta = Vector2.zero;

            // 1. Header: "¡Nivel 3 completado!"
            GameObject headerGO = CreateTextMesh(leftSection.transform, "HeaderTitle", "¡Nivel 3 completado!", 48, new Color(0.07f, 0.23f, 0.16f, 1f), TextAlignmentOptions.Left);
            RectTransform headerRect = headerGO.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(0f, 1f);
            headerRect.pivot = new Vector2(0f, 1f);
            headerRect.anchoredPosition = new Vector2(60f, -48f);
            headerRect.sizeDelta = new Vector2(600f, 56f);
            headerGO.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

            // 2. Subheader: "¡Muy bien, Colibrí! Lo lograste con una pista."
            GameObject subheaderGO = CreateTextMesh(leftSection.transform, "Subheader", "¡Muy bien, Colibrí! Lo lograste con una pista.", 22, new Color(0.31f, 0.42f, 0.36f, 1f), TextAlignmentOptions.Left);
            RectTransform subRect = subheaderGO.GetComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0f, 1f);
            subRect.anchorMax = new Vector2(0f, 1f);
            subRect.pivot = new Vector2(0f, 1f);
            subRect.anchoredPosition = new Vector2(60f, -110f);
            subRect.sizeDelta = new Vector2(600f, 36f);

            // 3. Stars (2 filled, 1 empty)
            GameObject starsRowGO = new GameObject("StarsRow");
            starsRowGO.transform.SetParent(leftSection.transform, false);
            RectTransform starsRect = starsRowGO.AddComponent<RectTransform>();
            starsRect.anchorMin = new Vector2(0f, 1f);
            starsRect.anchorMax = new Vector2(0f, 1f);
            starsRect.pivot = new Vector2(0f, 1f);
            starsRect.anchoredPosition = new Vector2(60f, -165f);
            starsRect.sizeDelta = new Vector2(240f, 64f);

            HorizontalLayoutGroup starsLayout = starsRowGO.AddComponent<HorizontalLayoutGroup>();
            starsLayout.spacing = 16f;
            starsLayout.childAlignment = TextAnchor.MiddleLeft;
            starsLayout.childControlWidth = false;
            starsLayout.childControlHeight = false;

            Image[] starImages = new Image[3];
            Sprite starFilled = LoadSprite("StarFilled");
            Sprite starEmpty = LoadSprite("StarEmpty");

            for (int i = 0; i < 3; i++)
            {
                GameObject starGO = CreateImage(starsRowGO.transform, $"Star_{i + 1}", new Vector2(60f, 60f), i < 2 ? starFilled : starEmpty, Color.white);
                starImages[i] = starGO.GetComponent<Image>();
            }

            // 4. Stat Cards (3 rounded cards)
            GameObject statRowGO = new GameObject("StatCardsRow");
            statRowGO.transform.SetParent(leftSection.transform, false);
            RectTransform statRowRect = statRowGO.AddComponent<RectTransform>();
            statRowRect.anchorMin = new Vector2(0f, 1f);
            statRowRect.anchorMax = new Vector2(0f, 1f);
            statRowRect.pivot = new Vector2(0f, 1f);
            statRowRect.anchoredPosition = new Vector2(60f, -270f);
            statRowRect.sizeDelta = new Vector2(580f, 115f);

            HorizontalLayoutGroup statLayout = statRowGO.AddComponent<HorizontalLayoutGroup>();
            statLayout.spacing = 18f;
            statLayout.childAlignment = TextAnchor.MiddleLeft;
            statLayout.childControlWidth = false;
            statLayout.childControlHeight = false;

            Sprite statCardSprite = LoadSprite("StatCardWhite");
            TextMeshProUGUI attemptsVal = CreateStatCard(statRowGO.transform, "CardAttempts", "Intentos", "Attempts", "2", statCardSprite);
            TextMeshProUGUI hintsVal = CreateStatCard(statRowGO.transform, "CardHints", "Pistas usadas", "Hints used", "1", statCardSprite);
            TextMeshProUGUI firstTryVal = CreateStatCard(statRowGO.transform, "CardFirstTry", "Primer intento", "First attempt", "No", statCardSprite);

            // 5. Level Progression (Label + 9 circles)
            GameObject lvlProgGO = new GameObject("LevelProgression");
            lvlProgGO.transform.SetParent(leftSection.transform, false);
            RectTransform lvlProgRect = lvlProgGO.AddComponent<RectTransform>();
            lvlProgRect.anchorMin = new Vector2(0f, 1f);
            lvlProgRect.anchorMax = new Vector2(0f, 1f);
            lvlProgRect.pivot = new Vector2(0f, 1f);
            lvlProgRect.anchoredPosition = new Vector2(60f, -415f);
            lvlProgRect.sizeDelta = new Vector2(580f, 75f);

            GameObject lvlLabelGO = CreateTextMesh(lvlProgGO.transform, "LevelProgressLabel", "Nivel 3 de 9", 20, new Color(0.08f, 0.23f, 0.16f, 1f), TextAlignmentOptions.Left);
            RectTransform lvlLabelRect = lvlLabelGO.GetComponent<RectTransform>();
            lvlLabelRect.anchorMin = new Vector2(0f, 1f);
            lvlLabelRect.anchorMax = new Vector2(0f, 1f);
            lvlLabelRect.pivot = new Vector2(0f, 1f);
            lvlLabelRect.anchoredPosition = Vector2.zero;
            lvlLabelRect.sizeDelta = new Vector2(250f, 26f);
            lvlLabelGO.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

            GameObject dotsContainerGO = new GameObject("DotsContainer");
            dotsContainerGO.transform.SetParent(lvlProgGO.transform, false);
            RectTransform dotsRect = dotsContainerGO.AddComponent<RectTransform>();
            dotsRect.anchorMin = new Vector2(0f, 0f);
            dotsRect.anchorMax = new Vector2(1f, 0f);
            dotsRect.pivot = new Vector2(0f, 0f);
            dotsRect.anchoredPosition = Vector2.zero;
            dotsRect.sizeDelta = new Vector2(0f, 42f);

            HorizontalLayoutGroup dotsLayout = dotsContainerGO.AddComponent<HorizontalLayoutGroup>();
            dotsLayout.spacing = 10f;
            dotsLayout.childAlignment = TextAnchor.MiddleLeft;
            dotsLayout.childControlWidth = false;
            dotsLayout.childControlHeight = false;

            Sprite circleFilled = LoadSprite("CircleDot");
            Sprite circleOutline = LoadSprite("CircleOutline");

            for (int i = 1; i <= 9; i++)
            {
                CreateLevelDot(dotsContainerGO.transform, i, 3, circleFilled, circleOutline);
            }

            // 6. Action Buttons Row: "Siguiente nivel", "Repetir", "Menú"
            GameObject btnRowGO = new GameObject("ActionButtonsRow");
            btnRowGO.transform.SetParent(leftSection.transform, false);
            RectTransform btnRowRect = btnRowGO.AddComponent<RectTransform>();
            btnRowRect.anchorMin = new Vector2(0f, 0f);
            btnRowRect.anchorMax = new Vector2(0f, 0f);
            btnRowRect.pivot = new Vector2(0f, 0f);
            btnRowRect.anchoredPosition = new Vector2(60f, 45f);
            btnRowRect.sizeDelta = new Vector2(580f, 68f);

            HorizontalLayoutGroup btnLayout = btnRowGO.AddComponent<HorizontalLayoutGroup>();
            btnLayout.spacing = 16f;
            btnLayout.childAlignment = TextAnchor.MiddleLeft;
            btnLayout.childControlWidth = false;
            btnLayout.childControlHeight = false;

            // Button: "Siguiente nivel"
            GameObject nextBtnGO = CreateButton(btnRowGO.transform, "NextLevelButton", new Vector2(240f, 66f), LoadSprite("PillButtonGreen"), Color.white, "Siguiente nivel", 24, Color.white);
            LocalizedText nextLoc = nextBtnGO.GetComponentInChildren<TextMeshProUGUI>().gameObject.AddComponent<LocalizedText>();
            nextLoc.SetTexts("Siguiente nivel", "Next level");

            // Button: "Repetir"
            GameObject retryBtnGO = CreateButton(btnRowGO.transform, "RetryButton", new Vector2(140f, 66f), LoadSprite("PillButtonOutline"), Color.white, "Repetir", 22, new Color(0.14f, 0.45f, 0.28f, 1f));
            LocalizedText retryLoc = retryBtnGO.GetComponentInChildren<TextMeshProUGUI>().gameObject.AddComponent<LocalizedText>();
            retryLoc.SetTexts("Repetir", "Retry");

            // Button: "Menú"
            GameObject menuBtnGO = CreateButton(btnRowGO.transform, "MenuButton", new Vector2(130f, 66f), LoadSprite("PillButtonOutline"), Color.white, "Menú", 22, new Color(0.14f, 0.45f, 0.28f, 1f));
            LocalizedText menuLoc = menuBtnGO.GetComponentInChildren<TextMeshProUGUI>().gameObject.AddComponent<LocalizedText>();
            menuLoc.SetTexts("Menú", "Menu");

            // ================= RIGHT SECTION =================
            // Big White Card: "Tu dominio por bote"
            GameObject rightCardGO = CreateImage(bgGO.transform, "RightCard", new Vector2(490f, 600f), LoadSprite("CardBackgroundWhite"), Color.white);
            RectTransform rightCardRect = rightCardGO.GetComponent<RectTransform>();
            rightCardRect.anchorMin = new Vector2(1f, 0.5f);
            rightCardRect.anchorMax = new Vector2(1f, 0.5f);
            rightCardRect.pivot = new Vector2(1f, 0.5f);
            rightCardRect.anchoredPosition = new Vector2(-60f, 0f);
            rightCardGO.GetComponent<Image>().type = Image.Type.Sliced;

            // Card Header
            GameObject masteryHeaderGO = CreateTextMesh(rightCardGO.transform, "MasteryHeader", "Tu dominio por bote", 26, new Color(0.08f, 0.23f, 0.16f, 1f), TextAlignmentOptions.Left);
            RectTransform mHeaderRect = masteryHeaderGO.GetComponent<RectTransform>();
            mHeaderRect.anchorMin = new Vector2(0f, 1f);
            mHeaderRect.anchorMax = new Vector2(0f, 1f);
            mHeaderRect.pivot = new Vector2(0f, 1f);
            mHeaderRect.anchoredPosition = new Vector2(32f, -32f);
            mHeaderRect.sizeDelta = new Vector2(400f, 36f);
            masteryHeaderGO.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

            // Container for the 6 mastery rows
            GameObject masteryRowsContainer = new GameObject("MasteryRowsContainer");
            masteryRowsContainer.transform.SetParent(rightCardGO.transform, false);
            RectTransform rowsContainerRect = masteryRowsContainer.AddComponent<RectTransform>();
            rowsContainerRect.anchorMin = new Vector2(0f, 1f);
            rowsContainerRect.anchorMax = new Vector2(1f, 1f);
            rowsContainerRect.pivot = new Vector2(0f, 1f);
            rowsContainerRect.anchoredPosition = new Vector2(32f, -80f);
            rowsContainerRect.sizeDelta = new Vector2(-64f, 320f);

            VerticalLayoutGroup rowsLayout = masteryRowsContainer.AddComponent<VerticalLayoutGroup>();
            rowsLayout.spacing = 10f;
            rowsLayout.childAlignment = TextAnchor.UpperLeft;
            rowsLayout.childControlWidth = true;
            rowsLayout.childControlHeight = false;

            List<BinMasteryRowUI> rowUIList = new List<BinMasteryRowUI>();
            Sprite trackSprite = LoadSprite("ProgressBarTrack");
            Sprite fillSprite = LoadSprite("ProgressBarFill");

            // Setup 6 Rows: Green(100%), Grey(100%), Red(50%), Blue(Pronto), Yellow(Pronto), Orange(Pronto)
            var binDefs = new[]
            {
                ("Verde", "Green", new Color(0.18f, 0.64f, 0.36f), 100),
                ("Gris", "Grey", new Color(0.44f, 0.50f, 0.56f), 100),
                ("Rojo", "Red", new Color(0.78f, 0.18f, 0.18f), 50),
                ("Azul", "Blue", new Color(0.16f, 0.45f, 0.84f), -1),
                ("Amarillo", "Yellow", new Color(0.90f, 0.71f, 0.07f), -1),
                ("Naranja", "Orange", new Color(0.90f, 0.44f, 0.00f), -1)
            };

            foreach (var b in binDefs)
            {
                BinMasteryRowUI row = CreateBinMasteryRow(masteryRowsContainer.transform, b.Item1, b.Item2, b.Item3, b.Item4, circleFilled, trackSprite, fillSprite);
                rowUIList.Add(row);
            }

            // Next Goal Card (Warm yellow box at bottom of right card)
            GameObject nextGoalGO = CreateImage(rightCardGO.transform, "NextGoalCard", new Vector2(426f, 96f), LoadSprite("CardBackgroundYellow"), Color.white);
            RectTransform nextGoalRect = nextGoalGO.GetComponent<RectTransform>();
            nextGoalRect.anchorMin = new Vector2(0.5f, 0f);
            nextGoalRect.anchorMax = new Vector2(0.5f, 0f);
            nextGoalRect.pivot = new Vector2(0.5f, 0f);
            nextGoalRect.anchoredPosition = new Vector2(0f, 26f);
            nextGoalGO.GetComponent<Image>().type = Image.Type.Sliced;

            GameObject nextGoalTitleGO = CreateTextMesh(nextGoalGO.transform, "GoalTitle", "Siguiente meta", 17, new Color(0.55f, 0.41f, 0.09f, 1f), TextAlignmentOptions.Left);
            RectTransform ngtRect = nextGoalTitleGO.GetComponent<RectTransform>();
            ngtRect.anchorMin = new Vector2(0f, 1f);
            ngtRect.anchorMax = new Vector2(0f, 1f);
            ngtRect.pivot = new Vector2(0f, 1f);
            ngtRect.anchoredPosition = new Vector2(18f, -14f);
            ngtRect.sizeDelta = new Vector2(390f, 24f);
            nextGoalTitleGO.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

            GameObject nextGoalDescGO = CreateTextMesh(nextGoalGO.transform, "GoalDescription", "Practica el bote rojo", 22, new Color(0.10f, 0.23f, 0.16f, 1f), TextAlignmentOptions.Left);
            RectTransform ngdRect = nextGoalDescGO.GetComponent<RectTransform>();
            ngdRect.anchorMin = new Vector2(0f, 1f);
            ngdRect.anchorMax = new Vector2(0f, 1f);
            ngdRect.pivot = new Vector2(0f, 1f);
            ngdRect.anchoredPosition = new Vector2(18f, -44f);
            ngdRect.sizeDelta = new Vector2(390f, 32f);
            nextGoalDescGO.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

            // Wire up LevelCompletedUI
            SetPrivateField(lvlUI, "headerTitleText", headerGO.GetComponent<TextMeshProUGUI>());
            SetPrivateField(lvlUI, "subheaderText", subheaderGO.GetComponent<TextMeshProUGUI>());
            SetPrivateField(lvlUI, "starImages", starImages);
            SetPrivateField(lvlUI, "starFilledSprite", starFilled);
            SetPrivateField(lvlUI, "starEmptySprite", starEmpty);
            SetPrivateField(lvlUI, "attemptsValueText", attemptsVal);
            SetPrivateField(lvlUI, "hintsValueText", hintsVal);
            SetPrivateField(lvlUI, "firstAttemptValueText", firstTryVal);
            SetPrivateField(lvlUI, "levelProgressLabel", lvlLabelGO.GetComponent<TextMeshProUGUI>());
            SetPrivateField(lvlUI, "levelDotsContainer", dotsContainerGO.transform);
            SetPrivateField(lvlUI, "circleFilledSprite", circleFilled);
            SetPrivateField(lvlUI, "circleOutlineSprite", circleOutline);
            SetPrivateField(lvlUI, "binMasteryHeader", masteryHeaderGO.GetComponent<TextMeshProUGUI>());
            SetPrivateField(lvlUI, "binRows", rowUIList);
            SetPrivateField(lvlUI, "nextGoalTitle", nextGoalTitleGO.GetComponent<TextMeshProUGUI>());
            SetPrivateField(lvlUI, "nextGoalDescription", nextGoalDescGO.GetComponent<TextMeshProUGUI>());
            SetPrivateField(lvlUI, "nextLevelButton", nextBtnGO.GetComponent<Button>());
            SetPrivateField(lvlUI, "retryButton", retryBtnGO.GetComponent<Button>());
            SetPrivateField(lvlUI, "menuButton", menuBtnGO.GetComponent<Button>());
            SetPrivateField(lvlUI, "mainMenuSceneName", "MainMenu");

            // Save Prefab (As requested by user: "y la de levelComPleted que sea un canva para convertirlo a un prefab y reutilizarlo en varios niveles")
            string prefabPath = $"{PrefabDir}/LevelCompletedCanvas.prefab";
            PrefabUtility.SaveAsPrefabAsset(canvasGO, prefabPath);
            Debug.Log($"[EcoSortUIBuilder] LevelCompletedCanvas saved as PREFAB: {prefabPath}");

            // Save Demo Scene
            string scenePath = $"{ScenesDir}/LevelCompletedDemo.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[EcoSortUIBuilder] LevelCompletedDemo Scene saved: {scenePath}");
        }

        private static TextMeshProUGUI CreateStatCard(Transform parent, string name, string esLabel, string enLabel, string value, Sprite bgSprite)
        {
            GameObject cardGO = CreateImage(parent, name, new Vector2(178f, 110f), bgSprite, Color.white);
            cardGO.GetComponent<Image>().type = Image.Type.Sliced;

            GameObject labelGO = CreateTextMesh(cardGO.transform, "Label", esLabel, 18, new Color(0.39f, 0.50f, 0.44f, 1f), TextAlignmentOptions.TopLeft);
            RectTransform lRect = labelGO.GetComponent<RectTransform>();
            lRect.anchorMin = new Vector2(0f, 1f);
            lRect.anchorMax = new Vector2(1f, 1f);
            lRect.pivot = new Vector2(0f, 1f);
            lRect.anchoredPosition = new Vector2(18f, -14f);
            lRect.sizeDelta = new Vector2(-36f, 26f);
            LocalizedText loc = labelGO.AddComponent<LocalizedText>();
            loc.SetTexts(esLabel, enLabel);

            GameObject valGO = CreateTextMesh(cardGO.transform, "Value", value, 42, new Color(0.08f, 0.23f, 0.16f, 1f), TextAlignmentOptions.BottomLeft);
            RectTransform vRect = valGO.GetComponent<RectTransform>();
            vRect.anchorMin = new Vector2(0f, 0f);
            vRect.anchorMax = new Vector2(1f, 0f);
            vRect.pivot = new Vector2(0f, 0f);
            vRect.anchoredPosition = new Vector2(18f, 12f);
            vRect.sizeDelta = new Vector2(-36f, 50f);
            valGO.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

            return valGO.GetComponent<TextMeshProUGUI>();
        }

        private static void CreateLevelDot(Transform parent, int number, int currentCompleted, Sprite filled, Sprite outline)
        {
            GameObject dotGO = new GameObject($"Dot_{number}");
            dotGO.transform.SetParent(parent, false);
            RectTransform rt = dotGO.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(40f, 40f);

            Image img = dotGO.AddComponent<Image>();
            img.raycastTarget = false;

            GameObject txtGO = CreateTextMesh(dotGO.transform, "Text", number.ToString(), 18, Color.white, TextAlignmentOptions.Center);
            RectTransform txtRt = txtGO.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;
            TextMeshProUGUI txt = txtGO.GetComponent<TextMeshProUGUI>();
            txt.fontStyle = FontStyles.Bold;

            if (number <= currentCompleted)
            {
                img.sprite = filled;
                img.color = new Color(0.14f, 0.45f, 0.28f, 1f); // #247348
                txt.color = Color.white;
            }
            else if (number == currentCompleted + 1)
            {
                img.sprite = outline;
                img.color = Color.white;
                txt.color = new Color(0.14f, 0.45f, 0.28f, 1f);
            }
            else
            {
                img.sprite = filled;
                img.color = new Color(0.86f, 0.91f, 0.88f, 1f); // #DCE8E0
                txt.color = new Color(0.34f, 0.45f, 0.39f, 1f); // #567363
            }
        }

        private static BinMasteryRowUI CreateBinMasteryRow(Transform parent, string esName, string enName, Color color, int percent, Sprite circleSprite, Sprite trackSprite, Sprite fillSprite)
        {
            GameObject rowGO = new GameObject($"Row_{esName}");
            rowGO.transform.SetParent(parent, false);
            RectTransform rowRt = rowGO.AddComponent<RectTransform>();
            rowRt.sizeDelta = new Vector2(426f, 42f);

            // Dot
            GameObject dotGO = CreateImage(rowGO.transform, "ColorDot", new Vector2(22f, 22f), circleSprite, color);
            RectTransform dotRt = dotGO.GetComponent<RectTransform>();
            dotRt.anchorMin = new Vector2(0f, 0.5f);
            dotRt.anchorMax = new Vector2(0f, 0.5f);
            dotRt.pivot = new Vector2(0f, 0.5f);
            dotRt.anchoredPosition = new Vector2(0f, 0f);

            // Name
            GameObject nameGO = CreateTextMesh(rowGO.transform, "BinName", esName, 20, new Color(0.20f, 0.30f, 0.24f, 1f), TextAlignmentOptions.Left);
            RectTransform nameRt = nameGO.GetComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0f, 0.5f);
            nameRt.anchorMax = new Vector2(0f, 0.5f);
            nameRt.pivot = new Vector2(0f, 0.5f);
            nameRt.anchoredPosition = new Vector2(32f, 0f);
            nameRt.sizeDelta = new Vector2(95f, 30f);
            nameGO.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

            // Slider Bar
            GameObject sliderGO = new GameObject("SliderBar");
            sliderGO.transform.SetParent(rowGO.transform, false);
            RectTransform sliderRt = sliderGO.AddComponent<RectTransform>();
            sliderRt.anchorMin = new Vector2(0f, 0.5f);
            sliderRt.anchorMax = new Vector2(0f, 0.5f);
            sliderRt.pivot = new Vector2(0f, 0.5f);
            sliderRt.anchoredPosition = new Vector2(135f, 0f);
            sliderRt.sizeDelta = new Vector2(195f, 16f);

            Slider slider = sliderGO.AddComponent<Slider>();
            slider.interactable = false;
            slider.transition = Selectable.Transition.None;

            // Slider Background
            GameObject trackGO = CreateImage(sliderGO.transform, "Background", Vector2.zero, trackSprite, Color.white);
            RectTransform trackRt = trackGO.GetComponent<RectTransform>();
            trackRt.anchorMin = Vector2.zero;
            trackRt.anchorMax = Vector2.one;
            trackRt.sizeDelta = Vector2.zero;
            trackGO.GetComponent<Image>().type = Image.Type.Sliced;

            // Fill Area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderGO.transform, false);
            RectTransform fillAreaRt = fillArea.AddComponent<RectTransform>();
            fillAreaRt.anchorMin = Vector2.zero;
            fillAreaRt.anchorMax = Vector2.one;
            fillAreaRt.sizeDelta = Vector2.zero;

            GameObject fillGO = CreateImage(fillArea.transform, "Fill", Vector2.zero, fillSprite, color);
            RectTransform fillRt = fillGO.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.sizeDelta = Vector2.zero;
            Image fillImg = fillGO.GetComponent<Image>();
            fillImg.type = Image.Type.Sliced;

            slider.fillRect = fillRt;
            slider.minValue = 0f;
            slider.maxValue = 100f;
            slider.value = percent >= 0 ? percent : 0;

            // Percent / Status text
            string statStr = percent >= 0 ? $"{percent}%" : "Pronto";
            Color statColor = percent >= 0 ? new Color(0.08f, 0.23f, 0.16f, 1f) : new Color(0.46f, 0.56f, 0.51f, 1f);
            GameObject statGO = CreateTextMesh(rowGO.transform, "StatusText", statStr, 18, statColor, TextAlignmentOptions.Right);
            RectTransform statRt = statGO.GetComponent<RectTransform>();
            statRt.anchorMin = new Vector2(1f, 0.5f);
            statRt.anchorMax = new Vector2(1f, 0.5f);
            statRt.pivot = new Vector2(1f, 0.5f);
            statRt.anchoredPosition = new Vector2(0f, 0f);
            statRt.sizeDelta = new Vector2(90f, 30f);
            statGO.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

            BinMasteryRowUI rowUI = rowGO.AddComponent<BinMasteryRowUI>();
            SetPrivateField(rowUI, "dotImage", dotGO.GetComponent<Image>());
            SetPrivateField(rowUI, "binNameText", nameGO.GetComponent<TextMeshProUGUI>());
            SetPrivateField(rowUI, "progressBarSlider", slider);
            SetPrivateField(rowUI, "progressBarFill", fillImg);
            SetPrivateField(rowUI, "percentOrStatusText", statGO.GetComponent<TextMeshProUGUI>());
            SetPrivateField(rowUI, "spanishName", esName);
            SetPrivateField(rowUI, "englishName", enName);
            SetPrivateField(rowUI, "themeColor", color);
            SetPrivateField(rowUI, "masteryPercent", percent);

            return rowUI;
        }

        #endregion

        #region Helpers

        private static GameObject CreateWorldSpaceCanvas(string name, Vector2 size)
        {
            GameObject canvasGO = new GameObject(name);
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 3f;
            scaler.referencePixelsPerUnit = 100f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // Try adding TrackedDeviceGraphicRaycaster if available
            Type trackedRaycasterType = Type.GetType("UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster, Unity.XR.Interaction.Toolkit");
            if (trackedRaycasterType != null)
            {
                canvasGO.AddComponent(trackedRaycasterType);
            }

            RectTransform rt = canvasGO.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.localScale = new Vector3(0.002f, 0.002f, 0.002f);

            return canvasGO;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;

            Image img = go.AddComponent<Image>();
            img.color = color;
            return go;
        }

        private static GameObject CreateImage(Transform parent, string name, Vector2 size, Sprite sprite, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = size;

            Image img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            return go;
        }

        private static GameObject CreateTextMesh(Transform parent, string name, string text, float fontSize, Color color, TextAlignmentOptions align)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            if (_interFont != null) tmp.font = _interFont;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = align;
            tmp.raycastTarget = false;
            return go;
        }

        private static GameObject CreateButton(Transform parent, string name, Vector2 size, Sprite bgSprite, Color bgColor, string label, float fontSize, Color textColor)
        {
            GameObject btnGO = new GameObject(name);
            btnGO.transform.SetParent(parent, false);
            RectTransform rt = btnGO.AddComponent<RectTransform>();
            rt.sizeDelta = size;

            Image img = btnGO.AddComponent<Image>();
            img.sprite = bgSprite;
            img.color = bgColor;
            img.type = Image.Type.Sliced;

            Button btn = btnGO.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(0.92f, 0.95f, 0.93f, 1f);
            cb.pressedColor = new Color(0.80f, 0.85f, 0.82f, 1f);
            btn.colors = cb;

            GameObject textGO = CreateTextMesh(btnGO.transform, "Text", label, fontSize, textColor, TextAlignmentOptions.Center);
            RectTransform textRt = textGO.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;
            textGO.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

            return btnGO;
        }

        private static void SetupEnvironmentAndVRRig(Scene scene)
        {
            // Directional Light
            GameObject lightGO = new GameObject("Directional Light");
            Light light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.98f, 0.95f);
            light.intensity = 1.2f;
            lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // VR Origin / Camera Setup
            string xrPrefabPath = "Assets/VRTemplateAssets/Prefabs/Setup/Complete XR Origin Set Up Variant.prefab";
            GameObject xrPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(xrPrefabPath);
            if (xrPrefab != null)
            {
                GameObject xrInstance = (GameObject)PrefabUtility.InstantiatePrefab(xrPrefab);
                xrInstance.name = "Complete XR Origin";
                xrInstance.transform.position = Vector3.zero;
            }
            else
            {
                // Fallback standard Camera
                GameObject camGO = new GameObject("Main Camera");
                camGO.tag = "MainCamera";
                Camera cam = camGO.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.85f, 0.91f, 0.88f);
                camGO.transform.position = new Vector3(0f, 1.4f, 0f);
            }

            // EventSystem
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();

            Type xrInputModuleType = Type.GetType("UnityEngine.XR.Interaction.Toolkit.UI.XRUIInputModule, Unity.XR.Interaction.Toolkit");
            if (xrInputModuleType != null)
            {
                eventSystemGO.AddComponent(xrInputModuleType);
            }
            else
            {
                Type inputSystemUIModuleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                if (inputSystemUIModuleType != null)
                {
                    eventSystemGO.AddComponent(inputSystemUIModuleType);
                }
                else
                {
                    eventSystemGO.AddComponent<StandaloneInputModule>();
                }
            }
        }

        [MenuItem("EcoSort/Register All Scenes In Build Settings")]
        public static void RegisterScenesInBuild()
        {
            string[] allScenes = new string[]
            {
                $"{ScenesDir}/MainMenu.unity",
                $"{ScenesDir}/TutorialLevel.unity",
                $"{ScenesDir}/Level1.unity",
                $"{ScenesDir}/Level2.unity",
                $"{ScenesDir}/Level3.unity",
                $"{ScenesDir}/Level4.unity",
                $"{ScenesDir}/Level5.unity",
                $"{ScenesDir}/Level6.unity",
                $"{ScenesDir}/Level7.unity",
                $"{ScenesDir}/Level8.unity",
                $"{ScenesDir}/Level9.unity",
                $"{ScenesDir}/LevelCompletedDemo.unity",
            };

            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();

            foreach (string path in allScenes)
            {
                if (File.Exists(path))
                {
                    buildScenes.Add(new EditorBuildSettingsScene(path, true));
                }
            }

            EditorBuildSettings.scenes = buildScenes.ToArray();
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }

        #endregion
    }
}

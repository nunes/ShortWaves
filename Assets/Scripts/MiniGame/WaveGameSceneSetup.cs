using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ShortWaves.MiniGame
{
    /// <summary>
    /// Helper script to set up the Wave Mini-Game scene programmatically.
    /// Attach this to an empty GameObject and run in Editor to create the scene hierarchy.
    /// </summary>
    public class WaveGameSceneSetup : MonoBehaviour
    {
        [ContextMenu("Setup Scene")]
        public void SetupScene()
        {
            // Create Canvas
            GameObject canvasGO = new GameObject("Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            // Create Background (CRT Monitor)
            GameObject bgGO = new GameObject("CRT_Background");
            bgGO.transform.SetParent(canvasGO.transform, false);
            Image bgImage = bgGO.AddComponent<Image>();
            bgImage.color = new Color(0.05f, 0.1f, 0.05f); // Dark green tint
            RectTransform bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            // Create Wave Display Area
            GameObject waveAreaGO = new GameObject("WaveDisplayArea");
            waveAreaGO.transform.SetParent(canvasGO.transform, false);
            RectTransform waveRect = waveAreaGO.GetComponent<RectTransform>();
            waveRect.anchorMin = new Vector2(0.1f, 0.4f);
            waveRect.anchorMax = new Vector2(0.9f, 0.9f);
            waveRect.sizeDelta = Vector2.zero;

            // Create Template Wave GameObject (in world space, will be rendered by camera)
            GameObject templateWaveGO = new GameObject("TemplateWave");
            templateWaveGO.transform.position = new Vector3(0, 2, 0);
            LineRenderer templateLine = templateWaveGO.AddComponent<LineRenderer>();
            ConfigureLineRenderer(templateLine, Color.green, 0.1f);
            templateWaveGO.AddComponent<WaveRenderer>();

            // Create Player Wave GameObject
            GameObject playerWaveGO = new GameObject("PlayerWave");
            playerWaveGO.transform.position = new Vector3(0, -2, 0);
            LineRenderer playerLine = playerWaveGO.AddComponent<LineRenderer>();
            ConfigureLineRenderer(playerLine, Color.cyan, 0.1f);
            playerWaveGO.AddComponent<WaveRenderer>();

            // Create UI Panel for Controls
            GameObject controlPanelGO = new GameObject("ControlPanel");
            controlPanelGO.transform.SetParent(canvasGO.transform, false);
            Image panelImage = controlPanelGO.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.7f);
            RectTransform panelRect = controlPanelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.05f);
            panelRect.anchorMax = new Vector2(0.9f, 0.35f);
            panelRect.sizeDelta = Vector2.zero;

            // Create Frequency Slider
            CreateSlider(controlPanelGO.transform, "FrequencySlider", new Vector2(20, -30), "Frequency", 0.5f, 10f, 1f);

            // Create Amplitude Slider
            CreateSlider(controlPanelGO.transform, "AmplitudeSlider", new Vector2(20, -80), "Amplitude", 0.1f, 2f, 0.5f);

            // Create Waveform Selector
            GameObject selectorGO = new GameObject("WaveformSelector");
            selectorGO.transform.SetParent(controlPanelGO.transform, false);
            RectTransform selectorRect = selectorGO.GetComponent<RectTransform>();
            selectorRect.anchorMin = new Vector2(0, 0);
            selectorRect.anchorMax = new Vector2(1, 0);
            selectorRect.anchoredPosition = new Vector2(0, 50);
            selectorRect.sizeDelta = new Vector2(-40, 40);

            CreateButton(selectorGO.transform, "SineButton", new Vector2(-100, 0), "SINE");
            CreateButton(selectorGO.transform, "SquareButton", new Vector2(100, 0), "SQUARE");

            // Create Level Text
            CreateText(canvasGO.transform, "LevelText", new Vector2(0, 0.95f), new Vector2(0.5f, 1f), "Security Level: 1", 36);

            // Create Match Text
            CreateText(canvasGO.transform, "MatchText", new Vector2(0, 0.88f), new Vector2(0.5f, 1f), "Signal Match: 0%", 24);

            // Create Instruction Text
            CreateText(canvasGO.transform, "InstructionText", new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.5f), "Match the wave!", 20);

            // Create Game Manager
            GameObject managerGO = new GameObject("WaveGameManager");
            WaveGameManager manager = managerGO.AddComponent<WaveGameManager>();

            Debug.Log("Scene setup complete! Connect references in WaveGameManager inspector.");
        }

        private void ConfigureLineRenderer(LineRenderer lr, Color color, float width)
        {
            lr.startWidth = width;
            lr.endWidth = width;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = color;
            lr.endColor = color;
            lr.useWorldSpace = false;
        }

        private GameObject CreateSlider(Transform parent, string name, Vector2 position, string label, float min, float max, float value)
        {
            GameObject sliderGO = new GameObject(name);
            sliderGO.transform.SetParent(parent, false);
            
            RectTransform sliderRect = sliderGO.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0, 1);
            sliderRect.anchorMax = new Vector2(1, 1);
            sliderRect.anchoredPosition = position;
            sliderRect.sizeDelta = new Vector2(-40, 30);

            // Label
            GameObject labelGO = new GameObject("Label");
            labelGO.transform.SetParent(sliderGO.transform, false);
            TextMeshProUGUI labelText = labelGO.AddComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.fontSize = 18;
            labelText.color = Color.green;
            RectTransform labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0.5f);
            labelRect.anchorMax = new Vector2(0, 0.5f);
            labelRect.anchoredPosition = new Vector2(0, 0);
            labelRect.sizeDelta = new Vector2(100, 30);

            // Slider
            GameObject sliderObj = new GameObject("Slider");
            sliderObj.transform.SetParent(sliderGO.transform, false);
            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = value;
            
            RectTransform sliderObjRect = sliderObj.GetComponent<RectTransform>();
            sliderObjRect.anchorMin = new Vector2(0.2f, 0);
            sliderObjRect.anchorMax = new Vector2(1, 1);
            sliderObjRect.sizeDelta = Vector2.zero;

            return sliderGO;
        }

        private GameObject CreateButton(Transform parent, string name, Vector2 position, string label)
        {
            GameObject buttonGO = new GameObject(name);
            buttonGO.transform.SetParent(parent, false);
            
            Button button = buttonGO.AddComponent<Button>();
            Image buttonImage = buttonGO.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.4f, 0.2f);
            
            RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = position;
            buttonRect.sizeDelta = new Vector2(120, 35);

            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);
            TextMeshProUGUI text = textGO.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 16;
            text.color = Color.green;
            text.alignment = TextAlignmentOptions.Center;
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            return buttonGO;
        }

        private GameObject CreateText(Transform parent, string name, Vector2 anchorPos, Vector2 pivot, string text, int fontSize)
        {
            GameObject textGO = new GameObject(name);
            textGO.transform.SetParent(parent, false);
            
            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = Color.green;
            tmp.alignment = TextAlignmentOptions.Center;
            
            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = anchorPos;
            textRect.anchorMax = anchorPos;
            textRect.pivot = pivot;
            textRect.sizeDelta = new Vector2(400, 50);

            return textGO;
        }
    }
}

using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

namespace HietakissaUtils.Tools
{
    public class HKToolsEditorWindow : EditorWindow
    {
        static HKTool[] tools;
        static HKTool currentTool;

        VisualElement leftPage;
        VisualElement rightPage;

        VisualElement promptOverlay;

        public float DeltaTime { get; private set; }
        double lastTimeSinceStartup;


        [MenuItem("Tools/HK Tools")]
        public static void OpenWindow()
        {
            HKToolsEditorWindow wnd = GetWindow<HKToolsEditorWindow>();
            wnd.titleContent = new GUIContent("HK Tools");
        }

        void Initialize()
        {
            SceneView.duringSceneGui += OnSceneGUI;

            tools = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(HKTool)) && !type.IsAbstract)
                .Select(type => (HKTool)Activator.CreateInstance(type))
                .ToArray();
        }

        public void CreateGUI()
        {
            Initialize();

            VisualElement root = rootVisualElement;

            TwoPaneSplitView splitView = new TwoPaneSplitView(0, 200, TwoPaneSplitViewOrientation.Horizontal);
            root.Add(splitView);

            leftPage = new VisualElement();
            leftPage.style.flexDirection = FlexDirection.Column;

            rightPage = new VisualElement();
            splitView.Add(leftPage);
            splitView.Add(rightPage);


            CreateToolButtonList();

            foreach (HKTool tool in tools) tool.Initialize(rightPage, this);
        }

        void OnDestroy()
        {
            SceneView.duringSceneGui -= OnSceneGUI;

            currentTool?.OnExit();
            currentTool = null;
        }

        void Update()
        {
            CalculateDeltaTime();

            currentTool?.OnUpdate();
        }

        void OnSceneGUI(SceneView sceneView) => currentTool?.OnSceneGUI(sceneView);

        void CalculateDeltaTime()
        {
            if (lastTimeSinceStartup == 0f)
            {
                lastTimeSinceStartup = EditorApplication.timeSinceStartup;
            }
            DeltaTime = (float)(EditorApplication.timeSinceStartup - lastTimeSinceStartup);
            lastTimeSinceStartup = EditorApplication.timeSinceStartup;
        }

        void CreateToolButtonList()
        {
            for (int i = 0; i < tools.Length; i++)
            {
                int ID = i;
                HKTool tool = tools[ID];
                CreateButtonForTool(tool, () => SelectTool(ID));
            }


            void CreateButtonForTool(HKTool tool, Action onClickEvent)
            {
                Button button = HKToolsUtils.CreateButton(leftPage, onClickEvent, tool.ToolName);
                button.style.height = 40f;
            }
        }

        void SelectTool(int toolID)
        {
            HKTool tool = tools[toolID];

            if (currentTool != null)
            {
                if (tool == currentTool) return;
                currentTool.OnExit();
            }

            rightPage.Clear();
            currentTool = tool;
            tool.OnEnter();
            CreatePromptOverlay();
        }



        void CreatePromptOverlay()
        {
            promptOverlay = new VisualElement();
            promptOverlay.style.position = Position.Absolute;
            promptOverlay.style.top = 0; promptOverlay.style.bottom = 0;
            promptOverlay.style.left = 0; promptOverlay.style.right = 0;
            promptOverlay.style.backgroundColor = new Color(0, 0, 0, 0.7f);
            promptOverlay.style.alignItems = Align.Center;
            promptOverlay.style.justifyContent = Justify.Center;
            promptOverlay.style.display = DisplayStyle.None;
            rightPage.Add(promptOverlay);
        }

        public void ShowPrompt(string title, string message, string okText, Action onOk, string cancelText, Action onCancel)
        {
            promptOverlay.Clear();
            promptOverlay.style.display = DisplayStyle.Flex;

            VisualElement box = new VisualElement();
            box.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
            box.style.SetPaddingAll(20);
            box.style.SetBorderAll(1, Color.black);

            box.Add(new Label(title) { style = { fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 10 } });
            box.Add(new Label(message) { style = { whiteSpace = WhiteSpace.Normal, width = 250, marginBottom = 20 } });

            VisualElement btnRow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween } };

            btnRow.Add(new Button(() => { promptOverlay.style.display = DisplayStyle.None; onCancel?.Invoke(); }) { text = cancelText });
            btnRow.Add(new Button(() => { promptOverlay.style.display = DisplayStyle.None; onOk?.Invoke(); }) { text = okText });

            box.Add(btnRow);
            promptOverlay.Add(box);
        }
    }

    public abstract class HKTool
    {
        public abstract string ToolName { get; }

        protected HKToolsEditorWindow window;
        protected VisualElement page;

        public const float CONST_TITLE_FONT_SIZE = 16f;
        public const float CONST_LABEL_FONT_SIZE = 12f;


        public void Initialize(VisualElement pageElement, HKToolsEditorWindow editorWindow)
        {
            page = pageElement;
            window = editorWindow;
        }

        public virtual void OnEnter()
        {
            //Debug.Log($"No OnEnter method found for tool {ToolName}. Override to draw your own GUI.");
            HKToolsUtils.CreateTitle(page, this);
            HKToolsUtils.CreateLabel(page, $"No OnEnter method. Override to draw your own GUI.", CONST_TITLE_FONT_SIZE);
        }

        public virtual void OnExit()
        {

        }

        public virtual void OnUpdate()
        {

        }

        public virtual void OnSceneGUI(SceneView sceneView)
        {

        }
    }

    public static class HKToolsUtils
    {
        public static void CreateTitle(VisualElement page, HKTool tool)
        {
            Label title = new Label(tool.ToolName);
            title.style.alignSelf = Align.Center;
            title.style.fontSize = HKTool.CONST_TITLE_FONT_SIZE;
            title.style.paddingTop = 5;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;

            page.Add(title);
        }


        public static VisualElement CreateVisualElement(VisualElement parent, float flexGrow = 0f)
        {
            VisualElement element = new VisualElement();
            element.style.flexGrow = flexGrow;

            parent.Add(element);
            return element;
        }

        public static Label CreateLabel(VisualElement parent, string text, float fontSize = HKTool.CONST_LABEL_FONT_SIZE, float flexGrow = 0f)
        {
            Label label = new Label(text);
            label.style.fontSize = fontSize;
            label.style.flexGrow = flexGrow;

            parent.Add(label);
            return label;
        }

        public static Button CreateButton(VisualElement parent, Action onClickEvent, string buttonText = "", float fontSize = HKTool.CONST_LABEL_FONT_SIZE, float flexGrow = 0f)
        {
            Button button = new Button(onClickEvent);
            button.text = buttonText;
            button.style.fontSize = fontSize;
            button.style.flexGrow = flexGrow;

            parent.Add(button);
            return button;
        }

        public static Toggle CreateToggle(VisualElement parent, EventCallback<ChangeEvent<bool>> callback, bool defaultValue = false, string toggleText = "", float fontSize = HKTool.CONST_LABEL_FONT_SIZE, float flexGrow = 0f)
        {
            Toggle toggle = new Toggle(toggleText);
            toggle.RegisterValueChangedCallback(callback);
            toggle.style.fontSize = fontSize;
            toggle.style.flexGrow = flexGrow;
            toggle.SetValueWithoutNotify(defaultValue);

            parent.Add(toggle);
            return toggle;
        }

        public static TextField CreateTextField(VisualElement parent, EventCallback<ChangeEvent<string>> callback, string fieldName = "", float fontSize = HKTool.CONST_LABEL_FONT_SIZE, float flexGrow = 0f, bool isDelayed = true)
        {
            TextField textField = new TextField(fieldName);
            textField.RegisterValueChangedCallback(callback);
            textField.style.fontSize = fontSize;
            textField.style.flexGrow = flexGrow;
            textField.isDelayed = isDelayed;

            parent.Add(textField);
            return textField;
        }

        public static IntegerField CreateIntegerField(VisualElement parent, EventCallback<ChangeEvent<int>> callback, string fieldName = "", float fontSize = HKTool.CONST_LABEL_FONT_SIZE, float flexGrow = 0f, bool isDelayed = true)
        {
            IntegerField intField = new IntegerField(fieldName);
            intField.RegisterValueChangedCallback(callback);
            intField.style.fontSize = fontSize;
            intField.style.flexGrow = flexGrow;
            intField.isDelayed = isDelayed;

            parent.Add(intField);
            return intField;
        }

        public static FloatField CreateFloatField(VisualElement parent, EventCallback<ChangeEvent<float>> callback, string fieldName = "", float fontSize = HKTool.CONST_LABEL_FONT_SIZE, float flexGrow = 0f, bool isDelayed = true)
        {
            FloatField floatField = new FloatField(fieldName);
            floatField.RegisterValueChangedCallback(callback);
            floatField.style.fontSize = fontSize;
            floatField.style.flexGrow = flexGrow;
            floatField.isDelayed = isDelayed;

            parent.Add(floatField);
            return floatField;
        }

        public static Slider CreateSlider(VisualElement parent, EventCallback<ChangeEvent<float>> callback, float min = 0f, float max = 1f, float defaultValue = 0f, SliderDirection direction = SliderDirection.Horizontal, string sliderName = "", float fontSize = HKTool.CONST_LABEL_FONT_SIZE, float flexGrow = 0f)
        {
            Slider slider = new Slider(sliderName, min, max, direction);
            slider.RegisterCallback(callback);
            slider.style.fontSize = fontSize;
            slider.style.flexGrow = flexGrow;
            slider.SetValueWithoutNotify(defaultValue);

            parent.Add(slider);
            return slider;
        }


        public static StyleLength GetStyleLengthForPercentage(float percentage) => new StyleLength(new Length(percentage, LengthUnit.Percent));



        public static void SetActive(this VisualElement element, bool active)
        {
            element.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
        }


        /*
         box.style.paddingAll = 20;
            box.style.borderAllWidth = 1;
            box.style.borderAllColor = Color.black;
         */
    }
}

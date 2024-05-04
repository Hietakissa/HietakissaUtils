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

        public float DeltaTime { get; private set; }
        double lastTimeSinceStartup;


        [MenuItem("Tools/HK Tools")]
        public static void OpenWindow()
        {
            //HKToolsEditorWindow wnd = GetWindow<HKToolsEditorWindow>();
            
        }

        void Initialize()
        {
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
            currentTool?.OnExit();
            currentTool = null;
        }

        void Update()
        {
            CalculateDeltaTime();

            currentTool?.OnUpdate();
        }

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
        }
    }

    public abstract class HKTool
    {
        public abstract string ToolName { get; }

        protected HKToolsEditorWindow window;
        protected VisualElement page;

        public void Initialize(VisualElement pageElement, HKToolsEditorWindow editorWindow)
        {
            page = pageElement;
            window = editorWindow;
        }

        public virtual void OnEnter()
        {
            Debug.Log($"No OnEnter method found for tool {ToolName}. Override to draw your own GUI.");
        }

        public virtual void OnExit()
        {

        }

        public virtual void OnUpdate()
        {

        }
    }

    public static class HKToolsUtils
    {
        public static void CreateTitle(VisualElement page, HKTool tool)
        {
            Label title = new Label(tool.ToolName);
            title.style.alignSelf = Align.Center;
            title.style.fontSize = 16f;
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

        public static Label CreateLabel(VisualElement parent, string text, float fontSize = 12f, float flexGrow = 0f)
        {
            Label label = new Label(text);
            label.style.fontSize = fontSize;
            label.style.flexGrow = flexGrow;

            parent.Add(label);
            return label;
        }

        public static Button CreateButton(VisualElement parent, Action onClickEvent, string buttonText = "", float fontSize = 12f, float flexGrow = 0f)
        {
            Button button = new Button(onClickEvent);
            button.text = buttonText;
            button.style.fontSize = fontSize;
            button.style.flexGrow = flexGrow;

            parent.Add(button);
            return button;
        }

        public static Toggle CreateToggle(VisualElement parent, EventCallback<ChangeEvent<bool>> callback, string toggleText = "", float fontSize = 12f, float flexGrow = 0f)
        {
            Toggle toggle = new Toggle(toggleText);
            toggle.RegisterValueChangedCallback(callback);
            toggle.style.fontSize = fontSize;
            toggle.style.flexGrow = flexGrow;

            parent.Add(toggle);
            return toggle;
        }

        public static TextField CreateTextField(VisualElement parent, EventCallback<ChangeEvent<string>> callback, string fieldName = "", float fontSize = 12f, float flexGrow = 0f, bool isDelayed = true)
        {
            TextField textField = new TextField(fieldName);
            textField.RegisterValueChangedCallback(callback);
            textField.style.fontSize = fontSize;
            textField.style.flexGrow = flexGrow;
            textField.isDelayed = isDelayed;

            parent.Add(textField);
            return textField;
        }

        public static IntegerField CreateIntegerField(VisualElement parent, EventCallback<ChangeEvent<int>> callback, string fieldName = "", float fontSize = 12f, float flexGrow = 0f, bool isDelayed = true)
        {
            IntegerField intField = new IntegerField(fieldName);
            intField.RegisterValueChangedCallback(callback);
            intField.style.fontSize = fontSize;
            intField.style.flexGrow = flexGrow;
            intField.isDelayed = isDelayed;

            parent.Add(intField);
            return intField;
        }

        public static FloatField CreateFloatField(VisualElement parent, EventCallback<ChangeEvent<float>> callback, string fieldName = "", float fontSize = 12f, float flexGrow = 0f, bool isDelayed = true)
        {
            FloatField floatField = new FloatField(fieldName);
            floatField.RegisterValueChangedCallback(callback);
            floatField.style.fontSize = fontSize;
            floatField.style.flexGrow = flexGrow;
            floatField.isDelayed = isDelayed;

            parent.Add(floatField);
            return floatField;
        }



        public static StyleLength GetStyleLengthForPercentage(float percentage) => new StyleLength(new Length(percentage, LengthUnit.Percent));



        public static void SetActive(this VisualElement element, bool active)
        {
            element.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}

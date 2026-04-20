namespace HietakissaUtils.Tools
{
    using System.Collections.Generic;
    using UnityEditor.SceneManagement;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using UnityEditor;
    using UnityEngine;

    public class UIDebuggerTool : HKTool
    {
        public override string ToolName => "UI Debugger";

        List<HierarchyElement> hierarchyElements = new List<HierarchyElement>();
        bool isDirty = true;

        bool enabled = true;
        float opacity = 0.6f;

        public override void OnEnter()
        {
            EditorApplication.hierarchyChanged += EditorHierarchyChanged;
            isDirty = true;

            CreateGUI();
        }

        public override void OnExit()
        {
            EditorApplication.hierarchyChanged -= EditorHierarchyChanged;
            hierarchyElements.Clear();
        }


        void CreateGUI()
        {
            HKToolsUtils.CreateTitle(page, this);
            HKToolsUtils.CreateToggle(page, (value) => enabled = value.newValue, toggleText: "Enabled");
            HKToolsUtils.CreateSlider(page, (value) => opacity = value.newValue, sliderName: "Opacity");
        }

        public override void OnSceneGUI(SceneView obj)
        {
            if (!enabled) return;
            if (isDirty) BuildHierarchy();

            for (int i = 0; i < hierarchyElements.Count; i++)
            {
                HierarchyElement element = hierarchyElements[i];
                Graphic graphic = element.Graphic;

                Vector3[] corners = new Vector3[4];
                graphic.rectTransform.GetWorldCorners(corners);


                Handles.DrawSolidRectangleWithOutline(corners, element.FillColor, element.OutlineColor);
                GUIStyle labelStyle = new GUIStyle(EditorStyles.label)
                {
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.UpperLeft,
                    normal = { textColor = element.TextColor },
                    hover = { textColor = element.TextColor },
                };
                Handles.Label(corners[1], graphic.gameObject.name, labelStyle);


                Vector4 padding = graphic.raycastPadding;
                if (padding != Vector4.zero)
                {
                    corners = GetPaddedWorldCorners(graphic);
                    Handles.DrawDottedLines(new[] { corners[0], corners[1], corners[1], corners[2], corners[2], corners[3], corners[3], corners[0] }, 4f);
                    Handles.Label(corners[1], $"{graphic.gameObject.name} [padding]", labelStyle);
                }
                
            }


            Vector3[] GetPaddedWorldCorners(Graphic graphic)
            {
                Vector3[] worldCorners = new Vector3[4];
                graphic.rectTransform.GetWorldCorners(worldCorners);

                // raycastPadding is Vector4(Left, Bottom, Right, Top)
                Vector4 padding = graphic.raycastPadding;

                // If there's no padding, just return the originals
                if (padding == Vector4.zero) return worldCorners;

                for (int i = 0; i < 4; i++)
                {
                    // 1. To Local Space
                    Vector3 localPoint = graphic.transform.InverseTransformPoint(worldCorners[i]);

                    // 2. Apply Padding 
                    // Corners are usually: 0: Bottom-Left, 1: Top-Left, 2: Top-Right, 3: Bottom-Right
                    switch (i)
                    {
                        case 0: // Bottom-Left
                            localPoint.x += padding.x;
                            localPoint.y += padding.y;
                            break;
                        case 1: // Top-Left
                            localPoint.x += padding.x;
                            localPoint.y -= padding.w;
                            break;
                        case 2: // Top-Right
                            localPoint.x -= padding.z;
                            localPoint.y -= padding.w;
                            break;
                        case 3: // Bottom-Right
                            localPoint.x -= padding.z;
                            localPoint.y += padding.y;
                            break;
                    }

                    // 3. Back to World Space
                    worldCorners[i] = graphic.transform.TransformPoint(localPoint);
                }

                return worldCorners;
            }
        }


        void EditorHierarchyChanged() => isDirty = true;

        void BuildHierarchy()
        {
            hierarchyElements.Clear();

            List<Canvas> canvases = new List<Canvas>();
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                Scene scene = EditorSceneManager.GetSceneAt(i);
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    FindCanvasesRecursively(root.transform, canvases);
                }
            }
            canvases.Sort((a, b) => b.sortingOrder.CompareTo(a.sortingOrder));

            for (int i = 0; i < canvases.Count; i++)
            {
                FindGraphicsRecursively(canvases[i].transform, hierarchyElements);
            }



            void FindCanvasesRecursively(Transform t, List<Canvas> list)
            {
                if (t is not RectTransform) return;
                
                if (t.TryGetComponent<Canvas>(out Canvas canvas))
                {
                    list.Add(canvas);
                    foreach (Transform child in t)
                    {
                        FindCanvasesRecursively(child, list);
                    }
                }
            }

            void FindGraphicsRecursively(Transform t, List<HierarchyElement> list, bool blocked = true)
            {
                if (t.TryGetComponent<CanvasGroup>(out CanvasGroup group) && group.ignoreParentGroups) blocked = group.blocksRaycasts;

                if (blocked)
                {
                    Graphic[] graphics = t.GetComponents<Graphic>();
                    foreach (Graphic graphic in graphics)
                    {
                        float hue = GetHueForGraphic(graphic);
                        Color fillColor = GetFillColor(hue);
                        Color outlineColor = GetOutlineColor(hue);
                        Color textColor = GetTextColorForBackground(fillColor);
                        list.Add(new HierarchyElement(graphic, fillColor, outlineColor, textColor));
                    }
                }


                foreach (Transform child in t)
                {
                    if (!child.TryGetComponent<Canvas>(out Canvas canvas)) FindGraphicsRecursively(child, list, blocked);
                }
            }
        }


        float GetHueForGraphic(Graphic graphic)
        {
            return Mathf.Repeat(graphic.GetInstanceID() * 0.61803398875f, 1.0f);
        }
        Color GetFillColor(float hue) => Color.HSVToRGB(hue, 0.7f, 0.85f).WithAlpha(opacity);
        Color GetOutlineColor(float hue) => Color.HSVToRGB(hue, 0.8f, 0.5f).WithAlpha(1f);
        Color GetTextColorForBackground(Color backgroundColor)
        {
            float luminance = (0.299f * backgroundColor.r) + (0.587f * backgroundColor.g) + (0.114f * backgroundColor.b);
            return luminance > 0.5f ? Color.black : Color.white;
        }

        readonly struct HierarchyElement
        {
            public readonly Graphic Graphic;
            public readonly Color FillColor;
            public readonly Color OutlineColor;
            public readonly Color TextColor;

            public HierarchyElement(Graphic graphic, Color fillColor, Color outlinecolor, Color textColor)
            {
                Graphic = graphic;
                FillColor = fillColor;
                OutlineColor = outlinecolor;
                TextColor = textColor;
            }
        }
    }
}
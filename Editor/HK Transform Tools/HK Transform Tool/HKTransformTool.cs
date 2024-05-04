using UnityEditor.ShortcutManagement;
using UnityEditor.EditorTools;
using UnityEditor;
using UnityEngine;


#if UNITY_EDITOR_WIN
using System.Runtime.InteropServices;
#endif

namespace HietakissaUtils.HKTransformTool
{
    [EditorTool("Custom Transform Tool")]
    public class HKTransformTool : EditorTool
    {
        [Shortcut("Use Custom Transform Tool", KeyCode.B)]
        static void TransformToolShortcut()
        {
            //if (Selection.GetFiltered<Transform>(SelectionMode.TopLevel).Length > 0)
            //{
            //    ToolManager.SetActiveTool<HKTransformTool>();
            //}

            ToolManager.SetActiveTool<HKTransformTool>();
        }

#if UNITY_EDITOR_WIN
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out int X, out int Y);
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);
#endif


        Color red = new Color(0.98f, 0.192f, 0.306f);
        Color green = new Color(0.533f, 0.835f, 0.051f);
        Color blue = new Color(0.192f, 0.537f, 0.925f);

        HKTransformOperation[] operations = new HKTransformOperation[]
        {
            new HKMoveOperation(new KeyCombination(KeyCode.G, EventModifiers.None)),
            new HKRotateOperation(new KeyCombination(KeyCode.R, EventModifiers.None)),
            new HKScaleOperation(new KeyCombination(KeyCode.S, EventModifiers.None)),
            new HKResetPositionOperation(new KeyCombination(KeyCode.G, EventModifiers.Shift)),
            new HKResetRotationOperation(new KeyCombination(KeyCode.R, EventModifiers.Shift)),
            new HKResetScaleOperation(new KeyCombination(KeyCode.S, EventModifiers.Shift))
        };
        HKTransformOperation activeOperation;

        new public Transform[] targets;

        public Vector3Int WorkingAxis { get; private set; } = Vector3Int.one;
        public Vector3 CommonOrigin => commonOrigin;
        Vector3 commonOrigin;

        public OriginMode OriginMode => originMode;
        OriginMode originMode = OriginMode.Median;

        public WorkingMode WorkingMode => workingMode;
        WorkingMode workingMode;


        public static bool RightClickCancelOperation = false;


        KeyCodeChar[] keyCodeChars = new KeyCodeChar[]
        {
            new KeyCodeChar(KeyCode.Alpha0, '0'),
            new KeyCodeChar(KeyCode.Alpha1, '1'),
            new KeyCodeChar(KeyCode.Alpha2, '2'),
            new KeyCodeChar(KeyCode.Alpha3, '3'),
            new KeyCodeChar(KeyCode.Alpha4, '4'),
            new KeyCodeChar(KeyCode.Alpha5, '5'),
            new KeyCodeChar(KeyCode.Alpha6, '6'),
            new KeyCodeChar(KeyCode.Alpha7, '7'),
            new KeyCodeChar(KeyCode.Alpha8, '8'),
            new KeyCodeChar(KeyCode.Alpha9, '9'),
            new KeyCodeChar(KeyCode.Period, ','),
            new KeyCodeChar(KeyCode.Comma, ','),
            new KeyCodeChar(KeyCode.Minus, '-')
        };
        string valueInput;
        public bool HasValueOverride { get; private set; }
        public float OverrideValue { get; private set; }

        public Vector2 EasedMouseInput => mouseInput * Mathf.Sqrt(Mathf.Max(1f, Vector3.Distance(Cam.transform.position, commonOrigin)));
        public Vector2 MouseInput => mouseInput;
        Vector2 mouseInput;

        const float sensitivity = 0.01f;
        bool hasWrapped;

        public Camera Cam { get; private set; }


        public override void OnActivated()
        {
            Debug.Log($"enter");
            ClearNumericInput();
            foreach (HKTransformOperation operation in operations) operation.Initialize(this);

            Selection.selectionChanged += SelectionChanged;
        }
        public override void OnWillBeDeactivated()
        {
            TryCancelOperation();
            Debug.Log($"exit");

            Selection.selectionChanged -= SelectionChanged;
        }


        /// todo:
        /// figure out GUI window
        /// start implementing operation logic
        /// -movement-
        /// -rotation-
        /// scale (stretching with global)
        /// snapping increments with ctrl
        /// finer control with shift
        /// Alt + G/R/S to reset

        public override void OnToolGUI(EditorWindow window)
        {
            if (window is not SceneView) return;

            targets = Selection.transforms;
            if (targets.Length == 0) return;

            Handles.color = Color.white;
            Handles.DrawLine(commonOrigin, commonOrigin + Vector3.up);


            //Camera Cam = SceneView.currentDrawingSceneView.camera;
            Cam = SceneView.currentDrawingSceneView.camera;
            Event current = Event.current;


            // Mouse input and wrapping
            if (!UnityEditor.Tools.viewToolActive && activeOperation != null && current.type == EventType.MouseMove && current.delta != Vector2.zero)
            {
                if (hasWrapped) hasWrapped = false;
                else mouseInput += Vector2.Scale(current.delta, new Vector2(1, -1)) * sensitivity;

#if UNITY_EDITOR_WIN
                WrapMouseInWindow((int)current.mousePosition.x, (int)current.mousePosition.y, (int)window.position.width, Cam.pixelHeight);
#endif
            }


            
            // Handle changing OriginMode
            if (current.type == EventType.KeyDown && current.keyCode == KeyCode.O)
            {
                originMode = originMode == OriginMode.Median ? OriginMode.Individual : OriginMode.Median;
                MarkEventAsUsed();
            }


            // Calculate common origin
            if (activeOperation != null && activeOperation.ShouldUpdateCommonOrigin) CalculateCommonOrigin();


            // Handle scene flythrough depending on setting
            if (!RightClickCancelOperation && UnityEditor.Tools.viewToolActive)
            {
                if (activeOperation != null) DrawHandles();
                return;
            }
            

            // Handle Applying and Canceling active operation
            if (activeOperation != null)
            {
                switch (current.type)
                {
                    case EventType.MouseDown:
                        
                        if (current.button == 0)
                        {
                            ApplyOperation();
                            MarkEventAsUsed();
                            return;
                        }
                        else if (RightClickCancelOperation && current.button == 1)
                        {
                            CancelOperation();
                            MarkEventAsUsed();
                            return;
                        }
                        break;

                    case EventType.KeyDown:

                        if (current.keyCode == KeyCode.Return)
                        {
                            ApplyOperation();
                            MarkEventAsUsed();
                            return;
                        }
                        else if (current.keyCode == KeyCode.Escape)
                        {
                            CancelOperation();
                            MarkEventAsUsed();
                            return;
                        }
                        break;
                }
            }


            // Handle starting operations
            if (activeOperation == null || !IsKeyCombinationPressed(activeOperation.Combination, current))
            {
                foreach (HKTransformOperation operation in operations)
                {
                    if (IsKeyCombinationPressed(operation.Combination, current))
                    {
                        TryCancelOperation();
                        StartOperation(operation);
                        MarkEventAsUsed();
                        break;
                    }
                }
            }


            // Handle configuring working axis
            HandleWorkingAxis();


            // Handle overriding operation value with numeric characters
            if (activeOperation != null) HandleNumericInputOverride();


            activeOperation?.OnUpdate();


            // Draw handles for object(s)
            if (activeOperation != null) DrawHandles();

            //Debug.Log($"origin: {originMode}, working: {workingMode}, axis: {WorkingAxis}");


            bool IsKeyCombinationPressed(KeyCombination combination, Event currentEvent)
            {
                return currentEvent.type == EventType.KeyDown
                && currentEvent.keyCode == combination.Key
                && currentEvent.modifiers == combination.Modifier;
            }

            void MarkEventAsUsed()
            {
                //int controlID = GUIUtility.GetControlID(FocusType.Passive);
                //GUIUtility.hotControl = controlID;
                current.Use();
            }

            void CycleWorkingModeAndAxis(Vector3Int axis)
            {
                if (WorkingAxis != axis)
                {
                    WorkingAxis = axis;
                    workingMode = WorkingMode.Global;
                    return;
                }

                if (workingMode == WorkingMode.Global) workingMode = WorkingMode.Local;
                else if (workingMode == WorkingMode.Local)
                {
                    workingMode = WorkingMode.Global;
                    WorkingAxis = Vector3Int.one;
                }
            }

            void HandleWorkingAxis()
            {
                if (activeOperation != null && current.type == EventType.KeyDown)
                {
                    if (current.keyCode == KeyCode.X)
                    {
                        if (current.modifiers == EventModifiers.Shift)
                        {
                            Vector3Int axis = Vector3Int.forward + Vector3Int.up;
                            CycleWorkingModeAndAxis(axis);
                        }
                        else
                        {
                            Vector3Int axis = Vector3Int.right;
                            CycleWorkingModeAndAxis(axis);
                        }
                        MarkEventAsUsed();
                    }
                    else if (current.keyCode == KeyCode.Y)
                    {
                        if (current.modifiers == EventModifiers.Shift)
                        {
                            Vector3Int axis = Vector3Int.right + Vector3Int.forward;
                            CycleWorkingModeAndAxis(axis);
                        }
                        else
                        {
                            Vector3Int axis = Vector3Int.up;
                            CycleWorkingModeAndAxis(axis);
                        }
                        MarkEventAsUsed();
                    }
                    else if (current.keyCode == KeyCode.Z)
                    {
                        if (current.modifiers == EventModifiers.Shift)
                        {
                            Vector3Int axis = Vector3Int.right + Vector3Int.up;
                            CycleWorkingModeAndAxis(axis);
                        }
                        else
                        {
                            Vector3Int axis = Vector3Int.forward;
                            CycleWorkingModeAndAxis(axis);
                        }
                        MarkEventAsUsed();
                    }
                }
            }

            void HandleNumericInputOverride()
            {
                foreach (KeyCodeChar keyChar in keyCodeChars)
                {
                    if (current.type == EventType.KeyDown)
                    {
                        if (valueInput.Length > 0 && current.keyCode == KeyCode.Backspace)
                        {
                            valueInput = valueInput.Remove(valueInput.Length - 1);
                            
                            if (valueInput.Length == 0)
                            {
                                HasValueOverride = false;
                                OverrideValue = 0f;
                            }
                            else if (float.TryParse(valueInput, out float value))
                            {
                                HasValueOverride = true;
                                OverrideValue = value;
                            }

                            MarkEventAsUsed();
                            break;
                        }
                        else if (current.keyCode == keyChar.Key)
                        {
                            MarkEventAsUsed();

                            if (keyChar.Char == ',' && valueInput.Contains(keyChar.Char)) return;
                            if (keyChar.Char == '-' && valueInput.Contains(keyChar.Char)) return;
                            valueInput += keyChar.Char;

                            if (float.TryParse(valueInput, out float value))
                            {
                                HasValueOverride = true;
                                OverrideValue = value;
                                break;
                            }
                        }
                    }
                }
            }

#if UNITY_EDITOR_WIN
            void WrapMouseInWindow(int mouseX, int mouseY, int width, int height)
            {
                hasWrapped = false;

                const int safeGuard = 60;
                if (mouseX < safeGuard)
                {
                    mouseX += width - safeGuard * 2;
                    hasWrapped = true;
                }
                else if (mouseX > width - safeGuard)
                {
                    mouseX = mouseX - width + safeGuard * 2;
                    hasWrapped = true;
                }

                if (mouseY < safeGuard)
                {
                    mouseY += height - safeGuard * 2;
                    hasWrapped = true;
                }
                else if (mouseY > height - safeGuard)
                {
                    mouseY = mouseY - height + safeGuard * 2;
                    hasWrapped = true;
                }

                if (hasWrapped) SetCursorPos((int)window.position.x + mouseX, Screen.height - Cam.pixelHeight + (int)window.position.y + mouseY);
            }
#endif
        }

        void CalculateCommonOrigin()
        {
            commonOrigin = Vector3.zero;
            foreach (Transform t in targets) commonOrigin += t.position;
            commonOrigin /= targets.Length;
        }
        void DrawHandles()
        {
            if (workingMode == WorkingMode.Global) // Draw handles for median or every object depending on OriginMode
            {
                switch (originMode)
                {
                    case OriginMode.Median:
                        DrawWorkingAxisHandles(commonOrigin, Vector3.right, Vector3.up, Vector3.forward);
                        break;

                    case OriginMode.Individual:
                        foreach (Transform t in targets)
                        {
                            DrawWorkingAxisHandles(t.position, Vector3.right, Vector3.up, Vector3.forward);
                        }
                        break;
                }
            }
            else if (workingMode == WorkingMode.Local) // Draw local handles for every object
            {
                foreach (Transform t in targets)
                {
                    DrawWorkingAxisHandles(t.position, t.right, t.up, t.forward);
                }
            }
        }
        void DrawWorkingAxisHandles(Vector3 point, Vector3 rightDir, Vector3 upDir, Vector3 forwardDir)
        {
            const float activeThickness = 1f;
            //const float inactiveThickness = 1f;
            const float lineLength = 100f;

            const float screenSpaceSize = 5f;


            Handles.zTest = UnityEngine.Rendering.CompareFunction.Less;



            // X
            Handles.color = red;
            if (WorkingAxis.x == 1) Handles.DrawLine(point + rightDir * lineLength, point - rightDir * lineLength, activeThickness);
            else Handles.DrawDottedLine(point + rightDir * lineLength, point - rightDir * lineLength, screenSpaceSize);
            //Handles.DrawLine(point + rightDir * lineLength, point - rightDir * lineLength, workingAxis == Vector3Int.one ? inactiveThickness : workingAxis.x == 1 ? activeThickness : inactiveThickness);
            // Y
            Handles.color = green;
            if (WorkingAxis.y == 1) Handles.DrawLine(point + upDir * lineLength, point - upDir * lineLength, activeThickness);
            else Handles.DrawDottedLine(point + upDir * lineLength, point - upDir * lineLength, screenSpaceSize);
            //Handles.DrawLine(point + upDir * lineLength, point - upDir * lineLength, workingAxis == Vector3Int.one ? inactiveThickness : workingAxis.y == 1 ? activeThickness : inactiveThickness);
            // Z
            Handles.color = blue;
            //Handles.DrawLine(point + forwardDir * lineLength, point - forwardDir * lineLength, workingAxis == Vector3Int.one ? inactiveThickness : workingAxis.z == 1 ? activeThickness : inactiveThickness);
            if (WorkingAxis.z == 1) Handles.DrawLine(point + forwardDir * lineLength, point - forwardDir * lineLength, activeThickness);
            else Handles.DrawDottedLine(point + forwardDir * lineLength, point - forwardDir * lineLength, screenSpaceSize);
        }

        void StartOperation(HKTransformOperation operation)
        {
            WorkingAxis = Vector3Int.one;
            workingMode = WorkingMode.Global;
            mouseInput = Vector2.zero;

            CalculateCommonOrigin();

            activeOperation = operation;
            activeOperation.OnStart();
        }
        public void ApplyOperation()
        {
            activeOperation.OnApply();
            activeOperation = null;

            ClearNumericInput();

            ClearWorkingAxis();
        }
        void TryCancelOperation()
        {
            if (activeOperation != null) CancelOperation();
        }
        void CancelOperation()
        {
            activeOperation.OnCancel();
            activeOperation = null;

            ClearNumericInput();

            ClearWorkingAxis();
        }

        void ClearWorkingAxis()
        {
            WorkingAxis = Vector3Int.one;
        }
        void ClearNumericInput()
        {
            valueInput = string.Empty;
            HasValueOverride = false;
        }

        void SelectionChanged()
        {
            if (activeOperation != null) CancelOperation();
        }


        public Vector3 GetOriginForTransform(Transform t)
        {
            return originMode switch
            {
                OriginMode.Median => commonOrigin,
                OriginMode.Individual => t.position,
                _ => Vector3.zero
            };
        }
    }

    struct KeyCombination
    {
        public readonly KeyCode Key;
        public readonly EventModifiers Modifier;

        public KeyCombination(KeyCode key, EventModifiers modifier)
        {
            Key = key;
            Modifier = modifier;
        }
    }

    struct KeyCodeChar
    {
        public readonly KeyCode Key;
        public readonly char Char;

        public KeyCodeChar(KeyCode key, char chararcter)
        {
            Key = key;
            Char = chararcter;
        }
    }

    public enum OriginMode
    {
        Median,
        Individual
    }

    public enum WorkingMode
    {
        Global,
        Local
    }
}

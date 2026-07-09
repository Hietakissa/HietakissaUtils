using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;

namespace HietakissaUtils.Tools
{
    // --- DATA CLASSES ---
    [Serializable]
    public class FavoritesData
    {
        public List<FavoriteGroup> groups = new List<FavoriteGroup>();
    }

    [Serializable]
    public class FavoriteGroup
    {
        public string groupName;
        public List<FavoriteItem> items = new List<FavoriteItem>();
    }

    [Serializable]
    public class FavoriteItem
    {
        public string name;
        public string globalObjectId;
        public bool isSceneObject;
    }

    // --- UNDO PROXY OBJECT ---
    // Unity's Undo system requires a ScriptableObject to track changes.
    public class FavoritesToolState : ScriptableObject
    {
        public FavoritesData data = new FavoritesData();
    }

    // --- MAIN TOOL ---
    public class FavoritesTool : HKTool
    {
        public override string ToolName => "Favorites";
        private const string PREFS_KEY = "HK_FavoritesData";

        private FavoritesToolState state;
        private FavoritesData data => state.data;

        private int currentGroupIndex = 0;
        private FavoriteGroup currentGroup => (data.groups.Count > 0 && currentGroupIndex < data.groups.Count) ? data.groups[currentGroupIndex] : null;

        private FavoriteItem currentSelectedItem;
        private VisualElement currentSelectedUIElement;

        private VisualElement leftPage;
        private VisualElement rightPage;
        private VisualElement promptOverlay;

        bool isItemsUIDirty = false;

        public override void OnEnter()
        {
            // Initialize Undo Proxy
            state = ScriptableObject.CreateInstance<FavoritesToolState>();
            state.hideFlags = HideFlags.HideAndDontSave;
            LoadData();

            if (data.groups.Count == 0)
            {
                data.groups.Add(new FavoriteGroup { groupName = "Default" });
                SaveData();
            }

            Undo.undoRedoPerformed += OnUndoRedo;
            EditorSceneManager.sceneOpened += OnSceneStateChanged;
            EditorSceneManager.sceneClosed += OnSceneStateChanged;
            

            CreateGUI();
        }

        public override void OnExit()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            EditorSceneManager.sceneOpened -= OnSceneStateChanged;
            EditorSceneManager.sceneClosed -= OnSceneStateChanged;

            if (state != null) Object.DestroyImmediate(state);
        }

        public override void OnUpdate()
        {
            if (isItemsUIDirty)
            {
                RefreshItemsUI();
                isItemsUIDirty = false;
            }
        }

        private void OnUndoRedo()
        {
            SaveData(); // Sync the restored Undo state back to EditorPrefs
            RefreshGroupsUI();
            RefreshItemsUI();
        }

        private void OnSceneStateChanged(Scene scene, OpenSceneMode mode) => isItemsUIDirty = true;
        private void OnSceneStateChanged(Scene scene) => isItemsUIDirty = true;

        private void LoadData()
        {
            if (EditorPrefs.HasKey(PREFS_KEY))
            {
                string json = EditorPrefs.GetString(PREFS_KEY);
                state.data = JsonUtility.FromJson<FavoritesData>(json);

                foreach (var item in data.groups)
                {
                    if (string.IsNullOrEmpty(item.groupName)) item.groupName = "Unnamed Group";
                }
            }
        }

        private void SaveData()
        {
            string json = JsonUtility.ToJson(data);
            EditorPrefs.SetString(PREFS_KEY, json);
        }

        private void RecordUndo(string actionName)
        {
            Undo.RegisterCompleteObjectUndo(state, actionName);
        }

        void CreateGUI()
        {
            page.RegisterCallback<KeyDownEvent>(OnKeyDown);
            page.focusable = true;

            HKToolsUtils.CreateTitle(page, this);

            TwoPaneSplitView splitView = new TwoPaneSplitView(0, 200, TwoPaneSplitViewOrientation.Horizontal);
            splitView.style.borderTopWidth = 1;
            splitView.style.borderTopColor = Color.gray;
            splitView.style.flexGrow = 1;

            leftPage = new VisualElement();
            leftPage.style.flexDirection = FlexDirection.Column;
            RefreshGroupsUI();

            rightPage = new VisualElement();
            rightPage.style.flexDirection = FlexDirection.Row;
            rightPage.style.flexWrap = Wrap.Wrap;
            rightPage.style.alignContent = Align.FlexStart;

            rightPage.RegisterCallback<DragUpdatedEvent>(evt =>
            {
                if (currentGroup != null)
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
            });
            rightPage.RegisterCallback<DragPerformEvent>(OnDragPerform);

            RefreshItemsUI();

            splitView.Add(leftPage);
            splitView.Add(rightPage);
            page.Add(splitView);

            CreatePromptOverlay();
        }

        private void RefreshGroupsUI()
        {
            leftPage.Clear();
            for (int i = 0; i < data.groups.Count; i++)
            {
                int index = i;
                FavoriteGroup group = data.groups[i];

                VisualElement groupContainer = new VisualElement();
                groupContainer.style.flexDirection = FlexDirection.Row;
                groupContainer.style.backgroundColor = (index == currentGroupIndex) ? new Color(0.2f, 0.4f, 0.6f) : StyleKeyword.Null;
                groupContainer.style.SetPaddingAll(4);
                groupContainer.style.borderBottomWidth = 1;
                groupContainer.style.borderBottomColor = new Color(0.15f, 0.15f, 0.15f);

                Label nameLabel = new Label(group.groupName);
                nameLabel.style.flexGrow = 1;

                // Select on single click, Rename on double click
                groupContainer.RegisterCallback<MouseDownEvent>(evt =>
                {
                    if (evt.clickCount == 1)
                    {
                        currentGroupIndex = index;
                        currentSelectedItem = null;
                        RefreshGroupsUI();
                        RefreshItemsUI();
                    }
                    else if (evt.clickCount == 2)
                    {
                        ShowRenameField(groupContainer, nameLabel, group);
                    }
                });

                groupContainer.Add(nameLabel);
                leftPage.Add(groupContainer);
            }

            Button addGroupBtn = new Button(() =>
            {
                RecordUndo("Add Favorites Group");
                data.groups.Add(new FavoriteGroup { groupName = "New Group " + (data.groups.Count + 1) });
                SaveData();
                RefreshGroupsUI();
            })
            { text = "+ Add Group", style = { marginTop = 10 } };

            leftPage.Add(addGroupBtn);
        }

        private void ShowRenameField(VisualElement container, Label label, FavoriteGroup group)
        {
            container.Remove(label);
            TextField renameField = new TextField { value = group.groupName };
            renameField.style.flexGrow = 1;

            Action applyRename = () =>
            {
                if (group.groupName != renameField.value && !string.IsNullOrEmpty(renameField.value))
                {
                    RecordUndo("Rename Favorites Group");
                    group.groupName = renameField.value;
                    SaveData();
                }
                RefreshGroupsUI();
            };

            renameField.RegisterCallback<FocusOutEvent>(e => applyRename());
            renameField.RegisterCallback<KeyDownEvent>(e =>
            {
                if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                    applyRename();
                else if (e.keyCode == KeyCode.Escape)
                    RefreshGroupsUI(); // Cancel
            });

            container.Add(renameField);
            renameField.Focus();
        }

        private void RefreshItemsUI()
        {
            rightPage.Clear();
            if (currentGroup == null) return;

            foreach (var item in currentGroup.items)
            {
                PreviewElement preview = new PreviewElement(item);
                preview.RegisterCallback<PointerDownEvent>(evt => SelectItem(item, preview));
                preview.RegisterCallback<MouseDownEvent>(evt =>
                {
                    if (evt.clickCount == 2) OpenItem(item);
                });

                rightPage.Add(preview);
            }
        }

        private void SelectItem(FavoriteItem item, VisualElement uiElement)
        {
            currentSelectedItem = item;
            if (currentSelectedUIElement != null)
                currentSelectedUIElement.style.borderBottomColor = StyleKeyword.Null;

            currentSelectedUIElement = uiElement;
            currentSelectedUIElement.style.borderBottomWidth = 2;
            currentSelectedUIElement.style.borderBottomColor = Color.cyan;

            page.Focus();
        }

        private void OnDragPerform(DragPerformEvent evt)
        {
            if (currentGroup == null) return;
            DragAndDrop.AcceptDrag();

            bool addedAny = false;
            foreach (Object obj in DragAndDrop.objectReferences)
            {
                GlobalObjectId id = GlobalObjectId.GetGlobalObjectIdSlow(obj);
                string idString = id.ToString();

                if (currentGroup.items.Any(i => i.globalObjectId == idString)) continue;

                if (!addedAny) RecordUndo("Add Favorite Items");
                addedAny = true;

                currentGroup.items.Add(new FavoriteItem
                {
                    name = obj.name,
                    globalObjectId = idString,
                    isSceneObject = id.identifierType == 2
                });
            }

            if (addedAny)
            {
                SaveData();
                RefreshItemsUI();
            }
        }

        private void OpenItem(FavoriteItem item)
        {
            if (!GlobalObjectId.TryParse(item.globalObjectId, out GlobalObjectId id)) return;
            Object obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);

            if (obj != null)
            {
                EditorGUIUtility.PingObject(obj);
                Selection.activeObject = obj;
                if (item.isSceneObject)
                    SceneView.FrameLastActiveSceneView();
            }
            else if (item.isSceneObject)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(id.assetGUID.ToString());
                if (!string.IsNullOrEmpty(assetPath))
                {
                    ShowPrompt("Unloaded Scene", $"This GameObject is in '{assetPath}'. Open the scene?",
                        "Yes", () =>
                        {
                            EditorSceneManager.OpenScene(assetPath, OpenSceneMode.Single);
                            OpenItem(item);
                        },
                        "Cancel", null);
                }
            }
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Delete)
            {
                if (currentSelectedItem != null && currentGroup != null)
                {
                    RecordUndo("Delete Favorite Item");
                    currentGroup.items.Remove(currentSelectedItem);
                    currentSelectedItem = null;
                    SaveData();
                    RefreshItemsUI();
                }
                else if (currentGroup != null && data.groups.Count > 1)
                {
                    if (currentGroup.items.Count > 0)
                    {
                        ShowPrompt("Delete Group", $"Group '{currentGroup.groupName}' is not empty. Delete anyway?",
                            "Delete", () => DeleteCurrentGroup(),
                            "Cancel", null);
                    }
                    else
                    {
                        DeleteCurrentGroup();
                    }
                }
            }
        }

        private void DeleteCurrentGroup()
        {
            RecordUndo("Delete Favorites Group");
            data.groups.RemoveAt(currentGroupIndex);
            currentGroupIndex = Mathf.Clamp(currentGroupIndex - 1, 0, data.groups.Count - 1);
            SaveData();
            RefreshGroupsUI();
            RefreshItemsUI();
        }

        // --- PROMPT OVERLAY ---
        private void CreatePromptOverlay()
        {
            promptOverlay = new VisualElement();
            promptOverlay.style.position = Position.Absolute;
            promptOverlay.style.top = 0; promptOverlay.style.bottom = 0;
            promptOverlay.style.left = 0; promptOverlay.style.right = 0;
            promptOverlay.style.backgroundColor = new Color(0, 0, 0, 0.7f);
            promptOverlay.style.alignItems = Align.Center;
            promptOverlay.style.justifyContent = Justify.Center;
            promptOverlay.style.display = DisplayStyle.None;
            page.Add(promptOverlay);
        }

        private void ShowPrompt(string title, string message, string okText, Action onOk, string cancelText, Action onCancel)
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

    // --- PREVIEW ELEMENT ---
    class PreviewElement : VisualElement
    {
        public PreviewElement(FavoriteItem item)
        {
            this.style.width = 80;
            this.style.height = 100;
            this.style.flexDirection = FlexDirection.Column;
            this.style.alignItems = Align.Center;
            this.style.justifyContent = Justify.Center;
            this.style.marginBottom = 8;
            this.style.SetPaddingAll(4);

            this.RegisterCallback<MouseEnterEvent>(evt => this.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.5f));
            this.RegisterCallback<MouseLeaveEvent>(evt => this.style.backgroundColor = StyleKeyword.Null);

            // Attempt to resolve object
            GlobalObjectId.TryParse(item.globalObjectId, out GlobalObjectId id);
            Object asset = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);

            bool exists = asset != null;
            bool isUnloadedScene = false;

            // Check if it's an unloaded scene object vs completely deleted
            if (!exists && item.isSceneObject)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(id.assetGUID.ToString());
                if (!string.IsNullOrEmpty(scenePath))
                {
                    var scene = EditorSceneManager.GetSceneByPath(scenePath);
                    if (!scene.isLoaded) isUnloadedScene = true;
                }
            }

            Image img = new Image { style = { width = 64, height = 64 } };

            if (exists)
            {
                Texture2D previewTex = AssetPreview.GetAssetPreview(asset);
                img.image = previewTex != null ? previewTex : asset is Material ? AssetPreview.GetMiniTypeThumbnail(asset.GetType()) : AssetPreview.GetMiniThumbnail(asset);
            }
            else if (isUnloadedScene)
            {
                img.image = EditorGUIUtility.IconContent("SceneAsset Icon").image;
            }
            else
            {
                img.image = EditorGUIUtility.IconContent("console.erroricon").image;
            }

            this.Add(img);

            Label label = new Label(exists ? asset.name : (isUnloadedScene ? $"{item.name}\n(Unloaded)" : "Missing"));
            label.style.fontSize = 11;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;

            // Color states: Normal (Gray), Warning (Yellow), Error (Red)
            if (exists) label.style.color = new Color(0.85f, 0.85f, 0.85f);
            else if (isUnloadedScene) label.style.color = new Color(1f, 0.8f, 0.3f);
            else label.style.color = new Color(0.9f, 0.3f, 0.3f);

            label.style.maxWidth = 76;
            label.style.textOverflow = TextOverflow.Ellipsis;
            label.style.overflow = Overflow.Hidden;
            // Allow wrapping for the unloaded state text
            if (isUnloadedScene) label.style.whiteSpace = WhiteSpace.Normal;

            this.Add(label);
        }
    }

    // --- EXTENSIONS ---
    public static class StyleExtensions
    {
        public static void SetPaddingAll(this IStyle style, int padding)
        {
            style.paddingTop = padding; style.paddingBottom = padding;
            style.paddingLeft = padding; style.paddingRight = padding;
        }
        public static void SetBorderAll(this IStyle style, int width, Color color)
        {
            style.borderTopWidth = width; style.borderBottomWidth = width;
            style.borderLeftWidth = width; style.borderRightWidth = width;
            style.borderTopColor = color; style.borderBottomColor = color;
            style.borderLeftColor = color; style.borderRightColor = color;
        }
    }
}
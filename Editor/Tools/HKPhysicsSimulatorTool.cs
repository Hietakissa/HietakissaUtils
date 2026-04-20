using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System;

namespace HietakissaUtils.Tools
{
    using QOL;

    public class HKPhysicsSimulatorTool : HKTool
    {
        public override string ToolName => "Physics Simulator";

        #region GUI VisualElement References
        ScrollView selectedObjectsPreviewScrollview;
        Label selectedObjectsLabel;
        Button createClosePhysicsSceneButton;
        FloatField simulationDurationFloatField;
        IntegerField ticksPerKeyframeIntField;
        Button simulateButton;
        Button applySimulationButton;
        Slider timelineSlider;
        Label simulationDurationLabel;
        #endregion

        List<Transform> selectionTransformsList = new List<Transform>();
        Dictionary<Transform, ObjectAvailability> selectionTransformAvailabilities = new Dictionary<Transform, ObjectAvailability>();
        Dictionary<Transform, KeyFrameInfo> objectKeyframedata = new Dictionary<Transform, KeyFrameInfo>();

        //Dictionary<Transform, Transform> objectClonePairs = new Dictionary<Transform, Transform>();
        List<Transform> objectClones = new List<Transform>();
        Dictionary<Transform, Transform> cloneObjectPairs = new Dictionary<Transform, Transform>();

        List<ObjectColliderData> colliderData = new List<ObjectColliderData>();
        Dictionary<int, Mesh> collisionMeshes = new Dictionary<int, Mesh>();

        Scene previewScene;
        PhysicsScene physicsScene;

        PlaybackMode mode = PlaybackMode.Paused;

        float simulationDuration = 5f;
        bool includeChildren;

        float playbackKeyframeTime;
        int playbackKeyframe;

        int ticksPerKeyframe = 3;
        float timePerKeyframe = 0.0588235294f;
        int maxKeyframe = 85;

        Color playbackButtonIconColor = new Color(0.7686274510f, 0.7686274510f, 0.7686274510f);


        public override void OnUpdate()
        {
            switch (mode)
            {
                case PlaybackMode.Paused: return;
                case PlaybackMode.Playing:
                    playbackKeyframeTime += window.DeltaTime;

                    while (playbackKeyframeTime >= timePerKeyframe)
                    {
                        playbackKeyframeTime -= timePerKeyframe;
                        playbackKeyframe++;
                    }
                    break;
                case PlaybackMode.Reversing:
                    playbackKeyframeTime -= window.DeltaTime;

                    while (playbackKeyframeTime <= 0f)
                    {
                        playbackKeyframeTime += timePerKeyframe;
                        playbackKeyframe--;
                        if (playbackKeyframe < 0)
                        {
                            playbackKeyframeTime = 0f;
                            break;
                        }
                    }
                    break;
            }


            //if (mode == PlaybackMode.Paused) return;
            //
            //playbackKeyframeTime += window.DeltaTime;
            //
            //while (playbackKeyframeTime >= timePerKeyframe)
            //{
            //    playbackKeyframeTime -= timePerKeyframe;
            //    if (mode == PlaybackMode.Playing) playbackKeyframe++;
            //    else if (mode == PlaybackMode.Reversing) playbackKeyframe--;
            //}

            if (playbackKeyframe < 0 || playbackKeyframe > maxKeyframe) PausePlayback();

            RefreshSimulationPreviewStep();
        }

        public override void OnEnter()
        {
            Selection.selectionChanged += EditorSelectionChanged;

            playbackKeyframeTime = 0f;
            playbackKeyframe = 0;
            CreateGUI();
        }

        public override void OnExit()
        {
            Selection.selectionChanged -= EditorSelectionChanged;

            ClosePhysicsScene();
            selectionTransformsList.Clear();
        }

        void EditorSelectionChanged() => UpdateSelectedObjects();

        void UpdateSelectedObjects()
        {
            Transform[] selectionTransforms = Selection.transforms;
            selectionTransformsList.Clear();
            selectionTransformAvailabilities.Clear();

            foreach (Transform transform in selectionTransforms)
            {
                if (includeChildren)
                {
                    CheckAvailabilityForTransform(transform);
                }
                else
                {
                    SetAvailabilityForTransform(transform);
                }
            }

            selectionTransformsList = selectionTransformsList.OrderBy(t => selectionTransformAvailabilities[t])
                .ThenBy(t => t.name)
                .ToList();

            selectedObjectsPreviewScrollview.Clear();

            selectedObjectsLabel.text = $"({selectionTransformsList.Count}) Selected Objects, Include Children:";
            foreach (Transform t in selectionTransformsList)
            {
                Label selectionInfoLabel = new Label(t.name);

                Color color;
                if (selectionTransformAvailabilities.TryGetValue(t, out ObjectAvailability availability)) color = GetColorForObjectAvailability(availability);
                else color = Color.black;

                selectionInfoLabel.style.color = color;

                selectedObjectsPreviewScrollview.Add(selectionInfoLabel);
            }



            if (simulateButton != null && previewScene.IsValid() && selectionTransformsList.Count > 0)
            {
                ObjectAvailability objectAvailability = GetAvailabilityForTransform(selectionTransformsList[0]);
                simulateButton.SetActive(objectAvailability != ObjectAvailability.Invalid);
            }


            void SetAvailabilityForTransform(Transform t)
            {
                selectionTransformAvailabilities[t] = GetAvailabilityForTransform(t);
                selectionTransformsList.Add(t);
            }

            void CheckAvailabilityForTransform(Transform transform)
            {
                SetAvailabilityForTransform(transform);

                foreach (Transform t in transform)
                {
                    if (t.gameObject.activeSelf) CheckAvailabilityForTransform(t);
                }
            }


            Color GetColorForObjectAvailability(ObjectAvailability availability) => availability switch
            {
                ObjectAvailability.Valid => Color.green,
                ObjectAvailability.MissingRigidbody => Color.yellow,
                ObjectAvailability.Invalid => Color.red,
                _ => Color.black,
            };
        }

        void CreateGUI()
        {
            HKToolsUtils.CreateTitle(page, this);

            DrawSelectedObjectsGUI();
            DrawDurationConfigGUI();
            DrawTimelineGUI();


            void DrawSelectedObjectsGUI()
            {
                Transform[] selectionTransforms = Selection.transforms;

                VisualElement line = HKToolsUtils.CreateVisualElement(page);
                line.style.marginTop = 10;
                line.style.flexDirection = FlexDirection.Row;
                selectedObjectsLabel = new Label($"({selectionTransforms.Length}) Selected Objects, Include Children:");
                selectedObjectsLabel.style.paddingLeft = 10;
                selectedObjectsLabel.style.paddingBottom = 5;
                line.Add(selectedObjectsLabel);

                Toggle toggle = HKToolsUtils.CreateToggle(line, (callback) => SetIncludeChildren(callback.newValue));
                toggle.SetValueWithoutNotify(includeChildren);

                selectedObjectsPreviewScrollview = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
                selectedObjectsPreviewScrollview.style.minHeight = 0f;
                selectedObjectsPreviewScrollview.style.maxHeight = HKToolsUtils.GetStyleLengthForPercentage(50f);

                selectedObjectsPreviewScrollview.style.backgroundColor = new Color(0.19f, 0.19f, 0.19f);
                selectedObjectsPreviewScrollview.style.paddingLeft = 10;
                page.Add(selectedObjectsPreviewScrollview);

                UpdateSelectedObjects();
            }
            void DrawDurationConfigGUI()
            {
                VisualElement holder = HKToolsUtils.CreateVisualElement(page);
                holder.style.flexDirection = FlexDirection.Row;
                holder.style.paddingTop = 10;
                holder.style.paddingBottom = 10;

                simulationDurationFloatField = HKToolsUtils.CreateFloatField(holder, (ChangeEvent<float> callback) => SetSimulationLength(callback.newValue), "Duration / Ticks per keyframe", 16f);
                simulationDurationFloatField.SetValueWithoutNotify(simulationDuration);
                simulationDurationFloatField.style.width = HKToolsUtils.GetStyleLengthForPercentage(40f);

                ticksPerKeyframeIntField = HKToolsUtils.CreateIntegerField(holder, (callback) => SetTicksPerKeyframe(callback.newValue), fontSize: 16f);
                ticksPerKeyframeIntField.SetValueWithoutNotify(ticksPerKeyframe);
                ticksPerKeyframeIntField.style.width = HKToolsUtils.GetStyleLengthForPercentage(10f);

                createClosePhysicsSceneButton = HKToolsUtils.CreateButton(holder, () => CreateClosePhysicsScene(), "Create Scene", 16f);
                createClosePhysicsSceneButton.style.width = HKToolsUtils.GetStyleLengthForPercentage(50f);

                page.Add(holder);

                simulateButton = HKToolsUtils.CreateButton(page, Simulate, "Simulate", 18f);
                simulateButton.SetActive(false);
                //simulateButton.style.display = DisplayStyle.None;
                applySimulationButton = HKToolsUtils.CreateButton(page, ApplySimulation, "Apply Simulation", 18f);
                applySimulationButton.SetActive(false);
                //applySimulationButton.style.display = DisplayStyle.None;

            }
            void DrawTimelineGUI()
            {
                // Background
                VisualElement background = HKToolsUtils.CreateVisualElement(page);
                background.style.backgroundColor = new Color(0.1882352941f, 0.1882352941f, 0.1882352941f);
                background.style.width = HKToolsUtils.GetStyleLengthForPercentage(100f);
                background.style.position = Position.Absolute;
                background.style.bottom = 0;
                background.style.right = 0;

                background.style.paddingTop = 5;
                background.style.paddingBottom = 5;
                background.style.paddingLeft = 5;
                background.style.paddingRight = 5;

                // Duration
                simulationDurationLabel = HKToolsUtils.CreateLabel(background, $"n/a", 14f);
                simulationDurationLabel.style.position = Position.Absolute;
                simulationDurationLabel.style.bottom = 12;
                simulationDurationLabel.style.left = 46;

                UpdateSimulationDurationText();

                // Title
                Label timelineLabel = HKToolsUtils.CreateLabel(background, "Timeline", 16);
                timelineLabel.style.alignSelf = Align.Center;

                // Slider
                VisualElement sliderRow = HKToolsUtils.CreateVisualElement(background);
                sliderRow.style.flexDirection = FlexDirection.Row;

                CreatePlaybackButton(sliderRow, MoveToStart, "HK Icons/HK_Sim_ToStart_Icon");

                timelineSlider = new Slider(0f, 1f);
                timelineSlider.style.flexGrow = 1f;
                timelineSlider.RegisterValueChangedCallback(ScrubTimeline);
                sliderRow.Add(timelineSlider);
                CreatePlaybackButton(sliderRow, MoveToEnd, "HK Icons/HK_Sim_ToEnd_Icon");

                // Buttons
                VisualElement buttonRow = HKToolsUtils.CreateVisualElement(background);
                buttonRow.style.flexDirection = FlexDirection.Row;
                buttonRow.style.alignItems = Align.Center;
                buttonRow.style.alignSelf = Align.Center;


                CreatePlaybackButton(buttonRow, MoveBackTick, "HK Icons/HK_Sim_BackTick_Icon");
                CreatePlaybackButton(buttonRow, ReversePlayback, "HK Icons/HK_Sim_Reverse_Icon");
                CreatePlaybackButton(buttonRow, PausePlayback, "HK Icons/HK_Sim_Pause_Icon");
                CreatePlaybackButton(buttonRow, ContinuePlayback, "HK Icons/HK_Sim_Play_Icon");
                CreatePlaybackButton(buttonRow, MoveForwardTick, "HK Icons/HK_Sim_ForwardTick_Icon");



                void CreatePlaybackButton(VisualElement parent, Action onClick, string iconPath)
                {
                    Button button = HKToolsUtils.CreateButton(parent, onClick);
                    button.style.width = 36f;
                    button.style.height = 22f;
                    //button.style.marginLeft = 0f;
                    button.style.marginRight = -1f;

                    Image buttonImage = new Image();
                    buttonImage.sprite = Resources.Load<Sprite>(iconPath);
                    buttonImage.tintColor = playbackButtonIconColor;
                    button.Add(buttonImage);
                }
            }
        }

        void CreateClosePhysicsScene()
        {
            if (previewScene.IsValid()) ClosePhysicsScene();
            else CreatePhysicsScene();
        }

        void CreatePhysicsScene()
        {
            previewScene = EditorSceneManager.NewPreviewScene();
            physicsScene = previewScene.GetPhysicsScene();

            createClosePhysicsSceneButton.text = "Cancel";
            //simulateButton.style.display = DisplayStyle.Flex;
            //simulateButton.SetActive(true);

            if (selectionTransformsList.Count > 0)
            {
                ObjectAvailability objectAvailability = GetAvailabilityForTransform(selectionTransformsList[0]);
                simulateButton.SetActive(objectAvailability != ObjectAvailability.Invalid);
            }


            UpdatePhysicsSceneColliders();
        }
        void ClosePhysicsScene()
        {
            EditorSceneManager.ClosePreviewScene(previewScene);
            createClosePhysicsSceneButton.text = "Create Scene";
            //applySimulationButton.style.display = DisplayStyle.None;
            //simulateButton.style.display = DisplayStyle.None;
            applySimulationButton.SetActive(false);
            simulateButton.SetActive(false);


            playbackKeyframe = 0;
            playbackKeyframeTime = 0f;
            RefreshSimulationPreviewStep();


            //selectionTransformsList.Clear();
            UpdateSelectedObjects();
            selectionTransformAvailabilities.Clear();
            objectKeyframedata.Clear();
            objectClones.Clear();
            cloneObjectPairs.Clear();
            colliderData.Clear();
            collisionMeshes.Clear();
        }

        void UpdatePhysicsSceneColliders()
        {
            colliderData.Clear();
            collisionMeshes.Clear();

            // Clear scene in case there'll be a separate update scene button
            if (previewScene.IsValid())
            {
                GameObject[] rootObjects = previewScene.GetRootGameObjects();

                foreach (GameObject go in rootObjects) QOL.Destroy(go);
            }


            int sceneCount = EditorSceneManager.sceneCount;
            for (int i = 0; i < sceneCount; i++)
            {
                Scene sourceScene = EditorSceneManager.GetSceneAt(i);
                GameObject[] rootObjects = sourceScene.GetRootGameObjects();

                // Gather collider data
                foreach (GameObject rootObject in rootObjects)
                {
                    AddCollidersIfValid(rootObject);
                }

                // Create colliders
                foreach (ObjectColliderData data in colliderData)
                {
                    GameObject go = new GameObject();
                    go.transform.position = data.OriginalTransform.position;
                    go.transform.localScale = data.OriginalTransform.lossyScale;
                    go.transform.rotation = data.OriginalTransform.rotation;
                    Collider createdCollider = data.Collider.CopyTo(go);

                    if (data.Collider is MeshCollider meshColl)
                    {
                        ((MeshCollider)createdCollider).sharedMesh = GetCachedMesh(meshColl);
                    }

                    EditorSceneManager.MoveGameObjectToScene(go, previewScene);


                    Mesh GetCachedMesh(MeshCollider coll)
                    {
                        if (collisionMeshes.TryGetValue(coll.sharedMesh.GetInstanceID(), out Mesh mesh)) return mesh;
                        else
                        {
                            Mesh originalMesh = coll.sharedMesh;
                            if (originalMesh == null) return null;


                            Mesh cachedMesh = new Mesh();
                            cachedMesh.vertices = originalMesh.vertices;
                            cachedMesh.triangles = originalMesh.triangles;

                            collisionMeshes[coll.sharedMesh.GetInstanceID()] = cachedMesh;
                            return cachedMesh;
                        }
                    }
                }



                void AddCollidersIfValid(GameObject go)
                {
                    if (!go.activeSelf) return;
                    if (go.TryGetComponent(out Rigidbody rb) && !rb.isKinematic) return;

                    if (go.isStatic)
                    {
                        Collider[] colliders = go.GetComponents<Collider>();
                        foreach (Collider collider in colliders)
                        {
                            if (collider.isTrigger) continue;
                            colliderData.Add(new ObjectColliderData(go.transform, collider));
                            //Debug.Log($"adding '{go.transform.name}' as collider");
                        }
                    }

                    foreach (Transform t in go.transform) AddCollidersIfValid(t.gameObject);
                }
            }
        }

        void SetIncludeChildren(bool includeChildren)
        {
            this.includeChildren = includeChildren;
            UpdateSelectedObjects();
        }

        void SetSimulationLength(float duration)
        {
            simulationDuration = Mathf.Max(0f, duration);

            simulationDurationFloatField.SetValueWithoutNotify(simulationDuration);

            UpdateSimulationDurationText();
        }

        void SetTicksPerKeyframe(int ticks)
        {
            if (ticks < 1)
            {
                ticks = 1;
                ticksPerKeyframeIntField.SetValueWithoutNotify(ticks);
            }

            ticksPerKeyframe = ticks;

            UpdateSimulationDurationText();
        }

        void Simulate()
        {
            int keyframeBefore = playbackKeyframe;
            float timeBefore = playbackKeyframeTime;
            bool saveKeyframe = false;

            if (objectKeyframedata.Count != 0)
            {
                saveKeyframe = true;

                playbackKeyframe = 0;
                playbackKeyframeTime = 0f;
                RefreshSimulationPreviewStep();
            }

            foreach (Transform clone in objectClones) QOL.Destroy(clone.gameObject);

            objectClones.Clear();
            cloneObjectPairs.Clear();

            foreach (Transform t in selectionTransformsList)
            {
                EditorUtility.SetDirty(t.gameObject);

                ObjectAvailability availability = GetAvailabilityForTransform(t);
                GameObject clone;

                switch (availability)
                {
                    case ObjectAvailability.Invalid: continue;

                    case ObjectAvailability.Valid:
                        clone = GameObject.Instantiate(t).gameObject;
                        clone.transform.position = t.position;
                        clone.transform.rotation = t.rotation;
                        clone.transform.localScale = t.lossyScale;
                        clone.GetComponent<Rigidbody>().sleepThreshold = 0f;

                        //objectClonePairs[t] = clone.transform;
                        objectClones.Add(clone.transform);
                        cloneObjectPairs[clone.transform] = t;
                        EditorSceneManager.MoveGameObjectToScene(clone, previewScene);
                        break;

                    case ObjectAvailability.MissingRigidbody:
                        clone = GameObject.Instantiate(t).gameObject;
                        clone.transform.position = t.position;
                        clone.transform.rotation = t.rotation;
                        clone.transform.localScale = t.lossyScale;
                        Rigidbody rb = clone.AddComponent<Rigidbody>();
                        rb.sleepThreshold = 0f;

                        //objectClonePairs[t] = clone.transform;
                        objectClones.Add(clone.transform);
                        cloneObjectPairs[clone.transform] = t;
                        EditorSceneManager.MoveGameObjectToScene(clone, previewScene);
                        break;
                }
            }


            int totalTicks = (int)(simulationDuration / Time.fixedDeltaTime);
            int totalKeyframes = totalTicks / ticksPerKeyframe;
            maxKeyframe = totalKeyframes + (totalTicks % ticksPerKeyframe == 0 ? 1 : 2) - 1;
            timePerKeyframe = simulationDuration / maxKeyframe;

            Dictionary<Transform, List<Vector3>> positionKeyFrames = new Dictionary<Transform, List<Vector3>>();
            Dictionary<Transform, List<Quaternion>> rotationKeyFrames = new Dictionary<Transform, List<Quaternion>>();
            foreach (Transform clone in objectClones)
            {
                positionKeyFrames[clone] = new List<Vector3>();
                rotationKeyFrames[clone] = new List<Quaternion>();
            }

            // Simulate
            SimulationMode beforeMode = Physics.simulationMode;
            Physics.simulationMode = SimulationMode.Script;

            int ticksSinceLastKeyframe = 0;
            AddKeyframes();

            for (int tick = 0; tick < totalTicks; tick++)
            {
                physicsScene.Simulate(Time.fixedDeltaTime);

                if (ticksSinceLastKeyframe >= ticksPerKeyframe)
                {
                    AddKeyframes();
                    ticksSinceLastKeyframe = 1;
                }
                else ticksSinceLastKeyframe++;
            }
            Physics.simulationMode = beforeMode;

            if (ticksSinceLastKeyframe != 0) AddKeyframes();

            foreach (Transform clone in objectClones)
            {
                objectKeyframedata[clone] = new KeyFrameInfo(positionKeyFrames[clone].ToArray(), rotationKeyFrames[clone].ToArray());
            }

            applySimulationButton.SetActive(true);

            if (saveKeyframe)
            {
                playbackKeyframeTime = timeBefore;
                playbackKeyframe = keyframeBefore;
            }
            RefreshSimulationPreviewStep();



            void AddKeyframes()
            {
                foreach (Transform clone in objectClones)
                {
                    positionKeyFrames[clone].Add(clone.position);
                    rotationKeyFrames[clone].Add(clone.rotation);
                }
            }
        }

        void ApplySimulation()
        {
            List<Transform> originalTransforms = new List<Transform>();

            foreach (Transform clone in objectClones)
            {
                Transform orig = cloneObjectPairs[clone];
                if (orig) originalTransforms.Add(orig);
            }

            int keyframeBefore = playbackKeyframe;
            float timeBefore = playbackKeyframeTime;

            playbackKeyframe = 0;
            playbackKeyframeTime = 0f;
            RefreshSimulationPreviewStep();

            Undo.RecordObjects(originalTransforms.ToArray(), "Apply HK Physics Sim");

            playbackKeyframeTime = timeBefore;
            playbackKeyframe = keyframeBefore;
            RefreshSimulationPreviewStep();

            foreach (Transform orig in originalTransforms) EditorUtility.SetDirty(orig);

            applySimulationButton.SetActive(false);

            objectKeyframedata.Clear();
        }

        void MoveToStart()
        {
            playbackKeyframeTime = 0f;
            playbackKeyframe = 0;
            PausePlayback();
            RefreshSimulationPreviewStep();
        }

        void MoveBackTick()
        {
            playbackKeyframeTime = 0f;
            playbackKeyframe--;
            PausePlayback();
            RefreshSimulationPreviewStep();
        }

        void ReversePlayback()
        {
            mode = PlaybackMode.Reversing;
        }

        void PausePlayback()
        {
            mode = PlaybackMode.Paused;
        }

        void ContinuePlayback()
        {
            mode = PlaybackMode.Playing;
        }

        void MoveForwardTick()
        {
            playbackKeyframeTime = 0f;
            playbackKeyframe++;
            PausePlayback();
            RefreshSimulationPreviewStep();
        }

        void MoveToEnd()
        {
            playbackKeyframeTime = 0f;
            playbackKeyframe = maxKeyframe;
            PausePlayback();
            RefreshSimulationPreviewStep();
        }

        void ScrubTimeline(ChangeEvent<float> callback)
        {
            float time = callback.newValue;
            playbackKeyframe = (int)(time * maxKeyframe);
            float totalTime = time * maxKeyframe * timePerKeyframe;
            playbackKeyframeTime = totalTime - playbackKeyframe * timePerKeyframe;

            PausePlayback();
            RefreshSimulationPreviewStep(false);
        }

        void RefreshSimulationPreviewStep(bool updateSlider = true)
        {
            if (objectKeyframedata.Count == 0 && selectionTransformsList.Count > 0 && previewScene.IsValid()) Simulate();


            playbackKeyframe = Mathf.Clamp(playbackKeyframe, 0, maxKeyframe);

            float value = ((float)playbackKeyframe / maxKeyframe) + (playbackKeyframeTime / timePerKeyframe / maxKeyframe);
            if (updateSlider) timelineSlider.SetValueWithoutNotify(value);


            UpdateSimulationDurationText();
            if (objectKeyframedata.Count == 0 || objectClones.Count == 0 || objectKeyframedata.Count == 0) return;

            int currentIndex = Mathf.Min(maxKeyframe, playbackKeyframe);
            int nextIndex = Mathf.Min(maxKeyframe, currentIndex + 1);
            float t = playbackKeyframeTime / timePerKeyframe;
            //Debug.Log($"scrub: from: {currentIndex} to: {nextIndex} with t: {t}. out of: {maxKeyframe}, keyframe time: {playbackKeyframeTime}");

            foreach (Transform clone in objectClones)
            {
                Transform orig = cloneObjectPairs[clone];
                if (!orig) continue;

                KeyFrameInfo keyframe = objectKeyframedata[clone];
                orig.transform.position = Vector3.Lerp(keyframe.PositionKeyframes[currentIndex], keyframe.PositionKeyframes[nextIndex], t);
                orig.transform.rotation = Quaternion.Lerp(keyframe.RotationKeyframes[currentIndex], keyframe.RotationKeyframes[nextIndex], t);
            }
        }

        void UpdateSimulationDurationText()
        {
            int totalTicks = (int)(simulationDuration / Time.fixedDeltaTime);
            int totalKeyframes = totalTicks / ticksPerKeyframe;
            maxKeyframe = totalKeyframes + (totalTicks % ticksPerKeyframe == 0 ? 1 : 2) - 1;
            timePerKeyframe = simulationDuration / maxKeyframe;

            float totalSeconds = playbackKeyframe * timePerKeyframe + playbackKeyframeTime;
            int minutes = (totalSeconds / 60f).RoundDown();
            int seconds = (totalSeconds - minutes * 60f).RoundDown();
            int milliseconds = ((totalSeconds - minutes * 60 - seconds) * 1000f).RoundDown();

            string formatString = string.Format("{0:D2}:{1:D2}:{2:D3}", minutes, seconds, milliseconds);
            simulationDurationLabel.text = $"[{playbackKeyframe}/{maxKeyframe}] {formatString}";
        }



        ObjectAvailability GetAvailabilityForTransform(Transform t)
        {
            if (t.gameObject.isStatic || !t.gameObject.activeSelf) return ObjectAvailability.Invalid;

            Rigidbody rb = t.GetComponent<Rigidbody>();
            Collider collider = t.GetComponent<Collider>();

            if (!collider) return ObjectAvailability.Invalid;
            else if (!rb) return ObjectAvailability.MissingRigidbody;
            else return ObjectAvailability.Valid;
        }

        enum ObjectAvailability
        {
            Valid,
            MissingRigidbody,
            Invalid,
        }

        enum PlaybackMode
        {
            Reversing,
            Paused,
            Playing,
        }

        struct KeyFrameInfo
        {
            public Vector3[] PositionKeyframes { get; private set; }
            public Quaternion[] RotationKeyframes { get; private set; }

            public KeyFrameInfo(Vector3[] positionKeyFrames, Quaternion[] rotationKeyframes)
            {
                PositionKeyframes = positionKeyFrames;
                RotationKeyframes = rotationKeyframes;
            }
        }

        struct ObjectColliderData
        {
            public readonly Transform OriginalTransform;
            public readonly Collider Collider;

            public ObjectColliderData(Transform transform, Collider coll)
            {
                OriginalTransform = transform;
                Collider = coll;
            }
        }
    }
}

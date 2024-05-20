using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;
using UnityEditor;

namespace HietakissaUtils
{
    [CreateAssetMenu(menuName = "HK Utils/Sound Container", fileName = "New Sound Container")]
    public class SoundContainer : ScriptableObject
    {
        [field: SerializeField] public SoundPlayMode Mode { get; private set; }

        [field: SerializeField] public AudioMixerGroup DefaultMixer { get; private set; }

        public Vector2 VolumeRange => volumeRange;
        [SerializeField][MinMaxRange(0f, 1f, true)] Vector2 volumeRange = new Vector2(1f, 1f);

        public Vector2 PitchRange => pitchRange;
        [SerializeField][MinMaxRange(-3f, 3f, true)] Vector2 pitchRange = new Vector2(1f, 1f);

        public SoundClip[] Sounds => sounds;
        [SerializeField] SoundClip[] sounds;

        List<int> shuffleIndexList = new List<int>();
        int lastIndex = -1;


#if UNITY_EDITOR
        void OnValidate()
        {
            foreach (SoundClip sound in sounds)
            {
                if (!sound._HasBeenManuallySet) sound.SetDefaults();

                sound.CalculateActualPitchAndVolume(pitchRange, volumeRange);
            }

            if (shuffleIndexList.Count != sounds.Length)
            {
                shuffleIndexList.Clear();
                for (int i = 0; i < sounds.Length; i++) shuffleIndexList.Add(i);

                if (shuffleIndexList.Count == 0) shuffleIndexList.Add(0);
                shuffleIndexList.Shuffle();
            }
        }


        [HorizontalGroup(2)]
        [SerializeField]
        [Button(nameof(Preview), "Preview", 40f)] bool _previewButton;
        [SerializeField, HideInInspector]
        [Button(nameof(StopPreview), "Stop", 40f)] bool _stopPreviewButton;

        [SerializeField]
        [Button(nameof(PreviewIndex), "Preview Selected", 40f)] bool _previewIndexButton;
        [SerializeField] [DynamicRange(nameof(sounds), DynamicRangeAttribute.DynamicRangeType.ArrayLength, true)] float _selectedClip;

        static AudioSource previewSource;

        public void Preview()
        {
            if (!previewSource) previewSource = EditorUtility.CreateGameObjectWithHideFlags("HK Sound Container Preview", HideFlags.HideAndDontSave, typeof(AudioSource)).GetComponent<AudioSource>();

            ApplyToAudioSource(previewSource);
            previewSource.Play();
        }

        public void PreviewIndex()
        {
            if (_selectedClip == -1f) return;

            if (!previewSource) previewSource = EditorUtility.CreateGameObjectWithHideFlags("HK Sound Container Preview", HideFlags.HideAndDontSave, typeof(AudioSource)).GetComponent<AudioSource>();

            SoundClip clip = sounds[_selectedClip.RoundToNearest()];
            if (clip == null) return;

            ApplyClipToAudioSource(previewSource, clip);
            previewSource.Play();
        }

        public void StopPreview()
        {
            previewSource?.Stop();
        }

        void OnEnable()
        {
            if (!previewSource) previewSource = EditorUtility.CreateGameObjectWithHideFlags("HK Sound Container Preview", HideFlags.HideAndDontSave, typeof(AudioSource)).GetComponent<AudioSource>();
        }

        void OnDisable()
        {
            DestroyImmediate(previewSource.gameObject);
        }
#endif


        public void ApplyToAudioSource(AudioSource source) => ApplyClipToAudioSource(source, sounds[GetSoundIndex()]);

        public void ApplyClipToAudioSource(AudioSource source, SoundClip soundClip)
        {
            source.outputAudioMixerGroup = soundClip.OverrideMixer ?? DefaultMixer;
            source.clip = soundClip.Clip;
            source.volume = Random.Range(soundClip.ActualVolumeRange.x, soundClip.ActualVolumeRange.y);
            source.pitch = Random.Range(soundClip.ActualPitchRange.x, soundClip.ActualPitchRange.y);
            source.spatialBlend = soundClip.SpatialBlend;
            source.loop = soundClip.Loop;
        }

        public int GetSoundIndex()
        {
            switch (Mode)
            {
                default: return 0;

                case SoundPlayMode.Random: return Random.Range(0, sounds.Length);

                case SoundPlayMode.Shuffle:

                    lastIndex++;
                    lastIndex %= shuffleIndexList.Count;
                    if (lastIndex == 0) shuffleIndexList.Shuffle();
                    return shuffleIndexList[lastIndex];

                case SoundPlayMode.Sequential:

                    lastIndex++;
                    lastIndex %= sounds.Length;
                    return lastIndex;

                case SoundPlayMode.All:

                    lastIndex++;
                    lastIndex %= sounds.Length;
                    return lastIndex;
            }
        }
    }

    [System.Serializable]
    public class SoundClip
    {
#if UNITY_EDITOR
        [SerializeField] [Tooltip("Only used for organization in the Editor.")] string name;
        [HideInInspector] public bool _HasBeenManuallySet;

        public void SetDefaults()
        {
            volumeRange = Vector2.one;
            pitchRange = Vector2.one;

            _HasBeenManuallySet = true;
        }

        public void CalculateActualPitchAndVolume(Vector2 basePitchVariation, Vector2 baseVolumeVariation)
        {
            //ActualPitchRange = (basePitchVariation + pitchRange) * 0.5f;
            //ActualVolumeRange = (baseVolumeVariation + volumeRange) * 0.5f;

            ActualPitchRange = basePitchVariation * pitchRange;
            ActualVolumeRange = baseVolumeVariation * volumeRange;
        }
#endif

        public AudioMixerGroup OverrideMixer => overrideMixer;
        [SerializeField] AudioMixerGroup overrideMixer;

        public AudioClip Clip => clip;
        [SerializeField] AudioClip clip;


        public Vector2 VolumeRange => volumeRange;
        [Space(15f)]
        [SerializeField][MinMaxRange(0f, 1f, true)] Vector2 volumeRange = new Vector2(1f, 1f);

        public Vector2 PitchRange => pitchRange;
        [SerializeField][MinMaxRange(-3f, 3f, true)] Vector2 pitchRange = new Vector2(1f, 1f);

        public float SpatialBlend => spatialBlend;
        [SerializeField][Range(0f, 1f)] float spatialBlend = 1f;

        public bool Loop => loop;
        [SerializeField] bool loop;

        public SoundContainer[] Next => next;
        [SerializeField] SoundContainer[] next;


        public Vector2 ActualVolumeRange { get; private set; }
        public Vector2 ActualPitchRange { get; private set; }
    }

    public enum SoundPlayMode
    {
        Random, // pick random clip
        Shuffle, // pick random clip from queue, randomize when reached end
        Sequential, // play clips in order
        All // play all clips
    }
}

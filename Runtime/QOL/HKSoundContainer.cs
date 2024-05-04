using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;
using UnityEditor;

namespace HietakissaUtils
{
    [CreateAssetMenu(menuName = "HK Utils/Sound Container", fileName = "New Sound Container")]
    public class HKSoundContainer : ScriptableObject
    {
        public SoundPlayMode Mode => mode;
        [SerializeField] SoundPlayMode mode;

        public AudioMixerGroup DefaultMixer => defaultMixer;
        [SerializeField] AudioMixerGroup defaultMixer;

        public Vector2 VolumeRange => volumeRange;
        [SerializeField][MinMaxRange(0f, 1f, true)] Vector2 volumeRange = new Vector2(1f, 1f);

        public Vector2 PitchRange => pitchRange;
        [SerializeField][MinMaxRange(-3f, 3f, true)] Vector2 pitchRange = new Vector2(1f, 1f);

        [field: SerializeField] public HKSoundClip[] Sounds { get; private set; }

        List<int> shuffleIndexList = new List<int>();
        int lastIndex = -1;



        void OnValidate()
        {
            foreach (HKSoundClip sound in Sounds)
            {
#if UNITY_EDITOR
                if (!sound._HasBeenManuallySet) sound.SetDefaults();
#endif

                sound.CalculateActualPitchAndVolume(pitchRange, volumeRange);
            }

            shuffleIndexList.Clear();
            for (int i = 0; i < Sounds.Length; i++) shuffleIndexList.Add(i);
            shuffleIndexList.Shuffle();
        }


#if UNITY_EDITOR

        [HorizontalGroup(2)]
        [SerializeField][Button(nameof(Preview), "Preview", 40f)] bool _previewButton;
        [SerializeField, HideInInspector][Button(nameof(StopPreview), "Stop", 40f)] bool _stopPreviewButton;

        static AudioSource previewSource;

        public void Preview()
        {
            if (!previewSource) previewSource = EditorUtility.CreateGameObjectWithHideFlags("HK Sound Container Preview", HideFlags.HideAndDontSave, typeof(AudioSource)).GetComponent<AudioSource>();

            ApplyToAudioSource(previewSource);
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


        public void ApplyToAudioSource(AudioSource source)
        {
            HKSoundClip soundClip = Sounds[GetSoundIndex()];
            source.outputAudioMixerGroup = soundClip.OverrideMixer ?? DefaultMixer;
            source.clip = soundClip.Clip;
            source.volume = Random.Range(soundClip.ActualVolumeRange.x, soundClip.ActualVolumeRange.y);
            source.pitch = Random.Range(soundClip.ActualPitchRange.x, soundClip.ActualPitchRange.y);
            source.spatialBlend = soundClip.SpatialBlend;
        }

        public int GetSoundIndex()
        {
            switch (mode)
            {
                default: return 0;

                case SoundPlayMode.Random: return Random.Range(0, Sounds.Length);

                case SoundPlayMode.Shuffle:

                    lastIndex++;
                    lastIndex %= shuffleIndexList.Count;
                    if (lastIndex == 0) shuffleIndexList.Shuffle();
                    return shuffleIndexList[lastIndex];

                case SoundPlayMode.Sequential:

                    lastIndex++;
                    lastIndex %= Sounds.Length;
                    return lastIndex;

                case SoundPlayMode.All:

                    lastIndex++;
                    lastIndex %= Sounds.Length;
                    return lastIndex;
            }
        }
    }

    [System.Serializable]
    public class HKSoundClip
    {
#if UNITY_EDITOR
        [HideInInspector] public bool _HasBeenManuallySet;

        public void SetDefaults()
        {
            volumeRange = Vector2.one;
            pitchRange = Vector2.one;

            _HasBeenManuallySet = true;
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


        public Vector2 ActualPitchRange { get; private set; }
        public Vector2 ActualVolumeRange { get; private set; }

        public void CalculateActualPitchAndVolume(Vector2 basePitchVariation, Vector2 baseVolumeVariation)
        {
            ActualPitchRange = (basePitchVariation + pitchRange) * 0.5f;
            ActualVolumeRange = (baseVolumeVariation + volumeRange) * 0.5f;
        }
    }

    public enum SoundPlayMode
    {
        Random, // pick random clip
        Shuffle, // pick random clip from queue, randomize when reached end
        Sequential, // play clips in order
        All // play all clips
    }
}

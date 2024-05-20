namespace HietakissaUtils.CameraShake
{
    using UnityEngine;

    public abstract class CameraShakeSO : ScriptableObject
    {
        public EnvelopeSO Envelope => envelope;
        [Header("Envelope")]
        [SerializeField] EnvelopeSO envelope;

        public float Intensity => intensity;
        [Header("General Shake Settings")]
        [SerializeField] float intensity = 1f;
        public Vector2 IntensityRange => intensityRange;
        [SerializeField][MinMaxRange(0f, 2f, true)] Vector2 intensityRange = new Vector2(0.9f, 1.1f);

        public float Length => length;
        [Space(7)]
        [SerializeField] float length = 1f;
        public Vector2 LengthRange => lengthRange;
        [SerializeField][MinMaxRange(0f, 2f, true)] Vector2 lengthRange = new Vector2(0.9f, 1.1f);

        public Vector3 AxisScale => axisScale;
        [SerializeField] Vector3 axisScale = Vector3.one;

        public EnvelopeSO AttenuationEnvelope => attenuationEnvelope;
        [Header("Attenuation")]
        [SerializeField] EnvelopeSO attenuationEnvelope;
        public float MaxRange => maxRange;
        [SerializeField] float maxRange;


        public float GetEnvelopeMultiplier(float t) => envelope.Evaluate(t);
        public abstract CameraShake GetShake(Attenuation attenuation, Vector3 direction, float intensity, float length);
    }

    public abstract class CameraShake
    {
        public readonly CameraShakeSO ShakeSettings;
        public readonly Attenuation Attenuation;
        public readonly float Intensity;
        public readonly float Speed;
        public float Progress;
        
        public bool IsFinished { get; protected set; }

        public CameraShake(CameraShakeSO shakeSettings, Attenuation attenuation, Vector3 direction, float intensity, float length)
        {
            ShakeSettings = shakeSettings;
            Attenuation = attenuation;
            Intensity = intensity;
            Speed = 1f / length;
        }

        public abstract Vector3 Evaluate(float deltaTime);
    }

    [System.Serializable]
    public struct Attenuation
    {
        public readonly Transform ShakeTransform;
        public readonly Vector3 ShakePosition;
        public readonly float MaxRange;

        public Attenuation(Transform shakeTransform, Vector3 shakePosition, float maxRange)
        {
            ShakeTransform = shakeTransform;
            ShakePosition = shakePosition;
            MaxRange = maxRange;
        }

        //public EnvelopeSO Envelope => envelope;
        //[SerializeField] EnvelopeSO envelope;
        //
        //public float MaxRange => maxRange;
        //[SerializeField] float maxRange;
    }
}

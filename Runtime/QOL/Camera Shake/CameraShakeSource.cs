namespace HietakissaUtils.CameraShake
{
    using UnityEngine;

    public class CameraShakeSource : MonoBehaviour
    {
        public CameraShakeSO CameraShake => cameraShake;
        [SerializeField] CameraShakeSO cameraShake;

        [SerializeField] bool playOnAwake;

        [HorizontalGroup(2)]
        [Space(7)]
        [SerializeField] bool overrideIntensity;
        [SerializeField, HideInInspector]
        [ConditionalField(nameof(overrideIntensity))] float intensity;

        [HorizontalGroup(2)]
        [SerializeField] bool overrideSpeed;
        [SerializeField, HideInInspector]
        [ConditionalField(nameof(overrideSpeed))] float speed;

        [HorizontalGroup(2)]
        [SerializeField] bool overrideRange;
        [SerializeField, HideInInspector]
        [ConditionalField(nameof(overrideRange))] float range;

        public CameraShake ShakeInstance => shakeInstance;
        CameraShake shakeInstance;

        public bool IsPlaying { get; private set; }

        public float GetMaxRange() => overrideRange ? range : cameraShake.MaxRange;
        public float GetSpeed() => overrideSpeed ? speed : 1f / cameraShake.Length;
        public float GetIntensity() => overrideIntensity ? intensity : cameraShake.Intensity;

        public Vector3 Evaluate(float deltaTime)
        {
            return shakeInstance.Evaluate(deltaTime);
        }

        public void Play(CameraShakeSO shake)
        {
            cameraShake = shake;
            shakeInstance = cameraShake?.GetShake(new Attenuation(transform, transform.position, GetMaxRange()), Vector3.zero, GetIntensity(), 1f / GetSpeed());
            Play();
        }
        public void Play()
        {
            if (!cameraShake) return;

            IsPlaying = true;
        }

        void Awake()
        {
            if (playOnAwake) Play();
        }


        //public void ShakeAt(CameraShakeSO cameraShake, Transform shakeTransform = null, Vector3 direction = new Vector3(), float intensity = -1, float length = -1, float? maxRange = null)
        //{
        //    intensity = GetIntensityForShake(cameraShake, intensity);
        //    length = GetLengthForShake(cameraShake, length);
        //
        //    float range = GetMaxRangeForShake(cameraShake, maxRange);
        //
        //    CameraShake shake = cameraShake.GetShake(new Attenuation(shakeTransform, Vector3.zero, range), direction, intensity, length);
        //    shakes.Add(shake);
        //}

        void OnEnable() => CameraShaker.Instance?.RegisterShakeSource(this);
        void OnDisable() => CameraShaker.Instance?.UnRegisterShakeSource(this);


        

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (!cameraShake && !overrideRange) return;

            Gizmos.DrawWireSphere(transform.position, GetMaxRange());
        }
#endif
    }
}

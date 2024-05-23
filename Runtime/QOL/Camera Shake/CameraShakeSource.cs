namespace HietakissaUtils.CameraShake
{
    using System.Collections.Generic;
    using UnityEngine;

    public class CameraShakeSource : MonoBehaviour
    {
        public static List<CameraShakeSource> Instances = new List<CameraShakeSource>();

        public CameraShakeSO CameraShake => cameraShake;
        [SerializeField] CameraShakeSO cameraShake;

        [SerializeField] bool playOnAwake;
        [SerializeField] bool loop;

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
        public float GetSpeed() => overrideSpeed ? speed / cameraShake.Length : 1f / cameraShake.Length;
        public float GetIntensity() => overrideIntensity ? intensity : cameraShake.Intensity;

        Vector3 offset;


        void Awake()
        {
            if (playOnAwake) Play();
        }

        void Update()
        {
            float deltaTime = Time.deltaTime * GetSpeed();
            shakeInstance.Progress = Mathf.Min(shakeInstance.Progress + deltaTime, 1f);
            offset = shakeInstance.Evaluate(deltaTime);

            if (shakeInstance.IsFinished)
            {
                if (loop) shakeInstance.Reset();
                else Stop();
            }
        }

        void OnValidate()
        {
            range = Mathf.Max(float.Epsilon, range);
            speed = Mathf.Max(float.Epsilon, speed);
            intensity = Mathf.Max(float.Epsilon, intensity);
        }

        public void Play(CameraShakeSO shake)
        {
            cameraShake = shake;
            Play();
        }
        public void Play()
        {
            if (!cameraShake) return;
            if (shakeInstance == null) shakeInstance = GetShakeInstance();
            else shakeInstance.Reset();

            IsPlaying = true;
            Instances.Add(this);
        }
        public void Stop()
        {
            IsPlaying = false;
            Instances.Remove(this);
        }

        public Vector3 Evaluate() => offset;

        CameraShake GetShakeInstance() => cameraShake.GetShake(new Attenuation(transform, transform.position, GetMaxRange()), Vector3.zero, GetIntensity(), 1f / GetSpeed());


#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (!cameraShake && !overrideRange) return;

            Gizmos.DrawWireSphere(transform.position, GetMaxRange());
        }
#endif
    }
}

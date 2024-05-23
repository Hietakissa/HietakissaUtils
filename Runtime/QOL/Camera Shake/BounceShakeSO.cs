namespace HietakissaUtils.CameraShake
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "HK Utils/Camera Shake/Shakes/Bounce Shake", fileName = "New Bounce Shake", order = 1)]
    public class BounceShakeSO : CameraShakeSO
    {
        public float Randomness => randomness;
        [Header("Bounce Settings")]
        [SerializeField]
        [Range(0f, 1f)] float randomness = 0.5f;

        public int Bounces => bounces;
        [SerializeField] int bounces = 5;


        public override CameraShake GetShake(Attenuation attenuation, Vector3 direction, float intensity, float length)
        {
            return new BounceShake(this, attenuation, direction, intensity, length);
        }
    }

    public class BounceShake : CameraShake
    {
        public BounceShake(CameraShakeSO shakeSettings, Attenuation attenuation, Vector3 direction, float intensity, float length) : base(shakeSettings, attenuation, direction, intensity, length)
        {
            shake = shakeSettings as BounceShakeSO;

            startDirection = direction;

            if (direction == Vector3.zero) this.direction = Random.insideUnitSphere;
            else this.direction = direction;
            currentOffset = this.direction;
        }

        BounceShakeSO shake;
        float t;
        int bounceIndex;
        Vector3 startDirection;
        Vector3 direction;
        Vector3 previousOffset;
        Vector3 currentOffset;

        public override Vector3 Evaluate(float deltaTime)
        {
            deltaTime *= shake.Bounces + 1;
            t += deltaTime;

            if (t <= 1f)
            {
                return Vector3.Lerp(previousOffset, currentOffset, Easing.InOutSine(t));
            }
            else
            {
                t -= 1f;
                Vector3 evaluation = currentOffset;
                previousOffset = currentOffset;

                bounceIndex++;
                if (bounceIndex > shake.Bounces)
                {
                    IsFinished = true;
                    return Vector3.zero;
                }

                Vector3 rnd = Random.insideUnitSphere;
                direction = (-direction + shake.Randomness * rnd).normalized;

                float decayValue = 1f - (float)bounceIndex / shake.Bounces;
                currentOffset = decayValue * decayValue * direction;

                return evaluation;
            }
        }

        public override void Reset()
        {
            Progress = 0f;
            t = 0f;
            IsFinished = false;

            if (startDirection == Vector3.zero) direction = Random.insideUnitSphere;
            else direction = startDirection;

            currentOffset = direction;
        }
    }
}
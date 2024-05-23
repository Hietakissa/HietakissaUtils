namespace HietakissaUtils.CameraShake
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "HK Utils/Camera Shake/Shakes/Kick Shake", fileName = "New Kick Shake", order = 2)]
    public class KickShakeSO : CameraShakeSO
    {
        public override CameraShake GetShake(Attenuation attenuation, Vector3 direction, float intensity, float length)
        {
            return new Kickshake(this, attenuation, direction, intensity, length);
        }
    }

    public class Kickshake : CameraShake
    {
        public Kickshake(CameraShakeSO shakeSettings, Attenuation attenuation, Vector3 direction, float intensity, float length) : base(shakeSettings, attenuation, direction, intensity, length)
        {
            startDirection = direction;
            if (direction == Vector3.zero) currentOffset = Random.insideUnitSphere;
            else currentOffset = direction;
        }

        Vector3 startDirection;
        Vector3 previousOffset;
        Vector3 currentOffset;

        bool returning;

        public override Vector3 Evaluate(float deltaTime)
        {
            //Progress += deltaTime;

            if (Progress >= 1f)
            {
                if (returning)
                {
                    IsFinished = true;
                    return Vector3.zero;
                }

                Progress -= 1f;
                returning = true;
                currentOffset = Vector3.zero;
            }

            return Vector3.Lerp(previousOffset, currentOffset, Progress);
        }

        public override void Reset()
        {
            Progress = 0f;
            IsFinished = false;

            if (startDirection == Vector3.zero) currentOffset = Random.insideUnitSphere;
            else currentOffset = startDirection;
        }
    }
}
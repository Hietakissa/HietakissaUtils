namespace HietakissaUtils.CameraShake
{
    using System.Collections.Generic;
    using UnityEngine;

    public class CameraShaker : MonoBehaviour
    {
        public static CameraShaker Instance;

        [SerializeField] Transform shakeTransform;
        [SerializeField] bool is2D;

        List<CameraShake> shakes = new List<CameraShake>();

        void Awake()
        {
            if (Instance) Destroy(Instance);
            else
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
        }


        void LateUpdate()
        {
            if (!shakeTransform) return;


            Vector3 totalDisplacement = Vector3.zero;
            for (int i = shakes.Count - 1; i >= 0; i--)
            {
                CameraShake shake = shakes[i];
                shake.Progress = Mathf.Min(shake.Progress + shake.Speed * Time.deltaTime, 1f);

                float attenuationMultiplier;
                if (shake.Attenuation.MaxRange == -1) attenuationMultiplier = 1f;
                else
                {
                    Vector3 attenuationPos = shake.Attenuation.ShakeTransform == null ? shake.Attenuation.ShakePosition : shake.Attenuation.ShakeTransform.position;

                    float distance = Vector3.Distance(attenuationPos, shakeTransform.position);
                    attenuationMultiplier = shake.ShakeSettings.AttenuationEnvelope.Evaluate(distance / shake.Attenuation.MaxRange);
                }

                totalDisplacement += Vector3.Scale(shake.Evaluate(Time.deltaTime * shake.Speed) * shake.Intensity * shake.ShakeSettings.GetEnvelopeMultiplier(shake.Progress) * attenuationMultiplier, shake.ShakeSettings.AxisScale);
                Debug.Log($"shake progress: {shake.Progress}");

                if (shake.IsFinished) shakes.RemoveAt(i);
            }

            //if (shakes.Count != 0) Debug.Log($"shakes: {shakes.Count}, displacement: {totalDisplacement}, intensity: {shakes[0].Intensity}");


            if (is2D)
            {
                shakeTransform.SetLocalPositionAndRotation(totalDisplacement.SetZ(0f), Quaternion.Euler(Vector3.Scale(totalDisplacement, Vector3.forward * 5f)));
                //shakeTransform.localPosition = totalDisplacement.SetZ(0f);
                //shakeTransform.localRotation = Quaternion.Euler(Vector3.Scale(totalDisplacement, Vector3.forward * 5f));
            }
            else shakeTransform.localRotation = Quaternion.Euler(new Vector3(totalDisplacement.y, totalDisplacement.x, totalDisplacement.z));
        }


        public void StopAllShakes() => shakes.Clear();
        public void Set2D(bool state) => is2D = state;
        public void SetShakeTransform(Transform shakeTransform)
        {
            this.shakeTransform?.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            this.shakeTransform = shakeTransform;
        }

        public void Shake(CameraShakeSO cameraShake, Vector3 direction = new Vector3(), float intensity = -1, float length = -1)
        {
            intensity = GetIntensityForShake(cameraShake, intensity);
            length = GetLengthForShake(cameraShake, length);

            CameraShake shake = cameraShake.GetShake(new Attenuation(null, Vector3.zero, -1), direction, intensity, length);
            shakes.Add(shake);
        }

        public void ShakeAt(CameraShakeSO cameraShake, Vector3 shakePosition, Vector3 direction = new Vector3(), float intensity = -1, float length = -1, float? maxRange = null)
        {
            intensity = GetIntensityForShake(cameraShake, intensity);
            length = GetLengthForShake(cameraShake, length);

            float range = GetMaxRangeForShake(cameraShake, maxRange);

            CameraShake shake = cameraShake.GetShake(new Attenuation(null, shakePosition, range), direction, intensity, length);
            shakes.Add(shake);
        }

        public void ShakeAt(CameraShakeSO cameraShake, Transform shakeTransform = null, Vector3 direction = new Vector3(), float intensity = -1, float length = -1, float? maxRange = null)
        {
            intensity = GetIntensityForShake(cameraShake, intensity);
            length = GetLengthForShake(cameraShake, length);

            float range = GetMaxRangeForShake(cameraShake, maxRange);

            CameraShake shake = cameraShake.GetShake(new Attenuation(shakeTransform, Vector3.zero, range), direction, intensity, length);
            shakes.Add(shake);
        }

        float GetIntensityForShake(CameraShakeSO cameraShake, float intensity)
        {
            if (intensity == -1) intensity = cameraShake.Intensity;
            intensity *= cameraShake.IntensityRange.GetRandomInRange();
            return intensity;
        }
        float GetLengthForShake(CameraShakeSO cameraShake, float length)
        {
            if (length == -1) length = cameraShake.Length;
            length *= cameraShake.LengthRange.GetRandomInRange();
            return length;
        }
        float GetMaxRangeForShake(CameraShakeSO cameraShake, float? maxRange)
        {
            if (maxRange == null) return cameraShake.MaxRange;
            else return maxRange.Value;
        }
    }
}
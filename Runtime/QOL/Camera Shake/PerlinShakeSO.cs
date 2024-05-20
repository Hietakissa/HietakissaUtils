namespace HietakissaUtils.CameraShake
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "HK Utils/Camera Shake/Shakes/Perlin Shake", fileName = "New Perlin Shake")]
    public class PerlinShakeSO : CameraShakeSO
    {
        public int Octaves => octaves;
        [Header("Perlin Settings")]
        [Tooltip("How many layers of noise to sample.")]
        [SerializeField][Range(1, 10)] int octaves = 2;

        public float Frequency => frequency;
        [Space(7)]
        [SerializeField] float frequency = 10f;

        public float Persistence => persistence;
        [Tooltip("How the frequency changes for each layer.")]
        [SerializeField][Range(0f, 1f)] float persistence = 0.85f;

        public float Magnitude => magnitude;
        [Space(7)]
        [SerializeField] float magnitude = 0.5f;

        public float Lacunarity => lacunarity;
        [Tooltip("How the magnitude changes for each layer.")]
        [SerializeField][Range(1f, 5f)] float lacunarity = 1.45f;


        public override CameraShake GetShake(Attenuation attenuation, Vector3 direction, float intensity, float length)
        {
            return new PerlinShake(this, attenuation, direction, intensity, length);
        }
    }

    class PerlinShake : CameraShake
    {
        public PerlinShake(CameraShakeSO shakeSettings, Attenuation attenuation, Vector3 direction, float intensity, float length) : base(shakeSettings, attenuation, direction, intensity, length)
        {
            shake = shakeSettings as PerlinShakeSO;
            offsets = new Vector2[shake.Octaves];

            for (int i = 0; i < offsets.Length; i++)
            {
                offsets[i] = Random.insideUnitCircle * 10f * shake.Magnitude;
            }
        }

        PerlinShakeSO shake;
        Vector2[] offsets;

        public override Vector3 Evaluate(float deltaTime)
        {
            Vector3 totalNoise = Vector3.zero;
            float magnitude = shake.Magnitude, frequency = shake.Frequency;

            for (int i = 0; i < shake.Octaves; i++)
            {
                totalNoise += SampleNoise(offsets[i], magnitude, frequency);

                if (i < shake.Octaves - 1)
                {
                    magnitude *= shake.Lacunarity;
                    frequency *= shake.Persistence;
                }
            }

            if (Progress >= 1f) IsFinished = true;
            return totalNoise / shake.Octaves;
        }

        Vector3 SampleNoise(Vector2 offset, float magnitude, float frequency)
        {
            Vector3 noiseVector = new Vector3(
                Mathf.PerlinNoise(offset.x + Time.time * frequency, offset.y),
                Mathf.PerlinNoise(offset.x, offset.y + Time.time * frequency),
                Mathf.PerlinNoise(offset.x + Time.time * frequency, offset.y + Time.time * frequency));

            noiseVector = (noiseVector - new Vector3(0.5f, 0.5f, 0.5f)) * magnitude; // Axis go from 0 > 1 to -0.5 > 0.5
            return noiseVector;
        }
    }
}
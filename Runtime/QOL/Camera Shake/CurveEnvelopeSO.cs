namespace HietakissaUtils.CameraShake
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "HK Utils/Camera Shake/Curve Envelope", fileName = "New Curve Envelope", order = 2)]
    public class CurveEnvelopeSO : EnvelopeSO
    {
        public AnimationCurve EnvelopeCurve => envelopeCurve;
        [SerializeField] AnimationCurve envelopeCurve;

        public override float Evaluate(float t) => envelopeCurve.Evaluate(t);
    }
}
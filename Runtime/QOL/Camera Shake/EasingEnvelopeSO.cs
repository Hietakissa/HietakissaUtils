namespace HietakissaUtils.CameraShake
{
    using UnityEngine;

    [CreateAssetMenu(menuName = "HK Utils/Camera Shake/Easing Envelope", fileName = "New Easing Envelope")]
    public class EasingEnvelopeSO : EnvelopeSO
    {
        [SerializeField] Easing.EasingMode mode;

        [HorizontalGroup(2)]
        [SerializeField] bool invertX;
        [SerializeField, HideInInspector] bool invertY;

        public override float Evaluate(float t)
        {
            t = Mathf.Clamp01(t);
            if (invertX) t = 1f - t;

            float result = mode switch
            {
                Easing.EasingMode.Linear => t,
                Easing.EasingMode.EaseInSine => Easing.InSine(t),
                Easing.EasingMode.EaseOutSine => Easing.OutSine(t),
                Easing.EasingMode.EaseInOutSine => Easing.InOutSine(t),
                Easing.EasingMode.EaseInQuad => Easing.InQuad(t),
                Easing.EasingMode.EaseOutQuad => Easing.OutQuad(t),
                Easing.EasingMode.EaseInOutQuad => Easing.InOutQuad(t),
                Easing.EasingMode.EaseInCubic => Easing.InCubic(t),
                Easing.EasingMode.EaseOutCubic => Easing.OutCubic(t),
                Easing.EasingMode.EaseInOutCubic => Easing.InOutCubic(t),
                Easing.EasingMode.EaseInQuart => Easing.InQuart(t),
                Easing.EasingMode.EaseOutQuart => Easing.OutQuart(t),
                Easing.EasingMode.EaseInOutQuart => Easing.InOutQuart(t),
                Easing.EasingMode.EaseInQuint => Easing.InQuint(t),
                Easing.EasingMode.EaseOutQuint => Easing.OutQuint(t),
                Easing.EasingMode.EaseInOutQuint => Easing.InOutQuint(t),
                Easing.EasingMode.EaseInCirc => Easing.InCirc(t),
                Easing.EasingMode.EaseOutCirc => Easing.OutCirc(t),
                Easing.EasingMode.EaseInOutCirc => Easing.InOutCirc(t),
                Easing.EasingMode.EaseInExpo => Easing.InExpo(t),
                Easing.EasingMode.EaseOutExpo => Easing.OutExpo(t),
                Easing.EasingMode.EaseInOutExpo => Easing.InOutExpo(t),
                Easing.EasingMode.EaseInBack => Easing.InBack(t),
                Easing.EasingMode.EaseOutBack => Easing.OutBack(t),
                Easing.EasingMode.EaseInOutBack => Easing.InOutBack(t),
                Easing.EasingMode.EaseInElastic => Easing.InElastic(t),
                Easing.EasingMode.EaseOutElastic => Easing.OutElastic(t),
                Easing.EasingMode.EaseInOutElastic => Easing.InOutElastic(t),
                Easing.EasingMode.EaseInBounce => Easing.InBounce(t),
                Easing.EasingMode.EaseOutBounce => Easing.OutBounce(t),
                Easing.EasingMode.EaseInOutBounce => Easing.InOutBounce(t),
                _ => t
            };

            if (invertY) return 1f - result;
            else return result;
        }
    }
}

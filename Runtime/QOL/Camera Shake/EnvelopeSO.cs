namespace HietakissaUtils.CameraShake
{
    using UnityEngine;

    public abstract class EnvelopeSO : ScriptableObject
    {
        public abstract float Evaluate(float t);
    }
}
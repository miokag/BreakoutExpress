using UnityEngine;

namespace BreakoutExpress
{
    [System.Serializable]
    public class PlayerEffect
    {
        public enum EffectType
        {
            PushBack,
            Slow
        }

        public EffectType type;
        public float duration;
        public float magnitude;
        public Vector3 direction;
    }
}
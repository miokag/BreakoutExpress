using UnityEngine;

namespace BreakoutExpress
{
    [System.Serializable]
    public class PlayerEffect
    {
        public enum EffectType
        {
            Slow
        }

        public EffectType type;
        public float duration;
        public float magnitude;
    }
}
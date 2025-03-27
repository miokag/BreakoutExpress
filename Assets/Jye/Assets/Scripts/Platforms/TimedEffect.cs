using UnityEngine;

namespace BreakoutExpress
{
    [System.Serializable]
    public class TimedEffect
    {
        public float activeDuration = 3f;
        public float inactiveDuration = 2f;
        public float initialDelay = 0f;
        
        [HideInInspector] public float nextToggleTime;
        [HideInInspector] public bool isActive;
        
        public void Initialize()
        {
            nextToggleTime = Time.time + initialDelay;
            isActive = true;
        }
        
        public bool CheckShouldToggle()
        {
            if (Time.time >= nextToggleTime)
            {
                isActive = !isActive;
                nextToggleTime = Time.time + (isActive ? activeDuration : inactiveDuration);
                return true;
            }
            return false;
        }
    }
}
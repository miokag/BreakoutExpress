using UnityEngine;

namespace BreakoutExpress
{
    public class PlayerEffects : MonoBehaviour
    {
        private PlayerController playerController;
        private float originalWalkSpeed;
        private float originalRunSpeed;
        private float effectEndTime;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            originalWalkSpeed = playerController.WalkSpeed;
            originalRunSpeed = playerController.RunSpeed;
        }

        public void ApplyEffect(PlayerEffect effect)
        {
            switch (effect.type)
            {
                case PlayerEffect.EffectType.PushBack:
                    ApplyPushBack(effect);
                    break;
                case PlayerEffect.EffectType.Slow:
                    ApplySlow(effect);
                    break;
            }

            effectEndTime = Time.time + effect.duration;
        }

        private void ApplyPushBack(PlayerEffect effect)
        {
            // Apply an instant force
            playerController.AddForce(effect.direction.normalized * effect.magnitude);
        }

        private void ApplySlow(PlayerEffect effect)
        {
            // Reduce movement speeds
            playerController.WalkSpeed = originalWalkSpeed * (1f - effect.magnitude);
            playerController.RunSpeed = originalRunSpeed * (1f - effect.magnitude);
        }
        
        public void CancelEffect(PlayerEffect.EffectType type)
        {
            switch (type)
            {
                case PlayerEffect.EffectType.Slow:
                    // Immediately reset movement speeds
                    playerController.WalkSpeed = originalWalkSpeed;
                    playerController.RunSpeed = originalRunSpeed;
                    break;
            
                case PlayerEffect.EffectType.PushBack:
                    // Push effects are instantaneous so nothing to cancel
                    break;
            }
    
            // Optional: Reset the effect timer if you want
            effectEndTime = Time.time;
        }

        private void Update()
        {
            if (Time.time >= effectEndTime)
            {
                ResetEffects();
            }
        }

        private void ResetEffects()
        {
            playerController.WalkSpeed = originalWalkSpeed;
            playerController.RunSpeed = originalRunSpeed;
        }
    }
}
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
                case PlayerEffect.EffectType.Slow:
                    ApplySlow(effect);
                    break;
                case PlayerEffect.EffectType.SpeedBoost:
                    ApplySpeedBoost(effect);
                    break;
            }
            effectEndTime = Time.time + effect.duration;
        }

        private void ApplySlow(PlayerEffect effect)
        {
            playerController.WalkSpeed = originalWalkSpeed * (1f - effect.magnitude);
            playerController.RunSpeed = originalRunSpeed * (1f - effect.magnitude);
        }

        private void ApplySpeedBoost(PlayerEffect effect)
        {
            playerController.WalkSpeed = originalWalkSpeed + effect.magnitude;
            playerController.RunSpeed = originalRunSpeed + effect.magnitude;
        }
        
        public void CancelEffect(PlayerEffect.EffectType type)
        {
            switch (type)
            {
                case PlayerEffect.EffectType.Slow:
                case PlayerEffect.EffectType.SpeedBoost:
                    playerController.WalkSpeed = originalWalkSpeed;
                    playerController.RunSpeed = originalRunSpeed;
                    break;
            }
            
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
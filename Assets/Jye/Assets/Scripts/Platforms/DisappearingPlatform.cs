using UnityEngine;

namespace BreakoutExpress
{
    public class DisappearingPlatform : MonoBehaviour
    {
        [SerializeField] private TimedEffect disappearEffect;
        [SerializeField] private Collider platformCollider;
        [SerializeField] private MeshRenderer platformRenderer;

        private void Start()
        {
            if (platformCollider == null) platformCollider = GetComponent<Collider>();
            if (platformRenderer == null) platformRenderer = GetComponent<MeshRenderer>();
            
            disappearEffect.Initialize();
        }

        private void Update()
        {
            if (disappearEffect.CheckShouldToggle())
            {
                platformCollider.enabled = disappearEffect.isActive;
                platformRenderer.enabled = disappearEffect.isActive;
            }
        }
    }
}
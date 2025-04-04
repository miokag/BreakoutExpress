using UnityEngine;

public class TrainTrigger : MonoBehaviour
{
    [SerializeField] private TicketTaker3D ghostToActivate;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ghostToActivate.ActivateTicketTaker();
            gameObject.SetActive(false);
        }
    }
}
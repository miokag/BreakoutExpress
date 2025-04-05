using UnityEngine;

public class TrainTrigger : MonoBehaviour
{
    [SerializeField] private TicketTaker3D ghostToActivate;
    [SerializeField] private TicketTaker2D ghostToActivate2D;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ghostToActivate.ActivateTicketTaker();
            gameObject.SetActive(false);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ghostToActivate2D.ActivateTicketTaker();
            gameObject.SetActive(false);
        }
    }
}
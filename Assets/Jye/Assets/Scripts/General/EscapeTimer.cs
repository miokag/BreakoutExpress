using UnityEngine;
using TMPro;

namespace BreakoutExpress
{
    public class EscapeTimer : MonoBehaviour
    {
        [Header("Timer Settings")]
        [SerializeField] private float initialTime = 60f;
        [SerializeField] private float warningThreshold = 15f; 
        [SerializeField] private TextMeshProUGUI timerText;

        private float currentTime;
        private bool timerActive = true;

        void Start()
        {
            currentTime = initialTime;
            UpdateTimerDisplay();
        }

        void Update()
        {
            if (!timerActive) return;

            currentTime -= Time.deltaTime;
            UpdateTimerDisplay();

            if (currentTime <= 0)
            {
                TimeUp();
            }
        }

        void UpdateTimerDisplay()
        {
            // Format as 1:23
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = $"{minutes}:{seconds:00}";

            // Change color when time is low
            timerText.color = currentTime <= warningThreshold ? Color.red : Color.white;
        }

        public void ModifyTime(float seconds)
        {
            currentTime = Mathf.Max(1f, currentTime + seconds); // Never goes below 1
        }

        public void PlayerEscaped()
        {
            timerActive = false;
            timerText.text = "ESCAPED!";
            timerText.color = Color.green;
        }

        void TimeUp()
        {
            timerActive = false;
            timerText.text = "TIME UP!";
        }
    }
}
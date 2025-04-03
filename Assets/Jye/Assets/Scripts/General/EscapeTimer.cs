using UnityEngine;
using TMPro;
using System.Collections;

namespace BreakoutExpress
{
    public class EscapeTimer : MonoBehaviour
    {
        [Header("Timer Settings")]
        [SerializeField] private float initialTime = 60f;
        [SerializeField] private float warningThreshold = 15f; 
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private float flashDuration = 0.5f; // Duration of color flash

        private float currentTime;
        private bool timerActive = true;
        private Color defaultColor = Color.white;

        void Start()
        {
            currentTime = initialTime;
            defaultColor = timerText.color;
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
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = $"{minutes}:{seconds:00}";
            
            // Only change to red if not already flashing
            if (timerText.color == defaultColor)
            {
                timerText.color = currentTime <= warningThreshold ? Color.red : defaultColor;
            }
        }

        public void ModifyTime(float seconds)
        {
            currentTime = Mathf.Max(1f, currentTime + seconds);
            UpdateTimerDisplay();
            
            // Flash color based on time change
            StartCoroutine(FlashTimerText(seconds > 0 ? Color.green : Color.red));
        }

        private IEnumerator FlashTimerText(Color flashColor)
        {
            Color originalColor = timerText.color;
            timerText.color = flashColor;
            
            yield return new WaitForSeconds(flashDuration);
            
            // Return to appropriate color (warning red or default)
            timerText.color = currentTime <= warningThreshold ? Color.red : defaultColor;
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
            timerText.color = Color.red;
        }
    }
}
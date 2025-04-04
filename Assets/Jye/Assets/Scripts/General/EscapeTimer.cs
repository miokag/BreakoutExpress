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
        [SerializeField] private float flashDuration = 0.5f;

        private float currentTime;
        private bool timerActive = true;
        private Color defaultColor = Color.white;

        void Start()
        {
            if (timerText == null)
            {
                Debug.LogError("Timer Text reference is missing!");
                enabled = false;
                return;
            }

            currentTime = initialTime;
            defaultColor = timerText.color;
            UpdateTimerDisplay();
        }

        void Update()
        {
            if (!timerActive || timerText == null) return;

            currentTime -= Time.deltaTime;
            UpdateTimerDisplay();

            if (currentTime <= 0)
            {
                TimeUp();
            }
        }

        void UpdateTimerDisplay()
        {
            if (timerText == null) return;

            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = $"{minutes}:{seconds:00}";
            
            if (timerText.color == defaultColor)
            {
                timerText.color = currentTime <= warningThreshold ? Color.red : defaultColor;
            }
        }

        public void ModifyTime(float seconds)
        {
            if (timerText == null) return;

            currentTime = Mathf.Max(1f, currentTime + seconds);
            UpdateTimerDisplay();
            
            StartCoroutine(FlashTimerText(seconds > 0 ? Color.green : Color.red));
        }

        private IEnumerator FlashTimerText(Color flashColor)
        {
            if (timerText == null) yield break;

            Color originalColor = timerText.color;
            timerText.color = flashColor;
            
            yield return new WaitForSeconds(flashDuration);
            
            if (timerText != null)
            {
                timerText.color = currentTime <= warningThreshold ? Color.red : defaultColor;
            }
        }

        public void PlayerEscaped()
        {
            timerActive = false;
            if (timerText != null)
            {
                timerText.color = Color.green;
            }
        }

        void TimeUp()
        {
            timerActive = false;
            if (timerText != null)
            {
                timerText.color = Color.red;
                timerText.text = "0:00";
            }
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
            else
            {
                Debug.LogWarning("GameManager instance not found!");
            }
        }
    }
}
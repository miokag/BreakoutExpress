using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class TicketTaker2D : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float heightAboveGround = 0.5f;

    [Header("Activation Settings")]
    [SerializeField] private string trainTriggerTag = "TrainTrigger";
    private bool isActivated = false;
    private bool isPaused = false;

    [Header("Visual Settings")]
    [SerializeField] private bool startInvisible = true;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Collision Settings")]
    [SerializeField] private string[] collidableTags = { "Player" };
    [SerializeField] private Collider2D[] colliders;
    
    [Header("Sound Settings")]
    [SerializeField] private AudioClip activationSound;
    [SerializeField] private AudioClip movementSound;
    [SerializeField] private float movementSoundInterval = 0.5f;
    [SerializeField] private float movementSoundVolume = 0.7f;
    [SerializeField] private float activationSoundVolume = 1f;
    [SerializeField] private AudioMixerGroup soundMixerGroup;
    [SerializeField] private float soundMaxDistance = 15f;
    [SerializeField] private float soundSpatialBlend = 0.8f;
    [SerializeField] private float muffledVolume = 0.2f;
    [SerializeField] private float soundTransitionSpeed = 2f;

    private AudioSource activationAudioSource;
    private AudioSource movementAudioSource;
    private float movementSoundTimer;
    private bool isPlayingMovementSound;
    private float targetMovementVolume;
    private Coroutine volumeAdjustCoroutine;

    private Transform player;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        activationAudioSource = CreateAudioSource("ActivationSound", activationSoundVolume, false);
        movementAudioSource = CreateAudioSource("MovementSound", movementSoundVolume, true);
        targetMovementVolume = movementSoundVolume;
    }

    private AudioSource CreateAudioSource(string name, float volume, bool loop)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.name = name;
        source.outputAudioMixerGroup = soundMixerGroup;
        source.spatialBlend = soundSpatialBlend;
        source.maxDistance = soundMaxDistance;
        source.volume = volume;
        source.loop = loop;
        source.playOnAwake = false;
        return source;
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        SetVisibility(!startInvisible);
        SetCollision(false);
    }

    public void ActivateTicketTaker()
    {
        if (!isActivated)
        {
            isActivated = true;
            SetVisibility(true);
            SetCollision(true);
            
            if (activationSound != null)
            {
                activationAudioSource.clip = activationSound;
                activationAudioSource.Play();
            }
            
            StartMovementSound();
        }
    }

    public void PauseMovement(bool pause)
    {
        isPaused = pause;
        
        if (pause)
        {
            // Store current velocity before pausing
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.isKinematic = true; // This will stop all physics interactions
            }
            
            StopMovementSound();
            Debug.Log(gameObject.name + " movement paused");
        }
        else if (isActivated)
        {
            if (rb != null)
            {
                rb.isKinematic = false; // Re-enable physics
            }
            
            StartMovementSound();
            Debug.Log(gameObject.name + " movement resumed");
        }
    }

    private void StartMovementSound()
    {
        if (movementSound != null)
        {
            if (!movementAudioSource.isPlaying)
            {
                movementAudioSource.clip = movementSound;
                movementAudioSource.Play();
            }
            
            targetMovementVolume = movementSoundVolume;
            AdjustMovementVolume();
            isPlayingMovementSound = true;
        }
    }

    private void StopMovementSound()
    {
        if (isPlayingMovementSound)
        {
            targetMovementVolume = muffledVolume;
            AdjustMovementVolume();
            isPlayingMovementSound = false;
        }
    }
    
    private void AdjustMovementVolume()
    {
        if (volumeAdjustCoroutine != null)
        {
            StopCoroutine(volumeAdjustCoroutine);
        }
        
        volumeAdjustCoroutine = StartCoroutine(AdjustVolumeSmoothly());
    }
    
    private IEnumerator AdjustVolumeSmoothly()
    {
        float startVolume = movementAudioSource.volume;
        float timer = 0f;
        
        while (Mathf.Abs(movementAudioSource.volume - targetMovementVolume) > 0.01f)
        {
            timer += Time.deltaTime * soundTransitionSpeed;
            movementAudioSource.volume = Mathf.Lerp(startVolume, targetMovementVolume, timer);
            yield return null;
        }
        
        movementAudioSource.volume = targetMovementVolume;
        volumeAdjustCoroutine = null;
    }

    private void FixedUpdate()
    {
        if (!isActivated || isPaused || player == null)
        {
            if (isPaused && rb != null)
            {
                rb.linearVelocity = Vector2.zero; // Ensure velocity stays zero while paused
            }
            return;
        }

        Vector2 direction = (player.position - transform.position).normalized;
        if (rb != null)
        {
            rb.linearVelocity = direction * moveSpeed;
        }

        HandleMovementSound();
    }

    private void HandleMovementSound()
    {
        if (!isPlayingMovementSound || movementSound == null) return;
        
        movementSoundTimer -= Time.deltaTime;
        if (movementSoundTimer <= 0f)
        {
            movementSoundTimer = movementSoundInterval + Random.Range(-0.1f, 0.1f);
            
            if (!movementAudioSource.isPlaying)
            {
                movementAudioSource.Play();
            }
        }
    }

    private void SetVisibility(bool visible)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = visible;
        }
    }
    
    private void SetCollision(bool enabled)
    {
        if (colliders != null)
        {
            foreach (Collider2D collider in colliders)
            {
                collider.enabled = enabled;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isActivated || isPaused) return;

        foreach (string tag in collidableTags)
        {
            if (collision.gameObject.CompareTag(tag))
            {
                StopMovementSound();
                GameManager.Instance.GameOver();
                break;
            }
        }
    }
    
    private void OnDestroy()
    {
        if (volumeAdjustCoroutine != null)
        {
            StopCoroutine(volumeAdjustCoroutine);
        }
    }
}
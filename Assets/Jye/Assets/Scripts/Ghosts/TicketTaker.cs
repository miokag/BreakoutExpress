using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class TicketTaker3D : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float heightAboveGround = 0.5f;
    [SerializeField] private Vector3 modelForwardAxis = Vector3.right;

    [Header("Activation Settings")]
    [SerializeField] private string trainTriggerTag = "TrainTrigger";
    private bool isActivated = false;
    private bool isPaused = false;

    [Header("Visual Settings")]
    [SerializeField] private bool startInvisible = true;
    [SerializeField] private Renderer[] modelRenderers;

    [Header("Collision Settings")]
    [SerializeField] private string[] collidableTags = { "Player", "Wall", "Obstacle" };
    [SerializeField] private Collider[] colliders;
    
    [Header("Sound Settings")]
    [SerializeField] private AudioClip activationSound;
    [SerializeField] private AudioClip movementSound;
    [SerializeField] private float movementSoundInterval = 0.5f;
    [SerializeField] private float movementSoundVolume = 0.7f;
    [SerializeField] private float activationSoundVolume = 1f;
    [SerializeField] private AudioMixerGroup soundMixerGroup;
    [SerializeField] private float soundMaxDistance = 15f;
    [SerializeField] private float soundSpatialBlend = 0.8f; // 0=2D, 1=3D
    [SerializeField] private float muffledVolume = 0.2f;
    [SerializeField] private float soundTransitionSpeed = 2f;

    private AudioSource activationAudioSource;
    private AudioSource movementAudioSource;
    private float movementSoundTimer;
    private bool isPlayingMovementSound;
    private float targetMovementVolume;
    private Coroutine volumeAdjustCoroutine;

    private Transform player;
    private Rigidbody rb;
    private Quaternion forwardRotationOffset;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody>();
        
        if (rb != null)
        {
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
        
        activationAudioSource = CreateAudioSource("ActivationSound", activationSoundVolume, false);
        movementAudioSource = CreateAudioSource("MovementSound", movementSoundVolume, true);
        targetMovementVolume = movementSoundVolume;
        
        forwardRotationOffset = Quaternion.FromToRotation(modelForwardAxis, Vector3.forward);
        SetVisibility(!startInvisible);
        SetCollision(false);
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

    public void ActivateTicketTaker()
    {
        if (!isActivated)
        {
            isActivated = true;
            SetVisibility(true);
            SetCollision(true);
            
            // Play activation sound
            if (activationSound != null)
            {
                activationAudioSource.clip = activationSound;
                activationAudioSource.Play();
            }
            
            // Start movement sound loop
            StartMovementSound();
        }
    }

    public void PauseMovement(bool pause)
    {
        isPaused = pause;
        
        // Handle sound pausing
        if (pause)
        {
            StopMovementSound();
        }
        else if (isActivated)
        {
            StartMovementSound();
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
        
            // Set target to full volume and adjust
            targetMovementVolume = movementSoundVolume;
            AdjustMovementVolume();
            isPlayingMovementSound = true;
        }
    }


    private void StopMovementSound()
    {
        if (isPlayingMovementSound)
        {
            // Set target to muffled volume and adjust
            targetMovementVolume = muffledVolume;
            AdjustMovementVolume();
            isPlayingMovementSound = false;
        }
    }
    
    private void AdjustMovementVolume()
    {
        // Stop any existing volume adjustment
        if (volumeAdjustCoroutine != null)
        {
            StopCoroutine(volumeAdjustCoroutine);
        }
    
        // Start new volume adjustment
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

    private void Update()
    {
        if (!isActivated || isPaused || player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        if (direction.magnitude > 0.01f)
        {
            Quaternion targetLookRotation = Quaternion.LookRotation(direction);
            Quaternion targetRotation = targetLookRotation * forwardRotationOffset;
            
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        Vector3 targetPosition = player.position;
        targetPosition.y = transform.position.y;
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        MaintainFloatingHeight();
        HandleMovementSound();
    }

    private void SetVisibility(bool visible)
    {
        if (modelRenderers != null)
        {
            foreach (Renderer renderer in modelRenderers)
            {
                renderer.enabled = visible;
            }
        }
    }
    
    private void HandleMovementSound()
    {
        if (!isPlayingMovementSound || movementSound == null) return;
        
        // Update sound position and volume based on movement
        movementSoundTimer -= Time.deltaTime;
        if (movementSoundTimer <= 0f)
        {
            // Add some variation to the sound timing
            movementSoundTimer = movementSoundInterval + Random.Range(-0.1f, 0.1f);
            
            // Play the movement sound (works with loop or one-shot)
            if (!movementAudioSource.isPlaying)
            {
                movementAudioSource.Play();
            }
        }
    }

    private void SetCollision(bool enabled)
    {
        if (colliders != null)
        {
            foreach (Collider collider in colliders)
            {
                collider.enabled = enabled;
            }
        }
    }

    private void MaintainFloatingHeight()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
        {
            transform.position = new Vector3(
                transform.position.x,
                hit.point.y + heightAboveGround,
                transform.position.z
            );
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isActivated || isPaused) return;

        bool shouldCollide = false;
        foreach (string tag in collidableTags)
        {
            if (collision.gameObject.CompareTag(tag))
            {
                shouldCollide = true;
                GameManager.Instance.GameOver();
                break;
            }
        }

        if (!shouldCollide)
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), collision.collider, true);
        }
    }
    
    private void OnDestroy()
    {
        // Clean up any running coroutines
        if (volumeAdjustCoroutine != null)
        {
            StopCoroutine(volumeAdjustCoroutine);
        }
    }
}
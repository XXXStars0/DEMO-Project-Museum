using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class Dinosaur : MonoBehaviour
{
    public enum DinoState { 
        Roaming, 
        Stunned, 
        Secured 
    }

    [Header("State Info")]
    public DinoState currentState = DinoState.Roaming;

    [Header("Persuasion & Stun")]
    public float maxPersuasion = 100f;
    public float decayRate = 10f; 
    private float currentPersuasion = 0f;

    [Header("Giant Interaction")]
    public float giantImpactPersuasion = 50f; 
    [Header("Normal Interaction")]
    public float knockbackForce = 15f;

    [Header("Physics (Pushing)")]
    public float pushDrag = 2f;

    [Header("Visuals")]
    public Renderer bodyRenderer;
    public GameObject flashObjectHost;
    public Color roamColor = Color.red;
    public Color stunColor = Color.yellow;
    public Color secureColor = Color.green;

    [Header("Feedback Settings")]
    public string colorPropertyName = "_Color";
    public Color flashColor = Color.red;
    public float flashDuration = 0.5f;
    private Coroutine _flashCoroutine;

    [Header("UI Elements")]
    public GameObject persuasionSliderPrefab;
    private Slider _sliderInstance;
    private GameObject _sliderObject;
    private Image _sliderFillImage;

    [Header("Movement")]
    public float wanderRadius = 10f;
    public float wanderTimer = 4f;
    private float moveTimer;

    private NavMeshAgent agent;
    private Rigidbody rb;
    private MaterialPropertyBlock propBlock;
    private Camera mainCam;
    
    private Renderer[] _flashRenderers;
    private readonly Dictionary<Renderer, Color> _originalFlashColors = new Dictionary<Renderer, Color>();

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        propBlock = new MaterialPropertyBlock();
        mainCam = Camera.main;

        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (flashObjectHost != null)
        {
            _flashRenderers = flashObjectHost.GetComponentsInChildren<Renderer>();
            foreach (var rend in _flashRenderers)
            {
                if (rend.sharedMaterial.HasProperty(colorPropertyName))
                {
                    _originalFlashColors[rend] = rend.sharedMaterial.GetColor(colorPropertyName);
                }
            }
        }
        
        InstantiateSlider();
        SetState(DinoState.Roaming);
    }

    void Update()
    {
        HandleUIVisibility();
        HandlePersuasionDecay();

        if (currentState == DinoState.Roaming)
        {
            RoamingLogic();
        }
    }

    void HandlePersuasionDecay()
    {
        if (currentState == DinoState.Secured) return;

        if (currentPersuasion > 0)
        {
            currentPersuasion -= decayRate * Time.deltaTime;
            
            if (currentPersuasion < 0) currentPersuasion = 0;

            if (_sliderInstance != null) _sliderInstance.value = currentPersuasion;
            
            if (currentState == DinoState.Stunned && currentPersuasion <= 0)
            {
                Debug.Log("Dino woke up!");
                SetState(DinoState.Roaming);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (currentState == DinoState.Secured) return;

        if (collision.gameObject.CompareTag("Player") || collision.gameObject.name.Contains("Player_Character"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player == null) return;

            // --- Large ---
            if (player.GetCurrentSize() == PlayerSize.Large)
            {
                if (currentState == DinoState.Roaming)
                {
                    // TODO:Collision to player
                    ApplyPersuasion(giantImpactPersuasion);                   
                }
                
            }
            // --- Normal/Small ---
            else
            {
             if (currentState == DinoState.Roaming)
                {
                    Rigidbody playerRb = player.GetComponent<Rigidbody>();
                    if (playerRb != null)
                    {
                        Vector3 dir = (player.transform.position - transform.position).normalized;
                        dir.y = 0.5f; 
                        playerRb.AddForce(dir * knockbackForce, ForceMode.Impulse);
                        player.TakeDamage();
                    }
                }
            }
        }
    }
    void HandleUIVisibility()
    {
        if (_sliderObject == null) return;

        Vector3 viewPos = mainCam.WorldToViewportPoint(transform.position);

        bool isOnScreen = viewPos.z > 0 && viewPos.x > 0 && viewPos.x < 1 && viewPos.y > 0 && viewPos.y < 1;

        if (_sliderObject.activeSelf != isOnScreen)
        {
            _sliderObject.SetActive(isOnScreen);
        }
    }

    public void ApplyPersuasion(float amount)
    {
        if (currentState != DinoState.Roaming) return;

        currentPersuasion += amount;

        // Trigger flash effect
        if (_flashRenderers != null && _flashRenderers.Length > 0)
        {
            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
            }
            _flashCoroutine = StartCoroutine(FlashEffectCoroutine());
        }
        
        if (currentPersuasion > maxPersuasion) currentPersuasion = maxPersuasion;

        if (_sliderInstance != null) _sliderInstance.value = currentPersuasion;

        if (currentPersuasion >= maxPersuasion)
        {
            SetState(DinoState.Stunned);
        }
    }

void SetState(DinoState newState)
    {
        currentState = newState;
        UpdateColor(); 

        switch (newState)
        {
            case DinoState.Roaming:
                agent.enabled = true;
                agent.isStopped = false;
                rb.isKinematic = true;
                break;

            case DinoState.Stunned:
                currentPersuasion = maxPersuasion;
                if (_sliderInstance != null) _sliderInstance.value = currentPersuasion;
                
                agent.isStopped = true;
                agent.enabled = false;
                rb.isKinematic = false;
                rb.drag = pushDrag;
                break;

            case DinoState.Secured:
                agent.enabled = false;
                rb.isKinematic = true;

                if (_sliderObject != null)
                {
                    Destroy(_sliderObject);
                    _sliderInstance = null;
                    _sliderFillImage = null;
                }
                break;
        }
    }

void UpdateColor()
    {
        Color targetColor = roamColor;
        switch (currentState)
        {
            case DinoState.Stunned: targetColor = stunColor; break;
            case DinoState.Secured: targetColor = secureColor; break;
        }

        if (bodyRenderer != null)
        {
            bodyRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_BaseColor", targetColor);
            propBlock.SetColor("_Color", targetColor);
            bodyRenderer.SetPropertyBlock(propBlock);
        }

        if (_sliderFillImage != null)
        {
            _sliderFillImage.color = targetColor;
        }
    }

  private void InstantiateSlider()
    {
        Transform canvasTransform = LevelManager.BubbleCanvas;
        if (canvasTransform == null)
        {
            Canvas[] canvases = GameObject.FindObjectsOfType<Canvas>();
            for (int i = 0; i < canvases.Length; i++)
            {
                if (canvases[i].renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    canvasTransform = canvases[i].transform;
                    break;
                }
            }

            if (canvasTransform == null && canvases.Length > 0)
            {
                canvasTransform = canvases[0].transform;
            }
        }

        if (canvasTransform == null || persuasionSliderPrefab == null) return;

        GameObject sliderGO = Instantiate(persuasionSliderPrefab);
        sliderGO.transform.SetParent(canvasTransform, false);
        _sliderObject = sliderGO;

        _sliderInstance = sliderGO.GetComponent<Slider>() ?? sliderGO.GetComponentInChildren<Slider>();
        if (_sliderInstance == null)
        {
            Debug.LogError($"Persuasion slider prefab '{persuasionSliderPrefab.name}' has no Slider component.");
        }

        FollowTargetUI follower = sliderGO.GetComponent<FollowTargetUI>() ?? sliderGO.GetComponentInChildren<FollowTargetUI>();
        if (follower == null)
        {
            Debug.LogError($"Persuasion slider prefab '{persuasionSliderPrefab.name}' has no FollowTargetUI component.");
        }
        else
        {
            follower.target = this.transform;
        }

        // Log missing script references to help track down broken prefab components
        LogMissingScriptReferences(sliderGO);

        if (_sliderInstance != null)
        {
            _sliderInstance.maxValue = maxPersuasion;
            _sliderInstance.value = currentPersuasion;

            if (_sliderInstance.fillRect != null)
            {
                _sliderFillImage = _sliderInstance.fillRect.GetComponent<Image>();
            }
        }

        // Start hidden; visibility is controlled by HandleUIVisibility or FollowTargetUI
        _sliderObject.SetActive(false);

        UpdateColor();
    }

    private void LogMissingScriptReferences(GameObject go)
    {
        foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
        {
            Component[] comps = t.gameObject.GetComponents<Component>();
            for (int i = 0; i < comps.Length; i++)
            {
                if (comps[i] == null)
                {
                    string path = t.name;
                    Transform p = t.parent;
                    while (p != null)
                    {
                        path = p.name + "/" + path;
                        p = p.parent;
                    }
                    Debug.LogWarning($"Missing script on instantiated prefab child: {path} (GameObject) in prefab '{go.name}'");
                }
            }
        }
    }

    void RoamingLogic()
    {
        moveTimer += Time.deltaTime;
        if (moveTimer >= wanderTimer)
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
            agent.SetDestination(newPos);
            moveTimer = 0;
        }
    }
    
    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }
    
    public void SecureDino(Transform anchorPoint)
    {
        if (currentState == DinoState.Secured) return;

        SetState(DinoState.Secured);
        
        if (anchorPoint != null)
        {
            transform.position = anchorPoint.position;
            transform.rotation = anchorPoint.rotation;
        }
    }
    void OnDestroy()
    {
        if (_sliderObject != null) Destroy(_sliderObject);

        if (Application.isPlaying && bodyRenderer != null)
        {
            if(bodyRenderer.material != null)
            {
                Destroy(bodyRenderer.material);
            }
        }
    }

    private System.Collections.IEnumerator FlashEffectCoroutine()
    {
        var flashPropBlock = new MaterialPropertyBlock();

        foreach (Renderer rend in _flashRenderers)
        {
            rend.GetPropertyBlock(flashPropBlock);
            flashPropBlock.SetColor(colorPropertyName, flashColor);
            if (rend.sharedMaterial.HasProperty("_BaseColor"))
                flashPropBlock.SetColor("_BaseColor", flashColor);
            rend.SetPropertyBlock(flashPropBlock);
        }

        yield return new WaitForSeconds(flashDuration);

        foreach (Renderer rend in _flashRenderers)
        {
            if (_originalFlashColors.TryGetValue(rend, out Color color))
            {
                rend.GetPropertyBlock(flashPropBlock);
                flashPropBlock.SetColor(colorPropertyName, color);
                if (rend.sharedMaterial.HasProperty("_BaseColor"))
                    flashPropBlock.SetColor("_BaseColor", color);
                rend.SetPropertyBlock(flashPropBlock);
            }
        }
        
        _flashCoroutine = null;
    }
}
using UnityEngine;
using UnityEngine.UI; 

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
[ExecuteAlways] 
public class Obstacle : MonoBehaviour
{
    [Header("Durability Settings")]
    public float maxDurability = 100f; 
    private float currentDamage = 0f; 
    public float damagePerHit = 50f;   
    [Header("UI Settings")]
    public GameObject damageSliderPrefab; 
    private Slider _sliderInstance;
    private GameObject _sliderObject;
    private Camera mainCam;

    [Header("Breakable Object Settings")]
    public GameObject debrisPrefab;
    public float breakForce = 5f;

    [Header("Feedback Settings")]
    public string colorPropertyName = "_BaseColor";
    public Color flashColor = Color.red;
    public float flashDuration = 0.5f;
    
    [Header("Grid Settings")]
    public float gridSize = 1f;
    public bool snapToGrid = true; 
    private Vector3 lastPosition;

    private Renderer _renderer;
    private Color _originalColor;
    private Coroutine _flashCoroutine;

    void Start()
    {
        _renderer = GetComponentInChildren<Renderer>();
        if (_renderer != null)
        {
            Material sourceMaterial = Application.isPlaying ? _renderer.material : _renderer.sharedMaterial;
            
            if (sourceMaterial != null && sourceMaterial.HasProperty(colorPropertyName))
            {
                _originalColor = sourceMaterial.GetColor(colorPropertyName);
            }
        }
        
        if (Application.isPlaying)
        {
            mainCam = Camera.main;
            InstantiateSlider();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (currentDamage >= maxDurability) return;

        if (collision.gameObject.name.Contains("Player_Character"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();

            if (player != null && player.GetCurrentSize() == PlayerSize.Large)
            {
               //if (collision.relativeVelocity.magnitude > 0.5f)
                HandleImpact(collision);
            }
        }
    }

    void HandleImpact(Collision collision)
    {
        currentDamage += damagePerHit;

        if (_renderer != null)
        {
            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
            }
            _flashCoroutine = StartCoroutine(FlashEffectCoroutine());
        }

        if (_sliderInstance != null)
        {
            _sliderInstance.value = currentDamage;
            // SetActive(true)
            if (_sliderObject != null && !_sliderObject.activeSelf) 
                _sliderObject.SetActive(true);
        }

        if (currentDamage >= maxDurability)
        {
            SmashObject(collision);
        }
    }

    private System.Collections.IEnumerator FlashEffectCoroutine()
    {
        if (_renderer.material.HasProperty(colorPropertyName))
        {
            _renderer.material.SetColor(colorPropertyName, flashColor);
            yield return new WaitForSeconds(flashDuration);
            _renderer.material.SetColor(colorPropertyName, _originalColor);
        }
        _flashCoroutine = null;
    }

    void SmashObject(Collision collision)
    {
        if (_sliderObject != null) Destroy(_sliderObject);

        if (debrisPrefab != null)
        {
            GameObject debris = Instantiate(debrisPrefab, transform.position, transform.rotation);
            Rigidbody[] rbs = debris.GetComponentsInChildren<Rigidbody>();
            foreach (var rb in rbs)
            {
                if (collision.contacts.Length > 0)
                    rb.AddExplosionForce(breakForce, collision.contacts[0].point, 3f);
                else
                    rb.AddExplosionForce(breakForce, transform.position, 3f);
            }
            Destroy(debris, 1.5f);
        }

        Destroy(gameObject);
    }

    private void InstantiateSlider()
    {
        if (damageSliderPrefab == null) return;

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
                canvasTransform = canvases[0].transform;
        }

        if (canvasTransform == null)
        {
            Debug.LogError("BubbleCanvas not found in LevelManager and no Canvas in scene to fall back to.");
            return;
        }

        GameObject sliderGO = Instantiate(damageSliderPrefab);
        sliderGO.transform.SetParent(canvasTransform, false);
        _sliderObject = sliderGO;

        _sliderInstance = sliderGO.GetComponent<Slider>() ?? sliderGO.GetComponentInChildren<Slider>();
        if (_sliderInstance == null)
        {
            Debug.LogError($"Damage slider prefab '{damageSliderPrefab.name}' has no Slider component.");
        }

        FollowTargetUI follower = sliderGO.GetComponent<FollowTargetUI>() ?? sliderGO.GetComponentInChildren<FollowTargetUI>();
        if (follower == null)
        {
            Debug.LogError($"Damage slider prefab '{damageSliderPrefab.name}' has no FollowTargetUI component.");
        }
        else
        {
            follower.target = this.transform;
            follower.offset = new Vector3(0, 2f, 0);
        }

        LogMissingScriptReferences(sliderGO);

        if (_sliderInstance != null)
        {
            _sliderInstance.maxValue = maxDurability;
            _sliderInstance.value = currentDamage;
        }

        // Start hidden; Update will unhide when needed
        // _sliderObject.SetActive(false);
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
        //if (_sliderObject != null) _sliderObject.SetActive(false);
    

    void Update()
    {
        #if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    if (snapToGrid && transform.position != lastPosition) SnapPosition();
                }
        #endif
    }

    private void SnapPosition()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Round(pos.x / gridSize) * gridSize;
        pos.y = Mathf.Round(pos.y / gridSize) * gridSize;
        pos.z = Mathf.Round(pos.z / gridSize) * gridSize;
        transform.position = pos;
        lastPosition = pos;
    }

    void OnDestroy()
    {
        if (_sliderObject != null) Destroy(_sliderObject);

        if (Application.isPlaying && _renderer != null)
        {
            Destroy(_renderer.material);
        }
    }
}
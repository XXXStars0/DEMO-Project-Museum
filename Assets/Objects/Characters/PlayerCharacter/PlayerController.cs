using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerSize
{
    Small,
    Normal,
    Large
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Actor))]
public class PlayerController : MonoBehaviour
{

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 720f;
    public float blockSize = 1f;
    public float stepSmooth = 0.15f;

    [Header("Key Bindings - Moving")]
    public KeyCode keyForward = KeyCode.W;
    public KeyCode keyBackward = KeyCode.S;
    public KeyCode keyLeft = KeyCode.A;
    public KeyCode keyRight = KeyCode.D;

    [Header("Key Bindings - Interaction")]
    public KeyCode keyDrop = KeyCode.Q;
    public KeyCode keyPickUp = KeyCode.E;
    public KeyCode keyInteraction = KeyCode.F;
    public KeyCode keyItemMode = KeyCode.LeftAlt;
    public KeyCode keyLeftUse = KeyCode.Mouse0;
    public KeyCode keyRightUse = KeyCode.Mouse1;

    [Header("Key Bindings - System")]
    public KeyCode keyPause = KeyCode.Escape;

    [Header("Layer Settings")]
    public LayerMask blockLayer;

    [Header("Damage Settings")]

    [Header("Feedback Settings")]
    public GameObject flashObjectHost;
    public string colorPropertyName = "_Color";
    public Color flashColor = Color.white; 
    public float flashDuration = 0.2f;

    //

    private Coroutine _flashCoroutine;
    private Renderer[] _flashRenderers;
    private readonly Dictionary<Renderer, Color> _originalFlashColors = new Dictionary<Renderer, Color>();

    [Header("Other")]
    public Transform stepCheckPoint;
    public PickableItem detectItem;
    public ReadableItem detectedReadable;
    public Block detectedBlock;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private Actor actor;

    private Vector3 gizmoCheckPos;

    //Player Sizing Logic
    private float defaultSpeed; 
    private Vector3 defaultScale; 
    private PlayerSize currentSize = PlayerSize.Normal;
    private Vector3 originalScale = Vector3.one;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        actor = GetComponent<Actor>();

        defaultSpeed = moveSpeed;
        defaultScale = transform.localScale;

        currentSize = PlayerSize.Normal;
        originalScale = transform.localScale;

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
    }

    void Update()
    {
        HandleMovementInput();
        //HandleItemInput();
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    void HandleMovementInput()
    {
        float h = 0f, v = 0f;
        if (Input.GetKey(keyForward)) v += 1f;
        if (Input.GetKey(keyBackward)) v -= 1f;
        if (Input.GetKey(keyRight)) h += 1f;
        if (Input.GetKey(keyLeft)) h -= 1f;

        moveDirection = new Vector3(h, 0, v).normalized;
    }


    void MovePlayer()
    {
        if (moveDirection.magnitude > 0.1f)
        {
            Transform cam = Camera.main.transform;
            Vector3 camForward = cam.forward;
            Vector3 camRight = cam.right;
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 desiredMoveDir = (camForward * moveDirection.z + camRight * moveDirection.x).normalized;
            Vector3 move = desiredMoveDir * moveSpeed * Time.fixedDeltaTime;

            TryStepUp(desiredMoveDir);
            DetectPickable(desiredMoveDir);
            rb.MovePosition(rb.position + move);

            Quaternion targetRotation = Quaternion.LookRotation(desiredMoveDir);
            rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    void TryStepUp(Vector3 moveDir)
    {
        Vector3 checkPos = stepCheckPoint != null
            ? stepCheckPoint.position
            : transform.position + moveDir * 0.6f;

        float checkRadius = 0.3f;

        Collider[] hits = Physics.OverlapSphere(checkPos, checkRadius, blockLayer);

        if (hits.Length == 0)
        {
            return;
        }

        foreach (var hit in hits)
        {
            Block block = hit.GetComponent<Block>();
            if (block){
                //Stair
                if (block.blockType == BlockType.Stair)
                {
                    //TO: MORE STATES
                    if (actor.currentState == ActorState.Normal || actor.currentState == ActorState.Carrying)
                    {

                        Vector3 stepTarget = rb.position + Vector3.up * blockSize;
                        rb.position = Vector3.Lerp(rb.position, stepTarget, stepSmooth);
                        return;
                    }
                }
            }
        }
    }

    void DetectPickable(Vector3 moveDir)
    {
        Vector3 checkPos = stepCheckPoint != null
            ? stepCheckPoint.position
            : transform.position + moveDir * 0.6f;

        float checkRadius = 0.3f;
        Collider[] hits = Physics.OverlapSphere(checkPos, checkRadius, blockLayer);

        detectItem = null;
        detectedReadable = null;
        detectedBlock = null;

        foreach (var hit in hits)
        {
            PickableItem item = hit.GetComponent<PickableItem>();
            if (item)
            {
                detectItem = item;
                //Debug.Log($"Detected: {item.name}");
            }

            ReadableItem readable = hit.GetComponent<ReadableItem>();
            if (readable)
            {
                detectedReadable = readable;
                //Debug.Log($"Detected: {readable.text}");
            }

            Block _b = hit.GetComponent<Block>();
            if (_b)
            {
                detectedBlock = _b;
                //Debug.Log($"Detected: {_b.name}");
            }
        }
    }

    public PlayerSize GetCurrentSize()
    {
        return currentSize;
    }

    public void setSize(PlayerSize newSize)
    {
        Vector3 targetScale = originalScale;

        switch (newSize)
        {
            case PlayerSize.Small:
                targetScale = originalScale * 0.5f;
                break;
            case PlayerSize.Normal:
                targetScale = originalScale;
                break; 
            case PlayerSize.Large:
                targetScale = originalScale * 1.5f;
                break;      
        }

        transform.localScale = targetScale;
        currentSize = newSize;
    }
        
        
    //Old Logic
    /*  
    public void SetPlayerScale(float scaleMultiplier)
        {
            transform.localScale = defaultScale * scaleMultiplier;
            moveSpeed = defaultSpeed * scaleMultiplier;
        }
    */
    
    public void TakeDamage()
    {
        // TODO: Implement actual damage logic
        // Debug.Log($"Player took {damage} damage!");

        // Trigger flash effect
        if (_flashRenderers != null && _flashRenderers.Length > 0)
        {
            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
            }
            _flashCoroutine = StartCoroutine(FlashEffectCoroutine());
        }
    }

    private IEnumerator FlashEffectCoroutine()
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

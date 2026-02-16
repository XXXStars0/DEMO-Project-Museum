using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Block))]
public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    public string requiredItemID;
    
    public bool consumeKey = false; 

    [Header("Hint Settings")]
    public ReadableItem hint;

    private bool isLocked = true;
    private Block _b;

    void Awake()
    {
        _b = GetComponent<Block>();
        if (_b.blockType != BlockType.Door)
        {
            _b.blockType = BlockType.Door;
        }

        hint = GetComponent<ReadableItem>();
        if (hint)
        {
            hint.title = "Door";
            hint.text = $"Key Require: {requiredItemID}";
        }
    }

    public void TryUnlock(PickableItem keyUsed)
    {
        if (!isLocked)
        {
            return;
        }

        if (keyUsed != null && keyUsed.itemID == requiredItemID)
        {
            UnlockSuccess(keyUsed);
        }
        else
        {
            // Debug.Log("Wrong key or no key used!");
        }
    }

    private void UnlockSuccess(PickableItem keyUsed)
    {
        isLocked = false;

        if (hint)
        {
            hint.text = $"Successfully unlocked!";
        }

        if (consumeKey)
        {
            if (keyUsed.holder != null)
            {
                keyUsed.holder.ConsumeHeldItem();
            }
            else
            {
                Destroy(keyUsed.gameObject);
            }
        }

        this.gameObject.SetActive(false);
    }
}
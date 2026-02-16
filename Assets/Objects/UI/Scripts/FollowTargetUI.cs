using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTargetUI : MonoBehaviour
{
    public Transform target;

    public Vector3 offset = new Vector3(0, 1.5f, 0);

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (target != null && mainCamera != null)
        {
            Vector3 worldPos = target.position + offset;
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

            bool isOnScreen = screenPos.z > 0 && 
                            screenPos.x >= 0 && screenPos.x <= Screen.width &&
                            screenPos.y >= 0 && screenPos.y <= Screen.height;

            if (gameObject.activeSelf != isOnScreen)
            {
                gameObject.SetActive(isOnScreen);
            }

            if (isOnScreen)
            {
                transform.position = screenPos;
            }
        }
        else if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }
}
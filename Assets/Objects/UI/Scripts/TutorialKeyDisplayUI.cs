using UnityEngine;
using TMPro;

public class TutorialKeyDisplayUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI moveText;
    public TextMeshProUGUI pickText;
    public TextMeshProUGUI dropText;
    public TextMeshProUGUI useText;
    public TextMeshProUGUI cameraText;
    public TextMeshProUGUI pauseText;

    [Header("Defaults (Main Menu Fallback)")]
    public string defaultMove = "W / A / S / D";
    public string defaultPick = "E";
    public string defaultDrop = "Q";
    public string defaultUse = "Left Click";
    public string defaultCam = "Right Click";
    public string defaultPause = "Esc";

    void OnEnable()
    {
        RefreshKeyBindings();
    }

    public void RefreshKeyBindings()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        CameraDragControl camControl = FindObjectOfType<CameraDragControl>();

        if (player != null)
        {
            moveText.text = $"Move: {FormatKey(player.keyForward)} / {FormatKey(player.keyLeft)} / {FormatKey(player.keyBackward)} / {FormatKey(player.keyRight)}";
            pickText.text = $"Pick Up: {FormatKey(player.keyPickUp)}"; 
            dropText.text = $"Drop Item: {FormatKey(player.keyDrop)}";   

            useText.text = $"Use: {FormatKey(player.keyLeftUse)}";         
            pauseText.text = $"Pause: {FormatKey(player.keyPause)}";
        }
        else
        {
            moveText.text = $"Move: {defaultMove}";
            pickText.text = $"Interact: {defaultPick}";
            dropText.text = $"Drop: {defaultDrop}";
            useText.text = $"Use: {defaultUse}";
            pauseText.text = $"Pause: {defaultPause}";
        }

        if (camControl != null)
        {
            cameraText.text = $"Camera: {FormatMouse(camControl.dragMouseButton)} (Hold Drag)";
        }
        else
        {
            cameraText.text = $"Camera: {defaultCam} (Hold Drag)";
        }
    }

    // --- 辅助函数：美化按键显示 ---
    
    // 把 KeyCode 变成易读的字符串
    string FormatKey(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.Mouse0: return "Left Click";
            case KeyCode.Mouse1: return "Right Click";
            case KeyCode.Mouse2: return "Middle Click";
            case KeyCode.Alpha0: return "0";
            case KeyCode.Alpha1: return "1";
            //More
            case KeyCode.Escape: return "Esc";
            case KeyCode.Return: return "Enter";
            default: return key.ToString();
        }
    }

    string FormatMouse(int buttonIndex)
    {
        switch (buttonIndex)
        {
            case 0: return "Left Click";
            case 1: return "Right Click";
            case 2: return "Middle Click";
            default: return "Mouse " + buttonIndex;
        }
    }

    public void CloseTutorial()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;

        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }
}
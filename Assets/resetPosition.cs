using UnityEngine;
using UnityEngine.InputSystem;

public class XRRigResetInputSystem : MonoBehaviour
{
    public Transform xrOrigin;  // Assign in inspector
    public Vector3 resetPosition = new Vector3(0,1,0);
    public Quaternion resetRotation = Quaternion.identity;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            ResetXRRig();
        }
    }

    void ResetXRRig()
    {
        if (xrOrigin != null)
        {
            xrOrigin.position = resetPosition;
            xrOrigin.rotation = resetRotation;
            Debug.Log("XR Rig reset to default position and rotation");
        }
        else
        {
            Debug.LogWarning("XR Origin reference not set!");
        }
    }
}

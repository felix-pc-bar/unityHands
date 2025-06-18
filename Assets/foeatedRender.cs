using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class FoveationStarter : MonoBehaviour
{
    List<XRDisplaySubsystem> xrDisplays = new List<XRDisplaySubsystem>();

    void Start()
    {
        SubsystemManager.GetSubsystems(xrDisplays);
        if (xrDisplays.Count == 1)
        {
            xrDisplays[0].foveatedRenderingLevel = 1.0f; // Full strength
            xrDisplays[0].foveatedRenderingFlags
                = XRDisplaySubsystem.FoveatedRenderingFlags.GazeAllowed;
        }
    }
}

using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR;
using System.Collections.Generic;
using UnityEngine.UI;

public class GestureRecognizer : MonoBehaviour
{
    public XRHandSubsystem handSubsystem;
    public XRNode handNode = XRNode.RightHand;
    public Camera xrUICamera;
    public LayerMask uiLayerMask = -1; // Set this to your UI layer in inspector

    private bool wasPinchingLastFrame = false;

    void Start()
    {
        var subsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetSubsystems(subsystems);
        if (subsystems.Count > 0)
        {
            handSubsystem = subsystems[0];
        }
        else
        {
            Debug.LogError("No XRHandSubsystem found. Ensure XR Hands is enabled and OpenXR is properly set up.");
        }
    }

    void Update()
    {
        if (handSubsystem == null) return;

        XRHand hand = handNode == XRNode.LeftHand ? handSubsystem.leftHand : handSubsystem.rightHand;
        if (!hand.isTracked) return;

        bool isPinching = pinchDist(hand) < 0.03f;

        if (TryGetJointPos(hand, XRHandJointID.Palm, out Vector3 palmPos) &&
            TryGetJointForward(hand, XRHandJointID.Palm, out Vector3 palmFwd))
        {
            Ray palmRay = new Ray(palmPos, palmFwd);

            // Draw debug ray in both Scene view and Game view
            Debug.DrawRay(palmRay.origin, palmRay.direction * 2f, Color.green);
            DrawLineInGameView(palmRay.origin, palmRay.origin + palmRay.direction * 2f, Color.green);

            if (isPinching && !wasPinchingLastFrame)
            {
                if (Physics.Raycast(palmRay, out RaycastHit hit, 5f, uiLayerMask))
                {
                    Debug.Log($"Pinch UI hit: {hit.collider.name}");

                    var button = hit.collider.GetComponent<Button>();
                    if (button)
                        button.onClick.Invoke();
                }
            }
        }

        wasPinchingLastFrame = isPinching;
    }

    float pinchDist(XRHand hand)
    {
        if (TryGetJointPos(hand, XRHandJointID.ThumbTip, out var thumb) &&
            TryGetJointPos(hand, XRHandJointID.IndexTip, out var index))
        {
            return Vector3.Distance(thumb, index);
        }
        return float.PositiveInfinity;
    }

    bool TryGetJointPos(XRHand hand, XRHandJointID id, out Vector3 pos)
    {
        if (hand.GetJoint(id).TryGetPose(out Pose pose))
        {
            pos = pose.position;
            return true;
        }
        pos = Vector3.zero;
        return false;
    }

    bool TryGetJointForward(XRHand hand, XRHandJointID id, out Vector3 forward)
    {
        if (hand.GetJoint(id).TryGetPose(out Pose pose))
        {
            forward = pose.rotation * Vector3.forward;
            return true;
        }
        forward = Vector3.forward;
        return false;
    }

    void DrawLineInGameView(Vector3 start, Vector3 end, Color color)
    {
        var lineObj = GameObject.Find("__PalmRayVisualizer") ?? new GameObject("__PalmRayVisualizer");
        var lineRenderer = lineObj.GetComponent<LineRenderer>() ?? lineObj.AddComponent<LineRenderer>();

        lineRenderer.startWidth = 0.005f;
        lineRenderer.endWidth = 0.005f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.positionCount = 2;
        lineRenderer.SetPositions(new Vector3[] { start, end });
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }
}
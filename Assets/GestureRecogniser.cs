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
    public GameObject cubePrefab; // Assign a cube prefab with Rigidbody in the inspector

    private bool wasPinchingLastFrame = false;
    private GameObject lineObj;
    private LineRenderer lineRenderer;

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

        lineObj = new GameObject("__PalmRayVisualizer");
        lineRenderer = lineObj.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.005f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.green;
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

            // Visualize the ray
            DrawLineInGameView(palmPos, palmPos + palmFwd * 2f, Color.green);

            if (isPinching && !wasPinchingLastFrame)
            {
                // Raycast to UI (optional)
                if (Physics.Raycast(palmRay, out RaycastHit hit, 5f, uiLayerMask))
                {
                    Debug.Log($"Pinch UI hit: {hit.collider.name}");

                    var button = hit.collider.GetComponent<Button>();
                    if (button)
                        button.onClick.Invoke();
                }

                // Spawn cube
                if (cubePrefab != null)
                {
                    GameObject cube = Instantiate(cubePrefab, palmPos + palmFwd * 0.1f, Quaternion.identity);
                    Rigidbody rb = cube.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.linearVelocity = palmFwd * 1f; // give it some forward velocity
                    }
                }
            }
        }

        wasPinchingLastFrame = isPinching;
    }

    void DrawLineInGameView(Vector3 start, Vector3 end, Color color)
    {
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }
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
}

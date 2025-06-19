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
    public Transform xrOrigin; // Assign XR Origin in inspector
    public Vector3 handPositionOffset = new Vector3(0f, 1.0f, 0f);

    private bool wasPinchingLastFrame = false;
    private GameObject lineObj;
    private LineRenderer lineRenderer;
    private GameObject grabbedObject = null;
    private int unpinchedFrames = 0;
    private float grabDist = 0;
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

        lineObj = new GameObject("__PalmRayVisualizer_" + handNode);
        lineRenderer = lineObj.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.005f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.startColor = handNode == XRNode.RightHand ? Color.pink : Color.azure;
        lineRenderer.endColor = lineRenderer.startColor;
    }

    void Update()
    {
        if (handSubsystem == null) return;

        XRHand hand = handNode == XRNode.LeftHand ? handSubsystem.leftHand : handSubsystem.rightHand;
        if (!hand.isTracked) return;

        bool isPinching = pinchDist(hand) < 0.03f;

        if (TryGetJointPose(hand, XRHandJointID.Palm, out Pose palmPose))
        {
            Vector3 palmPos = (xrOrigin ? xrOrigin.TransformPoint(palmPose.position) : palmPose.position) + handPositionOffset;
            Vector3 palmFwd = xrOrigin ? xrOrigin.TransformDirection(palmPose.rotation * Vector3.forward) : palmPose.rotation * Vector3.forward;
            Ray palmRay = new Ray(palmPos, palmFwd);

            // Visualize the ray
            DrawLineInGameView(palmPos, palmPos + palmFwd * 2f, lineRenderer.startColor);
            //  && !wasPinchingLastFrame
            if (isPinching && !wasPinchingLastFrame)
            {
                unpinchedFrames = 0; //reset counter
                if (handNode == XRNode.RightHand)
                {
                    if (cubePrefab != null)
                    {
                        GameObject cube = Instantiate(cubePrefab, palmPos + palmFwd * 0.1f, Quaternion.identity);
                        Rigidbody rb = cube.GetComponent<Rigidbody>();
                        for (int i = 0; i<5;i++) {
                            if (rb != null)
                            {
                                rb.linearVelocity = palmFwd * 1f;
                            }
                        }
                    }
                }
                else if (handNode == XRNode.LeftHand)
                {
                    if (Physics.Raycast(palmRay, out RaycastHit hit, 2f))
                    {
                        if (hit.collider != null && hit.collider.attachedRigidbody != null)
                        {
                            grabbedObject = hit.collider.gameObject;
                            grabDist = hit.distance;
                            Debug.Log(grabDist);
                        }
                    }
                }
            }
            else if (!isPinching && grabbedObject != null)
            {
                unpinchedFrames++; //inc counter
                if (unpinchedFrames > 10)
                {
                    grabbedObject = null;
                    unpinchedFrames = 0;
                }
            }

            if (grabbedObject != null)
            {
                grabbedObject.transform.position = palmPos + palmFwd * grabDist;
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
            pos = (xrOrigin ? xrOrigin.TransformPoint(pose.position) : pose.position) + handPositionOffset;
            return true;
        }
        pos = Vector3.zero;
        return false;
    }

    bool TryGetJointPose(XRHand hand, XRHandJointID id, out Pose pose)
    {
        if (hand.GetJoint(id).TryGetPose(out pose))
        {
            if (xrOrigin)
            {
                pose.position = xrOrigin.TransformPoint(pose.position) + handPositionOffset;
                pose.rotation = xrOrigin.rotation * pose.rotation;
            }
            return true;
        }
        return false;
    }
}

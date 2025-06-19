using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Hands;

public class PalmColliderUpdater : MonoBehaviour
{
    public XRHandSubsystem handSubsystem;
    public XRNode handNode = XRNode.LeftHand;

    public Transform palmCollider;  // Assign a GameObject with SphereCollider + Rigidbody (isKinematic) here
    public Transform xrOrigin;       // Assign XR Origin if used for coordinate transform

    void Update()
    {
        if (handSubsystem == null || palmCollider == null) return;

        XRHand hand = handNode == XRNode.LeftHand ? handSubsystem.leftHand : handSubsystem.rightHand;
        if (!hand.isTracked) return;

        if (hand.GetJoint(XRHandJointID.Palm).TryGetPose(out Pose pose))
        {
            if (xrOrigin != null)
            {
                palmCollider.position = xrOrigin.TransformPoint(pose.position);
                palmCollider.rotation = xrOrigin.rotation * pose.rotation;
            }
            else
            {
                palmCollider.position = pose.position;
                palmCollider.rotation = pose.rotation;
            }
        }
    }
}

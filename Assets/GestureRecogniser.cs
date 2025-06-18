using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR;

public class GestureRecognizer : MonoBehaviour
{
    public XRHandSubsystem handSubsystem;
    public XRNode handNode = XRNode.RightHand;

    void Update()
    {
        XRHand hand = handNode == XRNode.LeftHand ? handSubsystem.leftHand : handSubsystem.rightHand;
        if (!hand.isTracked) return;

        if (IsPinch(hand))
        {
            Debug.Log("Pinch detected!");
        }
        else if (IsFist(hand))
        {
            Debug.Log("Fist detected!");
        }
        // Add more gestures here...
    }

    bool IsPinch(XRHand hand)
    {
        if (TryGetJointPos(hand, XRHandJointID.ThumbTip, out var thumb) &&
            TryGetJointPos(hand, XRHandJointID.IndexTip, out var index))
        {
            return Vector3.Distance(thumb, index) < 0.03f;
        }
        return false;
    }

    bool IsFist(XRHand hand)
    {
        // Check if all fingertips are close to wrist
        if (TryGetJointPos(hand, XRHandJointID.Wrist, out var wrist))
        {
            XRHandJointID[] fingertips = {
                XRHandJointID.ThumbTip, XRHandJointID.IndexTip,
                XRHandJointID.MiddleTip, XRHandJointID.RingTip,
                XRHandJointID.LittleTip
            };

            foreach (var tip in fingertips)
            {
                if (TryGetJointPos(hand, tip, out var tipPos))
                {
                    if (Vector3.Distance(tipPos, wrist) > 0.08f)
                        return false;
                }
            }
            return true;
        }
        return false;
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
}

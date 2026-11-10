using System;
using UnityEngine;

public enum PoseType
{
    Standing,
    Sitting,
    Crouching,
    HandsRaised
}

/// <summary>
/// Infers the user's body pose from head and hand/controller tracking only - the headset has no
/// body tracking. All thresholds below are heuristics and are public so they can be tuned on the
/// actual device.
/// </summary>
public class PoseDetector : MonoBehaviour
{
    [Header("Tracked transforms")]
    public Transform head;
    public Transform leftHandOrController;
    public Transform rightHandOrController;

    [Header("Calibration")]
    [Tooltip("Head height (m) while standing. Defaults to 1.6m until CalibrateStandingHeight is called.")]
    public float standingHeight = 1.6f;

    [Header("Heuristic thresholds (tune on device)")]
    [Tooltip("Sitting: head height ratio to standingHeight is between crouchingRatio and this value.")]
    public float sittingUpperRatio = 0.85f;
    [Tooltip("Crouching: head height ratio to standingHeight is below this value (also Sitting's lower bound).")]
    public float crouchingRatio = 0.65f;
    [Tooltip("Seconds a pose must be held before it is raised as an event, to avoid flicker.")]
    public float PoseHoldSeconds = 0.5f;

    /// <summary>Raised once, when a pose is newly entered and has been held for PoseHoldSeconds.</summary>
    public event Action<PoseType> OnPoseEntered;

    public PoseType CurrentPose { get; private set; } = PoseType.Standing;

    private PoseType pendingPose = PoseType.Standing;
    private float pendingPoseSince;
    private bool hasPendingPose;

    public void CalibrateStandingHeight()
    {
        if (head == null) return;
        standingHeight = head.position.y;
    }

    private void Update()
    {
        if (head == null) return;

        PoseType detected = DetectPose();

        if (detected == CurrentPose)
        {
            hasPendingPose = false;
            return;
        }

        if (!hasPendingPose || pendingPose != detected)
        {
            pendingPose = detected;
            pendingPoseSince = Time.time;
            hasPendingPose = true;
            return;
        }

        if (Time.time - pendingPoseSince >= PoseHoldSeconds)
        {
            CurrentPose = detected;
            hasPendingPose = false;
            OnPoseEntered?.Invoke(CurrentPose);
        }
    }

    private PoseType DetectPose()
    {
        if (IsHandAboveHead(leftHandOrController) || IsHandAboveHead(rightHandOrController))
        {
            return PoseType.HandsRaised;
        }

        float ratio = standingHeight > 0f ? head.position.y / standingHeight : 1f;

        if (ratio < crouchingRatio) return PoseType.Crouching;
        if (ratio < sittingUpperRatio) return PoseType.Sitting;
        return PoseType.Standing;
    }

    private bool IsHandAboveHead(Transform hand)
    {
        return hand != null && hand.position.y > head.position.y;
    }

    /// <summary>The rule-text verb phrase for a pose (e.g. "sits down"), matching ECAEvent.EventStr
    /// for Pose modality events, as recorded by InteractionCreationController.RecordPoseEvent.</summary>
    public static string VerbPhraseFor(PoseType pose)
    {
        switch (pose)
        {
            case PoseType.Standing: return "stands";
            case PoseType.Sitting: return "sits down";
            case PoseType.Crouching: return "crouches";
            case PoseType.HandsRaised: return "raises the hands";
            default: return pose.ToString();
        }
    }
}

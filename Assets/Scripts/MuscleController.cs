using System;
using System.Collections;
using System.Collections.Generic;
using RootMotion.FinalIK;
using UnityEngine;

public enum MuscleIndex
{
    SpineFrontBack = 0,
    SpineLeftRight = 1,
    SpineTwistLeftRight = 2,
    ChestFrontBack = 3,
    ChestLeftRight = 4,
    ChestTwistLeftRight = 5,
    UpperChestFrontBack = 6,
    UpperChestLeftRight = 7,
    UpperChestTwistLeftRight = 8,
    NeckNodDownUp = 9,
    NeckTiltLeftRight = 10,
    NeckTurnLeftRight = 11,
    HeadNodDownUp = 12,
    HeadTiltLeftRight = 13,
    HeadTurnLeftRight = 14,
    LeftEyeDownUp = 15,
    LeftEyeInOut = 16,
    RightEyeDownUp = 17,
    RightEyeInOut = 18,
    JawClose = 19,
    JawLeftRight = 20,
    LeftUpperLegFrontBack = 21,
    LeftUpperLegInOut = 22,
    LeftUpperLegTwistInOut = 23,
    LeftLowerLegStretch = 24,
    LeftLowerLegTwistInOut = 25,
    LeftFootUpDown = 26,
    LeftFootTwistInOut = 27,
    LeftToesUpDown = 28,
    RightUpperLegFrontBack = 29,
    RightUpperLegInOut = 30,
    RightUpperLegTwistInOut = 31,
    RightLowerLegStretch = 32,
    RightLowerLegTwistInOut = 33,
    RightFootUpDown = 34,
    RightFootTwistInOut = 35,
    RightToesUpDown = 36,
    LeftShoulderDownUp = 37,
    LeftShoulderFrontBack = 38,
    LeftArmDownUp = 39,
    LeftArmFrontBack = 40,
    LeftArmTwistInOut = 41,
    LeftForearmStretch = 42,
    LeftForearmTwistInOut = 43,
    LeftHandDownUp = 44,
    LeftHandInOut = 45,
    RightShoulderDownUp = 46,
    RightShoulderFrontBack = 47,
    RightArmDownUp = 48,
    RightArmFrontBack = 49,
    RightArmTwistInOut = 50,
    RightForearmStretch = 51,
    RightForearmTwistInOut = 52,
    RightHandDownUp = 53,
    RightHandInOut = 54,
    LeftThumb1Stretched = 55,
    LeftThumbSpread = 56,
    LeftThumb2Stretched = 57,
    LeftThumb3Stretched = 58,
    LeftIndex1Stretched = 59,
    LeftIndexSpread = 60,
    LeftIndex2Stretched = 61,
    LeftIndex3Stretched = 62,
    LeftMiddle1Stretched = 63,
    LeftMiddleSpread = 64,
    LeftMiddle2Stretched = 65,
    LeftMiddle3Stretched = 66,
    LeftRing1Stretched = 67,
    LeftRingSpread = 68,
    LeftRing2Stretched = 69,
    LeftRing3Stretched = 70,
    LeftLittle1Stretched = 71,
    LeftLittleSpread = 72,
    LeftLittle2Stretched = 73,
    LeftLittle3Stretched = 74,
    RightThumb1Stretched = 75,
    RightThumbSpread = 76,
    RightThumb2Stretched = 77,
    RightThumb3Stretched = 78,
    RightIndex1Stretched = 79,
    RightIndexSpread = 80,
    RightIndex2Stretched = 81,
    RightIndex3Stretched = 82,
    RightMiddle1Stretched = 83,
    RightMiddleSpread = 84,
    RightMiddle2Stretched = 85,
    RightMiddle3Stretched = 86,
    RightRing1Stretched = 87,
    RightRingSpread = 88,
    RightRing2Stretched = 89,
    RightRing3Stretched = 90,
    RightLittle1Stretched = 91,
    RightLittleSpread = 92,
    RightLittle2Stretched = 93,
    RightLittle3Stretched = 94,
}

[Serializable]
public struct Synergy
{
    public MuscleIndex source;
    public MuscleIndex target;
    public float weight;
    public float reference;
}

public class MuscleController : MonoBehaviour
{
    [SerializeField] private VRIK _vrik;
    [SerializeField] private Animator _animator;
    [SerializeField] private Animator _ghostAnimator;

    public Synergy[] synergies = new Synergy[] {
    };

    private HumanPose _humanPose = new();
    private HumanPoseHandler _humanPoseHandler;
    private HumanPoseHandler _ghostHumanPoseHandler;

    private void Awake()
    {
        if (_vrik == null)
        {
            _vrik = GetComponent<VRIK>();
        }

        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }

        if (_vrik == null || _animator == null)
        {
            Debug.LogError("VRIK or Animator component is missing.");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        _vrik.solver.OnPostUpdate += OnVRIKPostSolve;
    }

    private void OnVRIKPostSolve()
    {
        if (_ghostAnimator == null) return;
        _humanPoseHandler ??= new HumanPoseHandler(_animator.avatar, _animator.transform);
        _ghostHumanPoseHandler ??= new HumanPoseHandler(_ghostAnimator.avatar, _ghostAnimator.transform);

        _humanPoseHandler.GetHumanPose(ref _humanPose);

        HumanPose _ghostHumanPose = new()
        {
            bodyPosition = _humanPose.bodyPosition,
            bodyRotation = _humanPose.bodyRotation,
            muscles = new float[_humanPose.muscles.Length]
        };
        for (int i = 0; i < _humanPose.muscles.Length; i++)
        {
            _ghostHumanPose.muscles[i] = _humanPose.muscles[i];
        }

        for (int i = 0; i < synergies.Length; i++)
        {
            var sourceMuscle = (int)synergies[i].source;
            var targetMuscle = (int)synergies[i].target;
            var sourceValue = _humanPose.muscles[sourceMuscle];
            var targetValue = _humanPose.muscles[targetMuscle];
            var newValue = targetValue + (sourceValue - synergies[i].reference) * synergies[i].weight;
            
            var firstOffset = (1 - synergies[i].reference) * synergies[i].weight;
            var secondOffset = (-1 - synergies[i].reference) * synergies[i].weight;
            var minValue = -1 + System.Math.Min(firstOffset, secondOffset);
            var maxValue = 1 + System.Math.Max(firstOffset, secondOffset);
            _ghostHumanPose.muscles[targetMuscle] = MapRange(newValue, minValue, maxValue, -1, 1);
        }

        _ghostHumanPose.bodyPosition = _humanPose.bodyPosition;
        _ghostHumanPose.bodyRotation = _humanPose.bodyRotation;

        _ghostHumanPoseHandler.SetHumanPose(ref _ghostHumanPose);

        _ghostAnimator.Update(0f);
    }

    private float MapRange(float value, float inMin, float inMax, float outMin, float outMax)
    {
        return (value - inMin) / (inMax - inMin) * (outMax - outMin) + outMin;
    }

}

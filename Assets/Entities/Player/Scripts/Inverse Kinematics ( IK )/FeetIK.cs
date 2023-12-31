using UnityEngine;

public class FeetIK : MonoBehaviour
{
    public bool DebugRays = true;
    public bool IsEnabled = true;
    public Animator Animator { get; set; }
    public FallSettings FallSettings;

    [Range(0, 2)][SerializeField] private float heightFromGroundRaycast = 1.14f;
    [Range(0, 2)][SerializeField] private float raycastDownDistance = 1.5f;
    [SerializeField] private float pelvisOffset = 0f;
    [Range(0, 1)][SerializeField] private float pelvisUpAndDownSpeed = 0.28f;
    [Range(0, 1)][SerializeField] private float feetToIkPositionSpeed = 0.5f;

    public string leftFootAnimVariableName = "LeftFootCurve";
    public string rightFootAnimVariableName = "RightFootCurve";
    [Range(-4, 5)]
    public float littlePelvisOffset;
    public bool forceFeetIk;

    private Vector3 rightFootPosition;
    private Vector3 leftFootPosition;

    [HideInInspector] public Vector3 leftFootIkPosition;
    [HideInInspector] public Vector3 rightFootIkPosition;
    [HideInInspector] public Quaternion leftFootIkRotation;
    [HideInInspector] public Quaternion rightFootIkRotation;

    private bool m_useHints;
    public bool UseHints { get { return m_useHints; } set { m_useHints = value; } }
    public Vector3 LeftIKHint { get; private set; }
    public Vector3 RightIKHint { get; private set; }

    private float lOffsetPosition;
    private float rOffsetPosition;

    private float lastPelvisPositionY;

    private float lastRightFootPositionY;
    private float lastLeftFootPositionY;


    private void FixedUpdate()
    {
        if (IsEnabled)
        {
            AdjustFeetTarget(ref rightFootPosition, HumanBodyBones.RightFoot);
            AdjustFeetTarget(ref leftFootPosition, HumanBodyBones.LeftFoot);

            //find and raycast to the ground to find positions
            FeetPositionSolver(rightFootPosition, ref rightFootIkPosition, ref rightFootIkRotation); // handle the solver for right foot
            FeetPositionSolver(leftFootPosition, ref leftFootIkPosition, ref leftFootIkRotation); //handle the solver for the left foot
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (forceFeetIk)
        {
            Animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
            Animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
            Animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
            Animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
            Animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootIkRotation);
            Animator.SetIKRotation(AvatarIKGoal.RightFoot, rightFootIkRotation);
            Animator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootIkPosition);
            Animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootIkPosition);

            if (UseHints)
            {
                Animator.SetIKHintPositionWeight(AvatarIKHint.RightKnee, 1);
                Animator.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, 1);
                Animator.SetIKHintPosition(AvatarIKHint.LeftKnee, LeftIKHint);
                Animator.SetIKHintPosition(AvatarIKHint.RightKnee, RightIKHint);
            }

            return;
        }

        if (IsEnabled)
        {
            Animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, Animator.GetFloat(rightFootAnimVariableName));
            Animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, Animator.GetFloat(rightFootAnimVariableName));
            MoveFeetToIkPoint(AvatarIKGoal.RightFoot, rightFootIkPosition, rightFootIkRotation, ref lastRightFootPositionY);
            Animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, Animator.GetFloat(leftFootAnimVariableName));
            Animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, Animator.GetFloat(leftFootAnimVariableName));
            MoveFeetToIkPoint(AvatarIKGoal.LeftFoot, leftFootIkPosition, leftFootIkRotation, ref lastLeftFootPositionY);
            MovePelvisHeight();
        }
    }

    #region FeetGroundingMethods

    void MoveFeetToIkPoint(AvatarIKGoal foot, Vector3 positionIkHolder, Quaternion rotationIkHolder, ref float lastFootPositionY)
    {
        Vector3 targetIkPosition = Animator.GetIKPosition(foot);

        if (positionIkHolder != Vector3.zero)
        {
            targetIkPosition = transform.InverseTransformPoint(targetIkPosition);
            positionIkHolder = transform.InverseTransformPoint(positionIkHolder);

            float yVariable = Mathf.Lerp(lastFootPositionY, positionIkHolder.y, feetToIkPositionSpeed);
            targetIkPosition.y += yVariable;

            lastFootPositionY = yVariable;

            targetIkPosition = transform.TransformPoint(targetIkPosition);

            Animator.SetIKRotation(foot, rotationIkHolder);
        }

        Animator.SetIKPosition(foot, targetIkPosition);
    }

    private void MovePelvisHeight()
    {
        if (rightFootIkPosition == Vector3.zero || leftFootIkPosition == Vector3.zero || lastPelvisPositionY == 0)
        {
            lastPelvisPositionY = Animator.bodyPosition.y;
            return;
        }

        lOffsetPosition = leftFootIkPosition.y - transform.position.y;
        rOffsetPosition = rightFootIkPosition.y - transform.position.y;

        float totalOffset = (lOffsetPosition < rOffsetPosition) ? lOffsetPosition : rOffsetPosition;

        if (totalOffset <= -0.45f && totalOffset > -0.5f)
        {
            littlePelvisOffset = 3.7f;
        }
        else if (totalOffset <= -0.3f && totalOffset > -0.4f)
        {
            littlePelvisOffset = 4.66f;
        }
        else if (totalOffset <= -0.6f && totalOffset > -0.7f)
        {
            littlePelvisOffset = 3.33f;
        }

        Vector3 newPelvisPosition = Animator.bodyPosition + (Vector3.up * (totalOffset / littlePelvisOffset));

        newPelvisPosition.y = Mathf.Lerp(lastPelvisPositionY, newPelvisPosition.y, pelvisUpAndDownSpeed);

        Animator.bodyPosition = newPelvisPosition;

        lastPelvisPositionY = Animator.bodyPosition.y;
    }

    private void FeetPositionSolver(Vector3 fromSkyPosition, ref Vector3 feetIkPositions, ref Quaternion feetIkRotations)
    {
        if (Physics.Raycast(fromSkyPosition, Vector3.down, out RaycastHit feetOutHit, raycastDownDistance + heightFromGroundRaycast, FallSettings.GroundLayers))
        {
            //finding our feet ik positions from the sky position
            feetIkPositions = fromSkyPosition;
            feetIkPositions.y = feetOutHit.point.y + pelvisOffset;
            feetIkRotations = Quaternion.FromToRotation(Vector3.up, feetOutHit.normal) * transform.rotation;


            if (DebugRays)
            {
                if (feetOutHit.point == Vector3.zero)
                {
                    Debug.DrawLine(fromSkyPosition, fromSkyPosition + Vector3.down * (raycastDownDistance + heightFromGroundRaycast), Color.yellow);
                }
                else
                {
                    Debug.DrawLine(fromSkyPosition, feetOutHit.point, Color.yellow);
                }
            }

            return;
        }

        feetIkPositions = Vector3.zero; //it didn't work :(
    }

    private void AdjustFeetTarget(ref Vector3 feetPositions, HumanBodyBones foot)
    {
        feetPositions = Animator.GetBoneTransform(foot).position;
        feetPositions.y = transform.position.y + heightFromGroundRaycast;
    }

    public void SetFeetIkEnabled(int enable)
    {
        IsEnabled = enable == 1;
    }

    #endregion


    public void SetIKHints(bool enable, Vector3 left, Vector3 right)
    {
        LeftIKHint = left;
        RightIKHint = right;
        UseHints = enable;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))] // Requires animator with parameter "flySpeed" catering for 0, 1 (idle, flap)
[RequireComponent(typeof(Rigidbody))] // Requires Rigidbody to move around

public class RandomFlyer : MonoBehaviour
{
    [SerializeField] float idleSpeed, turnSpeed, switchSeconds, idleRatio;
    [SerializeField] Vector2 animSpeedMinMax, moveSpeedMinMax, changeAnimEveryFromTo, changeTargetEveryFromTo;
    [SerializeField] Transform homeTarget, flyingTarget;
    [SerializeField] Vector2 radiusMinMax;
    [SerializeField] Vector2 yMinMax;
    [SerializeField] private float yMinReturning = 0.01f;
    private float defaultYMin;
    public bool returnToBase = false;
    public float delayStart = 0f;

    private readonly int flySpeedHash = Animator.StringToHash("flySpeed");
    private readonly int landHash = Animator.StringToHash("Land");
    private readonly int flyHash = Animator.StringToHash("Fly");
    public float minDistanceToBase = 0.1f;


    private Animator animator;
    private Rigidbody body;
    [System.NonSerialized]
    public float changeTarget = 0f, changeAnim = 0f, timeSinceTarget = 0f, timeSinceAnim = 0f, prevAnim, currentAnim = 0f, prevSpeed, speed, zturn, prevz,
        turnSpeedBackup;
    private Vector3 rotateTarget, position, direction;
    private Quaternion lookRotation;
    public float distanceFromBase, distanceFromTarget;


    void Start()
    {
        // Inititalize
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody>();
        turnSpeedBackup = turnSpeed;
        direction = Quaternion.Euler(transform.eulerAngles) * (Vector3.forward);
        defaultYMin = yMinMax.x;
        if (delayStart < 0f) body.velocity = idleSpeed * direction;
    }

    void FixedUpdate()
    {
        // Wait if start should be delayed (useful to add small differences in large flocks)
        if (delayStart > 0f)
        {
            delayStart -= Time.fixedDeltaTime;
            return;
        }
        // Calculate distances
        distanceFromBase = Vector3.Distance(homeTarget.position ,body.position);
        distanceFromTarget = Vector3.Magnitude(flyingTarget.position - body.position);
        // Allow landing on exact Spot
        if (returnToBase && yMinReturning < yMinMax.x)
        {
            yMinMax.x = yMinReturning;
        }
        else if (!returnToBase)
        {
            if(yMinMax.x < defaultYMin)
            {
                yMinMax.x = Mathf.Lerp(yMinMax.x, defaultYMin, 1 * Time.deltaTime);
            }
                
        }
        // Allow drastic turns close to base to ensure target can be reached
        if (returnToBase && distanceFromBase < 10f)
        {
            if (turnSpeed != 300f && body.velocity.magnitude != 0f)
            {
                turnSpeedBackup = turnSpeed;
                turnSpeed = 300f;
            }
            else if (distanceFromBase <= minDistanceToBase)
            {
                body.velocity = Vector3.zero;
                turnSpeed = turnSpeedBackup;
                return;
            }
        }

        // Time for a new animation speed
        if (returnToBase && distanceFromBase <= 0.5f)
        {
            animator.SetTrigger(landHash);
            animator.SetBool(flyHash, false);
        } 
        else if (changeAnim < 0f)
        {
            prevAnim = currentAnim;
            currentAnim = ChangeAnim(currentAnim);
            changeAnim = Random.Range(changeAnimEveryFromTo.x, changeAnimEveryFromTo.y);
            timeSinceAnim = 0f;
            prevSpeed = speed;
            animator.SetBool(flyHash, true);
            if (currentAnim == 0)
            {
                speed = idleSpeed;
            }
            else
            {
                speed = Mathf.Lerp(moveSpeedMinMax.x, moveSpeedMinMax.y, (currentAnim - animSpeedMinMax.x) / (animSpeedMinMax.y - animSpeedMinMax.x));
            }
        }



        // Time for a new target position
        if (changeTarget < 0f)
        {
            rotateTarget = ChangeDirection(body.transform.position);
            if (returnToBase)
            {
                changeTarget = 0.2f;
            }
            else
            {
                changeTarget = Random.Range(changeTargetEveryFromTo.x, changeTargetEveryFromTo.y);
            }

            timeSinceTarget = 0f;
        }
        // Turn when approaching height limits
        // ToDo: Adjust limit and "exit direction" by object's direction and velocity, instead of the 10f and 1f - this works in my current scenario/scale
        if (body.transform.position.y < yMinMax.x + minDistanceToBase || body.transform.position.y > yMinMax.y - minDistanceToBase)
        {
            if (body.transform.position.y < yMinMax.x + minDistanceToBase)
            {
                rotateTarget.y = 1f;
            }
            else
            {
                rotateTarget.y = -1f;
            }
        }
        //body.transform.Rotate(0f, 0f, -prevz, Space.Self); // If required to make Quaternion.LookRotation work correctly, but it seems to be fine
        zturn = Mathf.Clamp(Vector3.SignedAngle(rotateTarget, direction, Vector3.up), -45f, 45f);
        // Update times
        changeAnim -= Time.fixedDeltaTime;
        changeTarget -= Time.fixedDeltaTime;
        timeSinceTarget += Time.fixedDeltaTime;
        timeSinceAnim += Time.fixedDeltaTime;

        // Rotate towards target
        if (rotateTarget != Vector3.zero) lookRotation = Quaternion.LookRotation(rotateTarget, Vector3.up);
        Vector3 rotation = Quaternion.RotateTowards(body.transform.rotation, lookRotation, turnSpeed * Time.fixedDeltaTime).eulerAngles;
        if(returnToBase && distanceFromBase <= 0.5f)
        {
            body.transform.rotation = Quaternion.LerpUnclamped(body.transform.rotation, homeTarget.rotation, 10 * Time.deltaTime);
        }
        else
        {
            body.transform.eulerAngles = rotation;
        }
        // Rotate on z-axis to tilt body towards turn direction
        float temp = prevz;
        if (prevz < zturn) prevz += Mathf.Min(turnSpeed * Time.fixedDeltaTime, zturn - prevz);
        else if (prevz >= zturn) prevz -= Mathf.Min(turnSpeed * Time.fixedDeltaTime, prevz - zturn);
        // Min and max rotation on z-axis - can also be parameterized
        prevz = Mathf.Clamp(prevz, -45f, 45f);
        // Remove temp if transform is rotated back earlier in FixedUpdate
        if (!returnToBase && distanceFromBase <= 0.5f)
        {
            body.transform.Rotate(0f, 0f, prevz - temp, Space.Self);
        }
            
        if(distanceFromBase < 0.1f)
        {
            return;
        }

        // Move flyer
        direction = Quaternion.Euler(transform.eulerAngles) * Vector3.forward;
        if (returnToBase && distanceFromBase < 0.5f)
        {
            //body.velocity = Mathf.Min(idleSpeed, distanceFromBase) * direction;
            body.MovePosition(homeTarget.position);
            return;
            //print("chahcahcha");
        }
        else body.velocity = Mathf.Lerp(prevSpeed, speed, Mathf.Clamp(timeSinceAnim / switchSeconds, 0f, 1f)) * direction;
        // Hard-limit the height, in case the limit is breached despite of the turnaround attempt
        if (body.transform.position.y < yMinMax.x || body.transform.position.y > yMinMax.y)
        {
            position = body.transform.position;
            position.y = Mathf.Clamp(position.y, yMinMax.x, yMinMax.y);
            body.transform.position = position;
        }
    }

    // Select a new animation speed randomly
    private float ChangeAnim(float currentAnim)
    {
        float newState;
        if (Random.Range(0f, 1f) < idleRatio) newState = 0f;
        else
        {
            newState = Random.Range(animSpeedMinMax.x, animSpeedMinMax.y);
        }
        if (newState != currentAnim)
        {
            animator.SetFloat(flySpeedHash, newState);
            if (newState == 0) animator.speed = 1f; else animator.speed = newState;
        }
        return newState;
    }

    // Select a new direction to fly in randomly
    private Vector3 ChangeDirection(Vector3 currentPosition)
    {
        Vector3 newDir;
        if (returnToBase)
        {
            newDir = homeTarget.position - currentPosition;
        }
        else if (distanceFromTarget > radiusMinMax.y)
        {
            newDir = flyingTarget.position - currentPosition;
        }
        else if (distanceFromTarget < radiusMinMax.x)
        {
            newDir = currentPosition - flyingTarget.position;
        }
        else
        {
            // 360-degree freedom of choice on the horizontal plane
            float angleXZ = Random.Range(-Mathf.PI, Mathf.PI);
            // Limited max steepness of ascent/descent in the vertical direction
            float angleY = Random.Range(-Mathf.PI / 48f, Mathf.PI / 48f);
            // Calculate direction
            newDir = Mathf.Sin(angleXZ) * Vector3.forward + Mathf.Cos(angleXZ) * Vector3.right + Mathf.Sin(angleY) * Vector3.up;
        }
        return newDir.normalized;
    }

    public void ResetTurningSpeed()
    {
        turnSpeed = turnSpeedBackup;
    }
}

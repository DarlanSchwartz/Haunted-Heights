using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
#if UNITY_EDITOR
using Schwartz.MeshCreator;
#endif

public enum GrabTypes
{
    FreeHang,
    BracedHang
}

public enum BragableMovementType
{
    Static,
    Dynamic
}


public class GrabableObject : MonoBehaviour
{
    public GrabTypes GrabType = GrabTypes.BracedHang;
    public BragableMovementType Movement = BragableMovementType.Static;
    public bool fixedPositions = true;
    public Transform leftHandIKTarget;
    public Transform rightHandIKTarget;

    public Transform rightElbowTarget;
    public Transform leftElbowTarget;

    public Transform startTarget;
    public Transform endTarget;
    public Transform LookAtHanging;

    public List<GrabableObjectNeighBoor> Neighboors;

    [Range(1,100)]
    public int addAmount = 1;

    private Mesh endClimbMesh = null;
    private Mesh colliderMesh = null;

    #if UNITY_EDITOR
    private void OnValidate()
    {
        
        endClimbMesh = MeshCreator.GetPolyMesh(30, 1);
        if (GetComponent<MeshCollider>().sharedMesh == null)
        {
            GetComponent<MeshCollider>().sharedMesh = Resources.Load("PlaneMesh", typeof(Mesh)) as Mesh;
        }

        colliderMesh = GetComponent<MeshCollider>().sharedMesh;
       
    }

    private void OnDrawGizmos()
    {
        if(!EditorApplication.isPlaying)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawWireMesh(colliderMesh, transform.position, transform.rotation, Vector3.one);

            if (startTarget != null && endTarget != null)
            {
                Gizmos.color = Color.blue;

                Gizmos.DrawLine(startTarget.position, endTarget.position);
            }

            Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(startTarget.position, 0.05f);

            if (endTarget != null)
            {

                Gizmos.color = Color.blue;

                Gizmos.DrawMesh(endClimbMesh, endTarget.position, Quaternion.Euler(90, 0, 0), Vector3.one * 0.5f);

                Gizmos.color = Color.red;

                Gizmos.DrawSphere(endTarget.position, 0.05f);
            }

            Gizmos.color = Color.blue;

            Gizmos.DrawSphere(leftHandIKTarget.position, 0.02f);

            Gizmos.DrawSphere(rightHandIKTarget.position, 0.02f);

            Gizmos.color = Color.green;

            Gizmos.DrawLine(leftHandIKTarget.position, rightHandIKTarget.position);
        }
    }

#endif

    [ContextMenu("Add Front")]
    private void AddToTheFront()
    {
        Vector3 targetPosition = transform.forward;
        CreateNext(targetPosition, addAmount);
    }
    [ContextMenu("Add Back")]
    private void AddToTheBack()
    {
        Vector3 targetPosition = -transform.forward; 
        CreateNext(targetPosition, addAmount);
    }
    [ContextMenu("Add Left")]
    private void AddToTheLeft()
    {
        Vector3 targetPosition = -transform.right;
        CreateNext(targetPosition, addAmount);
    }
    [ContextMenu("Add Right")]
    private void AddToTheRight()
    {
        Vector3 targetPosition = transform.right;
        CreateNext(targetPosition, addAmount);
    }
    private void CreateNext(Vector3 offset, int amount)
    {

        #if UNITY_EDITOR

        if (amount == 0)
        {
            addAmount = 1;
            return;
        }
            

        for (int i = 0; i < amount; i++)
        {
            GameObject temp = null;

            temp = PrefabUtility.InstantiatePrefab(Resources.Load("BracedHang_Point")) as GameObject;

            temp.transform.SetParent(transform.parent);

            Vector3 targetPosition = transform.localPosition;

            targetPosition += offset * (i +1);

            temp.transform.localPosition = targetPosition;

            temp.transform.rotation = transform.rotation;

            temp.GetComponent<GrabableObject>().addAmount = addAmount;

            Selection.activeTransform = temp.transform;
           
        }

        #endif
    }
}

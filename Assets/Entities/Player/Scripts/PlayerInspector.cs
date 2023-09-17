using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PlayerInspector : MonoBehaviour
{
    public bool isInspecting;
    public float inspectingFocus = 1;
    public float defaultFocus = 6;
    public Transform objectFocus;
    public PostProcessProfile profile;
    private DepthOfField DOF;
    [SerializeField]
    private PlayerMove pmove;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    public Transform inspectPositionTransform;
    private RaycastHit m_fwdHit;
    [Range(0,359)]
    public float turnSpeed = 90;
    [SerializeField] private string InspectableTag = "Item";
    [SerializeField] private string LadderTag = "Ladder";
    //public PostProcessingProfile profile;

    // Start is called before the first frame update
    void Awake()
    {
        //profile.TryGetSettings(out DOF);
        //DOF.focusDistance.value = defaultFocus;
    }

    private Vector3 targetRot;

    // Update is called once per frame
    void Update()
    {
        if(!isInspecting)
        {
            if (Physics.Raycast(pmove.PlayerCamera.position, pmove.PlayerCamera.forward, out m_fwdHit,2))
            {
                if (m_fwdHit.transform.CompareTag(InspectableTag))
                {
                    if (Input.GetMouseButtonUp(0))
                    {
                        isInspecting = true;
                        pmove.m_inspecting = true;
                        objectFocus = m_fwdHit.transform;
                        objectFocus.GetComponent<Collider>().enabled = false;
                        originalPosition = objectFocus.transform.position;
                        originalRotation = objectFocus.transform.rotation;
                        //DOF.active = true;
                        //DOF.focusDistance.value = inspectingFocus;
                        targetRot.x = objectFocus.transform.rotation.eulerAngles.x;
                        targetRot.y = objectFocus.transform.rotation.eulerAngles.y;
                        targetRot.z = objectFocus.transform.rotation.eulerAngles.z;
                    }
                }

                if(Input.GetKeyDown(KeyCode.E) && m_fwdHit.transform.CompareTag(LadderTag))
                {
                    if(pmove.InLadderState)
                    {
                        return;
                    }

                   // Debug.Log(Quaternion.Angle(pmove.transform.rotation, m_fwdHit.transform.rotation));

                    pmove.EnterLadder(m_fwdHit.transform.GetComponent<Ladder>());
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                StopInspecting();
                return;
            }

            objectFocus.transform.position = Vector3.Slerp(objectFocus.transform.position, inspectPositionTransform.transform.position, 10 * Time.deltaTime);
            targetRot.x += -Input.GetAxis("Mouse Y") * turnSpeed * Time.deltaTime;
            targetRot.y += Input.GetAxis("Mouse X") * turnSpeed * Time.deltaTime;
            objectFocus.transform.rotation = Quaternion.Slerp(objectFocus.transform.rotation, Quaternion.Euler(targetRot), 30 * Time.deltaTime);
        }
    }

    void StopInspecting()
    {
        isInspecting = false;
        //DOF.focusDistance.value = defaultFocus;
        objectFocus.GetComponent<Collider>().enabled = true;
        objectFocus.transform.position = originalPosition;
        objectFocus.transform.rotation = originalRotation;
        objectFocus = null;
        pmove.m_inspecting = false;
        targetRot = Vector3.zero;
        //DOF.active = false;
    }
}

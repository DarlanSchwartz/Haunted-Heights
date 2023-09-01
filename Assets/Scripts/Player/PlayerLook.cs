using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public float XSensitivity = 2f;
    public float YSensitivity = 2f;
    public bool ClampVerticalRotation = true;
    public bool ClampHorizontalRotation = false;
    public float MinX = -90F;
    public float MaxX = 75F;

    public float MinY = -360;
    public float MaxY = 360;
    public bool Smooth;
    public float SmoothTime = 20f;

    public bool UpdateRotation { get; set; }

    public Transform  PlayerCamera { get; private set; }
    public Transform PlayerCharacter { get; private set; }

    public Vector3 CameraTargetRot;
    public Vector3 CharacterTargetRot;

    public void Init(Transform character, Transform camera)
    {
        CharacterTargetRot = character.localRotation.eulerAngles;
        CameraTargetRot = camera.localRotation.eulerAngles;
        PlayerCharacter = character;
        PlayerCamera = camera;
        UpdateRotation = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    float tempVal;

    public void LookRotation(bool lookPlayer)
    {
        if (!UpdateRotation)
        {
            return;
        }

        CameraTargetRot.x += MouseVertical;

        if(ClampVerticalRotation)
        {
            CameraTargetRot.x = Mathf.Clamp(CameraTargetRot.x, MinX, MaxX);
        }
                
        if(!lookPlayer)
        {
            if(CameraTargetRot.y >= 0)
            {
                if (CameraTargetRot.y + MouseHorizontal < 360)
                {
                    CameraTargetRot.y += MouseHorizontal;
                }
                else if (CameraTargetRot.y + MouseHorizontal > 360)
                {
                    tempVal = CameraTargetRot.y + MouseHorizontal;
                    tempVal -= 360;
                    CameraTargetRot.y = tempVal;
                }
            }
            else
            {
                if (CameraTargetRot.y + MouseHorizontal > -360)
                {
                    CameraTargetRot.y += MouseHorizontal;
                }
                else if (CameraTargetRot.y + MouseHorizontal < -360)
                {
                    tempVal = CameraTargetRot.y + MouseHorizontal;
                    tempVal += 360;
                    CameraTargetRot.y = tempVal;
                }
            }
        }
        else
        {
            if (CharacterTargetRot.y >= 0)
            {
                if (CharacterTargetRot.y + MouseHorizontal < 360)
                {
                    CharacterTargetRot.y += MouseHorizontal;
                }
                else if (CharacterTargetRot.y + MouseHorizontal > 360)
                {
                    tempVal = CharacterTargetRot.y + MouseHorizontal;
                    tempVal -= 360;
                    CharacterTargetRot.y = tempVal;
                }
            }
            else
            {
                if (CharacterTargetRot.y + MouseHorizontal > -360)
                {
                    CharacterTargetRot.y += MouseHorizontal;
                }
                else if (CharacterTargetRot.y + MouseHorizontal < -360)
                {
                    tempVal = CharacterTargetRot.y + MouseHorizontal;
                    tempVal += 360;
                    CharacterTargetRot.y = tempVal;
                }
            }
        }
            

        if (ClampHorizontalRotation)
        {
            if(!lookPlayer)
            {
                CameraTargetRot.y = Mathf.Clamp(CameraTargetRot.y, MinY, MaxY);
            }
            else
            {
                CharacterTargetRot.y = Mathf.Clamp(CharacterTargetRot.y, MinY, MaxY);
            }
        }

        CameraTargetRot.z = 0;

        if(!lookPlayer)
        {
            if (Smooth)
            {
                PlayerCamera.localRotation = Quaternion.Slerp(PlayerCamera.localRotation, Quaternion.Euler(CameraTargetRot), SmoothTime * Time.deltaTime);
            }
            else
            {
                PlayerCamera.localRotation = Quaternion.Euler(CameraTargetRot);
            }
        }
        else
        {
            if (Smooth)
            {
                CameraTargetRot.y = 0;
                PlayerCamera.localRotation = Quaternion.Slerp(PlayerCamera.localRotation, Quaternion.Euler(CameraTargetRot), SmoothTime * Time.deltaTime);
                PlayerCharacter.localRotation = Quaternion.Slerp(PlayerCharacter.localRotation, Quaternion.Euler(CharacterTargetRot), SmoothTime * Time.deltaTime);
            }
            else
            {
                CameraTargetRot.y = 0;
                PlayerCamera.localRotation = Quaternion.Euler(CameraTargetRot);
                PlayerCharacter.localRotation = Quaternion.Euler(CharacterTargetRot);
            }
        }
    }

    public void ApplyCameraForcedLook()
    {
        PlayerCamera.localRotation = Quaternion.Euler(CameraTargetRot);
    }



    public float MouseVertical
    {
        get
        {
            return -Input.GetAxis("Mouse Y") * XSensitivity;
        }
    }
    public float MouseHorizontal
    {
        get
        {
            return Input.GetAxis("Mouse X") * YSensitivity;
        }
    }
}

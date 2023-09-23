using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Collider))]
public class Plate : MonoBehaviour, IInteractable
{
    [SerializeField] private AudioClip breakSound;
    [SerializeField] private bool detachPieces = true;
    [SerializeField] private List<Rigidbody> brokenPieces;
    public UnityEvent OnBreak;
    private AudioSource audioSource;
   

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = breakSound;
    }
    void BreakPlate()
    {
        gameObject.GetComponent<Collider>().enabled = false;
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
        gameObject.GetComponent<Rigidbody>().useGravity = false;


        foreach(Rigidbody piece in brokenPieces)
        {
            if (detachPieces)
            {
                piece.transform.SetParent(null);
            }
           
            piece.gameObject.SetActive(true);
            piece.isKinematic = false;
            piece.useGravity = true;
            piece.AddForce(Random.Range(-0.5f, 0.5f), Random.Range(0.5f, 1.5f), Random.Range(-0.5f, 0.5f), ForceMode.Impulse);
        }

        BreakSound();
        OnBreak.Invoke();
    }

    void BreakSound()
    {
        audioSource.PlayOneShot(breakSound);
    }

    public void StartInteract()
    {

    }

    public void UpdateInteract()
    {
        
    }

    public void EndInteract()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        // if the velocity of the collision is greater than 2, break the plate
        if (collision.relativeVelocity.magnitude >5)
        {
            BreakPlate();
        }
    }
}

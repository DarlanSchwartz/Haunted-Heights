using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Candle : MonoBehaviour, IInteractable
{
    private bool dead;
    [SerializeField] bool _isLit = false;
    [SerializeField] Transform candleBody;
    [SerializeField] float degredationRate = 0.1f;
    [SerializeField] Vector3 degradatedSize = new(1, 0.3f, 1);
    [SerializeField] Color litEmissiveColor = Color.white;
    public UnityEvent OnStartFlame;
    public UnityEvent OnEndFlame;
    public UnityEvent OnDie;

    private void Start()
    {
        StartInteract();
    }

    private void Update()
    {
        if(!dead && _isLit)
        {
            candleBody.localScale = Vector3.Lerp(candleBody.localScale, degradatedSize, degredationRate * Time.deltaTime);
            if(candleBody.localScale.y <= degradatedSize.y || Vector3.Distance(candleBody.localScale,degradatedSize) <= 0.05f)
            {
                Unlit();
                Die();
            }
        }
    }
    public void EndInteract()
    {
        if (dead) return;
    }

    public void StartInteract()
    {
        if(dead) return;
        if (!_isLit)
        {
            Lit();
        }
        else
        {
            Unlit();
        }
    }

    public void UpdateInteract()
    {
        throw new System.NotImplementedException();
    }

    void Lit()
    {
        if (dead) return;
        _isLit = true;
        candleBody.GetComponent<MeshRenderer>().sharedMaterial.SetColor("_EmissiveColor", litEmissiveColor);
        OnStartFlame.Invoke();
    }

    void Unlit()
    {
        if (dead) return;
        _isLit = false;
        OnEndFlame.Invoke();
    }

    void Die ()
    {
        if (dead) return;
        OnDie.Invoke();
        candleBody.GetComponent<MeshRenderer>().sharedMaterial.SetColor("_EmissiveColor", Color.black);
        dead = true;
    }
}

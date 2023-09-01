using UnityEngine;

public enum VaultDirection { Left, Right}
public enum VaultSpeed { Idle, Walking, Running, Monkey}

public class VaultableObject : MonoBehaviour
{
    public Transform Vaultip;
    [SerializeField]
    private float _vaultHeight = 0.3f;
    [SerializeField]
    private float _vaultThickness = 0.4f;

    public float Height { get => _vaultHeight;}

    public float Thickness { get => _vaultThickness; }

    public VaultDirection defaultDirection = VaultDirection.Left;

    public void EnableTooltip()
    {
        if (!Vaultip)
            return;
        Vaultip.gameObject.SetActive(true);
    }

    public void DisableTooltip()
    {
        if (!Vaultip)
            return;
        Vaultip.gameObject.SetActive(false);
    }
}

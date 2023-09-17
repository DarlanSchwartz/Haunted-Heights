using UnityEngine;

public enum VaultDirection { Left, Right , Forward}
public enum VaultSpeed { Idle, Walking, Running, Monkey}

public class VaultableObject : MonoBehaviour
{
    [SerializeField]
    [Tooltip("This controls where the hand will be placed")]
    private float _vaultHeight = 0.3f;
    [SerializeField]
    private float _vaultThickness = 0.4f;

    public float Height { get => _vaultHeight;}

    public float Thickness { get => _vaultThickness; }

    public VaultDirection defaultDirection = VaultDirection.Left;
}

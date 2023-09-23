using UnityEngine;

public enum InteractionType { Ghost, Player, Other }
public interface IInteractable
{
    void StartInteract();
    void UpdateInteract();
    void EndInteract();
}

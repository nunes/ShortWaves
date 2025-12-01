using UnityEngine;

public interface IInteractable
{
    string InteractionPrompt { get; }
    void Interact(PlayerControl player);
}

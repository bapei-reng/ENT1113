using UnityEngine;

public class LockedExitDoor : MonoBehaviour
{
    [SerializeField] private GameEnding gameEnding;

    private void OnCollisionEnter(Collision collision)
    {
        if (gameEnding != null)
            gameEnding.NotifyLockedDoorContact(collision.gameObject);
    }
}

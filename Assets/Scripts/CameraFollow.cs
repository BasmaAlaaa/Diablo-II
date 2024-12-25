using UnityEngine;

public class CameraFollow : MonoBehaviour
{
  private Transform player; // Reference to the active player's transform
  private Vector3 offset;   // Offset between the camera and the player

  public void SetPlayer(Transform playerTransform)
  {
    player = playerTransform; // Assign the player's transform dynamically
    offset = transform.position - player.position; // Calculate the initial offset
  }

  void LateUpdate()
  {
    if (player != null)
    {
      // Smoothly follow the player
      Vector3 targetPosition = player.position + offset;
      transform.position = Vector3.Lerp(transform.position, targetPosition, 0.125f);
    }
  }
}

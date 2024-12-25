using UnityEngine;

public class Enemy : MonoBehaviour
{
  public int health = 100;
  public int xpReward = 50; 


  public void TakeDamage(int damage)
  {
    health -= damage;
    Debug.Log($"{name} took {damage} damage. Remaining health: {health}");

    if (health <= 0)
    {
      Die();
    }
  }

  private void Die()
  {
    Debug.Log($"{name} has died.");
    WandererStats wandererStats = FindObjectOfType<WandererStats>();
    if (wandererStats != null)
    {
      wandererStats.GainXP(xpReward);
    }
    Destroy(gameObject);
  }
}

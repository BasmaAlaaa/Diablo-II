using UnityEngine;

public class Fireball : MonoBehaviour
{
  private float speed = 10f; 
  private int damage = 5;  

  private Vector3 direction; 


  public void SetDirection(Vector3 dir)
  {
    direction = dir;
  }

  private void Update()
  {
    transform.Translate(direction * speed * Time.deltaTime, Space.World);
  }

  private void OnTriggerEnter(Collider other)
  {
    if (other.CompareTag("Minion"))
    {
      Debug.Log("Fireball hit the enemy!");
      other.GetComponent<MinionManager>().TakeDamage(damage); 
      Destroy(gameObject); 
    }
    else if (other.CompareTag("Demon"))
    {
      Debug.Log("Fireball hit Demon!");
      other.GetComponent<DemonManager>().TakeDamage(damage); 
      Destroy(gameObject); 
    }
    else if (other.CompareTag("Jolleen"))
    {
      Debug.Log("Fireball hit Jolleen!");
      other.GetComponent<LilithHealth>().TakeDamage(damage); 
      Destroy(gameObject); 
    }
    else if (other.CompareTag("Ground"))
    {
      Destroy(gameObject);
    }
  }
}

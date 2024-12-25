using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class MovementController : MonoBehaviour
{
  [SerializeField]
  private Camera _maincamera;
  private Animator animator;
  private NavMeshAgent agent;
  public bool canMove = true; // Allow movement by default

  void Start()
  {
    animator = GetComponent<Animator>();
    agent = GetComponent<NavMeshAgent>();
  }

  void Update()
  {
    if (IsPointerOverUIElement()) // Check if pointer is over UI
    {
      return; // Do nothing if interacting with UI
    }

    if (canMove && Input.GetMouseButtonDown(0)) // Movement only allowed if not throwing
    {
      Ray ray = _maincamera.ScreenPointToRay(Input.mousePosition);
      RaycastHit hit;
      if (Physics.Raycast(ray, out hit))
      {
        agent.SetDestination(hit.point);
      }
    }

    if (canMove)
    {
      UpdateAnimation();
    }
    else
    {
      animator.SetInteger("Speed", 0); // Reset to idle if not allowed to move
    }
  }

  private void UpdateAnimation()
  {
    if (agent.remainingDistance > 5)
    {
      animator.SetInteger("Speed", 2);
    }
    else if (agent.remainingDistance < 5 && agent.remainingDistance > 0.5f)
    {
      animator.SetInteger("Speed", 1);
    }
    else if (agent.remainingDistance < 0.5f)
    {
      animator.SetInteger("Speed", 0);
    }
  }

  private bool IsPointerOverUIElement()
  {
    return EventSystem.current.IsPointerOverGameObject();
  }
}

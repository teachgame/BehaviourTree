using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMovementAI : PlayerMovementBase
{
	public Vector3 lookDir;
	public Vector3 destination;
	public bool isMoving;

	private NavMeshAgent nav;

	void Start()
	{
		nav = GetComponent<NavMeshAgent>();
		nav.updateRotation = false;
		nav.speed = this.speed;
	}

	void Update()
	{
		if(Input.GetMouseButtonDown(1))
		{
			Ray camRay = Camera.main.ScreenPointToRay (Input.mousePosition);

			// Create a RaycastHit variable to store information about what was hit by the ray.
			RaycastHit floorHit;

			// Perform the raycast and if it hits something on the floor layer...
			if(Physics.Raycast (camRay, out floorHit, 100, LayerMask.GetMask ("Floor")))
			{
				destination = floorHit.point;
			}
		}
	}

	#region implemented abstract members of PlayerMovementBase
	protected override Vector3 GetLookDirection ()
	{
		return lookDir;
	}

	protected override bool Move ()
	{
		if(!isMoving)
		{
			nav.isStopped = true;
			return false;
		}

		//nav.SetDestination(destination);
		return true;
	}
	#endregion
	
}

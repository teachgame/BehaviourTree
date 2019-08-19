using UnityEngine;

public abstract class PlayerMovementBase : MonoBehaviour
{
	public float speed = 6f;            // The speed that the player will move at.

	protected Animator anim;                      // Reference to the animator component.
	protected Rigidbody playerRigidbody;          // Reference to the player's rigidbody.

	protected abstract bool Move ();
	protected abstract Vector3 GetLookDirection();

	protected virtual void Awake ()
	{
		// Set up references.
		anim = GetComponent <Animator> ();
		playerRigidbody = GetComponent <Rigidbody> ();
	}

	void FixedUpdate ()
	{
		// Move the player around the scene.
		bool walking = Move ();

		// Turn the player to face the mouse cursor.
		//Turning ();

		// Animate the player.
		Animating (walking);
	}

	void Turning ()
	{
		Vector3 lookDirection = GetLookDirection();

		if(lookDirection == Vector3.zero) return;

		// Create a quaternion (rotation) based on looking down the vector from the player to the mouse.
		Quaternion newRotatation = Quaternion.LookRotation (lookDirection);

		// Set the player's rotation to this new rotation.
		playerRigidbody.MoveRotation (newRotatation);
	}

	void Animating (bool walking)
	{
		// Tell the animator whether or not the player is walking.
		anim.SetBool ("IsWalking", walking);
	}
}

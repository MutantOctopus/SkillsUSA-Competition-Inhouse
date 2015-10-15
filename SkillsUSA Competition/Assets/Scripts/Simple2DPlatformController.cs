using UnityEditor;
using UnityEngine;
using System.Collections;

public class Simple2DPlatformController : MonoBehaviour
{
	#region Variables
	#region public variables
	public float speed = 10f; // Horizontal movement speed on ground
	public float slideTolerance = 5f; // How fast a slope slide can be going before it is stopped by the system
	// Used when calculating horizontal speed lerp: lower numbers mean "slippery" controls (like icy ground)
	public float groundControl = 0.5f;
	public float airControl = 0.2f; // Same as ground control, but calculated in air
	public float jumpForce = 700f; // Amount of force pulsed for a jump
	public Transform groundCheck; // The space that the ground will be checked at
	public LayerMask GroundMask; // The layer that ground objects should be in
	#endregion
	#region private variables
	bool faceR = true; /* If the object is facing right (true) or left (false); used to set sprite direction */
	Rigidbody2D rigid; // The object's physics calculator
	bool grounded = false; // Whether the object is on the ground or not
	float gcWidth = 0.4f;
	float gcHeight = 0.1f; // How high the ground check will spread in either direction
	//float logTimeDelay = 20;
	//float logTimer;
	#endregion
	#endregion
	/* Called when script is enabled */
	public void Start ()
	{
		// Notice:
		// that the automatic regions on functions start and end at the first and last braces, but the header
		// "public void Start ()" remains visible.
		rigid = GetComponent<Rigidbody2D> (); // Set up the Rigidbody2D component so the code can use it
		//logTimer = logTimeDelay;
	}
	/* Called once every frame */
	public void Update ()
	{
		if (grounded && Input.GetButtonDown ("Jump")) { // If on the ground and the button mapped to "jump" has been pressed
			/*
			 * Code clarity:
			 * Code comments like the above are important, even for seemingly obvious statements
			 * They can help to understand code if a person happens to forget what the code means
			 */
			/*
			 * Input:
			 * Unity's Input class functions can be used in a number of different ways.
			 * They can access particular keys one at a time (Input...(Keycode.space), Input...("space")),
			 * or they can access any key mapped to a particular set of buttons, defined in Unity's internal "Input" pane,
			 * located at edit -> project settings -> Input
			 */
			rigid.AddForce (new Vector2 (0, jumpForce)); // Add the upwards "push" to the object
			/*
			 * Adding force:
			 * AddForce is important to know: as the name implies, it adds force to your current velocity.
			 * This is a very flexible method, because it allows you to increase or decrease your velocity from any state.
			 * Contrast with "rigid.velocity =" below.
			 */
		}
	}
	/* Called once every physics step */
	public void FixedUpdate ()
	{
		Vector2 gcp = groundCheck.position;
		Vector2 A = new Vector2 (gcp.x - gcWidth, gcp.y + gcHeight); // Upper corner for ground check rectangle
		Vector2 B = new Vector2 (gcp.x + gcWidth, gcp.y - gcHeight); // Lower corner for ground check rectangle
		grounded = Physics2D.OverlapArea (A, B, GroundMask); // Check if ground is below object
		float move = Input.GetAxisRaw ("Horizontal"); // Recieve horizontal input
//		if (logTimer == 0) {
//			logTimer = logTimeDelay;
//			Debug.Log ("Y movement is " + rigid.velocity.y);
//		} else
//			logTimer--;
		#region Slidelock (Unused)
//		float yVeloctity;
//		// This section prevents the object from sliding down slopes when stopped
//		if (move == 0 && grounded && Mathf.Abs (rigid.velocity.y) < slideTolerance) {
//			// If no movement is held, the object is on the ground, and vertical movement is slow enough:
//			//rigid.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
//			// Freeze both the rotation, and the horizontal movement
//			yVeloctity = 0;
//		} else { // freeze only rotation
//			//rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
//			yVeloctity = rigid.velocity.y;
//		}
		#endregion
		// Below: Decide whether to use grounded horizontal speed or midair horizontal speed
		float usedLerp = (grounded ? groundControl : airControl);
		// Below: Alter velocity
		rigid.velocity = Vector2.Lerp (rigid.velocity, new Vector2 (move * speed, rigid.velocity.y), usedLerp);
		// Below: checks if you're facing the wrong way for your direction, and flips the sprite if you are.
		if ((move > 0 && !faceR) || (move < 0 && faceR))
			Flip ();
	}
	/* Flips the X-axis to mirror sprites */
	public void Flip ()
	{
		faceR = !faceR; // Toggle whether facing left or right
		Vector3 scale = transform.localScale; // Retrieves the current scale of the object
		scale.x *= -1; // Multiples the scale's x by -1, which turns the sprite inwards on itself and flips it
		transform.localScale = scale; // Sets the scale of the object to the new, inverted scale
	}
	// Lots of complex stuff below to create the custom Inspector GUI for this script!
	#region Inspector UI
	[CustomEditor(typeof(Simple2DPlatformController))]
	/* Custom inspector UI writer */ private class Plat2DControllerEditor : Editor {
		public override void OnInspectorGUI	() {
			// Create Script field
			{
				serializedObject.Update (); // Update object
				SerializedProperty prop = serializedObject.FindProperty ("m_Script"); // Recieve script
				EditorGUILayout.PropertyField (prop, true, new GUILayoutOption[0]); // Create field
				serializedObject.ApplyModifiedProperties (); // Apply any script changes
			}
			// Create others
			{
				Simple2DPlatformController control = (Simple2DPlatformController)target; // Access control script
				control.speed = EditorGUILayout.FloatField ("Speed", control.speed); // Create speed field
				// Below: Create slide tolerance field
				//control.slideTolerance = EditorGUILayout.FloatField ("Slide Tolerance", control.slideTolerance);
				// Below: Create ground control slider (0-1)
				control.groundControl = EditorGUILayout.Slider ("Ground Control", control.groundControl, 0, 1);
				// Create air control slider (0-1)
				control.airControl = EditorGUILayout.Slider ("Air Control", control.airControl, 0, 1);
				control.jumpForce = EditorGUILayout.FloatField ("Jump Force", control.jumpForce); // Create jump power field
				control.groundCheck = EditorGUILayout.ObjectField ("Ground Check", control.groundCheck, typeof(Transform),
				                                                   true) as Transform; // Create groundCheck field
			}
			// Create LayerMask field
			{
				serializedObject.Update ();
				SerializedProperty prop = serializedObject.FindProperty ("GroundMask");
				EditorGUILayout.PropertyField (prop, true, new GUILayoutOption[0]);
				serializedObject.ApplyModifiedProperties ();
			}
		}
	}
	#endregion
}

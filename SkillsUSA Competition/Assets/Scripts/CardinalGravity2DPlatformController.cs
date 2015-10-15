using UnityEngine;
using UnityEditor;
using System.Collections;
using OctoTools;
using Gravity = CardinalGravity2D;

[RequireComponent (typeof (Gravity))]
[RequireComponent (typeof (Rigidbody2D))]
public class CardinalGravity2DPlatformController : MonoBehaviour {
    #region Variables
    #region public variables
    [Range (0.01f, 999)]
    public float speed = 10f; // Horizontal movement speed on ground          
    [Range (0, 1)]
    public float groundControl = 0.5f; // Used when calculating horizontal speed lerp: lower numbers mean "slippery" controls (like icy ground)
    [Range (0, 1)]
    public float airControl = 0.2f; // Same as ground control, but calculated in air
    public float jumpForce = 100f; // Amount of force pulsed for a jump
    public Transform groundCheck; // The space that the ground will be checked at
    public LayerMask GroundMask; // The layer that ground objects should be in
    #endregion
    #region private variables
    bool faceR = true; /* If the object is facing right (true) or left (false); used to set sprite direction */
    Rigidbody2D rigid; // The object's physics calculator
    Gravity cgrav;
    bool grounded = false; // Whether the object is on the ground or not
    float gcWidth = 0.4f;
    float gcHeight = 0.1f; // How high the ground check will spread in either direction
    #endregion
    #endregion
    /* Called when script is enabled */
    public void Start () {
        rigid = GetComponent<Rigidbody2D> (); // Set up the Rigidbody2D component so the code can use it
        cgrav = GetComponent<Gravity> ();
    }
    /* Called once every frame */
    public void Update () {
        if (grounded && Input.GetButtonDown ("Jump")) { // If on the ground and the button mapped to "jump" has been pressed
            rigid.AddForce (-cgrav.DirectionForce * jumpForce); // Add the upwards "push" to the object
        }
    }
    /* Called once every physics step */
    public void FixedUpdate () {
        Vector2 gcp = groundCheck.position;
        Vector2 A = new Vector2 (gcp.x - gcWidth, gcp.y + gcHeight); // Upper corner for ground check rectangle
        Vector2 B = new Vector2 (gcp.x + gcWidth, gcp.y - gcHeight); // Lower corner for ground check rectangle
        grounded = Physics2D.OverlapArea (A, B, GroundMask);
        float move = Input.GetAxisRaw ("Horizontal");
        float usedLerp = (grounded ? groundControl : airControl);

        Vector3 v = rigid.velocity;
        v = cgrav.RotateNormal (v);
        float vy = v.y;

        Vector2 localMovement = new Vector2 (
            move * speed,
            cgrav.RotateNormal(rigid.velocity).y
            );
        rigid.velocity = Vector2.Lerp (rigid.velocity, cgrav.InvRotateNormal(localMovement), usedLerp);

        if ((move > 0 && !faceR) || (move < 0 && faceR))
            Flip ();
    }
    /* Flips the X-axis to mirror sprites */
    public void Flip () {
        faceR = !faceR; // Toggle whether facing left or right
        Vector3 scale = transform.localScale; // Retrieves the current scale of the object
        scale.x *= -1; // Multiples the scale's x by -1, which turns the sprite inwards on itself and flips it
        transform.localScale = scale; // Sets the scale of the object to the new, inverted scale
    }
}

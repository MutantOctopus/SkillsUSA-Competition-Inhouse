using UnityEngine;
using System.Collections;

namespace OctoTools {
    public class ComplexGravity2D : MonoBehaviour {
        public Vector2 gravity = new Vector2 (0, -9.8F);
        public float gravityAngle {
            get {
                float val = Mathf.Rad2Deg * Mathf.Acos (gravity.x / gravity.magnitude);
                print ("Grav angle: " + val);
                return val;
            }
        }
        Rigidbody2D r;

        // Use this for initialization
        void Start () {
            r = GetComponent<Rigidbody2D> ();
        }

        // Update is called once per frame
        void Update () {

        }

        public void FixedUpdate () {
            r.AddForce (gravity, ForceMode2D.Force);
        }
    }
}
using UnityEngine;
using System.Collections;

namespace OctoTools {
    public class ComplexGravity2D : MonoBehaviour {
        public Vector2 gravity = new Vector2 (0, -9.8F);
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
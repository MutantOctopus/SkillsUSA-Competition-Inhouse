using OctoTools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SurfacePlatformer {
    [ExecuteInEditMode]
    public class TransferRegion : MonoBehaviour {
        private enum Side {
            Left,
            Right,
            Top,
            Bottom
        }
        private enum GravRotation {
            Straight = 0,
            Clockwise90 = -90,
            CounterClockwise90 = 90,
            Invert = 180
        }

        //[SerializeField]
        private UnityEvent rescaled = new UnityEvent ();

        [SerializeField]
        private GravRotation turnToPaired;
        [SerializeField]
        private Side onwardEdge;
        [SerializeField]
        private TransferRegion pair;

        private Func<Vector2, Vector2> VecRotate;
        private Func<Vector2, bool> sideCheck;
        private Vector3 scale;
        private Vector2 offset;
        private IDictionary<GameObject, GameObject> localObjects = new Dictionary<GameObject, GameObject>();

        // Use this for initialization
        void Start () {
            if (!pair) {
                Debug.LogException (
                    new MissingReferenceException ("TransferRegion " + gameObject.name + " has no pair!"),
                    this
                    );
            }
            else {
                pair.rescaled.AddListener (OnPairRescaled);
                VecRotate = GetVectorTransform (turnToPaired);
                sideCheck = GetOffsideDetector (onwardEdge);
                scale = gameObject.transform.localScale;
                offset = pair.transform.position - transform.position;
            }
        }

        // Update is called once per frame
        void Update () {
            if (VecRotate == null) {
                VecRotate = GetVectorTransform (turnToPaired);
            }
            if (scale != gameObject.transform.localScale) {
                scale = gameObject.transform.localScale;
                rescaled.Invoke();
            }
        }

        public void OnTriggerEnter2D (Collider2D collision) {
            GameObject cobj = collision.gameObject;
            string ctag = cobj.tag;
            if (!pair.localObjects.ContainsKey(cobj) && (ctag == "Player" || ctag == "Enemy")) {
                GameObject clone = Instantiate (
                    cobj,
                    (Vector2)cobj.transform.position + offset,
                    cobj.transform.rotation
                    ) as GameObject;
                localObjects.Add (clone, cobj);
                var clRigid = clone.GetComponent<Rigidbody2D> ();
                var clGrav = clone.GetComponent<ComplexGravity2D> ();
                clRigid.velocity = VecRotate (collision.GetComponent<Rigidbody2D> ().velocity);
                clRigid.angularVelocity = collision.GetComponent<Rigidbody2D> ().angularVelocity;
                clGrav.gravity = VecRotate (clGrav.gravity);
                clone.transform.RotateAround (pair.transform.position, Vector3.forward, (int)turnToPaired);
            } else {
                pair.localObjects.Add (pair.localObjects [cobj], cobj);
            }
        }

        public void OnTriggerExit2D (Collider2D collision) {
            pair.localObjects.Remove (collision.gameObject);
            if (!sideCheck(collision.transform.position)) {
                Destroy (collision.gameObject);
            } else if (collision.gameObject.name.Contains("(Clone)")) {
                Debug.Log (collision.gameObject.name);
                collision.gameObject.name = collision.gameObject.name.Remove (collision.gameObject.name.IndexOf ("(Clone)"));
            }
        }

        public void OnPairRescaled () {
            gameObject.transform.localScale = pair.VecRotate(
                pair.transform.localScale
                )
            ;
            scale = gameObject.transform.localScale;
        }

        private Func<Vector2, Vector2> GetVectorTransform (GravRotation gr) {
            switch (gr) {
                case GravRotation.Straight:
                    return (v) => v;
                case GravRotation.Clockwise90:
                    return (v) => new Vector2 (v.y, v.x);
                case GravRotation.CounterClockwise90:
                    return (v) => new Vector2 (-v.y, v.x);
                case GravRotation.Invert:
                    return (v) => new Vector2 (-v.x, -v.y);
                default:
                    Debug.LogError ("Invalid GravRotation passed");
                    return (v) => {
                        Debug.LogError ("Using default Func <0,0>", this);
                        return new Vector2 ();
                    };
            }
        }

        private Func<Vector2, bool> GetOffsideDetector (Side s) {
            switch (s) {
                case Side.Left:
                    return (v) => v.x <= gameObject.transform.position.x;
                case Side.Right:
                    return (v) => v.x >= gameObject.transform.position.x;
                case Side.Top:
                    return (v) => v.y >= gameObject.transform.position.y;
                case Side.Bottom:
                    return (v) => v.y <= gameObject.transform.position.y;
                default:
                    Debug.Log ("Invalid Side passed.");
                    return (v) => {
                        Debug.Log ("Default detector (always true)", this);
                        return true;
                    };
            }
        }
    }
}

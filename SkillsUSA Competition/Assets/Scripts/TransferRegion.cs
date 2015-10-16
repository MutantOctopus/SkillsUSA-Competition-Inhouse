using OctoTools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Direction = CardinalGravity2D.Direction;

namespace SurfacePlatformer {
    [ExecuteInEditMode]
    public class TransferRegion : MonoBehaviour {
        private enum Side {
            Left,
            Right,
            Top,
            Bottom
        }
        private enum Rotation {
            Straight,
            CounterClockwise90,
            Invert,
            Clockwise90
        }

        [SerializeField]
        private UnityEvent rescaled = new UnityEvent ();

        [SerializeField]
        private Rotation turnToPaired = Rotation.Straight;
        [SerializeField]
        private Side onwardEdge;
        [SerializeField]
        private TransferRegion pair;

        private Func<Vector2, Vector2> VecRotate;
        private Func<Vector2, bool> sideCheck;
        private Vector3 scale;
        private Vector2 offset;
        private IDictionary<GameObject, GameObject> localObjects;

        // Use this for initialization
        void Start () {
            if (!pair) {
                Debug.LogException (
                    new MissingReferenceException ("TransferRegion " + gameObject.name + " has no pair!"),
                    this
                    );
            }
            else {
                if (localObjects == null) {
                    localObjects = new Dictionary<GameObject, GameObject> ();
                    pair.localObjects = localObjects;
                }
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
                rescaled.Invoke ();
            }
        }

        public void OnTriggerEnter2D (Collider2D collision) {
            if (sideCheck (collision.transform.position) && !localObjects.ContainsKey (collision.gameObject)) {
                GameObject cobj = collision.gameObject;
                string ctag = cobj.tag;
                if (!localObjects.ContainsKey (cobj) && (ctag == "Player" || ctag == "Enemy")) {
                    GameObject clone = Instantiate (
                        cobj,
                        (Vector2)cobj.transform.position + offset,
                        cobj.transform.rotation
                        ) as GameObject;
                    localObjects.Add (cobj, clone);
                    localObjects.Add (clone, cobj);
                    var clRigid = clone.GetComponent<Rigidbody2D> ();
                    var clGrav = clone.GetComponent<CardinalGravity2D> ();
                    clRigid.velocity = VecRotate (collision.GetComponent<Rigidbody2D> ().velocity);
                    clRigid.angularVelocity = collision.GetComponent<Rigidbody2D> ().angularVelocity;
                    clGrav.GravDirection = (Direction)(((int)clGrav.GravDirection + (int)turnToPaired) % 4);
                    clone.transform.RotateAround (pair.transform.position, Vector3.forward, (int)turnToPaired * 90);
                    /*clone.transform.rotation = Quaternion.Euler (
                        0,
                        0,
                        90 * (int)turnToPaired + clone.transform.rotation.eulerAngles.z
                        );*/
                }
            }
        }

        public void OnTriggerExit2D (Collider2D collision) {
            localObjects.Remove (collision.gameObject);
            if (!sideCheck (collision.transform.position)) {
                Destroy (collision.gameObject);
            }
            else if (collision.gameObject.name.Contains ("(Clone)")) {
                collision.gameObject.name = collision.gameObject.name.Remove (collision.gameObject.name.IndexOf ("(Clone)"));
            }
        }

        public void OnPairRescaled () {
            gameObject.transform.localScale = pair.VecRotate (pair.transform.localScale);
            scale = gameObject.transform.localScale;
        }

        private Func<Vector2, Vector2> GetVectorTransform (Rotation gr) {
            switch (gr) {
                case Rotation.Straight:
                    return (v) => v;
                case Rotation.Clockwise90:
                    return (v) => new Vector2 (v.y, v.x);
                case Rotation.CounterClockwise90:
                    return (v) => new Vector2 (-v.y, v.x);
                case Rotation.Invert:
                    return (v) => new Vector2 (-v.x, -v.y);
                default:
                    Debug.LogError ("Invalid Rotation passed");
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

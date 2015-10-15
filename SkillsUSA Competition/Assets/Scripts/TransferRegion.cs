using OctoTools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        private struct PairCloneReturn {
            public GameObject result;
            public bool? hadPair;
            public bool unlocked;
        }

        public bool scaleManuallyAltered {
            get; private set;
        }

        [SerializeField]
        private GravRotation turnToPaired;
        [SerializeField]
        private Side onwardEdge;
        [SerializeField]
        private TransferRegion pair;

        private Func<Vector2, Vector2> VecRotate;
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
                VecRotate = GetVectorTransform (turnToPaired);
                scale = gameObject.transform.localScale;
                scaleManuallyAltered = false;
                offset = pair.transform.position - transform.position;
            }
        }

        // Update is called once per frame
        void Update () {
            #region In-editor scale equalizing
            if (scale != gameObject.transform.localScale) {
                scaleManuallyAltered = true;
                scale = gameObject.transform.localScale;
            }
            if (pair && scaleManuallyAltered && !pair.scaleManuallyAltered) {
                pair.transform.localScale = VecRotate(scale);
            }
            #endregion
        }

        void LateUpdate () {
            scaleManuallyAltered = false;
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
                        Debug.LogError ("Using default Func <0,0>");
                        return new Vector2 ();
                    };
            }
        }
    }
}

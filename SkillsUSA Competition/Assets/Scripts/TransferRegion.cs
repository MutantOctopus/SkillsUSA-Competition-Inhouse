using OctoTools;
using System;
using System.Collections;
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
            Clockwise90 = 90,
            CounterClockwise90 = -90,
            Invert = 180
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
                pair.transform.localScale = scale;
            }
            scaleManuallyAltered = false;
            #endregion
        }

        public void OnTriggerEnter2D (Collider2D collision) {
            string ctag = collision.gameObject.tag;
            if (ctag == "Player" || ctag == "Enemy") {
                collision.gameObject.tag = "Untagged";
                GameObject clone = Instantiate (
                    collision.gameObject,
                    (Vector2) collision.gameObject.transform.position + offset,
                    collision.gameObject.transform.rotation
                    ) as GameObject;;
                var clRigid = clone.GetComponent<Rigidbody2D> ();
                var clGrav = clone.GetComponent<ComplexGravity2D> ();
                clRigid.velocity = VecRotate(collision.GetComponent<Rigidbody2D> ().velocity);
                clRigid.angularVelocity = collision.GetComponent<Rigidbody2D> ().angularVelocity;
                clGrav.gravity = VecRotate (clGrav.gravity);
                clone.transform.Rotate(pair.transform.forward, (int)turnToPaired, Space.World);
                StartCoroutine (changeTags(ctag, collision.gameObject, clone));
            }
        }

        public void OnTriggerExit2D (Collider2D collision) {

        }

        private IEnumerator changeTags (string tag, params GameObject [] objectsToChange) {
            yield return null;
            foreach (GameObject g in objectsToChange) {
                g.tag = tag;
            }
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

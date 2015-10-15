using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

[RequireComponent (typeof (Rigidbody2D))]
public class CardinalGravity2D : MonoBehaviour {
    public enum Direction {
        Up, Left, Down, Right
    }
    [SerializeField]
    [Range (0, 99999)]
    private float GravConstant = 9.8f;
    [SerializeField]
    private Direction _gDirec;
    private Vector2 force;
    private Rigidbody2D rb2D;

    public Vector2 GravityForce {
        get {
            return force * GravConstant;
        }
    }
    public Vector2 DirectionForce {
        get {
            return force;
        }
    }
    public Func<Vector2, Vector2> RotateNormal { get; private set; }
    public Func<Vector2, Vector2> InvRotateNormal { get; private set; }

    public Direction GravDirection {
        get {
            return _gDirec;
        }

        set {
            switch (value) {
                case Direction.Up:
                    force = Vector2.up;
                    RotateNormal = (v) => new Vector2 (-v.x, -v.y);
                    InvRotateNormal = RotateNormal;
                    break;
                case Direction.Down:
                    force = Vector2.down;
                    RotateNormal = (v) => v;
                    InvRotateNormal = RotateNormal;
                    break;
                case Direction.Left:
                    force = Vector2.left;
                    RotateNormal = (v) => new Vector2 (-v.y, v.x);
                    InvRotateNormal = (v) => new Vector2 (v.y, -v.x);
                    break;
                case Direction.Right:
                    force = Vector2.right;
                    RotateNormal = (v) => new Vector2 (v.y, -v.x);
                    InvRotateNormal = (v) => new Vector2 (-v.y, v.x);
                    break;
                default:
                    Debug.Log ("Invalid Direction.");
                    break;
            }
            _gDirec = value;
        }
    }

    void Start () {
        rb2D = GetComponent<Rigidbody2D> ();
        rb2D.gravityScale = 0;
        GravDirection = GravDirection;
    }

    void FixedUpdate () {
        rb2D.AddForce (GravityForce, ForceMode2D.Force);
    }

    [CustomEditor (typeof (CardinalGravity2D))]
    private class CardinalGravityInspector : Editor {
        public override void OnInspectorGUI () {
            CardinalGravity2D _target = target as CardinalGravity2D;
            _target.GravDirection = (Direction)EditorGUILayout.EnumPopup (
                "Gravity Direction",
                _target.GravDirection
                );
            _target.GravConstant = EditorGUILayout.Slider ("Force of Gravity", _target.GravConstant, 0, 1000);
        }
    }
}

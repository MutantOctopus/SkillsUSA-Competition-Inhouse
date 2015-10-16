using UnityEngine;
using System.Collections;

public class MouseDragRotator : MonoBehaviour {
    [SerializeField]
    private float RotationMultiplier = 10;

    private Vector3 startingPosition;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0)) {
            startingPosition = Input.mousePosition;
        }
	    if (Input.GetMouseButton(0)) {
            Vector3 mouseDelta = startingPosition - Input.mousePosition;
            Vector3 rotation = new Vector3 (-mouseDelta.y, mouseDelta.x, 0);
            transform.Rotate (rotation * RotationMultiplier, Space.World);
            startingPosition = Input.mousePosition;
        }
	}
}

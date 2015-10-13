using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour {
    public Transform center;
    Transform thisPos;
    Vector3 axis;
    public int mult = 50;

	// Use this for initialization
	void Start () {
        axis = center.forward;
        thisPos = GetComponent<Transform> ();
	}
	
	// Update is called once per frame
	void Update () {
        transform.RotateAround (center.position, Vector3.forward, mult * Time.deltaTime);
    }
}

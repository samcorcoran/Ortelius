using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunMovement : MonoBehaviour {

    // TODO: Rotation around X axis changes season (who gets permadark winters)
    // TODO: Rotation around Y axis is changes time of day (which side is shadowed)

    public float MovementSpeed = 1f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        transform.Rotate(Vector3.up, MovementSpeed*Time.deltaTime);
    }
}

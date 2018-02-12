using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCam : MonoBehaviour {

	[SerializeField]
	private int speed;

	void Start () {
	}
	
	void Update () {
		if (Input.GetKey(KeyCode.D)) {
			transform.position = new Vector3(transform.position.x + speed, transform.position.y, transform.position.z);
		}
		if (Input.GetKey(KeyCode.Q)) {
			transform.position = new Vector3(transform.position.x - speed, transform.position.y, transform.position.z);
		}
		if (Input.GetKey(KeyCode.Z)) {
			transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + speed);
		}
		if (Input.GetKey(KeyCode.S)) {
			transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - speed);
		}
	}
}

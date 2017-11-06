using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bounce : MonoBehaviour {
	private float lerpTime;
	private float currentLerpTime;
	private float perc = 1;

	private Vector3 startPos;
	private Vector3 endPos;

	private bool firstInput;
	public bool justJump;

	void Start () {

	}
	
	void Update () {
		if (Input.GetButtonDown("up") || Input.GetButtonDown("right") || Input.GetButtonDown("down") || Input.GetButtonDown("left"))
		{
			if (perc == 1)
			{
				lerpTime = 1;
				currentLerpTime = 0;
				firstInput = true;
				justJump = true;
			}
		}
		startPos = gameObject.transform.position;
		if (Input.GetButtonDown("right") && gameObject.transform.position == endPos) {
			endPos = new Vector3(transform.position.x + 1, transform.position.y, transform.position.z);
		} else if (Input.GetButtonDown("up") && gameObject.transform.position == endPos) {
			endPos = new Vector3(transform.position.x, transform.position.y, transform.position.z + 1);
		} else if (Input.GetButtonDown("left") && gameObject.transform.position == endPos) {
			endPos = new Vector3(transform.position.x - 1, transform.position.y, transform.position.z);
		} else if (Input.GetButtonDown("down") && gameObject.transform.position == endPos) {
			endPos = new Vector3(transform.position.x, transform.position.y, transform.position.z - 1);
		}
		if (firstInput == true) {
			currentLerpTime += Time.deltaTime * 5;
			perc = currentLerpTime / lerpTime;
			gameObject.transform.position = Vector3.Lerp(startPos, endPos, perc);
			if (perc > 0.9) {
				perc = 1;
			}
			if (Mathf.Round(perc) == 1) {
				justJump = false;
			}
		}
	}

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour {

	public static Transform playerTransform;

	void LateUpdate () {
		Vector3 newPosition = playerTransform.position;
		newPosition.y = transform.position.y;
		transform.position = newPosition;
		transform.rotation = Quaternion.Euler(90f, playerTransform.eulerAngles.y, 0f);
	}
}

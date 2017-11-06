using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour {

	Animator animator;
	public GameObject player;

	void Start () {
		animator = gameObject.GetComponent<Animator> ();
	}
	
	void Update () {
		Bounce bounceScript = player.GetComponent<Bounce> ();
		if (bounceScript.justJump == true) {
			animator.SetBool("Jump", true);
		} else {
			animator.SetBool("Jump", false);
		}
	}
}

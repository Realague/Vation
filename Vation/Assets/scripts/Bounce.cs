using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bounce : MonoBehaviour
{
	private float lerpTime;
	private float currentLerpTime;
	private float perc = 1;

	public Vector3 startPos;
	public Vector3 endPos;

	private void Start()
	{

	}
	
	private void Update()
	{
		if (Input.GetButtonDown("z") || Input.GetButtonDown("q") || Input.GetButtonDown("s") || Input.GetButtonDown("d"))
		{
			if (perc == 1)
			{
				
			}
		}
	}

}

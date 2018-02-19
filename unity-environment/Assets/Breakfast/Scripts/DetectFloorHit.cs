using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectFloorHit : MonoBehaviour {

	// Use this for initialization

	public BreakfastAgentRotate breakfast;
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	void OnCollisionEnter(Collision collision)
	{
		if(collision.collider.gameObject.tag == "food")
		{
			Debug.Log("Hit Floor");
			breakfast.HitFloor();
		}
	}
}

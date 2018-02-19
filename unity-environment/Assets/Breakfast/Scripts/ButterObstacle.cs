using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButterObstacle : MonoBehaviour {

	ButterAgent agent;
	void Start () {
		agent = GameObject.FindObjectOfType<ButterAgent>();
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	void OnCollisionEnter(Collision collision)
	{
		if(collision.gameObject.tag == "butter" || collision.gameObject.tag == "bot")
		{
			agent.HitObstacle();
		}
	}
}

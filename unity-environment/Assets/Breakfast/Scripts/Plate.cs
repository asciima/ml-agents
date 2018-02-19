using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plate : MonoBehaviour {

    public BreakfastAgentRotate agent;
    public float catchTime = 1f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    IEnumerator CatchTimer()
    {
        float tempTime = 0;
        while (tempTime < catchTime)
        {


            tempTime += Time.deltaTime;
            yield return null;
        }

        agent.Plated();

    }
    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "food")
        {
            StartCoroutine(CatchTimer());
        }
    }
    void OnCollisionExit(Collision col)
    {
        if(col.gameObject.tag == "food")
        {
            StopAllCoroutines();
        }
    }
}

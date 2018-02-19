using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanTrigger : MonoBehaviour {
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

        agent.FoodCaught();

    }
    void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.tag == "food")
        {
            StartCoroutine(CatchTimer());
            agent.inPan = true;
        }
    }
    void OnTriggerExit(Collider col)
    {
        if(col.gameObject.tag == "food")
        {
            StopAllCoroutines();
            agent.inPan = false;
        }
    }
}

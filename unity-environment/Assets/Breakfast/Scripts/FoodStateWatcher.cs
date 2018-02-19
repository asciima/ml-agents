using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodStateWatcher : MonoBehaviour {
	
	public BreakfastAgent bfastAgent;
	Transform foodItem;
	float maxHeight = 0;



    void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.tag == "food")
		{
			if(bfastAgent.foodState == BreakfastAgent.FoodState.goingDown)
			{
				Debug.Log("Food Caught!");
				bfastAgent.foodState = BreakfastAgent.FoodState.caught;
			}
			Debug.Log("Food entered Pan");
		}
	}
	void OnTriggerExit(Collider other)
	{
		if(other.gameObject.tag == "food")
		{
			if(bfastAgent.foodState == BreakfastAgent.FoodState.inPan && bfastAgent.runningTime > 1)
			{
				Debug.Log("Food is going up!");
				bfastAgent.foodState = BreakfastAgent.FoodState.goingUp;
			}
			Debug.Log("Food exited Pan");
		}
	}
}

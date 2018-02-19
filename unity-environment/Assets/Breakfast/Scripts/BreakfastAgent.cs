using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakfastAgent : Agent {

	public enum FoodState {inPan, goingUp, goingDown, caught, missed}

    public FoodState foodState = FoodState.inPan;
	//public FoodState foodState 
	//{
	//	get { return foodState; } 
	//	set { 
	//		foodState = value; 
	//		if (value == FoodState.goingUp)
	//			reward += 1;
	//		if (value == FoodState.caught)
	//			reward += 1;
	//		if (value == FoodState.missed)
	//			reward -= 1;
	//	}
	//}
	// Variables that need to be reset: 
	// Starting place of pan
	Vector3 panOrigin;
	float maxFoodHeight = 0;
	public float runningTime = 0;

	Vector3 pastVelocityFood;
	Vector3 pastVelocityPan;
	//Variables that don't need to be reset
	public Transform heightGoal;
	
	public Transform panCenter;
	public Transform food;
	public Transform[] limbs;
	// Pan is allowed to travel 1m in all directions from start pose, before being penalized
	public float panBoundary = 1f;

	// Penalty for time spent in launchingState
	float entryTimePenalty = 0.01f;
	float stateChangeReward = 0.5f;
	float heightRewardMult = 0.05f;
	float panBoundaryRewardMult = 0.05f;
	float dotRewardMult = 0.05f;
	float panPositionRewardMult = 0.05f;
	float finalPosRewardMult = 0.1f;

	public float torqueStrengthMult = 300;

	Dictionary<GameObject, Vector3> transformsPosition;
    Dictionary<GameObject, Quaternion> transformsRotation;

	public void HitFloor()
	{
		foodState = FoodState.missed;
		reward -= 2f;
	}

	public override void InitializeAgent()
    {
        panOrigin = panCenter.transform.position;

		transformsPosition = new Dictionary<GameObject, Vector3>();
        transformsRotation = new Dictionary<GameObject, Quaternion>();
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            transformsPosition[child.gameObject] = child.position;
            transformsRotation[child.gameObject] = child.rotation;
        }
    }

	public override List<float> CollectState()
	{
		List<float> state = new List<float>();
		state.Add(runningTime);
        state.Add(food.position.x);
        state.Add(food.position.y);
        state.Add(food.position.z);

        state.Add(food.rotation.x);
        state.Add(food.rotation.y);
        state.Add(food.rotation.z);
		state.Add(food.rotation.w);

        state.Add(food.gameObject.GetComponent<Rigidbody>().velocity.x);
        state.Add(food.gameObject.GetComponent<Rigidbody>().velocity.y);
        state.Add(food.gameObject.GetComponent<Rigidbody>().velocity.z);

        state.Add((food.gameObject.GetComponent<Rigidbody>().velocity.x - pastVelocityFood.x) / Time.fixedDeltaTime);
        state.Add((food.gameObject.GetComponent<Rigidbody>().velocity.y - pastVelocityFood.y) / Time.fixedDeltaTime);
        state.Add((food.gameObject.GetComponent<Rigidbody>().velocity.z - pastVelocityFood.z) / Time.fixedDeltaTime);
        pastVelocityFood = food.gameObject.GetComponent<Rigidbody>().velocity;


        state.Add(panCenter.position.x);
        state.Add(panCenter.position.y);
        state.Add(panCenter.position.z);

		state.Add(panCenter.rotation.x);
        state.Add(panCenter.rotation.y);
        state.Add(panCenter.rotation.z);
		state.Add(panCenter.rotation.w);

        //state.Add(panCenter.gameObject.GetComponent<Rigidbody>().velocity.x);
        //state.Add(panCenter.gameObject.GetComponent<Rigidbody>().velocity.y);
        //state.Add(panCenter.gameObject.GetComponent<Rigidbody>().velocity.z);

        //state.Add((panCenter.gameObject.GetComponent<Rigidbody>().velocity.x - pastVelocityPan.x) / Time.fixedDeltaTime);
        //state.Add((panCenter.gameObject.GetComponent<Rigidbody>().velocity.y - pastVelocityPan.y) / Time.fixedDeltaTime);
        //state.Add((panCenter.gameObject.GetComponent<Rigidbody>().velocity.z - pastVelocityPan.z) / Time.fixedDeltaTime);
        //pastVelocityPan = panCenter.gameObject.GetComponent<Rigidbody>().velocity;


        foreach (Transform t in limbs)
        {
         
            state.Add(t.localRotation.x);
            state.Add(t.localRotation.y);
            state.Add(t.localRotation.z);
            state.Add(t.localRotation.w);
            Rigidbody rb = t.gameObject.GetComponent < Rigidbody >();
            state.Add(rb.velocity.x);
            state.Add(rb.velocity.y);
            state.Add(rb.velocity.z);
            state.Add(rb.angularVelocity.x);
            state.Add(rb.angularVelocity.y);
            state.Add(rb.angularVelocity.z);

          
        }



		return state;
	}

	public override void AgentStep(float[] act)
	{
		runningTime += Time.deltaTime;

		if(foodState == FoodState.inPan)
		{
			if(runningTime < 1.5f)
			{
				reward += entryTimePenalty;
			}
			else
			{
				reward -= entryTimePenalty;
			}

			
		}
		else if(foodState == FoodState.goingUp)
		{
			// Reward for getting the food close to the heightGoal
			float foodHeight = (food.position.y - panOrigin.y)/(heightGoal.position.y - panOrigin.y);
			if(foodHeight > maxFoodHeight)
				maxFoodHeight = foodHeight;
			else
				foodState = FoodState.goingDown;
			// Penalize if food is below pan origin
			if(foodHeight < 0)
				reward += -(foodHeight * foodHeight) * heightRewardMult;
			else
				reward += foodHeight * foodHeight * heightRewardMult;
			
			// Measures the food flip
			// if food is parallel to ground, dot will be -1. At 90 degrees, dot will be 0. At 180, dot will be 1
			float foodDot = Vector3.Dot(Vector3.up, food.up);
			reward += foodDot * foodHeight * dotRewardMult;

		}
		else if(foodState == FoodState.goingDown)
		{
			// done = true;	
			float foodHeight = (food.position.y - panCenter.position.y)/(heightGoal.position.y - panOrigin.y);
			if(foodHeight <= - 0.25f)
				foodState = FoodState.missed;
			float foodDot = Vector3.Dot(panCenter.up, food.up);
			reward += foodDot * (1 - foodHeight) * dotRewardMult;
			
			// Reward Pan for being underneath food
			reward += (Mathf.Abs(food.position.x - panCenter.position.z) + Mathf.Abs(food.position.z - panCenter.position.z)) * (1 - foodHeight) * panPositionRewardMult;
			
		}
		else if(foodState == FoodState.caught || foodState == FoodState.missed)
		{
			float foodDot = Vector3.Dot(panCenter.up, food.up);
			reward += foodDot * 0.1f;
			reward -= (food.position - panCenter.position).sqrMagnitude * finalPosRewardMult;
			done = true;
			
		}

		
	
		// Always penalize pan for going outside of boundary
		if((panCenter.position - panOrigin).magnitude > panBoundary)
		{
			float dist = ((panCenter.position - panOrigin).magnitude - panBoundary);
			reward -= dist * dist * panBoundaryRewardMult;
		}

		// Move limbs according to inputs
		MoveLimbs(act);
		Monitor.Log("Reward", reward, MonitorType.slider, heightGoal.transform);

	}

	void MoveLimbs(float[] act)
	{
		// Shoulder rotates on both axes
		float torque_y = Mathf.Clamp(act[0], -1, 1) * torqueStrengthMult;
        float torque_z = Mathf.Clamp(act[1], -1, 1) * torqueStrengthMult;
        // limbs[0].GetComponent<Rigidbody>().AddTorque(new Vector3(torque_x, 0f, torque_z));
		limbs[0].GetComponent<Rigidbody>().AddTorque(limbs[0].up * torque_y);
		limbs[0].GetComponent<Rigidbody>().AddTorque(limbs[0].forward * torque_z);


		// Elbow only rotates on one
        torque_z = Mathf.Clamp(act[2], -1, 1) * torqueStrengthMult;
        limbs[1].GetComponent<Rigidbody>().AddTorque(limbs[1].forward * torque_z);

		// Wrist rotates on both axes
		torque_y = Mathf.Clamp(act[3], -1, 1) * torqueStrengthMult;
       	torque_z = Mathf.Clamp(act[4], -1, 1) * torqueStrengthMult;
        limbs[2].GetComponent<Rigidbody>().AddTorque(limbs[2].up * torque_y);
		limbs[2].GetComponent<Rigidbody>().AddTorque(limbs[2].forward * torque_z);


	}

	public override void AgentReset()
	{
        Transform[] allChildren = GetComponentsInChildren<Transform>();

		foreach (Transform child in allChildren)
        {
            
            child.position = transformsPosition[child.gameObject];
            child.rotation = transformsRotation[child.gameObject];
			if(child.gameObject.GetComponent<Rigidbody>()!= null)
			{
				child.gameObject.GetComponent<Rigidbody>().velocity = default(Vector3);
            	child.gameObject.GetComponent<Rigidbody>().angularVelocity = default(Vector3);

			}
            
        }
		
		maxFoodHeight = 0;
		runningTime = 0;
		foodState = FoodState.inPan;
		panOrigin = panCenter.position;
		pastVelocityFood = Vector3.zero;
		pastVelocityPan = Vector3.zero;

	}

	public override void AgentOnDone()
	{

	}
}

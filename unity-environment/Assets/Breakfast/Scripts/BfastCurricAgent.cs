using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BfastCurricAgent : Agent
{

    public enum FoodState { inPan, goingUp, goingDown, caught, missed }

    public FoodState foodState = FoodState.inPan;

    // Variables that need to be reset: 
    // Starting place of pan
    Vector3 panOrigin;
    float maxFoodHeight = 0;
    public float runningTime = 0;
    bool faceDown = false;
    Vector3 pastVelocityFood;
    Vector3 pastVelocityPan;


    public float foodDot;

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

    // Maxium amount it can move in one step
    public float moveMult = 0.1f;
    // Maximum amount it can rotate in one step
    public float rotateMult = 10f;

    public float moveLerp = 3f;
    public float rotateLerp = 3f;
    float armRadius2;
    float armRadius;

    Vector3 newPosition;
    Quaternion newRotation;

    Dictionary<GameObject, Vector3> transformsPosition;
    Dictionary<GameObject, Quaternion> transformsRotation;

    public void HitFloor()
    {
        foodState = FoodState.missed;
        //reward -= 2f;
        done = true;
    }

    public override void InitializeAgent()
    {
        panOrigin = panCenter.transform.position;
        armRadius2 = (limbs[2].position - limbs[0].position).sqrMagnitude;
        armRadius = Mathf.Sqrt(armRadius2);

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




        for (int i = 2; i < limbs.Length; i++)
        {
            Transform t = limbs[i];
            state.Add(t.localRotation.x);
            state.Add(t.localRotation.y);
            state.Add(t.localRotation.z);
            state.Add(t.localRotation.w);
            Rigidbody rb = t.gameObject.GetComponent<Rigidbody>();
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
        reward += 0.01f;

        reward += (3 - (panCenter.position - food.position).sqrMagnitude) * 0.05f;

        if (!faceDown)
            foodDot = Vector3.Dot(panCenter.up, food.up);
        else
            foodDot = Vector3.Dot(panCenter.up, -food.up);
        reward += (foodDot + 1) * 0.05f;
        if (foodDot >= 0.9f)
            faceDown = !faceDown;

        // Always penalize pan for going outside of boundary
        if ((panCenter.position - panOrigin).magnitude > panBoundary)
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
        Vector3 initial = limbs[2].position;
        Quaternion initialQ = limbs[2].rotation;

        float moveX = Mathf.Clamp(act[0], -1, 1) * moveMult;
        float moveY = Mathf.Clamp(act[1], -1, 1) * moveMult;
        float moveZ = Mathf.Clamp(act[2], -1, 1) * moveMult;

        float rotateY = Mathf.Clamp(act[3], -1, 1) * rotateMult;
        float rotateZ = Mathf.Clamp(act[4], -1, 1) * rotateMult;

        newPosition = initial + new Vector3(moveX, moveY, moveZ);
        newRotation = initialQ * Quaternion.AngleAxis(rotateY, Vector3.up) * Quaternion.AngleAxis(rotateZ, Vector3.forward);

       
        // Cannot extend past length of arm
        if((newPosition - limbs[0].position).sqrMagnitude > armRadius2)
        {
            // If it does, use same direction, but place it back within arm's reach
            Vector3 dir = newPosition - limbs[0].position;
            newPosition = limbs[0].position + dir * armRadius;

            // limbs[2].position = initial;
        }

        
        


    }

    private void FixedUpdate()
    {
        Vector3 pos = Vector3.Lerp(limbs[2].position, newPosition, Time.fixedDeltaTime * moveLerp);
        limbs[2].GetComponent<Rigidbody>().MovePosition(pos);
        Quaternion rot = Quaternion.Lerp(limbs[2].rotation, newRotation, Time.fixedDeltaTime * rotateLerp);
        limbs[2].GetComponent<Rigidbody>().MoveRotation(rot);
    }

    public override void AgentReset()
    {
        Transform[] allChildren = GetComponentsInChildren<Transform>();

        foreach (Transform child in allChildren)
        {

            child.position = transformsPosition[child.gameObject];
            child.rotation = transformsRotation[child.gameObject];
            if (child.gameObject.GetComponent<Rigidbody>() != null)
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
        faceDown = false;
    }

    public override void AgentOnDone()
    {

    }
}

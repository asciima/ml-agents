using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakfastAgentRotate : Agent
{
    public BreakfastAcademy academy;
    public enum ArmState { initializing, catching, placing}
    public ArmState armState = ArmState.initializing;

    // Variables that need to be reset: 
    // Starting place of pan

    Vector3 targetOrigin;
    Vector3 panOrigin;
    float maxFoodHeight = 0;
    public float runningTime = 0;
    bool faceDown = false;
    Vector3 pastVelocityFood;
    Vector3 pastVelocityPan;


    public float foodDot;

    //Variables that don't need to be reset
    public Transform target;

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
    float[] actions = new float[5];

    Quaternion[] newRotation = new Quaternion[3];

    Dictionary<GameObject, Vector3> transformsPosition;
    Dictionary<GameObject, Quaternion> transformsRotation;
    Vector3 initialOffset;

    public Transform referencePoint;

    public bool inPan { get{ return _inPan; } set{ 
        _inPan = value;
        if(armState == ArmState.initializing)
            {armState = ArmState.catching; } 
        } 
    }
    // Quaternion initialRotation;

    bool _inPan = false;
    public void HitFloor()
    {
        reward -= 1f;
        done = true;
    }
    public void FoodCaught()
    {
        // reward += 1f;
        armState = ArmState.placing;
    }
    public void Plated()
    {
        reward += 2f;
        Debug.Log("Win!");
        done = true;
    }
    

    public override void InitializeAgent()
    {
        targetOrigin = target.position;
        inPan = false;
        panOrigin = panCenter.transform.position;
        
        transformsPosition = new Dictionary<GameObject, Vector3>();
        transformsRotation = new Dictionary<GameObject, Quaternion>();
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            transformsPosition[child.gameObject] = child.position;
            transformsRotation[child.gameObject] = child.rotation;
        }
        initialOffset = limbs[2].position - limbs[1].position;
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

        Vector3 toTarget = target.position - food.position;
        state.Add(toTarget.x);
        state.Add(toTarget.y);
        state.Add(toTarget.z);




        for (int i = 2; i < limbs.Length; i++)
        {
            Transform t = limbs[i];
            state.Add(t.localRotation.x);
            state.Add(t.localRotation.y);
            state.Add(t.localRotation.z);
            state.Add(t.localRotation.w);
            Rigidbody rb = t.gameObject.GetComponent<Rigidbody>();
            if(rb != null)
            {
                state.Add(rb.velocity.x);
                state.Add(rb.velocity.y);
                state.Add(rb.velocity.z);
                state.Add(rb.angularVelocity.x);
                state.Add(rb.angularVelocity.y);
                state.Add(rb.angularVelocity.z);

            }
            

        }

        return state;
    }

    public override void AgentStep(float[] act)
    {
        actions = act;
        //runningTime += Time.deltaTime;
        //reward += 0.01f;

        reward += (3 - (target.position - food.position).sqrMagnitude) * 0.05f;

        //if (!faceDown)
        //    foodDot = Vector3.Dot(panCenter.up, food.up);
        //else
        //    foodDot = Vector3.Dot(panCenter.up, -food.up);
        //reward += (foodDot + 1) * 0.05f;
        //if (foodDot >= 0.9f)
        //    faceDown = !faceDown;

        //// Always penalize pan for going outside of boundary
        //if ((panCenter.position - panOrigin).magnitude > panBoundary)
        //{
        //    float dist = ((panCenter.position - panOrigin).magnitude - panBoundary);
        //    reward -= dist * dist * panBoundaryRewardMult;
        //}

        // Move limbs according to inputs
        // MoveLimbs(act);
        Monitor.Log("Reward", reward, MonitorType.slider, target);

    }

    void MoveLimbs(float[] act)
    {
        Quaternion initial0 = limbs[0].rotation;
        float rotateY = Mathf.Clamp(act[0], -1, 1) * rotateMult;
        float rotateZ = Mathf.Clamp(act[1], -1, 1) * rotateMult;
        // newRotation[0] = initial0 * Quaternion.AngleAxis(rotateY, limbs[0].up) * Quaternion.AngleAxis(rotateZ, limbs[0].forward);

        // Quaternion rot = Quaternion.Lerp(limbs[0].rotation, newRotation[0], Time.fixedDeltaTime * rotateLerp);
        // limbs[0].transform.rotation = (newRotation[0]);

        limbs[0].Rotate(0, rotateY, rotateZ);

        Quaternion initial1 = limbs[1].rotation;
        rotateZ = Mathf.Clamp(act[2], -1, 1) * rotateMult;
        // newRotation[1] = initial1 * Quaternion.AngleAxis(rotateZ, limbs[1].forward);
        limbs[1].Rotate(0,0,rotateZ);


        Quaternion initial2 = limbs[2].rotation;
        rotateY = Mathf.Clamp(act[3], -1, 1) * rotateMult;
        rotateZ = Mathf.Clamp(act[4], -1, 1) * rotateMult;
        newRotation[2] = initial2 * Quaternion.AngleAxis(rotateY, limbs[2].right) * Quaternion.AngleAxis(rotateZ, limbs[2].forward);

        // rot = Quaternion.Lerp(limbs[2].rotation, newRotation[2], Time.fixedDeltaTime * rotateLerp);
        // limbs[2].GetComponent<Rigidbody>().MoveRotation(newRotation[2]);
    }

    private void FixedUpdate()
    {
        if(armState != ArmState.initializing)
        {
            MoveLimbs(actions);
            // for(int i=0; i<limbs.Length; i++)
            // {
            //     Quaternion rot = Quaternion.Lerp(limbs[i].rotation, newRotation[i], Time.fixedDeltaTime * rotateLerp);
            //     limbs[i].GetComponent<Rigidbody>().MoveRotation(rot);
            // }
            limbs[2].GetComponent<Rigidbody>().MovePosition(referencePoint.position);
            limbs[2].GetComponent<Rigidbody>().MoveRotation(newRotation[2]);

        }
        
        
    }

    public override void AgentReset()
    {

        float randTarget = academy.GetComponent<BreakfastAcademy>().randomTarget;

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
        armState = ArmState.initializing;
        panOrigin = panCenter.position;
        pastVelocityFood = Vector3.zero;
        pastVelocityPan = Vector3.zero;
        target.position = targetOrigin;
        target.position = RandomTarget(randTarget);
        

        
    }

    Vector3 RandomTarget(float randTarget)
    {
        return target.position  += new Vector3(Random.Range(-randTarget, randTarget), 0, Random.Range(-randTarget, randTarget));
        
    }

    public override void AgentOnDone()
    {

    }
}

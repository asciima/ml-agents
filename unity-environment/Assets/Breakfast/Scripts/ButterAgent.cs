using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButterAgent : Agent
{
	public Transform butter;
	public Transform bot;
	public Transform sensorRayOrigin;
	public Transform goal;
	public List<Transform>obstacles;

	Dictionary<GameObject, Vector3> transformsPosition;
	Dictionary<GameObject, Quaternion> transformsRotation;

	Vector3[] sensorRays;
	public float sensorFov = 45;
	public float sensorRayLength = 1f;

	public float maxRotate = 10f;
	public float maxMove = 0.1f;

	public int obstaclesSeen = 0;

	public ObstacleManager obstacleManager;

    float initialOffset;


	public override void InitializeAgent()
    {
        initialOffset = (goal.position - butter.position).sqrMagnitude;
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
        Vector3 velocity = bot.GetComponent<Rigidbody>().velocity;


		state.Add(bot.position.x);
		state.Add(bot.position.z);
		state.Add(bot.position.y);
		state.Add(goal.position.x);
		state.Add(goal.position.z);
		// state.Add(butter.position.x);
		// state.Add(butter.position.z);

		state.Add(bot.rotation.y);
		// state.Add(butter.rotation.y);

		// Doesn't know the position of the goal, but knows the heading!
		state.Add(Vector3.Dot(goal.position - bot.position, bot.forward));
		obstaclesSeen = 0;

		for(int i=-1; i<2; i++)
		{
			for(int j=-1; j<2; j++)
			{
				float isObstacle = 0;
				Quaternion originalRotation = sensorRayOrigin.rotation;
				sensorRayOrigin.Rotate(i * sensorFov, j * sensorFov, 0);
				Vector3 direction = sensorRayOrigin.TransformDirection(Vector3.forward);
				sensorRayOrigin.rotation = originalRotation;
				// direction = sensorRayOrigin.forward * sensorRayLength;
				RaycastHit hit;
        		if(Physics.Raycast(sensorRayOrigin.position, direction, out hit, sensorRayLength))
				{
					if(hit.collider.gameObject.tag == "obstacle")
					{
						isObstacle = 1;
						obstaclesSeen += 1;
					}
				}
				state.Add(isObstacle);

			}
		}
	

		return state;
	}

	public void HitFloor()
	{
		reward -= 2f;
		done = true;
		Debug.Log("Hit floor");
	}
	public void HitObstacle()
	{
		reward -= 0.25f;
		Debug.Log("Hit Obstacle");
	}
	public void HitGoal()
	{
		reward += 2f;
		done = true;
		Debug.Log("Goal!");
	}

	// void OnDrawGizmos() {
    //     Gizmos.color = Color.green;
	// 	for(int i=-1; i<2; i++)
	// 	{
	// 		for(int j=-1; j<2; j++)
	// 		{
	// 			Quaternion originalRotation = sensorRayOrigin.rotation;
	// 			sensorRayOrigin.Rotate(i * sensorFov, j * sensorFov, 0);
	// 			Vector3 direction = sensorRayOrigin.TransformDirection(Vector3.forward) * sensorRayLength;
	// 			sensorRayOrigin.rotation = originalRotation;
	// 			// direction = sensorRayOrigin.forward * sensorRayLength;
    //     		Gizmos.DrawRay(sensorRayOrigin.position, direction);

	// 		}
	// 	}
        
    // }

    public void MoveAgent(float[] act) {

		// Debug.DrawRay(sensorRayOrigin.position, sensorRayOrigin.forward, Color.green);

		float yRotation = Mathf.Clamp(act[0], -1f, 1f) * maxRotate;
		float forwardDir = Mathf.Clamp(act[1], -1f, 1f) * maxMove;

		bot.Rotate(0, yRotation, 0);
		bot.Translate(forwardDir * Vector3.forward, Space.Self);


		// directionX = Mathf.Clamp(act[0], -1f, 1f);
		// directionZ = Mathf.Clamp(act[1], -1f, 1f);
		// directionY = Mathf.Clamp(act[2], -1f, 1f);
      

        // Vector3 fwd = transform.TransformDirection(Vector3.down);
        // if (!Physics.Raycast(transform.position, fwd, rayDepth) &&
        //     !Physics.Raycast(transform.position + new Vector3(edge, 0f, 0f), fwd, rayDepth) &&
        //     !Physics.Raycast(transform.position + new Vector3(-edge, 0f, 0f), fwd, rayDepth) &&
        //     !Physics.Raycast(transform.position + new Vector3(0.0f, 0f, edge), fwd, rayDepth) &&
        //     !Physics.Raycast(transform.position + new Vector3(0.0f, 0f, -edge), fwd, rayDepth) &&
        //     !Physics.Raycast(transform.position + new Vector3(edge, 0f, edge), fwd, rayDepth) &&
        //     !Physics.Raycast(transform.position + new Vector3(-edge, 0f, edge), fwd, rayDepth) &&
        //     !Physics.Raycast(transform.position + new Vector3(edge, 0f, -edge), fwd, rayDepth) &&
        //     !Physics.Raycast(transform.position + new Vector3(-edge, 0f, -edge), fwd, rayDepth))
        // {
        //     directionY = 0f;
        //     directionX = directionX / 5f;
        //     directionZ = directionZ / 5f;
        // }

        // gameObject.GetComponent<Rigidbody>().AddForce(new Vector3(directionX * 40f, directionY * 300f, directionZ * 40f));
        // if (GetComponent<Rigidbody>().velocity.sqrMagnitude > 25f)
        // {
        //     GetComponent<Rigidbody>().velocity *= 0.95f;
        // }
    }

	public override void AgentStep(float[] act)
	{
		reward -= 0.01f;
        MoveAgent(act);

		reward += ((initialOffset - (goal.position - butter.position).sqrMagnitude)/initialOffset) * 0.025f;

        Monitor.Log("Reward", reward, MonitorType.slider, goal.transform);

    }

	public override void AgentReset()
	{
        Debug.Log("Reward: " + reward);

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

		obstacleManager.Shuffle();

	}


}

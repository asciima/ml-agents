using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour {
	public GameObject[] obstacleTypes;
	public int numObstacles = 3;

	public GameObject[] currentObstacles;

	void Start()
	{
		currentObstacles = new GameObject[numObstacles];
		float width = gameObject.transform.localScale.x/2;
		float length = gameObject.transform.localScale.z/2;
		for(int i=0; i<numObstacles; i++)
		{
			// GameObject.Destroy(currentObstacles[i]);
			float x = transform.position.x + Random.Range(-width, width);
			float z = transform.position.z + Random.Range(-length, length);
			int obstacleIndex;
		
			obstacleIndex = i%obstacleTypes.Length;
			GameObject newObstacle = Instantiate(obstacleTypes[obstacleIndex], new Vector3(x, transform.position.y, z), Quaternion.identity);
			currentObstacles[i] = newObstacle;
			while(CheckPosition(i) == true)
			{
				x = transform.position.x + Random.Range(-width, width);
				z = transform.position.z + Random.Range(-length, length);
				currentObstacles[i].transform.position = new Vector3(x, transform.position.y, z);

			}
			
		}
	}
	

	public void Shuffle()
	{
		float width = gameObject.transform.localScale.x/2;
		float length = gameObject.transform.localScale.z/2;
		for(int i=0; i<numObstacles; i++)
		{
			float x = transform.position.x + Random.Range(-width, width);
			float z = transform.position.z + Random.Range(-length, length);
			currentObstacles[i].transform.position = new Vector3(x, transform.position.y, z);
			
			while(CheckPosition(i) == true)
			{
				x = transform.position.x + Random.Range(-width, width);
				z = transform.position.z + Random.Range(-length, length);
				currentObstacles[i].transform.position = new Vector3(x, transform.position.y, z);

			}
			
		}

	}

	bool CheckPosition(int i)
	{
		for(int p=0; p<i; p++)
		{
			if((currentObstacles[i].transform.position - currentObstacles[p].transform.position).magnitude < 0.75f)
			{
				return true;
			}
		}

		return false;

	}
}

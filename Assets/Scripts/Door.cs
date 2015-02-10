using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour {

	private GameObject leftSection;
	private GameObject rightSection;

	// Use this for initialization
	void Start () 
	{
		leftSection = GameObject.Find("Door/door_left");
		rightSection = GameObject.Find("Door/door_right");

		// Ignore collisions with the lab walls
		GameObject robotLab = GameObject.Find("env_robotLab_static");
		MeshCollider labCollider = robotLab.GetComponent<MeshCollider>();
		Physics.IgnoreCollision(labCollider, leftSection.GetComponent<BoxCollider>());
		Physics.IgnoreCollision(labCollider, rightSection.GetComponent<BoxCollider>());
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (leftSection.transform.position.x <= -4.2f)
			leftSection.rigidbody.velocity = new Vector3(0.0f, 0.0f, 0.0f);

		if (rightSection.transform.position.x >= 4.3f)
			rightSection.rigidbody.velocity = new Vector3(0.0f, 0.0f, 0.0f);
	}

	public void Open()
	{
		leftSection.rigidbody.velocity = new Vector3(-0.75f, 0.0f, 0.0f);
		rightSection.rigidbody.velocity = new Vector3(0.75f, 0.0f, 0.0f);
	}
}

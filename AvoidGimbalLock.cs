using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Rotations();
		//EulerAngles();
		Debug.DrawLine(transform.position, transform.position + transform.forward * 3, Color.blue);
		Debug.DrawLine(transform.position, transform.position + transform.right * 3, Color.red);
		Debug.DrawLine(transform.position, transform.position + transform.up * 3, Color.green);
	}

	private float ex = 0.0f;
	private float ey = 0.0f;
	private float ez = 0.0f;

	private void Rotations()
	{
		if(Input.GetKey(KeyCode.A))
		{
			ey = -1;
			transform.rotation *= Quaternion.Euler(ex, ey, ez);
		}
		if(Input.GetKey(KeyCode.D))
		{
			ey = 1;
			transform.rotation *= Quaternion.Euler(ex, ey, ez);
		}
		
		if(Input.GetKey(KeyCode.W))
		{
			ex = -1;
			transform.rotation *= Quaternion.Euler(ex, ey, ez);
		}
		if(Input.GetKey(KeyCode.S))
		{
			ex = 1;
			transform.rotation *= Quaternion.Euler(ex, ey, ez);
		}

		ex = 0;
		ey = 0;
		ez = 0;
	}
}

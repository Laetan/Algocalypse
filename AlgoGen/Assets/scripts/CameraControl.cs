using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

	public int moveSpeed;
	private bool running;
	private Vector3 target;

	void Start () {
		running = false;
		target = new Vector3 (0, 0);
		transform.LookAt (target, transform.up);
	}
	public void Launch(){running = true;}
	
	// Update is called once per frame
	void Update () {
		Vector3 move = new Vector3 (0, 0);
		bool moved = false;
		if(Input.GetKey(KeyCode.LeftArrow)){
			if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) 
				move.x -= moveSpeed/2 * Time.deltaTime;
			else
				move.x -= moveSpeed * Time.deltaTime;
		}
		if(Input.GetKey(KeyCode.RightArrow)){
			move.x += moveSpeed * Time.deltaTime;
		}
		if(Input.GetKey(KeyCode.UpArrow)){
			move.y += moveSpeed * Time.deltaTime;
		}
		if(Input.GetKey(KeyCode.DownArrow)){
			move.y -= moveSpeed * Time.deltaTime;
		}

		if(running)transform.position = transform.position + move;



	}
}

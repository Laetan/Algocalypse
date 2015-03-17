using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

	public int moveSpeed;
	private bool running;
	private Vector3 target;

	private float length, height, width;
	void Start () {
		width = 2.4F;
		height = 2.7F;
		length = 12;
		running = false;
	}
	public void setWidth(string value){width = float.Parse(value);}
	public void setHeight(string value){height = float.Parse(value);}
	public void setLength(string value){length = float.Parse(value);}

	public void Launch(){
		running = true;
		target = new Vector3 (length/2, height/2, width/2);
		transform.LookAt (target, transform.up);
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 move = new Vector3 (0, 0);
		bool moved = false;
		if(Input.GetKey(KeyCode.LeftArrow)){
			moved = true;
			if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) 
				target.x -= moveSpeed/2 * Time.deltaTime;
			else
				move.x -= moveSpeed * Time.deltaTime;
		}
		if(Input.GetKey(KeyCode.RightArrow)){
			moved = true;
			if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) 
				target.x += moveSpeed/2 * Time.deltaTime;
			else
				move.x += moveSpeed * Time.deltaTime;
		}
		if(Input.GetKey(KeyCode.UpArrow)){
			moved = true;
			if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) 
				target.y += moveSpeed/2 * Time.deltaTime;
			else
				move.y += moveSpeed * Time.deltaTime;
		}
		if(Input.GetKey(KeyCode.DownArrow)){
			moved = true;
			if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) 
				target.y -= moveSpeed/2 * Time.deltaTime;
			else
				move.y -= moveSpeed * Time.deltaTime;
		}

		if (transform.rotation.eulerAngles.x >= 85 && transform.rotation.eulerAngles.x <= 90 && move.y > 0)
						move.y = 0;
		else if (transform.rotation.eulerAngles.x <= 275 && transform.rotation.eulerAngles.x >= 270 && move.y < 0)
						move.y = 0;
		move = transform.rotation * move;
		if(running && moved){
			transform.position = transform.position + move;
			transform.LookAt(target);
		}



	}
}

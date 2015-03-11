using UnityEngine;
using System.Collections;

public class Default : MonoBehaviour {

	// Use this for initialization
	Transform origin;

	private int priority;

	void Start () {
		origin = transform.Find ("origin");
		//transform.position = -origin.position;
		//transform.position = new Vector3 (0, 0, 0);
	}
	public void getOrigin(){
		origin = transform.Find ("origin");
	}
	// x : width, y : length, z : height
	public void resize(Vector3 v){
		transform.localScale = new Vector3 (v.y, v.z, v.x);
	}

	public Vector3 getSize(){
		return new Vector3 (transform.localScale.z, transform.localScale.x,transform.localScale.y);
	}
	public void rotate(){
		transform.localScale = new Vector3 (transform.localScale.z, transform.localScale.y, transform.localScale.x);
	}

	// x : width, y : length, z : height
	public void move(Vector3 v){
		if(!origin)
			origin = transform.Find ("origin");
		transform.position = new Vector3 (0, 0, 0);
		//Debug.Log (origin.position);
		//Debug.Log (new Vector3 (v.y, v.z, v.x) - origin.position);
		transform.position = new Vector3 (v.y, v.z, -v.x) - origin.position;
		//Debug.Log (origin.position);

	}

	public void setPriority(int value){
		priority = value;
	}
	
	public int getPriority(){
		return priority;
	}
}

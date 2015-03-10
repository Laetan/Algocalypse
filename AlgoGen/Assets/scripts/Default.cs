using UnityEngine;
using System.Collections;

public class Default : MonoBehaviour {

	// Use this for initialization
	Transform origin;

	void Start () {
		origin = transform.Find ("origin");
		//transform.position = -origin.position;
	}
	public void getOrigin(){
		origin = transform.Find ("origin");
	}
	// x : width, y : length, z : height
	public void resize(Vector3 v){
		transform.localScale = new Vector3 (v.y, v.z, v.x);
	}

	public void rotate(){
		transform.localScale = new Vector3 (transform.localScale.z, transform.localScale.y, transform.localScale.x);
	}

	// x : width, y : length, z : height
	public void move(Vector3 v){
		if(!origin)
			origin = transform.Find ("origin");
		//Debug.Log (origin);
		transform.position = new Vector3 (v.y, v.z, v.x) - origin.position;
	}

}

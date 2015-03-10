using UnityEngine;
using System.Collections;

public class BoxScript : MonoBehaviour {

	public int priority;
	
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void setPriority(int value){
		priority = value;
	}
	
	int getPriority(){
		return priority;
	}

	void setSize(int width, int length, int height){
		GameObject box = GameObject.Find ("/box");
		box.transform.localScale.Set(width,height,length);
		box.transform.position.Set(width / 2,height / 2,length / 2);
	}
	Vector3 getSize(){
		GameObject box = GameObject.Find ("/box");
		return new Vector3(box.transform.localScale.x, box.transform.localScale.y, box.transform.localScale.z);
	}


}

using UnityEngine;
using System.Collections;

public class BoxScript : MonoBehaviour {

	public int priority;
	public int id;
	
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public void setPriority(int value){
		priority = value;
	}
	
	int getPriority(){
		return priority;
	}
	
	public void setId(int value){
		id = value;
	}
}

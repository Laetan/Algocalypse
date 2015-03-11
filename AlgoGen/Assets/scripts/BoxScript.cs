using UnityEngine;
using System.Collections;

public class BoxScript : MonoBehaviour {

	public int priority;
	
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
	
}

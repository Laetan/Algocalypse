using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour {
	public float contenerWidth;
	public float contenerLength;
	public float contenerHeight;

	public int numberOfBoxes;
	public bool randomBoxes;
	public GameObject floor, wall, box;


	private GameObject[] boxes;

	void Start () {
		GameObject f = (GameObject)Instantiate (floor);
		f.SendMessage ("resize", new Vector3 (contenerWidth,contenerLength,1));
		f.SendMessage("move", new Vector3 (0,0,0));

		f = Instantiate (wall)  as GameObject;

		f.SendMessage ("resize", new Vector3 (contenerHeight,contenerLength,1));
		f.SendMessage("move", new Vector3 (0,0,0));

		for (int i = 0; i < numberOfBoxes; i++) {
			int width, length, height, priority;
			priority = Mathf.RoundToInt(Random.Range())
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

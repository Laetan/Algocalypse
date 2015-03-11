using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Controller : MonoBehaviour {
	public float contenerWidth;
	public float contenerLength;
	public float contenerHeight;

	public int numberOfBoxes;
	public bool randomBoxes;
	public GameObject floor, wall, box;
	
	private List<GameObject> boxes;
	private int dimX, dimY, dimZ;
	private int[] boxByPriority;
	private int[][] boxesPushed;
	private int[][][][] spaceMat; //position{3} - id/priority

	void Start () {
		boxes = new List<GameObject> ();
		boxByPriority = new int[11];
		boxesPushed = new int[numberOfBoxes][2];
		GameObject f = (GameObject)Instantiate (floor);
		f.SendMessage ("resize", new Vector3 (contenerWidth,contenerLength,1));
		f.SendMessage("move", new Vector3 (0,0,0));

		f = Instantiate (wall)  as GameObject;

		f.SendMessage ("resize", new Vector3 (contenerHeight,contenerLength,1));
		f.SendMessage("move", new Vector3 (0,0,0));

		Debug.Log ("-------Box----------");
		for (int i = 0; i < numberOfBoxes; i++) {
			float width, length, height;
			int priority;
			priority = Random.Range(1,10);
			width = Random.Range(1,3);
			height = Random.Range(1,3);
			if(width==1){
				width =1.200F;
				length = 0.800F;
			}
			else if(width==2){
				width =0.800F;
				length = 0.600F;
			}
			else{
				width =2.400F;
				length = 0.800F;
			}
			height = height==1? .500F : (height == 2 ? 1.2F : 2.0F);
			boxes.Add((GameObject) Instantiate(box));
			boxes[i].SendMessage("resize", new Vector3(width,length,height));
			boxes[i].SendMessage("setPriority", priority);
			boxes[i].SendMessage("move", new Vector3 (0,0,0));
			++boxByPriority[priority];
			//boxes[i].SetActive(false);
		}
		dimX = contenerWidth / 100; dimY = contenerLength / 100; dimZ = contenerHeight / 100;
		spaceMat = new int[dimX][dimY][dimZ][2];
		getBoxAt (new Vector3 (0, 0));
	}
	int getBoxAt(Vector3 pos, bool priority = false){
		return spaceMat[pos.x][pos.y][pos.z][(priority ? 1:0)];
	}
	int getBoxAt(int x, int y, int z, bool priority = false){
		return spaceMat[x][y][z][(priority ? 1:0)];
	}

	bool checkPos(int x,int y, int z,Vector3 bSize){
		if (x + bSize.x > dimX || y + bSize.y > dimY || z + bSize.z > dimZ)
						return false;
		if(z>0){
			for(int xx = x; xx < x+bSize.x;xx++){
				for(int yy = y; yy<y+bSize.y;yy++){
					if(spaceMat[xx][yy][z-1][1] == 0)
						return false;
				}
			}
		}

		for(int xx = x; xx < x+bSize.x;xx++){
			for(int yy = y; yy<y+bSize.y;yy++){
				for(int zz = z; zz<z+bSize.z; zz++){
					if(spaceMat[xx][yy][z-1][1] != 0)
						return false;
				}
			}
		}
		return true;

	}
	bool moveBoxTo(int x, int y, int z, int boxIndex, bool rotate = false){
		Debug.Log ("moving box");
		Default sc = (Default)boxes [boxIndex].GetComponentsInChildren<Default> () [0];
		if(rotate) sc.rotate();
		if(rotate) Debug.Log ("rotate");
		Vector3 bSize = sc.getSize ();
		bSize*=10;
		if(!checkPos(x,y,z,bSize)) return false;

		int priority = sc.getPriority ();
		for(int xx = x; xx < x+bSize.x;xx++){
			for(int yy = y; yy<y+bSize.y;yy++){
				for(int zz = z; zz<z+bSize.z; zz++){
					spaceMat[xx][yy][zz][0] = boxIndex;
					spaceMat[xx][yy][zz][1] = priority;
				}
			}
		}
		for(int i =0; i<numberOfBoxes; i++){
			if(boxesPushed[i][1]==0){
				boxesPushed[i][0] = boxIndex;
				boxesPushed[i][1] = priority;
				return true;
			}
		}
		Debug.LogError ("Could not push box into boxesPushed : out of bound");
		return true;

	}
	void orderBoxes(int[] chrom){
		clearSpace ();
		for(int i = 0; i<numberOfBoxes; i++){
			boxesPushed[i][0] = boxesPushed[i][1] = 0;
		}
		for(int i = 0; i< chrom.Length; i++){
		LoopStart:bool pushed = false;
			for(int y = 0; y<dimY; y++){
				for(int x = 0; x<dimX; x++){
					for(int z = 0; z<dimZ; z++){
						if(getBoxAt(x,y,z)==0){
							if(moveBoxTo(x,y,z,chrom[i])||moveBoxTo(x,y,z,chrom[i], true)){
								pushed = true;
								break;
							}
						}
						if(pushed) break;
					}
					if(pushed) break;
				}
				if(pushed) break;
			}
		}
	}

	float scoring(){
		float fr, fo, kr, ko;
		int space_left = 0;
		for (int x = 0; x< dimX; x++)
						for (int y = 0; y<dimY; y++)
								for (int z = 0; z<dimZ; z++)
										if (spaceMat [x] [y] [z] [0] == 0)
												++space_left;
		fr = 1 - (space_left / (dimX * dimY * dimZ));

		float fo_c1 = 0, fo_c2 = 0, fo_c3 = 0;

		for (int p = 1; p<=10; p++) {
			int np = 0;
			for(int i =0; i<numberOfBoxes; i++)
				if(boxesPushed[i][1]==p)
					++np;

			if(np == boxByPriority[p]){
				++fo_c1;
				continue;
			}
			int vol_pm = 0, vol_p;			//vol_pm: volume boxes pose de priorté inf ; vol_p : plus petite caisse de priorité p
			for(int i =0; i<numberOfBoxes; i++)
				if(boxesPushed[i][1]==p+1)
					vol_pm += getVolume(boxesPushed[i][0]);
			for(
		}

	
	}

	int getVolume(int boxIndex){
		Vector3 v = ((Default)boxes [boxIndex].GetComponentsInChildren<Default> () [0]).getSize ();
		return v.x * v.y * v.z * 1000;
	}
	void clearSpace(){
		for (int x = 0; x< dimX; x++){
			for (int y = 0; y<dimY; y++){
				for (int z = 0; z<dimZ; z++){
					spaceMat [x] [y] [z] [0] = 0;
					spaceMat [x] [y] [z] [1] = 0;
				}
			}
		}

	}
	// Update is called once per frame
	void Update () {
	
	}
}

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
	private int[,] boxesPushed;
	private Vector3[] boxPositions, boxSizes;
	private int[,,,] spaceMat; //position{3} - id/priority
	private List<Vector3> vertex;
	private Vector3[,] boxSizePositions;
	private int[] chromosome;
	private int minx,minY,minZ;
	private int pasX, pasY,pasZ;
	/**
	 * Algo placement
	 * calculer point au angles des boites -> creer mesh de boites
	 * recuperer position et taille des boites posée
	 * Quand on place une nouvelle boites, la placer sur un point du mesh
	 * Verfifer si un autre point existe sans la zone de la boite
	 * verifier la collision avec toute les boites
	 * */
	void Start () {
		boxes = new List<GameObject> ();
		boxByPriority = new int[11];
		boxPositions = new Vector3[numberOfBoxes];
		boxesPushed = new int[numberOfBoxes,2];
		boxSizePositions = new Vector3[numberOfBoxes, 2];
		boxSizes = new Vector3[numberOfBoxes];
		vertex = new List<Vector3> ();
		GameObject f = (GameObject)Instantiate (floor);
		f.SendMessage ("resize", new Vector3 (contenerWidth/10,contenerLength/10,1));
		f.SendMessage("move", new Vector3 (0,0,0));

		f = Instantiate (wall)  as GameObject;

		f.SendMessage ("resize", new Vector3 (contenerHeight/10,contenerLength/10,1));
		f.SendMessage("move", new Vector3 (0,0,0));

		//Debug.Log ("-------Box----------");
		int[][] sizes = new int[3][]; 
		sizes [0] = new int[numberOfBoxes];
		sizes [1] = new int[numberOfBoxes];
		sizes [2] = new int[numberOfBoxes];
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
			sizes[0][i] = (int)(width*1000); sizes[1][i] = (int)(length*1000); sizes[2][i] = (int)(height*1000);
			boxSizes[i] = new Vector3(width,length,height);
			height = height==1? .500F : (height == 2 ? 1.2F : 2.0F);
			boxes.Add((GameObject) Instantiate(box));
			boxes[i].SendMessage("resize", boxSizes[i]);
			boxes[i].SendMessage("setPriority", priority);
			//boxes[i].SendMessage("move", new Vector3 (0,0,0));
			++boxByPriority[priority];
			//boxes[i].SetActive(false);
		}
		dimX = Mathf.RoundToInt(contenerWidth * 10); dimY = Mathf.RoundToInt(contenerLength * 10); dimZ = Mathf.RoundToInt(contenerHeight * 10);
		spaceMat = new int[dimX,dimY,dimZ,2];
		Debug.Log (getPas (getPas(sizes[0]),getPas(sizes[1])));
		//Debug.Log (dimX);
		//Debug.Log("dim: "+dimX+" ; "+dimY+" ; "+dimZ);
		//chromosome = initialisze ();
		//orderBoxes (chromosome);
		//Debug.Log ("score : " + scoring ());
		//Debug.Log ("----------------------");
		//Debug.Log ("move to 0,0,0");
		//boxes[0].SendMessage("move", new Vector3 (0,0,0));
		//boxes[0].SendMessage("move", new Vector3 (0,0,12));
		//Debug.Log ("----------------------");
		//Debug.Log ("move to 0,0,0.12");
		//((Default)boxes [0].GetComponentsInChildren<Default> () [0]).move (new Vector3 (24/  10, 0 / 10, 12 / 10));
	}
	int getBoxAt(Vector3 pos, bool priority = false){
		return spaceMat[(int)pos.x,(int)pos.y,(int)pos.z,(priority ? 1:0)];
	}
	int getBoxAt(int x, int y, int z, bool priority = false){
		return spaceMat[x,y,z,(priority ? 1:0)];
	}

	bool checkPos(int x,int y, int z,Vector3 bSize){

		if (x + bSize.x > dimX || y + bSize.y > dimY || z + bSize.z > dimZ)
						return false;
		//Debug.Log ("size : " + bSize.x + " ; " + bSize.y + " ; " + bSize.z);
		//Debug.Log("position : " + x + " ; " + y + " ; " + z);

		if(z>0){
			for(int xx = x; xx < x+bSize.x;xx++){
				for(int yy = y; yy<y+bSize.y;yy++){
					if(spaceMat[xx,yy,z-1,1] == 0)
						return false;
				}
			}
		}

		for(int xx = x; xx < x+bSize.x;xx++){
			for(int yy = y; yy<y+bSize.y;yy++){
				for(int zz = z; zz<z+bSize.z; zz++){
					if(spaceMat[xx,yy,zz,1] != 0)
						return false;
				}
			}
		}
		return true;

	}
	int[] initialisze(){
		int[] chrom = new int[numberOfBoxes];
		for (int i = 0; i<numberOfBoxes; ++i)
						chrom [i] = i;
		for (int i1 = 0; i1<numberOfBoxes-1; ++i1){
			int i2 = Random.Range(i1, numberOfBoxes-1);
			int temp = chrom[i2];
			chrom[i2] = chrom[i1];
			chrom[i1] = temp;
		}
		return chrom;
	}
	bool moveBoxTo(int x, int y, int z, int boxIndex, bool rotate = false){
		//Debug.Log ("moving box to : " + x + " ; " + y + " ; " + z);
		Default sc = (Default)boxes [boxIndex].GetComponentsInChildren<Default> () [0];
		if(rotate) sc.rotate();
		//if(rotate) Debug.Log ("rotate");
		Vector3 bSize = sc.getSize ();
		bSize*=10;
		if(!checkPos(x,y,z,bSize)) return false;
		sc.move (new Vector3 ((float)x/10, (float)y/10, (float)z/10));
		Debug.Log("move to : " + (float)x/10 + " ; " + (float)y/10 + " ; " + (float)z/10);
		int priority = sc.getPriority ();
		for(int xx = x; xx < x+bSize.x;xx++){
			for(int yy = y; yy<y+bSize.y;yy++){
				for(int zz = z; zz<z+bSize.z; zz++){
					spaceMat[xx,yy,zz,0] = boxIndex;
					spaceMat[xx,yy,zz,1] = priority;
				}
			}
		}
		for(int i =0; i<numberOfBoxes; i++){
			if(boxesPushed[i,1]==0){
				boxesPushed[i,0] = boxIndex;
				boxesPushed[i,1] = priority;
				return true;
			}
		}
		Debug.LogError ("Could not push box into boxesPushed : out of bound");
		return true;

	}
	void orderBoxes(int[] chrom){
		clearSpace ();
		for(int i = 0; i<numberOfBoxes; i++){
			boxesPushed[i,0] = boxesPushed[i,1] = 0;
			//boxPositions[i]=new Vector3(0,0);
		}
		for(int i = 0; i< chrom.Length; i++){
			bool pushed = false;
			for(int y = 0; y<dimY; y++){
				for(int x = 0; x<dimX; x++){
					for(int z = 0; z<dimZ; z++){
						if(spaceMat [x,y,z,0] == -1){
							if(z>0 && spaceMat [x,y,z-1,0]==-1) break;
							if(moveBoxTo(x,y,z,chrom[i])||moveBoxTo(x,y,z,chrom[i], true)){
								//Debug.Log("pushed "+chrom[i]+" to : " + x + " ; " + y + " ; " + z);
								boxPositions[chrom[i]] = new Vector3(x,y,z);
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
		float fr, fo, kr = 1, ko = 1;
		int space_left = 0;
		for (int x = 0; x< dimX; x++)
						for (int y = 0; y<dimY; y++)
								for (int z = 0; z<dimZ; z++)
										if (spaceMat [x,y,z,0] == -1)
												++space_left;
		fr = 1 - (space_left / (dimX * dimY * dimZ));

		float fo_c1 = 0, fo_c2 = 0, fo_c3 = 0;

		//---Calcul premiere composante---//
		for (int p = 1; p<=10; p++) {
			if(boxByPriority[p]==0){
				fo_c1 ++;
				continue;
			}
			int np = 0;
			for(int i =0; i<numberOfBoxes; i++)
				if(boxesPushed[i,1]==p)
					++np;

			if(np == boxByPriority[p]){
				++fo_c1;
				continue;
			}
			int vol_pm = 0, vol_p = 9999;			//vol_pm: volume boxes pose de priorté inf ; vol_p : plus petite caisse de priorité p
			for(int i =0; i<numberOfBoxes; i++)
				if(boxesPushed[i,1]==p+1)
					vol_pm += getVolume(boxesPushed[i,0]);
			for(int i = 0; i<numberOfBoxes; ++i){
				if(((Default)boxes [i].GetComponentsInChildren<Default> () [0]).getPriority ()==p){
					int v = getVolume(i);
					vol_p= (v>vol_p?v:vol_p);
				}
			}
			if(vol_pm+space_left<vol_p)
				++fo_c1;
			else
				fo_c1 += 1-(vol_pm+space_left)/(dimX * dimY * dimZ);
		}

		for(int priority = 1; priority <= 10; ++priority){
			if(boxByPriority[priority]==0){
				fo_c2 ++;
				fo_c3 ++;
				continue;
			}
			int index = 0;
			int n1 = 0, n2 = 0;
			while (index < boxesPushed.Length/2 && boxesPushed[index,1]!=0) {
				if(boxesPushed[index,1]!=priority){
					++index;
					continue;
				}
				Vector3 pos = boxPositions[boxesPushed[index,0]];
				Vector3 size = ((Default)boxes [boxesPushed[index,0]].GetComponentsInChildren<Default> () [0]).getSize ();
				size*=10;
				for (int x = (int)pos.x; x< pos.x+size.x; x++){
					for (int z = (int)pos.z; z<pos.z+size.z; z++){
						for(int y = (int)pos.y; y>=0; --y){
							if (spaceMat [x,y,z,1] > priority){
								++n1;
								goto Next_Comp;
							}
						}
					}
				}
			Next_Comp:for(int x = (int)pos.x; x< pos.x+size.x; x++){
					for (int y = (int)pos.y; y<pos.y+size.y; y++){
						for(int z = (int)pos.z+(int)size.z; z<dimZ; z++){
							if (spaceMat [x,y,z,1] > priority){
								++n2;
								goto Next_Loop;
							}
						}
					}
				}
			Next_Loop:++index;
			}
			fo_c2 += 1-n1/boxByPriority[priority];
			fo_c3 += 1-n2/boxByPriority[priority];

		}

		fo = fo_c1 + fo_c2 + fo_c3;
		fo /= 30;
	

		//--------Test---------//
		/*int vv = 0;
		int ii =0;
		//Debug.Log (boxesPushed.Length/2);
		while (ii < boxesPushed.Length/2 && boxesPushed[ii,1]!=0) {
			vv+=getVolume(boxesPushed[ii,0]);
			ii++;
		};
		Debug.Log (dimX * dimY * dimZ - space_left - vv);*/
		//END

		return (fo * ko + fr * kr) / (ko + kr);
	}

	int getVolume(int boxIndex){
		Vector3 v = ((Default)boxes [boxIndex].GetComponentsInChildren<Default> () [0]).getSize ();
		return (int)(v.x * v.y * v.z * 1000);
	}
	void clearSpace(){
		for (int x = 0; x< dimX; x++){
			for (int y = 0; y<dimY; y++){
				for (int z = 0; z<dimZ; z++){
					spaceMat [x,y,z,0] = -1;
					spaceMat [x,y,z,1] = 0;
				}
			}
		}
	}

	// Update is called once per frame
	void Update () {
	
	}

	int getPas(int[] sizes){
		if (sizes.Length > 2) {
			int mid = sizes.Length/2;
			int[] sub1 = new int[mid];
			int[] sub2 = new int[sizes.Length-mid];
			for(int i = 0; i< sizes.Length; ++i){
				if(i<mid) sub1[i] = sizes[i];
				else sub2[i-mid] = sizes[i];
			}
			int[] sub = {getPas(sub1), getPas(sub2)};
			return getPas(sub);
		}
		else if(sizes.Length==2){
			return getPas(sizes[0],sizes[1]);

		}
		else
			return sizes[0];
	}

	int getPas(int a, int b){
		return b == 0 ? a : getPas(b, a % b);
	}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Controller : MonoBehaviour {
	public float contenerWidth;
	public float contenerLength;
	public float contenerHeight;

	public int numberOfBoxes;
	public int population;
	public int numberOfEvolutions;
	public bool stepByStep;
	public bool randomBoxes;
	public GameObject floor, wall, box;
	public Canvas UI;
	
	private List<GameObject> boxes;
	private int dimX, dimY, dimZ;
	private int[] boxByPriority;
	private int[,] boxesPushed;
	private Vector3[] boxPositions, boxSizes;
	private int[,,,] spaceMat; //position{3} - id/priority
	private List<Vector3> startPoints;
	private List<Lane> lanes;
	private int maxActiv,activ;

	struct Lane{
		public float x1, y1, x2, y2, z;

		public Lane(float xa, float ya, float xb, float yb, float Z){
			x1 = (xa < xb ? xa : (xa > xb ? xb : (ya < yb ? xa : xb)));
			x2 = (xa < xb ? xb : (xa > xb ? xa : (ya < yb ? xb : xa)));
			y1 = (xa < xb ? ya : (xa > xb ? yb : (ya < yb ? ya : yb)));
			y2 = (xa < xb ? yb : (xa > xb ? ya : (ya < yb ? yb : ya)));
			z=Z;
		}
	}
	//private Vector3[,] boxSizePositions;
	private int[] chromosome;
	private float minX,minY,minZ;
	private int pasX, pasY,pasZ;
	private float time;
	/**
	 * Algo placement
	 * calculer point au angles des boites -> creer mesh de boites
	 * recuperer position et taille des boites posée
	 * Quand on place une nouvelle boites, la placer sur un point du mesh
	 * Verfifer si un autre point existe sans la zone de la boite
	 * verifier la collision avec toute les boites
	 * */

	private int[][] chromosomes;


	void Start () {
		time = Time.realtimeSinceStartup;
		boxes = new List<GameObject> ();
		boxByPriority = new int[11];
		boxPositions = new Vector3[numberOfBoxes];
		boxesPushed = new int[numberOfBoxes,2];
		//boxSizePositions = new Vector3[numberOfBoxes, 2];
		boxSizes = new Vector3[numberOfBoxes];
		startPoints = new List<Vector3> ();
		lanes = new List<Lane> ();
		minX = contenerWidth;
		minY = contenerLength;
		minZ = contenerHeight;


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
			height = height==1? .500F : (height == 2 ? 1.2F : 2.0F);
			sizes[0][i] = (int)(width*1000); sizes[1][i] = (int)(length*1000); sizes[2][i] = (int)(height*1000);
			minX = (minX > width ? width : minX);
			minY = (minY > length ? length : minY);
			minZ = (minZ > height ? height : minZ);
			boxSizes[i] = new Vector3(width,length,height);
			boxPositions[i] = new Vector3(-1,-1,-1);
			boxes.Add((GameObject) Instantiate(box));
			boxes[i].SendMessage("resize", boxSizes[i]);
			boxes[i].SendMessage("setPriority", priority);
			boxes[i].SendMessage("setId", i);
			//boxes[i].SendMessage("move", new Vector3 (0,0,0));
			++boxByPriority[priority];
			//boxes[i].SetActive(false);
		}
		dimX = Mathf.RoundToInt(contenerWidth * 10); dimY = Mathf.RoundToInt(contenerLength * 10); dimZ = Mathf.RoundToInt(contenerHeight * 10);
		spaceMat = new int[dimX,dimY,dimZ,2];


		startPoints.Add (new Vector3 (0, 0, 0));
		lanes.Add(new Lane(0,0,contenerWidth,0,0));
		lanes.Add(new Lane(0,0,0,contenerLength,0));
		lanes.Add(new Lane(0,contenerLength,contenerWidth,contenerLength,0));
		lanes.Add(new Lane(contenerWidth,0,contenerWidth,contenerLength,0));
		/*lanes.Add(new float[5]{0,0,contenerWidth,0,0});
		lanes.Add(new float[5]{0,0,0,contenerLength,0});
		lanes.Add(new float[5]{0,contenerLength,contenerWidth,contenerLength,0});
		lanes.Add(new float[5]{contenerWidth,0,contenerWidth,contenerLength,0});*/
		initialiszePopulation ();
		orderBoxes (chromosomes [0]);
		display ();
	}
	int getBoxAt(Vector3 pos, bool priority = false){
		return spaceMat[(int)pos.x,(int)pos.y,(int)pos.z,(priority ? 1:0)];
	}
	int getBoxAt(int x, int y, int z, bool priority = false){
		return spaceMat[x,y,z,(priority ? 1:0)];
	}

	/*bool checkPos(int x,int y, int z,Vector3 bSize){

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

	}*/
	void initialiszePopulation(){
		chromosomes = new int[population][];
		for (int j = 0; j < population; ++j) {
			chromosomes[j] = new int[numberOfBoxes];
		}
		for (int i = 0; i<numberOfBoxes; ++i)
			for(int j = 0; j < population; ++j)
				chromosomes [j][i] = i;
		for(int j = 0; j < population; ++j){
			for (int i1 = 0; i1<numberOfBoxes-1; ++i1){
				int i2 = Random.Range(0, numberOfBoxes-1);
				int temp = chromosomes[j][i2];
				chromosomes[j][i2] = chromosomes[j][i1];
				chromosomes[j][i1] = temp;
			}
		}
	}
	
	Vector3 pushToSide(Vector3 point, float length){
		if (point.x == 0)
						return point;
		Debug.Log ("pushing");
		float posX = 0;
		Debug.Log ("start : "+point.x);
		for (int i = 0; i < lanes.Count; ++i) {
			Lane lane = lanes[i];
			if(lane.x1 == lane.x2  && lane.x1 > posX && lane.x1<= point.x && lane.y1 < (point.y + length)  && lane.y2 > point.y){		// si ligne verticale et si ligne plus a droite que posX et si ligne dépasse sur boite
		//		Debug.Log ("mid : "+lane.x1);
				posX = lane.x1;								 
			} 
		}
		Debug.Log ("end : "+posX);
		point.x = posX;
		return point;
	}

	bool checkSpace(Vector3 pos, Vector3 size){
		if (pos.z + size.z > contenerHeight)
						return false;
		for (int i = 0; i < lanes.Count; ++i) {
			Lane lane = lanes[i];
			if(lane.y2>pos.y && lane.y1 < pos.y+ size.y && lane.x1<pos.x + size.x && lane.x2>pos.x)
				return false;
		}
		return true;
	}

	void createLanes(Vector3 pos, Vector3 size){
		addLane (new Lane (pos.x, pos.y, pos.x, pos.y + size.y, pos.z + size.z));
		addLane (new Lane (pos.x, pos.y, pos.x + size.x, pos.y, pos.z + size.z));
		addLane (new Lane (pos.x + size.x, pos.y, pos.x + size.x, pos.y + size.y, pos.z + size.z));
		addLane (new Lane (pos.x, pos.y + size.y, pos.x + size.x, pos.y + size.y, pos.z + size.z));
	}
	
	void addLane(Lane nl){
		if(nl.x1 == nl.x2 && nl.y1 == nl.y2) return;
		for (int i = lanes.Count-1; i >=0; --i) {
			Lane lane = lanes[i];
			if(nl.y1 == nl.y2 && lane.y2 == nl.y2 &&  lane.y1== nl.y1){
				if(nl.x1 == lane.x2){
					lanes.RemoveAt(i);
					nl.x1 = lane.x1;
				}
				else if(nl.x2 == lane.x1){
					lanes.RemoveAt(i);
					nl.x2 = lane.x2;
				}
				else if(nl.x1 < lane.x1 && nl.x2 > lane.x2){
					if(nl.z >= lane.z){lanes.RemoveAt(i); cleanStartPoints(lane);}
					if(nl.z <= lane.z){
						addLane(new Lane(lane.x2, lane.y2, nl.x2, nl.y2, nl.z)); 
						nl.x2 = lane.x1;
					}
				}
				else if(nl.x1 > lane.x1 && nl.x2 < lane.x2){
					if(nl.z >= lane.z){
						addLane(new Lane(nl.x2, nl.y2, lane.x2, lane.y2, lane.z)); 
						lane.x2 = nl.x1;
						cleanStartPoints(nl);
					}
					if(nl.z == lane.z) return;

				}
				else if(nl.x1 <= lane.x1 && nl.x2 >= lane.x1){
					if(nl.x1 == lane.x1 && nl.x2 == lane.x2){ 
						//cleanStartPoints(nl);
						if(nl.z >= lane.z)lanes.Remove(lane);
						if(nl.z <= lane.z)return;
					}
					if(nl.z > lane.z)lane.x1 = nl.x2;
					else if(nl.z == lane.z) {
						cleanStartPoints(new Lane(lane.x1, lane.y1, nl.x2,lane.y1, nl.z)); //To copy
						float temp = lane.x1;
						lane.x1 = nl.x2;
						nl.x2 = temp;
					}
					else
						nl.x2 = lane.x1;
				}
				else if(nl.x1 <= lane.x2 && nl.x2 >= lane.x2){
					if(nl.z > lane.z) lane.x2 = nl.x1;
					else if(nl.z == lane.z) {
						cleanStartPoints(new Lane(lane.x2, lane.y1, nl.x1,lane.y1, nl.z)); //To copy
						float temp = lane.x2;
						lane.x2 = nl.x1;
						nl.x1 = temp;
					}
					else
						nl.x1 = lane.x2;
				}

			}
			else if(nl.x1 == nl.x2 && lane.x2 == nl.x2 &&  lane.x1== nl.x1){
				if(nl.y1 == lane.y2){
					lanes.RemoveAt(i);
					nl.y1 = lane.y1;
				}
				else if(nl.y2 == lane.y1){
					lanes.RemoveAt(i);
					nl.y2 = lane.y2;
				}
				else if(nl.y1 < lane.y1 && nl.y2 > lane.y2){
					if(nl.z >= lane.z){lanes.RemoveAt(i); cleanStartPoints(lane);}
					if(nl.z <= lane.z){
						addLane(new Lane(lane.x2, lane.y2, nl.x2, nl.y2, nl.z)); 
						nl.y2 = lane.y1;
					}
				}
				else if(nl.y1 > lane.y1 && nl.y2 < lane.y2){
					if(nl.z >= lane.z){
						addLane(new Lane(nl.x2, nl.y2, lane.x2, lane.y2, lane.z)); 
						lane.y2 = nl.y1;
						cleanStartPoints(nl);
					}
					if(nl.z >= lane.z) return;
					
				}
				else if(nl.y1 <= lane.y1 && nl.y2 >= lane.y1){
					if(nl.y1 == lane.y1 && nl.y2 == lane.y2){ 
						//cleanStartPoints(nl);
						if(nl.z >= lane.z)lanes.Remove(lane);
						if(nl.z <= lane.z)return;
					}
					if(nl.z > lane.z) lane.y1 = nl.y2;
					else if(nl.z == lane.z) {
						cleanStartPoints(new Lane(lane.x1, lane.y1, lane.x1,nl.y2, nl.z)); //To copy
						float temp = lane.y1;
						lane.y1 = nl.y2;
						nl.y2 = temp;
					}
					else
						nl.y2 = lane.y1;
				}
				else if(nl.y1 <= lane.y2 && nl.y2 >= lane.y2){
					if(nl.z > lane.z) lane.y2 = nl.y1;
					else if(nl.z == lane.z) {
						cleanStartPoints(new Lane(lane.x1, nl.y1, lane.x1,lane.y2, nl.z)); //To copy
						float temp = lane.y2;
						lane.y2 = nl.y1;
						nl.y1 = temp;
					}
					else
						nl.y1 = lane.y2;
				}
			}
			if(lane.x1 == lane.x2 && lane.y1 == lane.y2) lanes.Remove(lane);
			if(nl.x1 == nl.x2 && nl.y1 == nl.y2) return;
		}
		lanes.Add (nl);

	}

	void createStartPoints(Vector3 pos, Vector3 size){
		//Debug.Log ("Adding start points");
		//Debug.Log ("size : "+size.x+" ; "+size.y+" ; "+size.z);
		//Debug.Log ("pos : "+pos.x+" ; "+pos.y+" ; "+pos.z);
		Vector3 p1 = pos, p2 = pos, p3 = pos;
		p1.z += size.z;
		p2.x += size.x;
		p3.y += size.y;
		addStartPoint (p1);
		addStartPoint (p2);
		addStartPoint (p3);
	}

	void cleanStartPoints(Lane lane){
		for (int i = startPoints.Count-1; i >= 0; --i) {
			Vector3 sp = startPoints[i];
			if(sp.z == lane.z && lane.x1< sp.x && lane.x2 > sp.x && lane.y1 < sp.y && lane.y2 > sp.y)
				startPoints.Remove(sp);
		}
	}

	void addStartPoint(Vector3 point){
		//Debug.Log ("taille : "+point.x);
		if (point.x + minX > contenerWidth || point.y + minY > contenerLength || point.z + minZ > contenerHeight)
						return;
		for (int i = startPoints.Count-1; i >= 0; --i)
						if (point.y == startPoints [i].y && point.x == startPoints [i].x && point.z > startPoints [i].z)
								return;
						else if (point.y == startPoints [i].y && point.x == startPoints [i].x && point.z < startPoints [i].z)
								startPoints.Remove (startPoints [i]);


		startPoints.Add (point);
		//Debug.Log ("Added");
	}

	void orderStartPoints(){
		bool flag = true;
		while(flag){
			flag = false;
			for(int i = 0; i < startPoints.Count-1; ++i){
				if(startPoints[i+1].y < startPoints[i].y ||
				   (startPoints[i+1].y == startPoints[i].y && startPoints[i+1].x < startPoints[i].x) ||
				   (startPoints[i+1].y == startPoints[i].y && startPoints[i+1].x == startPoints[i].x && startPoints[i+1].z < startPoints[i].z)){

					Vector3 temp = startPoints[i];
					startPoints[i] = startPoints[i+1];
					startPoints[i+1] = temp;
					flag = true;

				}
			}
		}
		for(int i = 0; i < startPoints.Count; ++i)Debug.Log ("sp : "+startPoints[i].x+" ; "+startPoints[i].y+" ; "+startPoints[i].z);
		for(int i = 0; i < lanes.Count; ++i)Debug.Log ("lane : "+lanes[i].x1+" ; "+lanes[i].y1+" - "+lanes[i].x2+" ; "+lanes[i].y2+" - "+lanes[i].z);
	}
	void orderBoxes(int[] chrom){
		for(int i = 0; i<numberOfBoxes; i++){
			boxPositions[i]=new Vector3(-1,-1);
		}
		Debug.Log (chrom.Length);
		startPoints.Clear ();
		lanes.Clear ();
		startPoints.Add (new Vector3 (0, 0, 0));
		lanes.Add(new Lane(0,0,contenerWidth,0,0));
		lanes.Add(new Lane(0,0,0,contenerLength,0));
		lanes.Add(new Lane(0,contenerLength,contenerWidth,contenerLength,0));
		lanes.Add(new Lane(contenerWidth,0,contenerWidth,contenerLength,0));
		for(int i = 0; i< chrom.Length; i++){
			bool pushed = false;

			Debug.Log("--------------------------------------BOX "+chrom[i]+"-----------------");
			Debug.Log ("boxSize : "+boxSizes[chrom[i]].x+" ; "+boxSizes[chrom[i]].y+" ; "+boxSizes[chrom[i]].z);
			for(int sp = 0; sp < startPoints.Count && !pushed; ++sp){
				//Debug.Log(startPoints[sp].x+" ; "+startPoints[sp].y+" ; "+startPoints[sp].z);
				Vector3 startPoint;
				startPoint = pushToSide(startPoints[sp], boxSizes[chrom[i]].y);
				if(checkSpace(startPoint, boxSizes[chrom[i]])){
					Debug.Log("Space found");
					Debug.Log(startPoint.x+" ; "+startPoint.y+" ; "+startPoint.z);
					boxPositions[chrom[i]] = startPoint;
					startPoints.Remove(startPoint);
					createStartPoints(startPoint, boxSizes[chrom[i]]);
					createLanes(startPoint, boxSizes[chrom[i]]);

					pushed = true;
				}
				else{
					startPoint = pushToSide(startPoints[sp], boxSizes[chrom[i]].x);
					if(checkSpace(startPoint, rotateSize(boxSizes[chrom[i]]))){
						Debug.Log("Space found by rotating");
						Debug.Log(startPoint.x+" ; "+startPoint.y+" ; "+startPoint.z);
						boxSizes[chrom[i]] = rotateSize(boxSizes[chrom[i]]);
						boxPositions[chrom[i]] = startPoint;
						startPoints.Remove(startPoint);
						((Default)boxes [chrom[i]].GetComponentsInChildren<Default> () [0]).rotate();
						createStartPoints(startPoint, boxSizes[chrom[i]]);
						createLanes(startPoint, boxSizes[chrom[i]]);
						pushed = true;
					}
				}

			}
			if(pushed)orderStartPoints();
			//Debug.Log(boxPositions[i].x+" ; "+boxPositions[i].y+" ; "+boxPositions[i].z);

		}

	}

	Vector3 rotateSize(Vector3 size){
		float temp = size.x;
		size.x = size.y;
		size.y = temp;
		return size;
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

	int[,] permut(int[,] chromO , float probPermut){
		int[,] chromP = new int[population, numberOfBoxes];

		for (int parent1 = 0; parent1 < population; ++parent1) {
			int parent2 = Random.Range(0,population-1);
			for(int g = 0; g < numberOfBoxes; ++g)
				chromP[parent1 ,g]=0;
			for(int g = 0; g < numberOfBoxes; ++g){
				float prob = Random.value;
				if(prob<=probPermut && chromP[parent1 ,g]==0){
					chromP[parent1 ,g] = chromO[parent2 ,g];
					chromP[parent2 ,g] = chromO[parent1 ,g];
					for(int i = 0; i < numberOfBoxes; ++i){
						if(chromO[parent1, i] == chromP[parent1,g]){
							for(int j = 0; j < numberOfBoxes; ++j){
								if(chromO[parent2, j] == chromP[parent2,g]){
									chromP[parent1,i] = chromO[parent2,j];
									chromP[parent2,j] = chromO[parent1,i];
									break;
								}
							}
							break;
						}

					}

				}
				else if( chromP[parent1 ,g]==0)
					chromP[parent1 ,g] = chromO[parent1 ,g];

			}
		}

		return chromP;
	}

	int[,] mutat(int[,] chromO , float probMut){
		for(int chr = 0; chr < population; ++chr){
			for(int g = 0; g < numberOfBoxes; ++g){
				if(Random.value < probMut){
					int g2 = Random.Range(0,numberOfBoxes-1);
					int temp = chromO[chr,g];
					chromO[chr,g] = chromO[chr,g2];
					chromO[chr,g2] = temp;
				}
			}
		}
		return chromO;
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

	void display(){
		Debug.Log ("lanes : " + lanes.Count);
		Debug.Log ("startpoints : " + startPoints.Count);

		activ = boxPositions.Length-1;
		maxActiv = boxPositions.Length-1;
		for (int i = 0; i < boxPositions.Length; ++i) {
			if(boxPositions[i].x ==-1){ boxes[i].SetActive(false); activ--;}
			Default sc = (Default)boxes [i].GetComponentsInChildren<Default> () [0];
			sc.move (boxPositions[i]);
		}
		time = Time.realtimeSinceStartup - time;
		Debug.Log ("Time : "+time);
	}

	void Update () {
		bool flag = false;
		if (Input.GetKeyDown (KeyCode.Return)){
			flag = true;
			activ = -1;
		}
		if (Input.GetKeyDown (KeyCode.Space)){
			flag = true;
			activ++;
		}
		if (Input.GetKeyDown (KeyCode.Backspace)){
			flag = true;
			activ--;
		}
		activ = (activ > maxActiv ? maxActiv : activ);
		if(flag){
			for(int i = 0; i <= activ; ++i)
				boxes[chromosomes[0][i]].SetActive(true);
			if(activ>=1)Debug.Log("Last moved : "+(chromosomes[0][activ-1]));
			//Debug.Log("Last moved : "+(activ));
			for(int i = (activ>=0?activ : 0); i <= maxActiv; ++i)
				boxes[chromosomes[0][i]].SetActive(false);
		}
	}
}


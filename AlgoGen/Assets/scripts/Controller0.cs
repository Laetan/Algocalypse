using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class Controller0 : MonoBehaviour {

	public float contenerWidth;
	public float contenerLength;
	public float contenerHeight;
	
	public int numberOfBoxes;
	public int numberOfChrom;
	public float probabilitePermutation, probabiliteMutation, tauxConservation;
	public int numberOfEvolutions;
	public bool stepByStep;
	public bool randomBoxes;
	public int cleanTimer;
	public GameObject floor, wall, box;
	public int maxThreads;

	public Canvas ui_settings, ui_runtime;

	private List<GameObject> boxes;
	private int dimX, dimY, dimZ;
	private int[] boxByPriority;
	//private int[,,,] spaceMat; //position{3} - id/priority
	//private List<Vector3> startPoints;
	private int[][] chromosomes;
	private int minX,minY,minZ;
	private float pasXY,pasZ;
	private BoxInfo[] boxInfos;
	private int activ, maxActiv;
	private int currentEvolution;
	private int kr, ko;
	private float time;
	private Vector2 lastScore;
	private int evolCounter;
	private bool debug, next, running, write;
	private FileWrite writerScript;
	struct BoxInfo{
		public Vector3 size;
		public Vector3 pos;
		public int priority;
		public Default script;
		public bool rotated;
		public bool pushed;
		public void rotate(){
			script.rotate();
			rotated = !rotated;
			float temp = size.x;
			size.x = size.y;
			size.y = temp;
		}
		public void move(Vector3 worldCoordinates){
			script.move (worldCoordinates);
		}
	} 

	private float [] scoreO, scoreP, scoreM, scorePM;

	void Start () {
		write = true;
		ui_runtime.enabled = false;
		kr = 1;
		ko = 1;
		running = false;
		boxes = new List<GameObject> ();
		boxByPriority = new int[11];
		//startPoints = new List<Vector3> ();
		debug = false;
		next = true;
		currentEvolution = 0;
		contenerWidth = 2.4F;
		contenerHeight = 2.7F;
		contenerLength = 12;
		numberOfBoxes = 250;
		numberOfChrom= 100;
		kr= 1;
		ko= 1;
		probabilitePermutation = 0.5F;
		probabiliteMutation = 0.5F;
		tauxConservation = 0.8F;
		numberOfEvolutions= 100;
		stepByStep = true;
		lastScore = new Vector2 (-1, -1);
	}


	//UI Setters
	public void setWidth(string value){contenerWidth = float.Parse(value);}
	public void setHeight(string value){contenerHeight = float.Parse(value);}
	public void setLength(string value){contenerLength = float.Parse(value);}
	public void setNumberOfBoxes(string value){numberOfBoxes = int.Parse(value);}
	public void setNumberOfChromosomes(string value){numberOfChrom= int.Parse(value);}
	public void setKr(string value){kr= int.Parse(value);}
	public void setKo(string value){ko= int.Parse(value);}
	public void setProbaPermut(string value){probabilitePermutation = float.Parse(value);}
	public void setProbaMuta(string value){probabiliteMutation = float.Parse(value);}
	public void setConservRate(string value){tauxConservation = float.Parse(value);}
	public void setNumberOfEvolutions(string value){numberOfEvolutions= int.Parse(value);}
	public void setStepByStep(bool value){stepByStep = value;}
	public void setWriting(bool value){write = value;}

	public void Launch(){
		if (!ui_settings.enabled)
						return;
		ui_settings.enabled = false;
		ui_runtime.enabled = true;
		writerScript = ui_runtime.GetComponentInChildren<FileWrite> ();
		running = true;
		boxInfos = new BoxInfo[numberOfBoxes];
		/*GameObject f = (GameObject)Instantiate (floor);
		f.SendMessage ("resize", new Vector3 (contenerWidth/10,contenerLength/10,1));
		f.SendMessage("move", new Vector3 (0,0,0));
		
		f = Instantiate (wall)  as GameObject;
		
		f.SendMessage ("resize", new Vector3 (contenerHeight/10,contenerLength/10,1));
		f.SendMessage("move", new Vector3 (0,0,0));
		*/
		int[][] sizes = new int[3][]; 
		sizes [0] = new int[numberOfBoxes];
		sizes [1] = new int[numberOfBoxes];
		sizes [2] = new int[numberOfBoxes];
		Vector3[] boxSizes = new Vector3[numberOfBoxes];
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
			boxSizes[i] = new Vector3(width,length,height);
			boxInfos[i].priority = priority;
			boxInfos[i].pushed = false;
			boxInfos[i].rotated = false;
			boxes.Add((GameObject) Instantiate(box));
			boxes[i].SendMessage("resize", boxSizes[i]);
			boxes[i].SendMessage("setPriority", priority);
			boxes[i].SendMessage("setId", i);
			boxInfos[i].script = (Default)boxes[i].GetComponentsInChildren<Default>()[0];
			++boxByPriority[priority];
			//float p = 2-(float)priority/5;
			//Debug.Log(p);
			boxes[i].renderer.material.color = new Color(1,(float)(1+priority)/11,(float)(1+priority)/11);
		}
		pasXY = (float)getPas (getPas (sizes [0]), getPas (sizes [1]))/1000;
		pasZ = (float)getPas (sizes [2])/1000;
		dimX = Mathf.RoundToInt(contenerWidth / pasXY); dimY = Mathf.RoundToInt(contenerLength / pasXY); dimZ = Mathf.RoundToInt(contenerHeight / pasZ);
		minX = dimX; minY = dimY; minZ = dimZ;
		for (int i = 0; i < numberOfBoxes; i++) {
			Vector3 gridCoord = worldToGrid (boxSizes[i]);
			minX = (gridCoord.x<minX ? (int)gridCoord.x : minX);
			minY = (gridCoord.y<minY ? (int)gridCoord.y : minY);
			minZ = (gridCoord.z<minZ ? (int)gridCoord.z : minZ);
			boxInfos [i].size = gridCoord;
		}

		Debug.Log("-----------------Space config-------");
		Debug.Log("Dimension : "+ dimX + " ; " + dimY+ " ; " + dimZ);
		Debug.Log("Pas : "+ pasXY + " ; " + pasXY+ " ; " + pasZ); 
		Debug.Log("-----------------Initialize population-------");
		initialiszePopulation ();
	}

	/*
	void displayChrom(int[][] chrom){
		Debug.Log ("---------------------------------------------DEBUGGING-CHROMOSOMES---");
		for (int i = 0; i < numberOfChrom; ++i) {
			Debug.Log("Chromosome "+(i+1));
			string str= "";
			for(int j = 0; j < numberOfBoxes; ++j){
				str += chrom[i][j]+" ; ";
			}
			Debug.Log(str);
			for(int j = 0; j < numberOfBoxes-1; ++j){
				for(int jj = j+1; jj < numberOfBoxes; ++jj){
					//Debug.Log(j+";"+jj);
					if(chrom[i][j] == chrom[i][jj])
						Debug.LogError("Invalid chromosome : "+j+" ; "+jj);
				}
			}
		}	
	}
	*/
	
	bool checkPos(int x,int y, int z,Vector3 bSize, int[,,,] spaceMat){
		
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

	void initialiszePopulation(){
		chromosomes = new int[numberOfChrom][];
		for (int j = 0; j < numberOfChrom; ++j) {
			chromosomes[j] = new int[numberOfBoxes];
		}
		for (int i = 0; i<numberOfBoxes; ++i)
			for(int j = 0; j < numberOfChrom; ++j)
				chromosomes [j][i] = i;
		for(int j = 0; j < numberOfChrom; ++j){
			for (int i1 = 0; i1<numberOfBoxes-1; ++i1){
				int i2 = Random.Range(0, numberOfBoxes-1);
				int temp = chromosomes[j][i2];
				chromosomes[j][i2] = chromosomes[j][i1];
				chromosomes[j][i1] = temp;
			}
		}

		for(int j = 0; j < numberOfChrom; ++j){
			bool flag = true;
			while(flag){
				flag = false;
				for (int i = 0; i<numberOfBoxes-1; ++i){
					if(boxInfos[chromosomes[j][i]].priority > boxInfos[chromosomes[j][i+1]].priority){
						int temp= chromosomes[j][i+1];
						chromosomes[j][i+1] = chromosomes[j][i];
						chromosomes[j][i] = temp;
					}
				}
			}
		}
	}

	void createStartPoints(List<Vector3> startPoints,Vector3 pos, Vector3 size){
		//Debug.Log ("Adding start points");
		//Debug.Log ("size : "+size.x+" ; "+size.y+" ; "+size.z);
		//Debug.Log ("pos : "+pos.x+" ; "+pos.y+" ; "+pos.z);
		//startPoints.Clear ();
		Vector3 p1 = pos, p2 = pos, p3 = pos;
		p1.z += size.z;
		p2.x += size.x;
		p3.y += size.y;
		addStartPoint (startPoints,p1);
		addStartPoint (startPoints,p2);
		addStartPoint (startPoints,p3);

	}

	void addStartPoint(List<Vector3> startPoints,Vector3 point){

		if (point.x + minX > dimX || point.y + minY > dimY || point.z + minZ > dimZ)
			return;	
		/*if (spaceMat [(int)point.x, (int)point.y, (int)point.z, 1] != 0)
						return;
		if (point.z > 0 && spaceMat [(int)point.x, (int)point.y, (int)point.z - 1, 1] == 0)
						return;*/

		startPoints.Add (point);
		//Debug.Log ("Added");
	}

	bool moveBoxTo(Vector3 v, int boxIndex,ref int[,,,] spaceMat, ref bool rotated, bool rotate = false){
		return moveBoxTo((int)v.x,(int)v.y,(int)v.z,boxIndex, ref spaceMat, ref rotated, rotate);
	}
	bool moveBoxTo(int x, int y, int z, int boxIndex,  ref int[,,,] spaceMat, ref bool rotated, bool rotate = false){
		//Debug.Log ("moving box to : " + x + " ; " + y + " ; " + z);
		Vector3 bSize = boxInfos [boxIndex].size;
		if (rotate) {
			float temp = bSize.x;
			bSize.x = bSize.y;
			bSize.y = temp;
		}
		//if(rotate) Debug.Log ("rotate");
		//Vector3 bSize = boxInfos [boxIndex].size;

		if(!checkPos(x,y,z,bSize,spaceMat)) return false;
		if(debug)Debug.Log("move to : " + x + " ; " + y + " ; " + z);
		for(int xx = x; xx < x+bSize.x;xx++){
			for(int yy = y; yy<y+bSize.y;yy++){
				for(int zz = z; zz<z+bSize.z; zz++){
					spaceMat[xx,yy,zz,0] = boxIndex;
					spaceMat[xx,yy,zz,1] = boxInfos [boxIndex].priority;
				}
			}
		}
		rotated = rotate;
		if(rotate) Debug.Log ("rotate : "+boxIndex);
		return true;
		
	}
	void orderBoxes(int[]chrom, ref int[,,,] spaceMat, ref Vector3[] boxPositions, ref bool[] rotated ){
		//clearSpace ();
		for(int i = 0; i<numberOfBoxes; i++){
			boxPositions[i] = new Vector3(-1,-1);
			rotated[i] = false;

			//boxInfos[i].pushed = false;
			if(boxInfos[i].rotated) Debug.LogError("Box is rotated !");
		}
		List<Vector3> startPoints = new List<Vector3> ();
		startPoints.Add (new Vector3 (0, 0, 0));
		int cleanFlag = cleanTimer;
		for(int i = 0; i< numberOfBoxes; i++){

			if(cleanFlag == 0){
				cleanFlag = cleanTimer;
				for (int sp = startPoints.Count-1; sp>=0; --sp) {
					if(spaceMat [(int)startPoints[sp].x, (int)startPoints[sp].y, (int)startPoints[sp].z, 1] != 0)
						startPoints.RemoveAt(sp);
					else if(startPoints[sp].z>0 && spaceMat [(int)startPoints[sp].x, (int)startPoints[sp].y, (int)startPoints[sp].z-1, 1] == 0)
						startPoints.RemoveAt(sp);
				}
			}
			cleanFlag--;
			if(debug) Debug.Log("-------------BOX "+chrom[i]+"-------------");
			if(debug)for(int sp = 0; sp < startPoints.Count; ++sp) Debug.Log("sp : "+ startPoints[sp].x + " ; " + startPoints[sp].y+ " ; " + startPoints[sp].z);
			for(int sp = 0; sp < startPoints.Count; ++sp){
				//Debug.Log("sp :"+startPoints[sp]);
				if(moveBoxTo(startPoints[sp],chrom[i], ref spaceMat, ref rotated[chrom[i]])||moveBoxTo(startPoints[sp],chrom[i],ref spaceMat, ref rotated[chrom[i]], true)){
					//boxInfos[chrom[i]].pos = startPoints[sp];
					boxPositions[chrom[i]] = startPoints[sp];
					Vector3 bSize = boxInfos [chrom[i]].size;
					if (rotated[chrom[i]]) {
						float temp = bSize.x;
						bSize.x = bSize.y;
						bSize.y = temp;
					}
					createStartPoints(startPoints,startPoints[sp],bSize);
					//boxInfos[chrom[i]].pushed = true;
					break;
				}
			}
		}
		/*for(int i = 0; i<numberOfBoxes; i++){
			if(boxInfos[i].rotated) boxInfos[i].rotate(displaying);
		}*/
	}
	void scoring_thread(int[]chrom, int modif, int pos){
		int[,,,] t_spaceMat = new int[dimX,dimY,dimZ,2];
		Vector3[] t_boxPos = new Vector3[numberOfBoxes];
		bool [] t_rotated = new bool[numberOfBoxes];
		//debug = true;
		orderBoxes (chrom, ref t_spaceMat, ref t_boxPos, ref t_rotated);
		//Debug.Log (t_boxPos[0]);
		float score = scoring (chrom, t_spaceMat, t_boxPos, t_rotated);
		//Debug.Log (t_boxPos[0]);
		//float score = 0;
		switch (modif) {
				case 0:
			//Debug.Log("scoring original number "+pos);
						scoreO [pos] = score;
						break;
				case 1:
			//Debug.Log("scoring permuted number "+pos);
						scoreP [pos] = score;
						break;
				case 2:
			//Debug.Log("scoring muted number "+pos);
						scoreM [pos] = score;
						break;
				case 3:
			//Debug.Log("scoring permut-muted number "+pos);
						scorePM [pos] = score;
						break;
				}
	}

	float scoring(int[]chrom, int[,,,] spaceMat, Vector3[] boxPositions, bool[] rotated){
		//orderBoxes (chrom);
		float fr, fo;
		int space_left = 0;
		int boxPushed = 0;
		for (int i = 1; i <= 10; ++i)
			boxPushed += boxByPriority [i];
		if (boxPushed == numberOfBoxes)
						fr = 1;
		else{
			for (int x = 0; x< dimX; x++)
				for (int y = 0; y<dimY; y++)
					for (int z = 0; z<dimZ; z++)
						if (spaceMat [x,y,z,0] == -1)
							++space_left;

			fr = 1 - ((float)space_left / (float)(dimX * dimY * dimZ));
		}
		
		float fo_c1 = 0, fo_c2 = 0, fo_c3 = 0;
		
		//---Calcul premiere composante---//
		for (int p = 1; p<=10; p++) {
			if(boxByPriority[p]==0){
				fo_c1 ++;
				continue;
			}
			int np = 0;
			for(int i =0; i<numberOfBoxes; i++)
				if(boxInfos[i].priority == p && boxPositions[i].x!=-1)
					++np;
			
			if(np == boxByPriority[p]){
				++fo_c1;
				continue;
			}
			int vol_pm = 0, vol_p = dimX*dimY*dimZ;			//vol_pm: volume boxes pose de priorté inf ; vol_p : plus petite caisse de priorité p
			for(int i =0; i<numberOfBoxes; i++)
				if(boxInfos[i].priority >= p && boxPositions[i].x!=-1)
					vol_pm += getVolume(i);
			for(int i = 0; i<numberOfBoxes; ++i){
				if(boxPositions[i].x ==-1 && boxInfos[i].priority == p){
					int v = getVolume(i);
					vol_p= (v>vol_p?v:vol_p);
				}
			}
			if(vol_pm+space_left<vol_p)
				++fo_c1;
			else
				fo_c1 += 0;// 1-(vol_pm+space_left)/(dimX * dimY * dimZ);    // 0 Si on pouvait placer une caisse
		}

		//---Calcul deuxieme et troisième composante---//
		for(int priority = 1; priority <= 10; ++priority){
			if(boxByPriority[priority]==0){
				fo_c2 ++;
				fo_c3 ++;
				continue;
			}
			int n1 = 0, n2 = 0;
			//while (index < boxesPushed.Length/2 && boxesPushed[index,1]!=0) {
			for(int i = 0; i < numberOfBoxes; ++i){
				if(boxInfos[i].priority!=priority || boxPositions[i].x ==-1) continue;
				Vector3 pos =boxPositions[i];
				Vector3 size = boxInfos[i].size;
				if(rotated[i]){
					float temp = size.x;
					size.x = size.y;
					size.y = temp;
				}
				bool blocked = false;
				for (int x = (int)pos.x; x< pos.x+size.x; x++){
					for (int z = (int)pos.z; z<pos.z+size.z; z++){
						for(int y = (int)pos.y; y>=0; --y){
							if (spaceMat [x,y,z,1] > priority){
								++n1;
								blocked = true;
								break;
							}
							if(blocked) break;
						}
						if(blocked) break;
					}
					if(blocked) break;
				}
				blocked = false;
				for(int x = (int)pos.x; x< pos.x+size.x; x++){
					for (int y = (int)pos.y; y<pos.y+size.y; y++){
						for(int z = (int)pos.z+(int)size.z; z<dimZ; z++){
							if (spaceMat [x,y,z,1] > priority){
								++n2;
								blocked = true;
								break;
							}
							if(blocked) break;
						}
						if(blocked) break;
					}
					if(blocked) break;
				}
			}
			fo_c2 += 1-(float)n1/(float)boxByPriority[priority];
			fo_c3 += 1-(float)n2/(float)boxByPriority[priority];
			
		}
		fo = fo_c1 + fo_c2 + fo_c3;
		fo /= 30;

		return (fo * ko + fr * kr) / (ko + kr);
	}
	
	int getVolume(int boxIndex){
		Vector3 v = boxInfos [boxIndex].size;
		return (int)(v.x * v.y * v.z * 1000);
	}
	void clearSpace(){
		//spaceMat = new int[dimX,dimY,dimZ,2];
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

	public Vector3 worldToGrid(Vector3 world){
		Vector3 grid;
		grid.x = Mathf.Round(world.x/pasXY);
		grid.y = Mathf.Round(world.y/pasXY);
		grid.z = Mathf.Round(world.z/pasZ);
		return grid;
	}

	public Vector3 gridToWorld(Vector3 grid){
		Vector3 world;
		world.x = grid.x*pasXY;
		world.y = grid.y*pasXY;
		world.z = grid.z*pasZ;
		return world;
	}

	void display(int[]chrom, Vector2 score){
		//debug = true;
		int[,,,] spaceMat = new int[dimX,dimY,dimZ,2];
		Vector3[] boxPos = new Vector3[numberOfBoxes];
		bool [] rotated = new bool[numberOfBoxes];
		orderBoxes (chrom, ref spaceMat, ref boxPos, ref rotated);
		//orderBoxes (chrom);

		maxActiv = numberOfBoxes;
		activ = maxActiv;
		for (int i = 0; i < numberOfBoxes; ++i) {
			if(boxPos[i].x ==-1){ boxes[i].SetActive(false); activ--;}
			else boxes[i].SetActive(true);
			if(rotated[i]) boxInfos[i].rotate();
			boxInfos[i].move(gridToWorld(boxPos[i]));

		}
		if (lastScore [0] != -1 && score [1] - lastScore [1] < 1e-4)
						evolCounter++;
				else
						evolCounter -= 2;

		Text[] ui_fields = ui_runtime.GetComponentsInChildren<Text> ();
		for(int i = 0 ; i < ui_fields.Length; i++){
			if(ui_fields[i].name == "Evolution")
				ui_fields[i].text = "Current Evolution : "+currentEvolution;
			else if (ui_fields[i].name == "ScoreMax")
				ui_fields[i].text = "Score max : "+score[0];
			else if (ui_fields[i].name == "ScoreMoy")
				ui_fields[i].text = "Score moy : "+score[1];
			else if (ui_fields[i].name == "Warning"){
				if(evolCounter>=10)
					ui_fields[i].text = "No evolution !";
				else
					ui_fields[i].text = "";
			}
		}


		lastScore = score;
		debug = false;

	}

	int[][] permut(int[][] chromO , float probPermut){
		int[][] popN= new int[numberOfChrom][];
		for (int i = 0; i < numberOfChrom; ++i) {
			popN[i] = new int[numberOfBoxes];
			for (int j = 0; j < numberOfBoxes; ++j)
				popN[i][j] = chromO[i][j];
			
		}

		for (int parent1 = 0; parent1 < numberOfChrom; ++parent1) {
			int parent2 = Random.Range(0,numberOfChrom-1);
			parent2 = (parent2 != parent1 ? parent2 : (parent2 == 0 ? 1 : parent2 -1));

			for(int g = 0; g < numberOfBoxes; ++g){
				float prob = Random.value;
				if(prob<=probPermut){
					int gene1 = popN[parent1][g];
					int gene2 = popN[parent2][g];
					int g1p2 = -1, g2p1 = -1;
					for(int i = 0; i < numberOfBoxes; ++i){
						if(popN[parent2][i]==gene1){
							g1p2 = i;
						}
						if(popN[parent1][i]==gene2){
							g2p1 = i;
						}
						if(g1p2!=-1 && g2p1 != -1) break;
					}
					popN[parent1][g] = gene2;
					popN[parent2][g] = gene1;
					popN[parent1][g2p1] = gene1;
					popN[parent2][g1p2] = gene2;
				}
			}
		}
		
		return popN;
	}
	
	int[][] mutat(int[][] chromO , float probMut){
		int[][] popN= new int[numberOfChrom][];
		for (int i = 0; i < numberOfChrom; ++i) {
			popN[i] = new int[numberOfBoxes];
			for (int j = 0; j < numberOfBoxes; ++j)
				popN[i][j] = chromO[i][j];
			
		}
		for(int chr = 0; chr < numberOfChrom; ++chr){
			for(int g = 0; g < numberOfBoxes; ++g){
				if(Random.value < probMut){
					int g2 = Random.Range(0,numberOfBoxes-1);
					int temp = popN[chr][g];
					popN[chr][g] = popN[chr][g2];
					popN[chr][g2] = temp;
				}
			}
		}
		return popN;
	}
	void triage(ref int[][] chromosomes, ref float[] score){
		for(int i = 0; i < numberOfChrom-1; i++){
			if(score[i+1] > score[i]){
				float temp = score[i];
				score[i] = score[i+1];
				score[i+1] = temp;
				int[] tem = chromosomes[i];
				chromosomes[i] = chromosomes[i+1];
				chromosomes[i+1] = tem;
			}
		}
	}
	Vector2 evolution(){
		for(int i = 0; i<numberOfBoxes; i++){
			if(boxInfos[i].rotated) boxInfos[i].rotate();
		}
		int[][] popPerm, popMut, popPerMut, popTri;
		float[]  scoreTri;
		int nbrToKeep = Mathf.RoundToInt (tauxConservation * numberOfChrom);
		//Debug.Log ("nbrToKeep : " + nbrToKeep);
		scoreO = new float[numberOfChrom];
		scoreP = new float[numberOfChrom];
		scoreM = new float[numberOfChrom];
		scorePM = new float[numberOfChrom];
		scoreTri = new float[numberOfChrom*3];
		popTri = new int[numberOfChrom*3][];

		popPerm = permut (chromosomes, probabilitePermutation);

		popMut = mutat (chromosomes, probabiliteMutation);

		popPerMut = permut (popPerm, probabiliteMutation);

		Thread[] threads = new Thread[maxThreads];
		/*
		for(int i = 0; i < numberOfChrom; ++i){
			scoreO[i] = scoring(chromosomes[i]);
			scoreP[i] = scoring(popPerm[i]);
			scoreM[i] = scoring(popMut[i]);
			scorePM[i] = scoring(popPerMut[i]);
		}*/
		//Debug.Log ("start threading");
		int j = 0; 
		while(j < numberOfChrom){
			for(int i = 0; i < maxThreads/4; ++i){
				int pos = i + j;
				threads[i*4] = new Thread(()=>scoring_thread(chromosomes[pos],0,pos));
				threads[i*4].Start();
				threads[i*4+1] = new Thread(()=>scoring_thread(popPerm[pos],1,pos));
				threads[i*4+1].Start();
				threads[i*4+2] = new Thread(()=>scoring_thread(popMut[pos],2,pos));
				threads[i*4+2].Start();
				threads[i*4+3] = new Thread(()=>scoring_thread(popPerMut[pos],3,pos));
				threads[i*4+3].Start();
				if(pos+1>numberOfChrom) break;
			}

			for (int i = 0; i < maxThreads; ++i)
					threads [i].Join ();
			j+= maxThreads/4;
		}
		//Debug.Log ("end threading");
		triage (ref chromosomes, ref scoreO);
		triage (ref popPerm, ref scoreP);
		triage (ref popMut, ref scoreM);
		triage (ref popPerMut, ref scorePM);

		for (int i = 0; i < numberOfChrom; ++i) {
			scoreTri[i*3] = scoreP[i];
			scoreTri[i*3+1] = scoreM[i];	
			scoreTri[i*3+2] = scorePM[i];

			popTri[i*3] = popPerm[i];
			popTri[i*3+1] = popMut[i];	
			popTri[i*3+2] = popPerMut[i];
		}

		for(int i = nbrToKeep; i < numberOfChrom; ++i){
			chromosomes[i] = popTri[i-nbrToKeep];
			scoreO[i] = scoreTri[i-nbrToKeep];
		}

		triage (ref chromosomes, ref scoreO);
		float scoreMoy = 0;
		for (int i = 0; i < numberOfChrom; ++i)
						scoreMoy += scoreO [i];
		scoreMoy /= numberOfChrom;

		currentEvolution++;
		return new Vector2(scoreO[0], scoreMoy);
	}

	void Update () {
		bool flag = false; 
		/*if (Input.GetKeyDown (KeyCode.Return)){
			flag = true;
			activ = 0;
		}*/
		if (Input.GetKeyDown (KeyCode.Space)){
			next = true;
		}
		/*if (Input.GetKeyDown (KeyCode.LeftArrow)){
			flag = true;
			activ--;
		}
		if (Input.GetKeyDown (KeyCode.RightArrow)){
			flag = true;
			activ++;
		}*/

		activ = (activ > maxActiv ? maxActiv : activ);
		activ = (activ < 0 ? 0 : activ);
		if(running && flag){
			for(int i = 0; i < activ; ++i)
				boxes[chromosomes[0][i]].SetActive(true);
			if(activ>0)Debug.Log("Last moved : "+(chromosomes[0][activ-1]));

			for(int i = (activ>=0?activ : 0); i < numberOfBoxes; ++i)
				boxes[chromosomes[0][i]].SetActive(false);
		}
		if (running && next && currentEvolution < numberOfEvolutions) {
			//Debug.Log("-------------------------------------------------------Evolution "+currentEvolution+"-------");
			time = Time.realtimeSinceStartup;
			Vector2 score = evolution();
			display (chromosomes[0], score);
			Debug.Log ("Evolution time : " + (Time.realtimeSinceStartup - time));
			//Debug.Log ("Score Max : "+score[0]+" ; Score Moyen : "+score[1]);
			next = !stepByStep;
			if(write) writerScript.write(currentEvolution, score);
		}
	}


}

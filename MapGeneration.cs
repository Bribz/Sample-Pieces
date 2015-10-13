using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MapGeneration : MonoBehaviour {
	
	#region Singleton Stuff
	private static MapGeneration _instance;
	
	public static MapGeneration instance
	{
		get
		{
			if(_instance == null)
			{
				_instance = GameObject.FindObjectOfType<MapGeneration>();
				
				//Tell unity not to destroy this object when loading a new scene!
				//DontDestroyOnLoad(_instance.gameObject);
			}
			
			return _instance;
		}
	}
	
	void Awake() 
	{
		if(_instance == null)
		{
			//If I am the first instance, make me the Singleton
			_instance = this;
			//DontDestroyOnLoad(this);
		}
		else
		{
			//If a Singleton already exists and you find
			//another reference in scene, destroy it!
			if(this != _instance)
				Destroy(this.gameObject);
		}
		
		//TODO: MAKE SURE THIS DOESNT DOUBLEFIRE
		Init ();
	}
	
	#endregion
	
	/*
	ArrayList ObjectParts;
	ArrayList prevPositions;
	
	public GameObject piece;
	public GameObject Hallwaypiece;
	
	public float HallwaySize = 5.0f;
	
	public Vector3 start, end;
	*/
	
	public List<Vector3> prevLocations;
	public List<PieceInfo> directionInfo;
	public List<GameObject> mapPieces;
	public List<GameObject> hallPieces;
	private List<List<GameObject>> previousMaps;
	private List<List<GameObject>> previousHalls;
	
	public float PIECE_WIDTH = 30f;
	
	public Vector3 finalPiece;
	
	public int amountOfRooms;
	public float HallwaySize = 5.0f;
	
	public GameObject MapHallPiece;
	public bool finished = false;
	public bool THREE_DIMENSIONAL = true;
	public GameObject currentMap;
	
	public int bossCount;
	public bool isLoaded;
	
	private enum TRAVEL_DIRECTION { NORTH, SOUTH, EAST, WEST };
	
	void Init()
	{
		//ObjectParts = new ArrayList();
		//prevPositions = new ArrayList();;
		//piece = Resources.Load<GameObject> ("Resources/MapPiece");
		//OnMapLoad();
		previousMaps = new List<List<GameObject>> ();
		previousHalls = new List<List<GameObject>> ();
		mapPieces = new List<GameObject> ();
		prevLocations = new List<Vector3> ();
		directionInfo = new List<PieceInfo> ();
		previousMaps.Clear ();
		mapPieces.Clear ();
		
		//MapRoomPiece = null;
		
		//finalPiece = Generate_Map (Vector3.zero, BIOME.GRASSLAND);
		Generate_Map (Vector3.zero, BIOME.DUNGEON);
	}
	
	/*
	void OnMapLoad()
	{
		GameObject g = new GameObject ();
		g.transform.name = "Generated_Map";
		
		start = Vector3.zero;
		
		//Generate Transforms
		//TODO: Change GameObject Generation to a series of positions for future pieces.
		for(int x = 0; x < 3; x++)
		{
			for(int y = 0; y < 3; y++)
			{
				GameObject tmp = (GameObject)GameObject.Instantiate(piece,new Vector3(x*(piece.transform.localScale.x+HallwaySize),y*(piece.transform.localScale.x+HallwaySize),0),piece.transform.rotation);
				tmp.transform.parent = g.transform;
				ObjectParts.Add(tmp);
			}
		}
		
		Generate_Path ();
	}
	

	
	void Generate_Path ()
	{
		//Find End
		//TODO: Find Side of end. Replace current formula
		//end = new Vector3 (Random.Range (0, 3) * (piece.transform.localScale.x + HallwaySize), 2 * (piece.transform.localScale.x + HallwaySize), 0);//
		end = new Vector3 (start.x, 2 * (piece.transform.localScale.x + HallwaySize), 0);
		Vector3 positionTmp = start;
		prevPositions.Clear ();
		while (positionTmp != end)
		{
			//Pick a direction. If weve been there or its an edge, pick a new direction.
			int direction = Random.Range(0,4);
			Vector3 positionTmp2 = Vector3.zero;
			switch (direction)
			{
				//Move Up
			case 0:
				
				//Check Bounds
				if(positionTmp.y+(piece.transform.localScale.y + HallwaySize) > 2*(piece.transform.localScale.y+HallwaySize))
				{
					continue;
				}
				//Check HasBeenThere
				else if(CheckPrevPositions(new Vector3(positionTmp.x, positionTmp.y+(piece.transform.localScale.y + HallwaySize),0)))
				{
					continue;
				}
				else 
				{
					prevPositions.Add(positionTmp);
					positionTmp = new Vector3(positionTmp.x, positionTmp.y+(piece.transform.localScale.y + HallwaySize),0);
					GameObject.Instantiate(Hallwaypiece,new Vector3(positionTmp.x,positionTmp.y-piece.transform.localScale.x/2.0f - HallwaySize/2.0f,0),Hallwaypiece.transform.rotation);
				}
				break;
				
				//Move Down
			case 1:
				//Check Bounds
				if(positionTmp.y-(piece.transform.localScale.y + HallwaySize) < 0)
				{
					continue;
				}
				//Check HasBeenThere
				else if(CheckPrevPositions( new Vector3(positionTmp.x, positionTmp.y-(piece.transform.localScale.y + HallwaySize),0)))
				{
					continue;
				}
				else 
				{
					prevPositions.Add(positionTmp);
					positionTmp = new Vector3(positionTmp.x, positionTmp.y-(piece.transform.localScale.y + HallwaySize),0);
					GameObject.Instantiate(Hallwaypiece,new Vector3(positionTmp.x,positionTmp.y+piece.transform.localScale.x/2.0f + HallwaySize/2.0f,0),Hallwaypiece.transform.rotation);
				}
				break;
				
				//Move Right
			case 2:
				//Check Bounds
				if(positionTmp.x+(piece.transform.localScale.x + HallwaySize) > 2*(piece.transform.localScale.x+HallwaySize))
				{
					continue;
				}
				//else if(CheckPrevPositions(new Vector3(positionTmp.x+(piece.transform.localScale.x + HallwaySize),positionTmp.y,0)))
				//{
				//	continue;
				//}
				else 
				{
					prevPositions.Add(positionTmp);
					positionTmp = new Vector3(positionTmp.x+(piece.transform.localScale.x + HallwaySize),positionTmp.y,0);
					GameObject gTmp = (GameObject)GameObject.Instantiate(Hallwaypiece,new Vector3(positionTmp.x-piece.transform.localScale.x/2.0f - HallwaySize/2.0f,positionTmp.y,0),Hallwaypiece.transform.rotation);
					gTmp.transform.Rotate(new Vector3(0,0,-1),90);
				}
				break;
				
				//Move Left
			case 3:
				//Check Bounds
				if(positionTmp.x-(piece.transform.localScale.x + HallwaySize) < 0)
				{
					continue;
				}
				//Check HasBeenThere
				//else if(CheckPrevPositions(new Vector3(positionTmp.x-(piece.transform.localScale.x + HallwaySize),positionTmp.y,0)))
				//{
				//	continue;
				//}
				else 
				{
					prevPositions.Add(positionTmp);
					positionTmp = new Vector3(positionTmp.x-(piece.transform.localScale.x + HallwaySize),positionTmp.y,0);
					GameObject gTmp = (GameObject)GameObject.Instantiate(Hallwaypiece,new Vector3(positionTmp.x+piece.transform.localScale.x/2.0f + HallwaySize/2.0f,positionTmp.y,0),Hallwaypiece.transform.rotation);
					gTmp.transform.Rotate(new Vector3(0,0,-1),-90);
				}
				break;
			}
		}
	
	
	}
	
	void Update()
	{
		OnMapDraw();
	}
	
	void OnMapDraw()
	{
		
	}
	*/
	
	bool CheckPrevPositions(Vector3 check)
	{
		if(prevLocations.Count == 0)
		{
			return false;
		}
		foreach(Vector3 v in prevLocations)
		{
			if(check.x == v.x)
				if(check.y == v.y)
					return true;
		}
		return false;
	}
	
	public Vector3 Generate_Map(Vector3 input, BIOME b)
	{
		Vector3 start;
		prevLocations.Clear();
		directionInfo.Clear();
		mapPieces.Clear();
		
		currentMap = new GameObject ();
		currentMap.name = "LOADED_MAP";
		
		if(hallPieces.Count > 0 && finished == false)
		{
			foreach(GameObject g in hallPieces)
			{
				Destroy(g);
			}
		}
		
		finished = false;
		
		hallPieces.Clear();
		
		if(input == Vector3.zero)
		{
			start = transform.position;
		}
		else
		{
			start = input;
		}
		
		TRAVEL_DIRECTION tDir;
		
		Vector3 moveDir = Vector3.zero;
		Vector3 proposedNewLocation = Vector3.zero;
		Vector3 currentLoc = start;
		bool newLoc = false;
		int tallestHeight = 0;
		int currentHeight = 0;
		float baseHeight = input.y;
		string prevDir;
		int currInfo = 0;
		
		//GameObject startMapPiece = (GameObject)GameObject.Instantiate(MapRoomPiece,start,Quaternion.identity);
		//mapPieces.Add(startMapPiece);
		prevLocations.Add(start);
		directionInfo.Add (new PieceInfo(b,""));
		directionInfo[currInfo].addDir("N");
		
		MapHallPiece = (Resources.Load("MapPieces/"+Enum.GetName(typeof(BIOME),b)+"/Hall NS", typeof(GameObject)) as GameObject);
		GameObject hp = (GameObject)GameObject.Instantiate(MapHallPiece,proposedNewLocation - ((DesignValues.DirectionNorth*(PIECE_WIDTH+HallwaySize))/2.0f),Quaternion.identity);
		hp.transform.position = new Vector3(hp.transform.position.x, hp.transform.position.y, hp.transform.position.z + 0f);
		//hp.transform.FindChild("SPAWNER").GetComponent<RoomSpawner>().SetRoom("N", hallPieces.Count);
		hallPieces.Add(hp);
		
		for(int i = 0; i < amountOfRooms-1; i++)
		{
			newLoc = false;
			prevDir = "";
			int repeats = 0;
			while(!newLoc){
				tDir = (TRAVEL_DIRECTION)UnityEngine.Random.Range(0,4);
				#region DETERMINE DIRECTION TO MOVE
				switch(tDir)
				{
				case TRAVEL_DIRECTION.NORTH:
					moveDir = DesignValues.DirectionNorth;
					tallestHeight++;
					currentHeight++;
					prevDir = "S";
					break;
					
				case TRAVEL_DIRECTION.SOUTH:
					if(currentHeight-1 < tallestHeight - 2 || prevLocations[currInfo].y < baseHeight+30)
						continue;
					moveDir = DesignValues.DirectionSouth;
					currentHeight--;
					prevDir = "N";
					break;
					
				case TRAVEL_DIRECTION.EAST:
					moveDir = DesignValues.DirectionEast;
					prevDir = "W";
					break;
					
				case TRAVEL_DIRECTION.WEST:
					moveDir = DesignValues.DirectionWest;
					prevDir = "E";
					break;
				}
				#endregion
				
				//Debug.Log(moveDir);
				
				if(repeats > 8)
				{
					newLoc = true;
					moveDir = DesignValues.DirectionNorth;
					prevDir = "S";
					proposedNewLocation = currentLoc + (moveDir*(PIECE_WIDTH+HallwaySize));
					continue;
				}
				
				proposedNewLocation = currentLoc + (moveDir*(PIECE_WIDTH+HallwaySize));
				
				if(!CheckPrevPositions(proposedNewLocation))
				{
					newLoc = true;
					//I HAVE FOUND A NEW LOCATION TO PLACE STUFF IN.
				}
				
				repeats++;
				
			}
			currentLoc = proposedNewLocation;
			
			if(repeats < 8)
			{
				prevLocations.Add(proposedNewLocation);
				
				
				if(moveDir == DesignValues.DirectionEast || moveDir == DesignValues.DirectionWest)
					MapHallPiece = (Resources.Load("MapPieces/"+Enum.GetName(typeof(BIOME),b)+"/Hall EW", typeof(GameObject)) as GameObject);
				else
					MapHallPiece = (Resources.Load("MapPieces/"+Enum.GetName(typeof(BIOME),b)+"/Hall NS", typeof(GameObject)) as GameObject);
				
				//GameObject newMapPiece = (GameObject)GameObject.Instantiate(MapRoomPiece,proposedNewLocation,Quaternion.identity);
				GameObject newHallPiece = (GameObject)GameObject.Instantiate(MapHallPiece,currentLoc - ((moveDir*(PIECE_WIDTH+HallwaySize))/2.0f),Quaternion.identity);
				newHallPiece.transform.position = new Vector3(newHallPiece.transform.position.x, newHallPiece.transform.position.y, newHallPiece.transform.position.z + 0f);
				//newHallPiece.transform.FindChild("SPAWNER").GetComponent<RoomSpawner>().SetRoom(prevDir, hallPieces.Count);
				
				hallPieces.Add(newHallPiece);
				//mapPieces.Add(newMapPiece);
				directionInfo[currInfo].addDir(prevDir);
				currInfo++;
				directionInfo.Add(new PieceInfo(b,prevDir));
				//mapPieces.Add(newHallPiece);
			}
			else
			{
				Debug.Log("Case Exception!");
				Vector3 vt = Generate_Map(input,b);
				return vt;
			}
			
		}
		
		moveDir = DesignValues.DirectionNorth;
		prevDir = "S";
		proposedNewLocation = currentLoc + (moveDir*(PIECE_WIDTH+HallwaySize));
		
		if(CheckPrevPositions(proposedNewLocation))
		{
			moveDir = DesignValues.DirectionEast;
			prevDir = "W";
			proposedNewLocation = currentLoc + (moveDir*(PIECE_WIDTH+HallwaySize));
			if(CheckPrevPositions(proposedNewLocation))
			{
				moveDir = DesignValues.DirectionWest;
				prevDir = "E";
				proposedNewLocation = currentLoc + (moveDir*(PIECE_WIDTH+HallwaySize));
			}
		}
		//GameObject finalMapPiece = (GameObject)GameObject.Instantiate(MapRoomPiece,proposedNewLocation,Quaternion.identity);0
		if(moveDir == DesignValues.DirectionEast || moveDir == DesignValues.DirectionWest)
			MapHallPiece = (Resources.Load("MapPieces/"+Enum.GetName(typeof(BIOME),b)+"/Hall EW", typeof(GameObject)) as GameObject);
		else
			MapHallPiece = (Resources.Load("MapPieces/"+Enum.GetName(typeof(BIOME),b)+"/Hall NS", typeof(GameObject)) as GameObject);
		//finalHallPiece.transform.Rotate(new Vector3(0,0,-1),90*moveDir.x);
		GameObject finalHallPiece = (GameObject)GameObject.Instantiate(MapHallPiece,proposedNewLocation - ((moveDir*(PIECE_WIDTH+HallwaySize))/2.0f),Quaternion.identity);
		finalHallPiece.transform.position = new Vector3(finalHallPiece.transform.position.x, finalHallPiece.transform.position.y, finalHallPiece.transform.position.z + 0f);
		//finalHallPiece.transform.FindChild("SPAWNER").GetComponent<RoomSpawner>().SetRoom(prevDir, hallPieces.Count);
		
		hallPieces.Add(finalHallPiece);
		
		directionInfo[currInfo].addDir(prevDir);
		currInfo++;
		directionInfo.Add (new PieceInfo (b, prevDir));
		directionInfo[currInfo].addDir("S");
		
		RemapWithDirections (b);
		
		previousMaps.Add (mapPieces);
		previousHalls.Add (hallPieces);
		
		finished = true;
		
		MapGraph.instance.BuildGraph();
		
		return proposedNewLocation;
	}
	
	void RemapWithDirections(BIOME b)
	{
		bossCount = 0;
		int type = 0;
		string tmp = "";
		for (int i = 0; i < prevLocations.Count; i++)
		{ 	type = UnityEngine.Random.Range(1,3);
			tmp = directionInfo[i].getDirection();
			GameObject loaded =(Resources.Load("MapPieces/"+Enum.GetName(typeof(BIOME),b)+"/Dungeon "+ tmp + " "+ type, typeof(GameObject)) as GameObject);
			GameObject loadedItems =(Resources.Load("MapPieces/"+Enum.GetName(typeof(BIOME),b)+"/Room Objects/Room Objs "+ tmp + " "+ type, typeof(GameObject)) as GameObject);
			//int randNum = UnityEngine.Random.Range(1,3);
			//int randNum = UnityEngine.Random.Range(1,3);
			if(!THREE_DIMENSIONAL)
			{
				//Debug.Log ("MapPieces/"+Enum.GetName(typeof(BIOME),b)+"/Dungeon "+ tmp + " "+ type);
				if((tmp != "EW" && tmp != "NS")){
					loaded =(Resources.Load("MapPieces/"+Enum.GetName(typeof(BIOME),b)+"/Dungeon "+ tmp + " "+ type, typeof(GameObject)) as GameObject);//+ " " + randNum, typeof(GameObject)) as GameObject);
					loadedItems =(Resources.Load("MapPieces/"+Enum.GetName(typeof(BIOME),b)+"/Room Objects/Room Objs "+ tmp + " "+ type, typeof(GameObject)) as GameObject);
				}
				//else if(tmp == "EW" || tmp=="NS")
				else if(tmp=="NS")
				{
					bossCount ++;
					if(bossCount > 1)
					{
						//Debug.Log("BossTiem");
						loaded =(Resources.Load("MapPieces/"+Enum.GetName(typeof(BIOME),b)+"/Dungeon Arena " + tmp, typeof(GameObject)) as GameObject);

						Debug.Log ("MapPieces/"+Enum.GetName(typeof(BIOME),b)+"/Dungeon Arena " + tmp);
						loadedItems =(Resources.Load("MapPieces/"+Enum.GetName(typeof(BIOME),b)+"/Room Objects/Room Objs Arena "+ tmp + " "+ type, typeof(GameObject)) as GameObject);
						bossCount = -2;
						
					}
					else
					{

						loaded =(Resources.Load("MapPieces/"+Enum.GetName(typeof(BIOME),b)+"/Dungeon "+ tmp + " "+ type, typeof(GameObject)) as GameObject);
						loadedItems =(Resources.Load("MapPieces/"+Enum.GetName(typeof(BIOME),b)+"/Room Objects/Room Objs "+ tmp + " "+ type, typeof(GameObject)) as GameObject);
					}
				}
			}
			else
			{
				loaded =(Resources.Load("MapPieces/"+Enum.GetName(typeof(BIOME),b)+"/3D"+ tmp, typeof(GameObject)) as GameObject);
			}
			//Debug.Log(loaded);
			Vector3 pos = new Vector3(prevLocations[i].x,prevLocations[i].y, 0.0f);

			//GameObject spawner = (GameObject)GameObject.Instantiate(Resources.Load ("MapPieces/" + Enum.GetName(typeof(BIOME),b) + "/RoomSpawner"),pos + new Vector3(0,0,-0.405f), Quaternion.identity);
			//spawner.GetComponent<Spawner>().biome = b;
			//Debug.Log (b);
			if(loaded == null) Debug.Log ("NULL: " +tmp+type +b );
			else
			{
				GameObject g2 = (GameObject)GameObject.Instantiate(loaded,pos,Quaternion.identity);
				g2.transform.parent = currentMap.transform;
				GameObject g2items = (GameObject)GameObject.Instantiate(loadedItems,pos,Quaternion.identity);
				mapPieces.Add(g2);
			}
		}
		isLoaded = true;
	}
}

[Serializable]
public struct PieceInfo
{
	public BIOME biome;
	public bool[] directions;
	
	public PieceInfo(BIOME b, string d)
	{
		biome = b;
		directions = new bool[4];
		
		if (d == "")
			return;
		if(d.Contains("N"))
		{
			directions[0] = true;
		}
		if(d.Contains("S"))
		{
			directions[1] = true;
		}
		if(d.Contains("E"))
		{
			directions[2] = true;
		}
		if(d.Contains("W"))
		{
			directions[3] = true;
		}
		
	}
	
	public void addDir(string d)
	{
		if(d.Contains("N"))
		{
			directions[1] = true;
		}
		if(d.Contains("S"))
		{
			directions[0] = true;
		}
		if(d.Contains("E"))
		{
			directions[3] = true;
		}
		if(d.Contains("W"))
		{
			directions[2] = true;
		}
	}
	
	public string getDirection()
	{
		string tmp = "";
		if(directions[0])
		{
			tmp += "N";
		}
		if(directions[1])
		{
			tmp += "S";
		}
		if(directions[2])
		{
			tmp += "E";
		}
		if(directions[3])
		{
			tmp += "W";
		}
		return tmp;
	}
}

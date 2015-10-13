using UnityEngine;
using System.Collections;

//	INPUT_MANAGER
//	HANDLES INDIVIDUAL INPUT REGISTRATIONS
//
// 	NOTE: THIS NEEDS TO BE IN THE FIRST SCENE!!!
//	OTHER SYSTEMS READ FROM THIS SINGLETON

public class Input_Manager : MonoBehaviour {
	
	#region Singleton Stuff
	private static Input_Manager _instance;
	
	public static Input_Manager instance
	{
		get
		{
			if(_instance == null)
			{
				_instance = GameObject.FindObjectOfType<Input_Manager>();
				
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
	}

	#endregion

	#region Declaration Station

	//DATA FOR INPUT_MANAGER

	//STORE ALL DATA FOR INPUT
	[HideInInspector] public bool[] A = new bool[4];
	[HideInInspector] public bool[] B = new bool[4];
	[HideInInspector] public bool[] X = new bool[4];
	[HideInInspector] public bool[] Y = new bool[4];
	[HideInInspector] public bool[] LB = new bool[4];
	[HideInInspector] public bool[] RB = new bool[4];
	[HideInInspector] public bool[] Back = new bool[4];
	[HideInInspector] public bool[] Start = new bool[4];
	[HideInInspector] public bool[] L_TRIG = new bool[4];
	[HideInInspector] public bool[] R_TRIG = new bool[4];
	[HideInInspector] public bool[] L_STICK_PRESS = new bool[4];
	[HideInInspector] public bool[] R_STICK_PRESS = new bool[4];
	[HideInInspector] public Vector2[] INPUT_L = new Vector2[4];
	[HideInInspector] public Vector2[] INPUT_R = new Vector2[4];
	[HideInInspector] public Vector3[] STICK_L = new Vector3[4];
	[HideInInspector] public Vector3[] STICK_R = new Vector3[4];

	#endregion


	void Update()
	{

		if(Input.GetKeyDown(KeyCode.Escape))
		{
			Application.LoadLevel(0);
		}

		CheckInputs();

	}

	void CheckInputs()
	{	

		A[(int)Player.One] = Player_Manager.instance.players[(int)Player.One].p.GetButtonDown ("A");//Input.GetButtonDown("A_1");
		A[(int)Player.Two] = Player_Manager.instance.players[(int)Player.Two].p.GetButtonDown ("A");
		A[(int)Player.Three] = Player_Manager.instance.players[(int)Player.Three].p.GetButtonDown ("A");
		A[(int)Player.Four] = Player_Manager.instance.players[(int)Player.Four].p.GetButtonDown ("A");

		B[(int)Player.One] = Player_Manager.instance.players[(int)Player.One].p.GetButtonDown ("B");
		B[(int)Player.Two] = Player_Manager.instance.players[(int)Player.Two].p.GetButtonDown ("B");
		B[(int)Player.Three] = Player_Manager.instance.players[(int)Player.Three].p.GetButtonDown ("B");
		B[(int)Player.Four] = Player_Manager.instance.players[(int)Player.Four].p.GetButtonDown ("B");

		X[(int)Player.One] = Player_Manager.instance.players[(int)Player.One].p.GetButtonDown ("X");
		X[(int)Player.Two] = Player_Manager.instance.players[(int)Player.Two].p.GetButtonDown ("X");
		X[(int)Player.Three] = Player_Manager.instance.players[(int)Player.Three].p.GetButtonDown ("X");
		X[(int)Player.Four] = Player_Manager.instance.players[(int)Player.Four].p.GetButtonDown ("X");

		Y[(int)Player.One] = Player_Manager.instance.players [(int)Player.One].p.GetButtonDown ("Y");
		Y[(int)Player.Two] = Player_Manager.instance.players[(int)Player.Two].p.GetButtonDown ("Y");
		Y[(int)Player.Three] = Player_Manager.instance.players[(int)Player.Three].p.GetButtonDown ("Y");
		Y[(int)Player.Four] = Player_Manager.instance.players[(int)Player.Four].p.GetButtonDown ("Y");

		Start[(int)Player.One] = Player_Manager.instance.players [(int)Player.One].p.GetButtonDown ("Start");
		Start[(int)Player.Two] = Player_Manager.instance.players[(int)Player.Two].p.GetButtonDown ("Start");
		Start[(int)Player.Three] = Player_Manager.instance.players[(int)Player.Three].p.GetButtonDown ("Start");
		Start[(int)Player.Four] = Player_Manager.instance.players[(int)Player.Four].p.GetButtonDown ("Start");

		Back[(int)Player.One] = Player_Manager.instance.players [(int)Player.One].p.GetButtonDown ("Back");
		Back[(int)Player.Two] = Player_Manager.instance.players[(int)Player.Two].p.GetButtonDown ("Back");
		Back[(int)Player.Three] = Player_Manager.instance.players[(int)Player.Three].p.GetButtonDown ("Back");
		Back[(int)Player.Four] = Player_Manager.instance.players[(int)Player.Four].p.GetButtonDown ("Back");

		LB[(int)Player.One] = Player_Manager.instance.players[(int)Player.One].p.GetButtonDown ("LB");
		LB[(int)Player.Two] = Player_Manager.instance.players[(int)Player.Two].p.GetButtonDown ("LB");
		LB[(int)Player.Three] = Player_Manager.instance.players[(int)Player.Three].p.GetButtonDown ("LB");
		LB[(int)Player.Four] = Player_Manager.instance.players[(int)Player.Four].p.GetButtonDown ("LB");

		RB[(int)Player.One] = Player_Manager.instance.players[(int)Player.One].p.GetButtonDown ("RB");
		RB[(int)Player.Two] = Player_Manager.instance.players[(int)Player.Two].p.GetButtonDown ("RB");
		RB[(int)Player.Three] = Player_Manager.instance.players[(int)Player.Three].p.GetButtonDown ("RB");
		RB[(int)Player.Four] = Player_Manager.instance.players[(int)Player.Four].p.GetButtonDown ("RB");

		L_STICK_PRESS[(int)Player.One] = Player_Manager.instance.players[(int)Player.One].p.GetButton("L_Stick_Press");
		L_STICK_PRESS[(int)Player.Two] = Player_Manager.instance.players[(int)Player.Two].p.GetButton ("L_Stick_Press");
		L_STICK_PRESS[(int)Player.Three] = Player_Manager.instance.players[(int)Player.Three].p.GetButton ("L_Stick_Press");
		L_STICK_PRESS[(int)Player.Four] = Player_Manager.instance.players[(int)Player.Four].p.GetButton ("L_Stick_Press");

		R_STICK_PRESS[(int)Player.One] = Player_Manager.instance.players[(int)Player.One].p.GetButton ("R_Stick_Press");
		R_STICK_PRESS[(int)Player.Two] = Player_Manager.instance.players[(int)Player.Two].p.GetButton ("R_Stick_Press");
		R_STICK_PRESS[(int)Player.Three] = Player_Manager.instance.players[(int)Player.Three].p.GetButton ("R_Stick_Press");
		R_STICK_PRESS[(int)Player.Four] = Player_Manager.instance.players[(int)Player.Four].p.GetButton ("R_Stick_Press");

	
		L_TRIG [(int)Player.One] = Player_Manager.instance.players[(int)Player.One].p.GetAxis ("L_Trig") > 0.0f? true:false;
		L_TRIG [(int)Player.Two] = Player_Manager.instance.players[(int)Player.Two].p.GetAxis ("L_Trig") > 0.0f? true:false;
		L_TRIG [(int)Player.Three] = Player_Manager.instance.players[(int)Player.Three].p.GetAxis ("L_Trig") > 0.0f? true:false;
		L_TRIG [(int)Player.Four] = Player_Manager.instance.players[(int)Player.Four].p.GetAxis ("L_Trig") > 0.0f? true:false;

		R_TRIG [(int)Player.One] = Player_Manager.instance.players[(int)Player.One].p.GetAxis ("R_Trig") > 0.0f? true:false;
		R_TRIG [(int)Player.Two] = Player_Manager.instance.players[(int)Player.Two].p.GetAxis ("R_Trig") > 0.0f? true:false;
		R_TRIG [(int)Player.Three] = Player_Manager.instance.players[(int)Player.Three].p.GetAxis ("R_Trig") > 0.0f? true:false;
		R_TRIG [(int)Player.Four] = Player_Manager.instance.players[(int)Player.Four].p.GetAxis ("R_Trig") > 0.0f? true:false;

		INPUT_L[(int)Player.One] = new Vector2(Player_Manager.instance.players[(int)Player.One].p.GetAxis ("L_Stick_X"), Player_Manager.instance.players[(int)Player.One].p.GetAxis ("L_Stick_Y"));
		INPUT_L[(int)Player.Two] = new Vector2(Player_Manager.instance.players[(int)Player.Two].p.GetAxis ("L_Stick_X"), Player_Manager.instance.players[(int)Player.Two].p.GetAxis ("L_Stick_Y"));
		INPUT_L[(int)Player.Three] = new Vector2(Player_Manager.instance.players[(int)Player.Three].p.GetAxis ("L_Stick_X"), Player_Manager.instance.players[(int)Player.Three].p.GetAxis ("L_Stick_Y"));
		INPUT_L[(int)Player.Four] = new Vector2(Player_Manager.instance.players[(int)Player.Four].p.GetAxis ("L_Stick_X"), Player_Manager.instance.players[(int)Player.Four].p.GetAxis ("L_Stick_Y"));

		INPUT_R[(int)Player.One] = new Vector2(Player_Manager.instance.players[(int)Player.One].p.GetAxis ("R_Stick_X"), Player_Manager.instance.players[(int)Player.One].p.GetAxis ("R_Stick_Y"));
		INPUT_R[(int)Player.Two] = new Vector2(Player_Manager.instance.players[(int)Player.Two].p.GetAxis ("R_Stick_X"), Player_Manager.instance.players[(int)Player.Two].p.GetAxis ("R_Stick_Y"));
		INPUT_R[(int)Player.Three] = new Vector2(Player_Manager.instance.players[(int)Player.Three].p.GetAxis ("R_Stick_X"), Player_Manager.instance.players[(int)Player.Three].p.GetAxis ("R_Stick_Y"));
		INPUT_R[(int)Player.Four] = new Vector2(Player_Manager.instance.players[(int)Player.Four].p.GetAxis ("R_Stick_X"), Player_Manager.instance.players[(int)Player.Four].p.GetAxis ("R_Stick_Y"));

		STICK_L[(int)Player.One] = new Vector3(INPUT_L[(int)Player.One].x, INPUT_L[(int)Player.One].y,0);
		STICK_L[(int)Player.Two] = new Vector3(INPUT_L[(int)Player.Two].x, INPUT_L[(int)Player.Two].y,0);
		STICK_L[(int)Player.Three] = new Vector3(INPUT_L[(int)Player.Three].x, INPUT_L[(int)Player.Three].y,0);
		STICK_L[(int)Player.Four] = new Vector3(INPUT_L[(int)Player.Four].x, INPUT_L[(int)Player.Four].y,0);

		STICK_R[(int)Player.One] = new Vector3(INPUT_R[(int)Player.One].x, INPUT_R[(int)Player.One].y,0);
		STICK_R[(int)Player.Two] = new Vector3(INPUT_R[(int)Player.Two].x, INPUT_R[(int)Player.Two].y,0);
		STICK_R[(int)Player.Three] = new Vector3(INPUT_R[(int)Player.Three].x, INPUT_R[(int)Player.Three].y,0);
		STICK_R[(int)Player.Four] = new Vector3(INPUT_R[(int)Player.Four].x, INPUT_R[(int)Player.Four].y,0);

		STICK_L[(int)Player.One].Normalize ();
		STICK_L[(int)Player.Two].Normalize ();
		STICK_L[(int)Player.Three].Normalize ();
		STICK_L[(int)Player.Four].Normalize ();

		STICK_R[(int)Player.One].Normalize ();
		STICK_R[(int)Player.Two].Normalize ();
		STICK_R[(int)Player.Three].Normalize ();
		STICK_R[(int)Player.Four].Normalize ();

	}
}
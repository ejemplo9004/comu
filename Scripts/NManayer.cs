using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using SocketIO;

public class NManayer : MonoBehaviour {

	public static NManayer instance;
	public Canvas canvas;
	public InputField nombreJugador;
	public GameObject jugador;
	public Slider slPersonaje;

	public string nombreDelJugador;

	public SocketIOComponent socket;

	void Awake()
	{
		if (instance == null) {
			instance = this;
		} else {
			DestroyImmediate (gameObject);

		}
		DontDestroyOnLoad (gameObject);
	}

	void Start () 
	{
		//socket.On("enemies",OnOtroJugadorConectado);
		socket.On("otro jugador conectado",OnOtroJugadorConectado);
		socket.On("Entro un jugador nuevo", OnEntroJugadorNuevo);
		socket.On("player move", OnPlayerMove);
		socket.On("player turn", OnPlayerTurn);
		socket.On("play", OnPlay);
		socket.On("player shoot", OnPlayerShoot);
		socket.On("player dance", OnPlayerDance);
		socket.On("salud", OnSalud);
		socket.On("muerte", OnMuerte);
		socket.On("otro jugador desconectado",OnOtroDesconectado);
	}

	public void JoinGame ()
	{
		StartCoroutine (ConectarAlServidor ());
	}

	#region Commands

	public IEnumerator ConectarAlServidor()
	{
		print("Inicia la Conexión");
		yield return new WaitForSeconds (0.5f);

		socket.Emit("conectar jugador");

		yield return new WaitForSeconds(1f);

		string nJugador = Mathf.RoundToInt(slPersonaje.value).ToString() + nombreJugador.text;
		List<SpawnPoint> playerSpawnPoints = GetComponent<SpawnerJugadores>().spawnPointsJugadores;
		PlayerJSON playerJSON = new PlayerJSON(nJugador, playerSpawnPoints);
		string data = JsonUtility.ToJson(playerJSON);
		socket.Emit("play", new JSONObject(data));
		canvas.gameObject.SetActive(false);

	}

	public void CommandMove (Vector3 vec3)
	{
		string data = JsonUtility.ToJson((new PositionJSON(vec3)));
		socket.Emit("player move",new JSONObject(data));
	}

	public void CommandRotate (Quaternion quate)
	{
		string data = JsonUtility.ToJson((new RotationJSON(quate)));
		socket.Emit("player turn",new JSONObject(data));
	}

	public void CommandShoot ()
	{
		socket.Emit("player shoot");
	} 

	public void CommandDance ()
	{
		socket.Emit("player dance");
	} 

	public void CommandSalud(GameObject desde, GameObject para, float cuanto)
	{
		SaludCambiarJSON saludCambiar = new SaludCambiarJSON(para.name, cuanto, desde.name, false);
		socket.Emit("salud",new JSONObject(JsonUtility.ToJson(saludCambiar)));
	} 

	public void CommandMuerte(GameObject para, int cuanto)
	{
		MuerteCambiarJSON muerteCambiar = new MuerteCambiarJSON(para.name, cuanto);
		socket.Emit("muerte",new JSONObject(JsonUtility.ToJson(muerteCambiar)));
	} 


	#endregion


	#region Listening


	void OnOtroJugadorConectado (SocketIOEvent socketIOEvent)
	{
		
	} 
	void OnPlay (SocketIOEvent socketIOEvent)
	{
		print("Entraste Al Juego");
		string data = socketIOEvent.data.ToString();
		UserJSON userJSON = UserJSON.CreateFromJSON(data);
		Vector3 pos = new Vector3(userJSON.position[0],userJSON.position[1],userJSON.position[2]);
		Quaternion rot = Quaternion.Euler(userJSON.rotation[0], userJSON.rotation[1], userJSON.rotation[2]);
		GameObject p = GameObject.Find(userJSON.nombre);
		if(p == null)
		{
			p = Instantiate(jugador,pos,rot) as GameObject;
		}
		ControlPersonaje cp = p.GetComponent<ControlPersonaje>();
		cp.Inicializar(userJSON.nombre, true);
		// Inicializo el nombre en el Manager
		nombreDelJugador = userJSON.nombre;
	} 

	void OnEntroJugadorNuevo (SocketIOEvent socketIOEvent)
	{
		print("Entro otro jugador");
		string data = socketIOEvent.data.ToString();
		UserJSON userJSON = UserJSON.CreateFromJSON(data);
		Vector3 pos = new Vector3(userJSON.position[0],userJSON.position[1],userJSON.position[2]);
		Quaternion rot = Quaternion.Euler(userJSON.rotation[0], userJSON.rotation[1], userJSON.rotation[2]);
		GameObject o = GameObject.Find(userJSON.nombre) as GameObject;
		if(o != null)
		{
			return;
		} 
		GameObject p = GameObject.Find(userJSON.nombre);
		if(p == null)
		{
			p = Instantiate(jugador,pos,rot) as GameObject;
		}
		ControlPersonaje cp = p.GetComponent<ControlPersonaje>();
		cp.personaje = userJSON.personaje;
		cp.Inicializar(userJSON.nombre, false);

		Salud s = p.GetComponent<Salud>();
		s.saludActual = userJSON.salud;
		s.CambiarSalud();
	} 

	void OnPlayerMove (SocketIOEvent socketIOEvent)
	{
		string data = socketIOEvent.data.ToString();
		UserJSON userJSON = UserJSON.CreateFromJSON(data);
		Vector3 pos = new Vector3(userJSON.position[0],userJSON.position[1],userJSON.position[2]);
		// Si este es el jugador actual
		if(userJSON.nombre.Substring(1) == nombreJugador.text)
		{
			return;
		}
		GameObject p = GameObject.Find(userJSON.nombre) as GameObject;
		if(p==null){
			p = Instantiate(jugador,pos, Quaternion.identity) as GameObject;
			ControlPersonaje cp = p.GetComponent<ControlPersonaje>();
			cp.personaje = userJSON.personaje;
			cp.Inicializar(userJSON.nombre, false);

			Salud s = p.GetComponent<Salud>();
			s.saludActual = userJSON.salud;
			s.CambiarSalud();
		}
		p.transform.position=pos;
	} 

	void OnPlayerTurn (SocketIOEvent socketIOEvent)
	{
		string data = socketIOEvent.data.ToString();
		UserJSON userJSON = UserJSON.CreateFromJSON(data);
		Quaternion rot = Quaternion.Euler(userJSON.rotation[0],userJSON.rotation[1],userJSON.rotation[2]);
		// Si este es el jugador actual
		if(userJSON.nombre == nombreJugador.text)
		{
			return;
		}
		GameObject p = GameObject.Find(userJSON.nombre) as GameObject;
		if(p!=null)
		{
			p.transform.rotation=rot;
		} 
	} 

	void OnPlayerShoot (SocketIOEvent socketIOEvent)
	{
		string data = socketIOEvent.data.ToString();
		ShootJSON shootJSON = ShootJSON.CreateFromJSON(data);
		GameObject p = GameObject.Find(shootJSON.nombre);
		if(p!=null && shootJSON.nombre != nombreDelJugador)	
		{
			ControlPersonaje cp = p.GetComponent<ControlPersonaje>();
			cp.Disparar();
		} 
	} 

	void OnPlayerDance (SocketIOEvent socketIOEvent)
	{
		string data = socketIOEvent.data.ToString();
		ShootJSON shootJSON = ShootJSON.CreateFromJSON(data);
		GameObject p = GameObject.Find(shootJSON.nombre);
		if(p!=null && shootJSON.nombre != nombreDelJugador)	
		{
			ControlPersonaje cp = p.GetComponent<ControlPersonaje>();
			cp.Bailar();
		} 
	} 

	void OnSalud (SocketIOEvent socketIOEvent)
	{
		string data = socketIOEvent.data.ToString();
		SaludUsuarioJSON saludUsuarioJSON = SaludUsuarioJSON.CreateFromJSON(data);
		GameObject p = GameObject.Find(saludUsuarioJSON.nombre);
		Salud s = p.GetComponent<Salud>();
		s.saludActual = saludUsuarioJSON.salud;
		s.CambiarSalud();
	} 

	void OnMuerte (SocketIOEvent socketIOEvent)
	{
		//print("Acabo de pasar por muertes");
		string data = socketIOEvent.data.ToString();
		MuerteUsuarioJSON muerteUsuarioJSON = MuerteUsuarioJSON.CreateFromJSON(data);
		GameObject p = GameObject.Find(muerteUsuarioJSON.nombre);
		ControlPersonaje cp = p.GetComponent<ControlPersonaje>();
		cp.muertes = muerteUsuarioJSON.muertes;
		cp.CambiarMuertes();
	} 

	void OnOtroDesconectado (SocketIOEvent socketIOEvent)
	{
		print("Usuario desconectado");
		string data = socketIOEvent.data.ToString();
		UserJSON userJson = UserJSON.CreateFromJSON(data);
		Destroy(GameObject.Find(userJson.nombre));
	} 


	#endregion


	#region JSONMessageClases
	[Serializable]
	public class PlayerJSON
	{
		public string nombre;
		public List<PointJSON> spawnPoints;

		public PlayerJSON (string _nombre, List<SpawnPoint> _spawnPoints)
		{
			nombre = _nombre;
			spawnPoints = new List<PointJSON>();

			foreach (SpawnPoint punto in _spawnPoints) 
			{
				PointJSON pjson = new PointJSON(punto);
				spawnPoints.Add(pjson);
			}
		}
	}

	[Serializable]
	public class PointJSON
	{
		public float[] position;
		public float[] rotation;
		public PointJSON(SpawnPoint punto)
		{
			position = new float[] {
				punto.transform.position.x,
				punto.transform.position.y,
				punto.transform.position.z
			};
			rotation = new float[] {
				punto.transform.eulerAngles.x,
				punto.transform.eulerAngles.y,
				punto.transform.eulerAngles.z
			};
		}
	}

	[Serializable]
	public class PositionJSON
	{
		public float[] position;
		public PositionJSON(Vector3 pos)
		{
			position = new float[] {
				pos.x,
				pos.y,
				pos.z
			};
		}
	}

	[Serializable]
	public class RotationJSON
	{
		public float[] rotation;
		public RotationJSON(Quaternion rot)
		{
			rotation = new float[] {
				rot.eulerAngles.x,
				rot.eulerAngles.y,
				rot.eulerAngles.z
			};
		}
	}

	[Serializable]
	public class UserJSON
	{
		public string 	nombre;
		public float[] 	position;
		public float[] 	rotation;
		public float 	salud;
		public int 		personaje;
		public int		muertes;

		public static UserJSON CreateFromJSON(string data)
		{
			return JsonUtility.FromJson<UserJSON> (data);
		}
	}

	[Serializable]
	public class  SaludCambiarJSON
	{
		public string nombre;
		public float saludCambio;
		public string from;
		public bool esEnemigo;

		public SaludCambiarJSON (String _nombre, float _saludCambio, string _from, bool _esEnemigo)
		{
			nombre = _nombre;
			saludCambio = _saludCambio;
			from = _from;
			esEnemigo = _esEnemigo;
		}
	}
	[Serializable]
	public class  MuerteCambiarJSON
	{
		public string nombre;
		public int muertesCambio;

		public MuerteCambiarJSON (String _nombre, int _muertesCambio)
		{
			nombre = _nombre;
			muertesCambio = _muertesCambio;
		}
	}

	[Serializable]
	public class ShootJSON
	{
		public string nombre;
		public static ShootJSON CreateFromJSON(string data)
		{
			return JsonUtility.FromJson<ShootJSON> (data);
		}
	}


	[Serializable]
	public class SaludUsuarioJSON
	{
		public string nombre;
		public float salud;

		public static SaludUsuarioJSON CreateFromJSON (string data)
		{
			return JsonUtility.FromJson<SaludUsuarioJSON> (data);
		}
	}

	[Serializable]
	public class MuerteUsuarioJSON
	{
		public string 	nombre;
		public int 		muertes;

		public static MuerteUsuarioJSON CreateFromJSON (string data)
		{
			return JsonUtility.FromJson<MuerteUsuarioJSON> (data);
		}
	}

	#endregion





}

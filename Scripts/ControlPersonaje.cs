using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlPersonaje : MonoBehaviour {
	public float 		velocidad;
	public float 		velRotacion;
	public Animator		animPersonaje;
	public Animator[]	animPersonajes;
	public GameObject[]	personajes;
	public int			muertes;

	public GameObject 	prPoder;
	public Transform	puntoPoder;
	public bool 		esLocalPlayer;
	public GameObject	camara;

	public Text 		txtNombre;
	public Text 		txtMuertes;

	public float 		velocidadPoder = 20;
	public float[]		tiemposBloqueoPorPoder;
	public float		tiempoBloqueoPorPoder=1;
	public float[]		tiemposEsperaPorPoder;
	public float		tiempoEsperaPorPoder=1;

	public int			personaje;
	
	float 	tiempoBloqueo;

	Vector3 posAnterior;
	Vector3 posActual;
	Quaternion rotAnterior;
	Quaternion rotActual;
	bool dancing=false;

	Vector3 animPosAnterior;
	void Start () 
	{
		posAnterior = transform.position;
		rotAnterior = transform.rotation;
		posActual = transform.position;
		rotActual = transform.rotation;
		animPosAnterior = transform.position;
		InvokeRepeating("ControlAnimaciones",0.5f, 0.2f);
	}

	public void ControlAnimaciones()
	{
		float tam = (animPosAnterior-transform.position).sqrMagnitude;
		animPersonaje.SetFloat("velocidad", tam);
		if(dancing && tam>0.2f)
		{
			animPersonaje.SetBool("bailando", false);
			dancing = false;
		}
		animPosAnterior = transform.position;
		
	}

	public void CambiarNombre(string n)
	{
		txtNombre.text = n.Substring(1);
		personaje = int.Parse(n.Substring(0, 1));
	}

	public void Inicializar (string nombre, bool esLocal)
	{
		CambiarNombre(nombre);
		esLocalPlayer = esLocal;
		gameObject.name = nombre;
		if(!esLocal)
		{
			Destroy(camara);
		} 
		animPersonaje = animPersonajes[personaje];
		tiempoBloqueoPorPoder = tiemposBloqueoPorPoder[personaje];
		tiempoEsperaPorPoder = tiemposEsperaPorPoder[personaje];
		for(int i = 0; i<personajes.Length; i++)
		{
			personajes[i].SetActive(i==personaje);
		}
	} 

	public void Morir ()
	{
		Destroy(gameObject,4);
		animPersonaje.SetTrigger("morir");
		Destroy(this);
	} 
	void Update () 
	{
//		animPersonaje.SetFloat ("velocidad", Input.GetAxis ("Vertical"));
		if(!esLocalPlayer || Time.time < tiempoBloqueo){
			return;
		}

		transform.Translate (Vector3.forward * velocidad * Input.GetAxis ("Vertical")*Time.deltaTime);
		transform.Rotate (Vector3.up * velRotacion * Input.GetAxis ("Horizontal")*Time.deltaTime);

		posActual = transform.position;
		rotActual = transform.rotation;

		if (posActual != posAnterior) {
			//Networking
			NManayer.instance.CommandMove(transform.position);
			posAnterior = posActual;
			if(dancing)
			{
				animPersonaje.SetBool("bailando", false);
				dancing = false;
			}
		}
		if (rotActual != rotAnterior) {
			NManayer.instance.CommandRotate(transform.rotation);
			rotAnterior = rotActual;
		}
		if (Input.GetKeyDown(KeyCode.Space)) {
			NManayer.instance.CommandShoot();
			Disparar();
		}
		if (Input.GetKeyDown(KeyCode.M)) {
			NManayer.instance.CommandDance();
			Bailar();
		}
	}

	public void AgregarMuerte()
	{
		if(esLocalPlayer) muertes++;
		NManayer.instance.CommandSalud(gameObject, gameObject, -10);
		CambiarMuertes ();
	}


	public void Disparar()
	{
		if(Time.time < tiempoBloqueo) return;
		tiempoBloqueo = Time.time + tiempoBloqueoPorPoder;
		animPersonaje.SetTrigger("shoot");
		StartCoroutine(InstanciaPoder());
	}

	public void CambiarMuertes ()
	{
		//print("Muertes Cambiadas");
		txtMuertes.text = "< " + muertes.ToString() + " >";
		TopCinco.singleton.ActualizarElemento();
	} 

	public void Bailar()
	{
		dancing = true;
		animPersonaje.SetBool("bailando", true);
	}

	public IEnumerator InstanciaPoder ()
	{
		yield return new WaitForSeconds(tiempoEsperaPorPoder);
		GameObject Objeto = Instantiate (prPoder, puntoPoder.position, puntoPoder.rotation) as GameObject;
		Poder p = Objeto.GetComponent<Poder> ();
		Objeto.GetComponent<Rigidbody> ().velocity = transform.forward * velocidadPoder;
		p.JugadorCreador = this;
	} 
}

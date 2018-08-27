using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class TopCinco : MonoBehaviour {
	public List<ElementoTop> listaElementos;
	public Image[] 	imagenes;
	public Text[] 	nombres;
	public Text[] 	puntos;
	public Sprite[] imReferencias;
	public Sprite 	imReferenciaNula;
	public GameObject[] listaJugadores;
	public static TopCinco singleton;

	// Use this for initialization
	void Awake () 
	{
		singleton = this;	
	}
	
	void Start ()
	{
		InvokeRepeating("ActualizarElemento", 0.5f, 2.5f);
	} 
	/*
	public void ActualizarElemento (string nombre, int muertes, int personaje) 
	{
		print("Acutalizar Elementos del Top");
		int a = -1;

		for (int i=0; i < listaElementos.Count ; i++)
		{
			if(listaElementos[i].nombre == nombre)
			{
				a = i;
			}
		}
		ElementoTop e;
		if(a == -1)
		{
			e = new ElementoTop(muertes, nombre, personaje);
			listaElementos.Add(e);
		}else{
			e = listaElementos[a];
		}
		e.muertes = muertes;
		ActualizarInterfaz();
	}*/

	public void ActualizarElemento () 
	{
		//print("Actualizando elementos");
		listaElementos.Clear();
		//print("1");
		listaJugadores = GameObject.FindGameObjectsWithTag("Personaje");
		//print("2");
		for(int i = 0; i<listaJugadores.Length;i++)
		{
			//print("0");
			ControlPersonaje cp = listaJugadores[i].GetComponent<ControlPersonaje>();
			if(cp != null)
			{
				ElementoTop e = new ElementoTop(cp.muertes, cp.gameObject.name, cp.personaje);
				listaElementos.Add(e);
			}
		}
		//print("3");
		//print("Ordenar elementos");
		OrdenarLista ();

		//print("4");
		//print("Mostrar elementos");
		ActualizarInterfaz();
	}

	void ActualizarInterfaz ()
	{
		for (int i=0; i < 5 ; i++)
		{
			imagenes[i].sprite = imReferenciaNula;
			nombres[i].text = "- - - - -";
			puntos[i].text = "0";
		}
		
		for (int i=0; i < listaElementos.Count ; i++)
		{
			imagenes[i].sprite = imReferencias[listaElementos[i].personaje];
			nombres[i].text = listaElementos[i].nombre.Substring(1);
			puntos[i].text = listaElementos[i].muertes.ToString();
		}
	} 

	void OrdenarLista ()
	{
		listaElementos = listaElementos.OrderByDescending(o => o.muertes).ToList();
	} 
}
[System.Serializable]
public class ElementoTop
{
	public int 		muertes;
	public string 	nombre;
	public int 		personaje;
	public ElementoTop (int _muertes, string _nombre, int _personaje)
	{
		muertes = _muertes;
		nombre = _nombre;
		personaje = _personaje;
	}
}

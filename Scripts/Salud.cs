using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Salud : MonoBehaviour {
	public float saludMaxima = 100;
	public bool destruirAlMorir = true;

	public float saludActual;
	public bool esEnemigo = false;

	public Slider slSalud;
	private bool esLocalPlayer;
	// Use this for initialization
	void Start () {
		ControlPersonaje cp = GetComponent<ControlPersonaje> ();
		esLocalPlayer = cp.esLocalPlayer;
		saludActual = saludMaxima;
	}

	public void CausarDaño(ControlPersonaje plFrom, float cuanto)
	{
		saludActual -= cuanto;
		//CambiarSalud ();
		NManayer.instance.CommandSalud(plFrom.gameObject, gameObject, cuanto);
		if(saludActual==0){
			NManayer.instance.CommandMuerte(plFrom.gameObject, plFrom.muertes+1);
		}
	}

	public void CambiarSalud()
	{
		slSalud.value = saludActual / saludMaxima;
		if (saludActual<= 0) {
			if (true) 
			{
				gameObject.GetComponent<ControlPersonaje>().Morir();
			} else {
				saludActual = saludMaxima;
				slSalud.value = 1;
			}
		}
	}

	public void Respawn()
	{
		if (esLocalPlayer) {
			transform.position = Vector3.zero;
			transform.rotation = Quaternion.identity;
		}
	}
}

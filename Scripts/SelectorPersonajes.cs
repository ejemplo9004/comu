using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorPersonajes : MonoBehaviour {
	public GameObject[] personajes;

	public void CambiarPersonaje (float cual)
	{
		for(int i = 0; i<personajes.Length; i++)
		{
			personajes[i].SetActive(i==Mathf.FloorToInt(cual));
		}
	}
}

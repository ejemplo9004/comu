using UnityEngine;

public class Poder : MonoBehaviour 
{
	public float 		daño = 10;
	public ControlPersonaje JugadorCreador;
	public float 		tiempoVida = 10;
	public GameObject 	particulasExplosion;

	void Start()
	{
		Destroy(gameObject, tiempoVida);
	} 

	void OnTriggerEnter(Collider otro)
	{
		Salud s = otro.gameObject.GetComponent<Salud> ();
		if (s != null) {
			s.CausarDaño (JugadorCreador, daño);
		}
		GameObject op = Instantiate(particulasExplosion, transform.position, transform.rotation) as GameObject;
		Destroy(op,4);
		Destroy (gameObject);
	}
}

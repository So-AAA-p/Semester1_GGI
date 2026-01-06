using UnityEngine;

public class Laser : MonoBehaviour
    
{
    public GameObject Kugel;
    public GameObject Wurfel;
    public LineRenderer Liniee;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        
        
    }

    // Update is called once per frame
    void Update()
    {
        Liniee.SetPosition(0, Kugel.transform.position);                    // erstes Ende der Linie wird auf die Position der Kugel gesetzt
		Liniee.SetPosition(1, Wurfel.transform.position);                   // zweites Ende der Linie wird auf die Position des Würfels gesetzt
	}
}

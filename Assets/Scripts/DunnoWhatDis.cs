using UnityEngine;

public class visuals : MonoBehaviour
{
    
    public Vektormanager VektorManager; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(VektorManager.BallPosition + VektorManager.BallPosition);


    }
}

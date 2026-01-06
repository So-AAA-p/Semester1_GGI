using UnityEngine;

public class Vektormanager : MonoBehaviour
{
    public GameObject Ball;
    public GameObject BallFollow;
    public Vector3 BallPosition;
    public string BallDebugStirng;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {

        //Ball.transform.position = BallFollow.transform.position;

        Ball.transform.position = Vector3.Lerp(
            Ball.transform.position, BallFollow.transform.position, Time.deltaTime);

    }
}

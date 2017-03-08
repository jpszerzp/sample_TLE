using UnityEngine;
using System.Collections;

public class BossManager : Singleton<BossManager> {

    float idlePeriod;
    public float IdlePeriod
    {
        get; set;
    }

    bool idleStarted;
    public bool IdleStarted
    {
        get; set;
    }

    bool idling;
    public bool Idling
    {
        get; set;
    }

    float startX;
    float startY;
    float ampX = 2;
    float ampY = 2;
    float idleSpd = 2;

	// Use this for initialization
	void Start () {
        idleStarted = false;
        idling = false;
	}
	
	// Update is called once per frame
	void Update () {

        // IdleStarted will be turned on to start floating process;
        // Idling will be turned off to stop the process.
        if (idleStarted)
        {
            startX = transform.position.x;
            startY = transform.position.y;
            idleStarted = false;
            idling = true;
        }

        if (idling && idlePeriod > 0)
        {
            float tempX = startX + ampX * Mathf.Sin(idleSpd * Time.time);
            float tempY = startY + ampY * Mathf.Sin(idleSpd * Time.time);
            transform.position = new Vector3(tempX, tempY, transform.position.z);

            idlePeriod -= Time.deltaTime;
        }

        if (idlePeriod < 0)
        {
            idlePeriod = 0;    // idlePeriod exits with 0
            idling = false;    // idle cycle ends, idling and idleStarted exits with false as expected
        }

        //print(transform.position.x);
	}
}

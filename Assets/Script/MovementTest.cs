using UnityEngine;
using System.Collections;

public class MovementTest : MonoBehaviour {

    public Transform startmarker;
    public Transform endmarker;

    float spd = 1f;

    private float starttime;
    private float journeylength;

	// Use this for initialization
	void Start () {
        starttime = Time.time;
        journeylength = Vector3.Distance(startmarker.position, endmarker.position);
	}
	
	// Update is called once per frame
	void Update () {
        print(transform.position);
        float dist = (Time.time - starttime) * spd;
        float frac = dist / journeylength;
        transform.position = Vector3.Lerp(startmarker.position, endmarker.position, frac);
	}
}

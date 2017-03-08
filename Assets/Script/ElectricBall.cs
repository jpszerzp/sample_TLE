using UnityEngine;
using System.Collections;

public class ElectricBall : MonoBehaviour {

	float lifeSpan = 4f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (lifeSpan < 0f) {
			Destroy (this.gameObject);
		}

		lifeSpan -= Time.deltaTime;
	}
}

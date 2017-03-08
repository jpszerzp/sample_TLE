using UnityEngine;
using System.Collections;

public class Bullets : MonoBehaviour {

	float spd = 2f;
    Rigidbody2D rb2d;
    float guaranteedDestroyed = 10f;

	Vector2 dir = new Vector2(-1, 0);

	public Vector2 Dir
    {
        get
        {
            return dir;
        }
        set
        {
            dir = value;
        }
    }

    // Use this for initialization
    void Start () {
        rb2d = GetComponent<Rigidbody2D>();

	}
	
	// Update is called once per frame
	void Update () {
        guaranteedDestroyed -= Time.deltaTime;
		rb2d.velocity = dir * spd;

        if (guaranteedDestroyed < 0)
        {
            Destroy(gameObject);
        }
	}

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            Destroy(gameObject);
        }
    }
		
}

using UnityEngine;
using System.Collections;

public class Rocket : MonoBehaviour {

    public GameObject groundspot;

    GameObject spotclone;
    float scale = 20f;
    const float minSky = -10f;
    const float maxSky = 10f;
    const float minFloor = -12.5f;
    const float maxFloor = 12.5f;
    readonly Vector3 antiGravity = new Vector3(0, 9.8f, 0);
    readonly Vector3 forwardComparison = new Vector3(-1, 0f, 0f);
    readonly Vector3 adjustment = new Vector3(0.45f, 0, 0);
    Vector3 rocketDir;
    Vector3 netDir;
    Vector3 skyStart;
    Vector3 groundEnd;
    float countdown = -10f;
    float guaranteeDeathCountdown = 20f;
    bool repositioned = false;
    float deviation = 0;

    Rigidbody2D rb2d;
    Animator anim;

    // Use this for initialization
    void Start () {
        rb2d = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
//        rocketDir = new Vector3 (0.81f, 1.5f, 0);    // initial direction in vector
//        deviation = AngleWithSign(forwardComparison, rocketDir);    // initial direction in deviation (in degree)
//        transform.Rotate(new Vector3 (0, 0, deviation));
//        netDir = Vector3.Normalize(rocketDir) * scale + antiGravity;

        // position of groundspot not known yet
        spotclone = Instantiate(groundspot, Vector3.zero, new Quaternion(0, 0, 0, 0)) as GameObject;  

        Random.seed = (int)System.DateTime.Now.Ticks;
    }

	void AdjustParams (bool flipped) {
		if (!flipped) {
			rocketDir = new Vector3 (0.81f, 1.5f, 0f);    // initial direction in vector
		} else {
			rocketDir = new Vector3 (-0.81f, 1.5f, 0f);
		}
		deviation = AngleWithSign (forwardComparison, rocketDir);    // initial direction in deviation (in degree)
		transform.Rotate (new Vector3 (0, 0, deviation));
		netDir = Vector3.Normalize (rocketDir) * scale + antiGravity;
	}
	
	// Update is called once per frame
	void Update () {
        if (countdown > 0)
        {
            countdown -= Time.deltaTime;
            //print(countdown);
        }

        else if (countdown <= 0 && countdown > -10f)
        {
            // Starts firing
            rb2d.AddForce(netDir, ForceMode2D.Force);
        }

        if (guaranteeDeathCountdown > 0)
        {
            guaranteeDeathCountdown -= Time.deltaTime;
        }
        else
        {
            Destroy(gameObject);
            spotclone.SendMessage("DestroyExplosion");
        }
	}

    // Physics updates 
    void FixedUpdate()
    {
        // Once rocket goes above 8.5 and beyond scene (when players cannot see it),
        // reset position and direction, stop for fixed amount of time, play hint animation
        // during the time, and fire in high speed 
        if (transform.position.y > 8.5f)
        {
            if (!repositioned)
            {
                rb2d.velocity = Vector3.zero;
                rb2d.angularVelocity = 0f;

                float rndSky = Random.Range(minSky, maxSky);
                float rndFloor = Random.Range(minFloor, maxFloor);
                skyStart = new Vector3(rndSky, 15f, 0f);
                groundEnd = new Vector3(rndFloor, -6.129552f, 0f);
                rocketDir = groundEnd - skyStart;
                float ang = AngleWithSign(forwardComparison, rocketDir);
                transform.position = skyStart;
                deviation = -deviation + ang;
                //print(-deviation + ang);
                transform.Rotate(new Vector3(0, 0, deviation));
                scale = 100f;

                netDir = Vector3.Normalize(rocketDir) * scale + antiGravity;

                countdown = 2f;
                repositioned = true;

                rb2d.AddForce(antiGravity, ForceMode2D.Force);

                // As soon as we know rokcet's drop point, set explosion/hint position through instance of GroundSpot
                spotclone.SendMessage("ResetPosition", groundEnd);
                spotclone.SendMessage("AdjustX", Vector3.Normalize(rocketDir).x);
            }
            else
            {
                rb2d.AddForce(antiGravity, ForceMode2D.Force);
                //rb2d.velocity = Vector3.zero;
                //rb2d.angularVelocity = 0f;
            }
        }

        else
        {
            //print(Vector3.Normalize(rocketDir) * scale);
            rb2d.AddForce(netDir, ForceMode2D.Force);
        }
    }

    float AngleWithSign(Vector3 from, Vector3 to)
    {
        float ang = Vector3.Angle(from, to);
        ang = (to.y >= 0) ? -ang : ang;
        return ang;
    }

    /*
    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "Ground" || col.gameObject.tag == "Player")
        {
            // Notify to play explosion animation
            spotclone.SendMessage("Explode");
            spotclone.SendMessage("DestroyExplosion");

            Destroy(gameObject);
        }
    }
    */

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Ground" || other.gameObject.tag == "Player")
        {
            // Notify to play explosion animation
            spotclone.SendMessage("Explode");
            spotclone.SendMessage("DestroyExplosion");

            Destroy(gameObject);
        }
    }

    /*
    public void ReverseModel()
    {
        rocketDir = new Vector3(-0.81f, 1.5f, 0);
        deviation = AngleWithSign(forwardComparison, rocketDir);
        transform.Rotate(new Vector3(0, 0, deviation));
        netDir = Vector3.Normalize(rocketDir) * scale + antiGravity;
    }
    */
}

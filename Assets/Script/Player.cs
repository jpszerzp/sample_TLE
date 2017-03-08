using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	const float BULLET_OFFSET_X = 0.5f;
	const float BULLET_OFFSET_Y = 0.5f;

//	Vector3 RIGHT_ANGLE = new Vector3 (0f, 0f, 180f);
//	Vector3 LEFT_ANGLE = new Vector3 (0f, 0f, 0f);
//	Vector3 UP_ANGLE = new Vector3 (0f, 0f, 90f);
//	Vector3 DOWN_ANGLE = new Vector3 (0f, 0f, -90f);
//	Vector3 UP_RIGHT_ANGLE = new Vector3 (0f, 0f, 135f);
//	Vector3 UP_LEFT_ANGLE = new Vector3 (0f, 0f, 45f);
//	Vector3 DOWN_LEFT_ANGLE = new Vector3 (0f, 0f, -45f);
//	Vector3 DOWN_RIGHT_ANGLE = new Vector3 (0f, 0f, -135f);

	Vector3 RIGHT_DIR = new Vector3 (1.4f, 0, 0);
	Vector3 LEFT_DIR = new Vector3 (-1.4f, 0, 0);
	Vector3 UP_DIR = new Vector3 (0, 1.4f, 0);
	Vector3 DOWN_DIR = new Vector3 (0, -1.4f, 0);
	Vector3 UP_RIGHT_DIR = new Vector3 (1f, 1f, 0);
	Vector3 UP_LEFT_DIR = new Vector3 (-1f, 1f, 0);
	Vector3 DOWN_LEFT_DIR = new Vector3 (-1f, -1f, 0);
	Vector3 DOWN_RIGHT_DIR = new Vector3 (1f, -1f, 0);

    public float maxSpeed = 10f;
    public float jumpforce = 400f;
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask whatIsGround;

	//bool jumping = false;
    bool grounded;
    Rigidbody2D rb2d;
	//Animator anim;

	public GameObject bullet;
	float move = 0f;

	readonly Vector3 LEFT = new Vector3 (-1, 0, 0);
	readonly Vector3 RIGHT = new Vector3 (1, 0, 0); 

	// Bullet offsets and directions
	float offsetX = 0f;
	float offsetY = 0f;
	Vector3 direction = new Vector3 (0f, 0f, 0f);
	float angle = 0f;
	Quaternion dirQuaternion = new Quaternion (0f, 0f, 0f, 0f);

	bool firing = false;

	// Use this for initialization
	void Start () {
        rb2d = GetComponent<Rigidbody2D>();
		//anim = GetComponent<Animator> ();
	}
	
	// FixedUpdate
	void FixedUpdate () {
		grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, whatIsGround);
		//anim.SetBool ("Ground", grounded);

		//anim.SetFloat ("vSpeed", rb2d.velocity.y);

        move = Input.GetAxis("Horizontal");
        rb2d.velocity = new Vector2(move * maxSpeed, rb2d.velocity.y);

        if (move > 0 && transform.localScale.x > 0)
        {
            Flip ();
        }
        else if (move < 0 && transform.localScale.x < 0)
        {
            Flip ();
        }

		//Debug.Log ("move variable from input is: " + move);

//		if (move != 0) {
//			//anim.SetBool ("running", true);
//		} else {
//			//anim.SetBool ("running", false);
//		}
	}

    void Update() {
        // y of ground is -6.129552f
        /*
        if (grounded)
        {
            print(groundCheck.position.y);
        }
        */

		if (grounded && Input.GetKeyDown (KeyCode.Space)) {
			//anim.SetBool ("Ground", false);
			rb2d.AddForce (new Vector2 (0, jumpforce));
		}

		if (Input.GetKeyDown (KeyCode.J)) {
			//anim.SetTrigger ("Atk");
	
			offsetX = transform.localScale.x > 0 ? -BULLET_OFFSET_X : BULLET_OFFSET_X;
			offsetY = 0f;
			direction = transform.localScale.x > 0 ? LEFT_DIR : RIGHT_DIR;
			AdjustAngle (direction);
			AdjustQuat (angle);

			firing = true;
		}

		if (Input.GetKey (KeyCode.W)) {
			// Up and right
			if (move > 0) {
				OffsetAndDirection (BULLET_OFFSET_X, BULLET_OFFSET_Y, UP_RIGHT_DIR);
				AdjustAngle (direction);
				AdjustQuat (angle);
			}
			// Up and left
			else if (move < 0) {
				OffsetAndDirection (-BULLET_OFFSET_X, BULLET_OFFSET_Y, UP_LEFT_DIR);
				AdjustAngle (direction);
				AdjustQuat (angle);
			} 
			// Up
			else {
				OffsetAndDirection (0, BULLET_OFFSET_Y, UP_DIR);
				AdjustAngle (direction);
				AdjustQuat (angle);
			}
		} else if (Input.GetKey (KeyCode.S)) {
			// Down and right
			if (move > 0) {
				OffsetAndDirection (BULLET_OFFSET_X, -BULLET_OFFSET_Y, DOWN_RIGHT_DIR);
				AdjustAngle (direction);
				AdjustQuat (angle);
			}
			// Down and left
			else if (move < 0) {
				OffsetAndDirection (-BULLET_OFFSET_X, -BULLET_OFFSET_Y, DOWN_LEFT_DIR);
				AdjustAngle (direction);
				AdjustQuat (angle);
			}
			// Down only
			else {
				OffsetAndDirection (0, -BULLET_OFFSET_Y, DOWN_DIR);
				AdjustAngle (direction);
				AdjustQuat (angle);
			}
		}

		if (firing) {
			GameObject bltClone = Instantiate (bullet, transform.position + new Vector3(offsetX, offsetY, 0f), dirQuaternion) as GameObject;
			bltClone.GetComponent<Bullet> ().Dir = direction;
		}

		firing = false;
    }

    // Flip
    void Flip() {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

	GameObject SpawnBullet (float bulletOffsetX, float bulletOffsetY, Quaternion dirQuat) {
		GameObject bulletClone = Instantiate (bullet, transform.position + new Vector3(bulletOffsetX, bulletOffsetY, 0f), dirQuat) as GameObject;
		//Instantiate (bullet, transform.position + new Vector3(bulletOffsetX, bulletOffsetY, 0f), dirQuat);
		return bulletClone;
	}

	void OffsetAndDirection (float ofstX, float ofstY, Vector3 dir) {
		offsetX = ofstX;
		offsetY = ofstY;
		direction = dir;
	}

	void AdjustAngle (Vector3 dir) {
		angle = Vector3.Angle (LEFT, dir);
	}

	void AdjustQuat (float ang) {
		dirQuaternion = Quaternion.Euler (new Vector3(0, 0, ang));
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum State {
	// In order
	Judge,
	Attack,
	Idle,
	Roam
	//Complete    // test only
}

public enum Attack {
	Bullet,
	Rocket,
	Lazer,
	//Smash,
	Discharge
}

public enum Stage {
	Healthy,    // Smash (long), Bullet (mid), Discharge (close)
	Damaged,    // Smash (minor, long), Bullet (long), Rocket (mid), Discharge (close)
	Dying       // Smash (minor, long), Bullet (minor, long), Lazer (long), Rocket (mid), Discharge (close)
}

public enum Range {
	Far,
	Close
}

public class NewMidboss : MonoBehaviour {

	// Constants
	const float HEALTHY_BULLET_DELAY = 4.5f;
	const float DMGED_BULLET_DELAY = 7f;

	const float HEALTHY_DISCHARGE_DELAY = 5f;
	const float DMGED_DISCHARGE_DELAY = 5f;
	const float DYING_DISCHARGE_DELAY = 5f;

	const float DMGED_ROCKET_DELAY = 4f;
	const float DYING_ROCKET_DELAY = 22f;

	const float LAZER_DELAY = 8f;
	//const float SMASH_DELAY = 5f;

	const float HEALTHY_DISCHARGE_THRESHOLD = 6.5f;
	const float HEALTHY_DISCHARGE_THRESHOLD_X = 4.5f;
	const float DMGED_DISCHARGE_THRESHOLD = 6.5f;
	const float DMGED_DISCHARGE_THRESHOLD_X = 5f;
	const float DYING_DISCHARGE_THRESHOLD = 6.5f;
	const float DYING_DISCHARGE_THRESHOLD_X = 4f;

	const float DYING_ROCKET_OVERLAP_THRESHOLD = 7f;

	const float CLOSE_AND_FAR_BOUND_X = 10f;
	const float CLOSE_AND_FAR_BOUND_Y = 8.5f;

	Vector3 BulletOffset;

	readonly Vector3 DMGED_BULLET_LEFT_START = new Vector3 (-11.49f, 2.49f, 0f);
	readonly Vector3 DMGED_BULLET_RIGHT_START = new Vector3 (11.44f, 2.67f, 0f);

	readonly Vector3 DYING_LAZER_LEFT_START = new Vector3 (-6.37f, -2.5f, 0f);
	readonly Vector3 DYING_LAZER_RIGHT_START = new Vector3 (6.37f, -2.5f, 0f);

	const float RAND_ROAM_EFFECTIVE_UPPERBOUND = 6.21f;
	const float RAND_ROAM_EFFECTIVE_LOWERBOUND = -0.6f;

	readonly Vector3 FIXED_BULLET_OFFSET_LEFT = new Vector3 (-1f, -0.72f, 0f);
	readonly Vector3 FIXED_BULLET_OFFSET_RIGHT = new Vector3 (1f, -0.72f, 0f);
	readonly Vector3 MOVING_BULLET_OFFSET_LEFT = new Vector3 (-0.5f, 0f, 0f);
	readonly Vector3 MOVING_BULLET_OFFSET_RIGHT = new Vector3 (0.5f, 0f, 0f);

	Vector3 roamDest;
	Vector3 lastRoamDest;
	readonly Vector3 INITIAL_VEC = new Vector3 (0, 0, 0);
	List<Vector3> predictableSpot = new List<Vector3>();
	readonly Vector3 HEALTHY_PREDICTABLE_1 = new Vector3 (-10.15f, -0.18f, 0f);
	readonly Vector3 HEALTHY_PREDICTABLE_2 = new Vector3 (-5.47f, 0.8f, 0f);
	readonly Vector3 HEALTHY_PREDICTABLE_3 = new Vector3 (0f, 3.44f, 0f);
	readonly Vector3 HEALTHY_PREDICTABLE_4 = new Vector3 (4.96f, 0.8f, 0f);
	readonly Vector3 HEALTHY_PREDICTABLE_5 = new Vector3 (9.82f, -1.18f, 0f);

	const float HEALTHY_IDLETIME = 3f;
	const float DMGED_IDLETIME = 1f;
	const float DYING_IDLETIME = 2f;

	public Attack atk;
	bool attacking = false;
	public bool Attacking { 
		get { return attacking; } 
		set { attacking = value; } 
	}
	Stage healthStage = Stage.Healthy;
	Range rangeX;
	Range rangeY;
	State stat = State.Judge;
	public State Stat {
		get { return stat; }
		set { stat = value; }
	}

	float hp = 210f;	// PRE: hp is multiple of 70
	readonly Vector3 LEFT = new Vector3 (-1, 0, 0);
	readonly Vector3 RIGHT = new Vector3 (1, 0, 0); 

	public GameObject player;
	public GameObject bullets;
	public GameObject rocket;
	float xDisplacementToPlayer;
	public float XDisplacementToPlayer {
		get { return xDisplacementToPlayer; }
		set { xDisplacementToPlayer = value; }
	}
	Animator anim;

	public GameObject lightning;
	Animator lightningAnim;

	public GameObject lazer;
	Animator lazerAnim;

	public GameObject electricBall;
	//Animator electricBallAnim;
	int sideCount = 1;

	float floatYAmp = 0.5f;
	float floatYAnchor;
	public float FloatYAnchor {
		get { return floatYAnchor; }
		set { floatYAnchor = value; }
	}

	bool faceLeft = true;

	bool movingLeft;

	bool flipRocket = false;

	bool lazerRoaming = false;

	bool rocketFirst = true;

	Vector3 dyingRocketCloseOverlap = new Vector3 (0, 0, 0);
	Vector3 dyingRocketDest = new Vector3 (0, 0, 0);

	bool dead = false;
	public bool Dead {
		get { return dead; }
		set { dead = value; }
	}
	float allowDestroy = 2f;

	bool healthSwitch = false;

	Rigidbody2D rb2d;
	CircleCollider2D circleCol;

	void Start () {
		anim = GetComponent<Animator> ();
		lightningAnim = lightning.GetComponent<Animator> ();
		lazerAnim = lazer.GetComponent<Animator> ();
		rb2d = GetComponent<Rigidbody2D> ();
		circleCol = GetComponent<CircleCollider2D> ();
		//electricBallAnim = electricBall.GetComponent<Animator> ();

		roamDest = INITIAL_VEC;
		PopulatePredictableSpot ();

		Random.seed = (int)System.DateTime.Now.Ticks;
	}
		
	void Update() {
//		if (Input.GetKeyDown (KeyCode.A)) {
//			AdvanceHealthState ();
//		}

		Debug.Log ("hp is " + hp);

		if (dead) {
			allowDestroy -= Time.deltaTime;
			rb2d.isKinematic = false;
			circleCol.isTrigger = true;

			if (allowDestroy < 0f) {
				Destroy (this.gameObject);
			}
		} else {
			if (healthSwitch) {
				AdvanceHealthState ();
			}
//			if (hp == 140f && healthStage == Stage.Healthy) {
//				healthStage = Stage.Damaged;
//			} else if (hp == 50f && healthStage == Stage.Damaged) {
//				healthStage = Stage.Dying;
//			} else if (hp == 0f && healthStage == Stage.Dying) {
//				dead = true;
//				anim.SetTrigger ("dead");
//			}

			xDisplacementToPlayer = gameObject.transform.position.x - player.transform.position.x;

			if (!attacking) {
				//			if (faceLeft && xDisplacementToPlayer < 0) {
				//				if (transform.localScale.x > 0) {
				//					// default flip
				//					Flip ();
				//				} else {
				//					ReverseFlip ();
				//				}
				//			}
				//
				//			// entered from left to right while facing left 
				//			if (!faceLeft && xDisplacementToPlayer > 0) {
				//				if (transform.localScale.x < 0) {
				//					Flip ();
				//				} else {
				//					// default reverse flip
				//					ReverseFlip ();
				//				}
				//			}

				if ((faceLeft && xDisplacementToPlayer < 0) ||
					(!faceLeft && xDisplacementToPlayer > 0)) {
					Flip ();
				}
			} else {
				if ((atk == Attack.Bullet) || (atk == Attack.Lazer && lazerRoaming)) {
					if ((faceLeft && xDisplacementToPlayer < 0) || (!faceLeft && xDisplacementToPlayer > 0)) {
						//FlipLocal (true);
						Flip();
					}
				}
			}	
		}
	}

	// Returns next attack phase based on distance vector from boss to player
	public void Judge () {

		switch (healthStage) {
		case Stage.Healthy:
			// Possible attacks are: Bullet, Smash, Discharge
			// TODO: encapsulate a function for healthy atk
			if (Mathf.Abs(transform.position.x - player.transform.position.x) > HEALTHY_DISCHARGE_THRESHOLD_X) {
				atk = Attack.Bullet;
			} else {
				atk = Attack.Discharge;
			}
			break;

		case Stage.Damaged:
			RangeX ();
			if (Mathf.Abs(transform.position.x - player.transform.position.x) <= DMGED_DISCHARGE_THRESHOLD_X) {
				atk = Attack.Discharge;
			} else if (rangeX == Range.Close) {
				//atk = Attack.Bullet;
				atk = Attack.Rocket;
			} else {
				atk = Attack.Bullet;
			}
			break;

		case Stage.Dying:
			// Possible attacks are: +Lazer
			RangeX ();
			if (Mathf.Abs(transform.position.x - player.transform.position.x) <= DYING_DISCHARGE_THRESHOLD_X) {
				atk = Attack.Discharge;
			} else if (rangeX == Range.Close) {
				atk = Attack.Lazer;
			} else {
				// 50-50 chance to pick rocket or discharge
				atk = Random.Range (0, 2) == 0 ? Attack.Rocket : Attack.Discharge;
				//atk = Attack.Rocket;
			}
			break;

		default:
			Debug.LogError ("Health stage ambiguous.");
			break;
		}

		Debug.Log ("Decided to use " + atk);
		//return atk;
	}
		
	/// <summary>
	/// shoot to player's location
	/// </summary>
	void Shoot () {
		Vector3 dir = player.transform.position - transform.position;

		if (healthStage == Stage.Healthy) {
			if (dir.x < 0) {
				// player on left of boss - boss should shoot left, so changes the offset
				BulletOffset = FIXED_BULLET_OFFSET_LEFT;
			} else {
				BulletOffset = FIXED_BULLET_OFFSET_RIGHT;
			}
		} else if (healthStage == Stage.Damaged) {
			if (movingLeft) {
				if (faceLeft) {
					BulletOffset = FIXED_BULLET_OFFSET_LEFT + MOVING_BULLET_OFFSET_LEFT;
				} else {
					BulletOffset = FIXED_BULLET_OFFSET_RIGHT + MOVING_BULLET_OFFSET_LEFT;
				}
			} else {
				if (faceLeft) {
					BulletOffset = FIXED_BULLET_OFFSET_LEFT + MOVING_BULLET_OFFSET_RIGHT;
				} else {
					BulletOffset = FIXED_BULLET_OFFSET_RIGHT + MOVING_BULLET_OFFSET_RIGHT;
				}
			}
		}

		float angle = Vector3.Angle (LEFT, dir);
		Quaternion dirQuat = Quaternion.Euler (new Vector3(0,0,angle));
		GameObject bltClone = Instantiate (bullets, transform.position + BulletOffset, dirQuat) as GameObject;
		bltClone.GetComponent<Bullets> ().Dir = dir;
	}

	void Launch (bool flipRocket) {
			GameObject rocketClone = Instantiate (rocket, transform.position, new Quaternion (0, 0, 0, 0)) as GameObject;
			rocketClone.SendMessageUpwards ("AdjustParams", flipRocket);
	}

	public void RangeX () {
		float distanceX = Mathf.Abs (player.transform.position.x - transform.position.x);
		if (distanceX <= CLOSE_AND_FAR_BOUND_X) {
			rangeX = Range.Close;
		} else {
			rangeX = Range.Far;
		}
	}

	public void RangeY () {
		float distanceY = Mathf.Abs (player.transform.position.y - transform.position.y);
		if (distanceY <= CLOSE_AND_FAR_BOUND_Y) {
			rangeY = Range.Close;
		} else {
			rangeY = Range.Far;
		}
	}

	public void AttackAnim (Attack atk) {
		switch (atk) {
		case Attack.Bullet:
			switch (healthStage) {
			case Stage.Healthy:
//				if ((xDisplacementToPlayer < 0 && transform.localScale.x > 0)||
//					(xDisplacementToPlayer > 0 && transform.localScale.x < 0)) {
//					FlipLocal (false);
//				}
				//anim.SetTrigger ("bltatk3");
				StartCoroutine (RepeatShoot (3));
				break;

			case Stage.Damaged:
				// TODO: pass-sides attack of bullets happens here 
				// goes to the side to which player is farther 
				Vector3 dest = player.transform.position.x > 0 ? DMGED_BULLET_LEFT_START : DMGED_BULLET_RIGHT_START;
				movingLeft = !(dest == DMGED_BULLET_LEFT_START);
				Vector3 reverseDest = dest == DMGED_BULLET_LEFT_START ? DMGED_BULLET_RIGHT_START : DMGED_BULLET_LEFT_START;

				StartCoroutine (DamagedBulletAtk (dest, reverseDest));

				// roam to destination - the side farther from player
				//StartCoroutine (RoamTo (transform.position, dest, 2));

				// roam to the other side after above roam is done
				//StartCoroutine (RoamTo (transform.position, reverseDest, 4));
				break;

			case Stage.Dying:
				// NOTHING; no bullet-attack during this phase
				break;

			default:
				Debug.Log ("Cannot decide on bullet attack since health is ambiguous.");
				break;
			}
			break;

		case Attack.Discharge:
			// flip is not allowed while discharging
			// separate case for different hp status of boss when discharge animation is accessible.
			switch (healthStage) {
			case Stage.Healthy:
				StartCoroutine(Discharge(false));
				break;
			case Stage.Damaged:
				StartCoroutine(DischargeDmged());
				break;
			case Stage.Dying:
				StartCoroutine (DischargeDying ());
				break;
			default:
				Debug.LogError ("hp stage ambiguous.");
				break;
			}
			break;

		case Attack.Rocket:
			switch (healthStage) {
			case Stage.Healthy:
				// NOTHING; no rocket-attack during this phase.
				break;
			case Stage.Damaged:
//				if ((xDisplacementToPlayer < 0 && transform.localScale.x > 0)||
//					(xDisplacementToPlayer > 0 && transform.localScale.x < 0)) {
//					//flipRocket = !flipRocket;
//					FlipLocal (false);
//				}

				// rocket-attack during dmg phase.
				// if boss faces left, do not flip rockets
				StartCoroutine (RepeatRocket(5, !faceLeft));
				break;
			case Stage.Dying:
				// TODO: rocket attack during dying phase.
				//Debug.Log("dying rocket");
				StartCoroutine(DyingRocket());

//				for (int i = 0; i < 4; i++) {
//					do {
//						dyingRocketDest = new Vector3 (Random.Range (-11f, 11f), 
//							Random.Range (RAND_ROAM_EFFECTIVE_LOWERBOUND, RAND_ROAM_EFFECTIVE_UPPERBOUND), 0f);
//					} while (Vector3.Distance (dyingRocketDest, dyingRocketCloseOverlap) < DYING_ROCKET_OVERLAP_THRESHOLD);
//					StartCoroutine (RoamTo (transform.position, dyingRocketDest, 1.5f, true));
//					//yield return new WaitForSeconds (1.5f);
//				}
				break;
			default:
				Debug.Log ("hp stage ambiguous");
				break;
			}
			break;

		case Attack.Lazer:
			switch (healthStage) {
			case Stage.Healthy:
			case Stage.Damaged:
				// no lazer at these two stages
				break;
			case Stage.Dying:
				// TODO: lazer attack at dying stage
				//Vector3 lazerPos = player.transform.position.x > 0 ? DYING_LAZER_LEFT_START : DYING_LAZER_RIGHT_START;
				//Debug.Log ("dying lazer");
				lazerRoaming = true;
				StartCoroutine(Lazer());
				break;
			default:
				Debug.Log ("hp stage ambiguous");
				break;
			}
			break;

			/*
		case Attack.Smash:
			// TODO: Smash animation
			break;
			*/

		default:
			Debug.LogError ("Attack form not confirmed.");
			break;
		}
	}

	IEnumerator Lazer () {
		Vector3 lazerPos = player.transform.position.x > 0 ? DYING_LAZER_LEFT_START : DYING_LAZER_RIGHT_START;
		StartCoroutine (RoamTo(transform.position, lazerPos, 2f, true));
		yield return new WaitForSeconds (2f);

		// start spawning lazer
		//Debug.Log("start spawning lazer");
		anim.SetTrigger("lazerstart");
		yield return new WaitForSeconds (0.7f);

		// local scale and position should already be adjusted in editor

		lazerAnim.SetTrigger ("lazerspawn");
	}

	/*
	public void Complete () {
		Debug.Log ("Completion test just started.");
	}
	*/

	IEnumerator DyingRocket() {
//		Vector3 closeOverlap = new Vector3(0f, 0f, 0f); 
//		Vector3 dest;
		for (int i = 0; i < 4; i++) {
			do {
				dyingRocketDest = new Vector3 (Random.Range (-11f, 11f), 
					Random.Range (RAND_ROAM_EFFECTIVE_LOWERBOUND, RAND_ROAM_EFFECTIVE_UPPERBOUND), 0f);
			} while (Vector3.Distance (dyingRocketDest, dyingRocketCloseOverlap) < DYING_ROCKET_OVERLAP_THRESHOLD);
			StartCoroutine (RoamTo (transform.position, dyingRocketDest, 1.5f, true));
			yield return new WaitForSeconds (5.5f);

			// not the first series of rocket attack for this 4-times streak anymore
			if (i == 0) {
				rocketFirst = false;
			}
		}
	}

	/// <summary>
	/// Boss's discharge attack
	/// </summary>
	/// <param name="ballReleased">If set to <c>true</c> electrical balls will be spawned.</param>
	IEnumerator Discharge (bool ballReleased) {
		StartCoroutine (RoamTo (transform.position, new Vector3 (player.transform.position.x, -0.5f, 
			player.transform.position.z), 1f, true));
		yield return new WaitForSeconds (1f);

//		if ((xDisplacementToPlayer < 0 && transform.localScale.x > 0)||
//			(xDisplacementToPlayer > 0 && transform.localScale.x < 0)) {
//			//flipRocket = !flipRocket;
//			FlipLocal (true);
//		}

		if (!ballReleased) {
			anim.SetTrigger ("discharge");
			yield return new WaitForSeconds (0.5f);
			lightningAnim.SetTrigger ("lightningspawn");
		}
	}

	IEnumerator DischargeDmged () {
		StartCoroutine (RoamTo (transform.position, new Vector3 (player.transform.position.x, 0f, 
			player.transform.position.z), 1f, true));
		yield return new WaitForSeconds (1f);

		anim.SetTrigger ("discharge");
		yield return new WaitForSeconds (0.5f);

		// stretch lightning to match new height of boss and change local position accordingly
		Transform lightningTransform = lightningAnim.gameObject.transform;
		lightningTransform.localScale = new Vector3 (lightningTransform.localScale.x, 5f, lightningTransform.localScale.z);
		lightningTransform.localPosition = new Vector3 (lightningTransform.localPosition.x, -9.7f, lightningTransform.localPosition.z);

		lightningAnim.SetTrigger ("lightningspawn");

		// introduce 1s delay ahead of electric ball
		yield return new WaitForSeconds (1f);

		for (int i = 0; i < 4; i++) {
			GameObject eBall = Instantiate (electricBall, GameObject.Find ("Lightning").transform, false) as GameObject;
//			GameObject eBall = Instantiate (electricBall, new Vector3 (0,0,0), new Quaternion (0,0,0,0)) as GameObject;
//			eBall.transform.parent = GameObject.Find ("Lightning").transform;
			float randx = Random.Range (0f, 2f);
			float randy = Random.Range (-1.3f, 0f);
			randx = sideCount * randx;
			sideCount *= -1;
			eBall.transform.localPosition = new Vector3 (randx, randy, 0f);
			eBall.transform.localScale = new Vector3 (1.5f, 0.8f, 0.5f);
			eBall.GetComponent<Animator> ().SetTrigger ("discharge");

			// delay between balls
			yield return new WaitForSeconds (0.3f);
		}
	}

	IEnumerator DischargeDying() {
		StartCoroutine (RoamTo (transform.position, new Vector3 (player.transform.position.x, 2.3f, 
			player.transform.position.z), 1f, true));
		yield return new WaitForSeconds (1f);

		anim.SetTrigger ("discharge");
		yield return new WaitForSeconds (0.5f);

		Transform lightningTransform = lightningAnim.gameObject.transform;
		lightningTransform.localScale = new Vector3 (lightningTransform.localScale.x, 7f, lightningTransform.localScale.z);
		lightningTransform.localPosition = new Vector3 (lightningTransform.localPosition.x, -12.5f, lightningTransform.localPosition.z);

		lightningAnim.SetTrigger ("lightningspawn");

		yield return new WaitForSeconds (1f);

		for (int i = 0; i < 8; i++) {
			GameObject eBall = Instantiate (electricBall, GameObject.Find ("Lightning").transform, false) as GameObject;
			float randx = Random.Range (0f, 4f);
			float randy = Random.Range (-1.3f, 0.5f);
			randx = sideCount * randx;
			sideCount *= -1;
			eBall.transform.localPosition = new Vector3 (randx, randy, 0f);
			eBall.transform.localScale = new Vector3 (1.5f, 0.5f, 0.5f);
			eBall.GetComponent<Animator> ().SetTrigger ("discharge");

			// delay between balls
			yield return new WaitForSeconds (0.3f);
		}
	}

	public float GetAtkTime (Attack atk) {
		float atkTime = 0f;

		switch (atk) {
		case Attack.Bullet:
			switch (healthStage) {
			case Stage.Healthy:
				atkTime = HEALTHY_BULLET_DELAY;
				break;
			case Stage.Damaged:
				atkTime = DMGED_BULLET_DELAY;
				break;
			case Stage.Dying:
				// no bullet attack during dying phase
				break;
			default:
				Debug.Log ("hp stage ambiguous.");
				break;
			}
			break;

		case Attack.Discharge:
			switch (healthStage) {
			case Stage.Healthy:
				atkTime = HEALTHY_DISCHARGE_DELAY;
				break;

			case Stage.Damaged:
				atkTime = DMGED_DISCHARGE_DELAY;
				break;

			case Stage.Dying:
				atkTime = DYING_DISCHARGE_THRESHOLD;
				break;

			default:
				Debug.Log ("hp stage ambiguous.");
				break;
			}
			break;

		case Attack.Rocket:
			switch (healthStage) {
			case Stage.Healthy:
				// no rocket-attack when healthy
				break;
			case Stage.Damaged:
				atkTime = DMGED_ROCKET_DELAY;
				break;
			case Stage.Dying:
				atkTime = DYING_ROCKET_DELAY;
				break;
			default:
				Debug.Log ("hp stage ambiguous.");
				break;
			}
			break;

		case Attack.Lazer:
			switch (healthStage) {
			case Stage.Healthy:
				//no lazer in healthy stage
			    break;
			case Stage.Damaged:
				// no lazer in damaged stage
				break;
			case Stage.Dying:
				atkTime = LAZER_DELAY;
				break;
			default:
				Debug.Log ("hp stage ambiguous");
				break;
			}
			break;

			/*
		case Attack.Smash:
			atkTime = SMASH_DELAY;
			break;
			*/

		default:
			Debug.LogError ("Attack form ambiguous!");
			break;
		}

		return atkTime;
	}

	public void AtkTakeback (Attack atk) {
		switch (atk) {
		case Attack.Rocket:
			anim.SetTrigger ("rockettakeback");
			rocketFirst = true;
			break;

		case Attack.Discharge:
			anim.SetTrigger ("dischargetakeback");
			break;

		case Attack.Bullet:
			Debug.Log ("take back animation not confirmed.");
			break;

		case Attack.Lazer:
			anim.SetTrigger ("lazertakeback");
			break;

		default:
			Debug.Log ("atk form ambiguous");
			break;
		}
	}

	public float GetIdleTime () {
		float time = 0f;
		switch (healthStage) {
		case Stage.Healthy:
			time = HEALTHY_IDLETIME;
			break;
		case Stage.Damaged:
			time = DMGED_IDLETIME;
			break;
		case Stage.Dying:
			time = DYING_IDLETIME;
			break;
		default:
			Debug.Log ("hp stage ambiguous.");
			break;
		}
		return time;
	}

	IEnumerator RepeatShoot (int count) {
		anim.SetTrigger ("bltatk" + count);
		for (int i = 0; i < count; i++) {
			yield return new WaitForSeconds (1);
			Shoot ();
		}
	}

	IEnumerator RepeatRocket (int count, bool flipRocket) {
		if (rocketFirst) {
			anim.SetTrigger ("rocketstart");
		}
		for (int i = 0; i < count; i++) {
			if (i == 0) {
				yield return new WaitForSeconds (1);
			} else {
				yield return new WaitForSeconds (0.5f);
			}
			Launch (flipRocket);
		}
	}

	public IEnumerator RoamTo(Vector3 startpos, Vector3 endpos, float sec, bool smooth)
	{
		float time = 0f;

		while (time < 1f)
		{
			time += Time.deltaTime / sec;
			transform.position = smooth ? Vector3.Lerp(startpos, endpos, Mathf.SmoothStep(0, 1, Mathf.SmoothStep(0, 1, time))) 
				: Vector3.Lerp(startpos, endpos, time);
			if (atk == Attack.Rocket) {
				if ((faceLeft && xDisplacementToPlayer < 0) || (!faceLeft && xDisplacementToPlayer > 0)) {
					//FlipLocal (true);
					Flip ();
				}
			}
			yield return null;
		}

		// flag roam of lazer atk to be done
		if (atk == Attack.Lazer) {
			lazerRoaming = false;
		}
			
		if (atk == Attack.Rocket && attacking) {
			dyingRocketCloseOverlap = dyingRocketDest;

			StartCoroutine(RepeatRocket (4, !faceLeft));
		}
	}

	public void Idle () {
		float temp = floatYAnchor + floatYAmp * Mathf.Sin (2f * Time.time);
		transform.position = new Vector3 (transform.position.x, temp, transform.position.z);
	}

	public void Flip () {
		//anim.SetTrigger ("flip");
		Vector3 tempScale = transform.localScale;
		tempScale.x *= -1;
		transform.localScale = tempScale;
		faceLeft = !faceLeft;
	}
		
	public void ReverseFlip () {
		anim.SetTrigger ("reverseflip");
		faceLeft = !faceLeft;
	}

	/// <summary>
	/// flip horizontal local scale, flip left/right flag if necessary.
	/// </summary>
	/// <param name="flag">If set to <c>false</c> left/right flag will not be converted since it is already set to correct value.</param>
	public void FlipLocal (bool flag) {
		Vector3 tempScale = transform.localScale;
		tempScale.x *= -1;
		transform.localScale = tempScale;
		if (flag) {
			faceLeft = !faceLeft;
		}
	}

	public void Roam (){
		switch (healthStage) {
		case Stage.Healthy:
			lastRoamDest = roamDest;
			while (roamDest == lastRoamDest) {
				roamDest = predictableSpot [Random.Range (0, 5)];
			}

			//Debug.Log ("Roam destination just picked is x: " + roamDest.x + ", y: " + roamDest.y + ", z: " + roamDest.z);
			StartCoroutine (RoamTo (transform.position, roamDest, 2, true));
			break;

		case Stage.Damaged:
		case Stage.Dying:
			lastRoamDest = roamDest;
			while (roamDest == lastRoamDest) {
				roamDest = new Vector3 (Random.Range (DMGED_BULLET_LEFT_START.x, DMGED_BULLET_RIGHT_START.x), 
					Random.Range (RAND_ROAM_EFFECTIVE_LOWERBOUND, RAND_ROAM_EFFECTIVE_UPPERBOUND), 0f);
			}

			Debug.Log ("DMGED-roam destination just picked is x: " + roamDest.x + ", y: " + roamDest.y + ", z: " + roamDest.z);
			StartCoroutine (RoamTo (transform.position, roamDest, 2, true));
			break;

		default:
			Debug.Log ("hp stage ambiguous.");
			break;
		}
	}		

	void PopulatePredictableSpot () {
		predictableSpot.Add (HEALTHY_PREDICTABLE_1);
		predictableSpot.Add (HEALTHY_PREDICTABLE_2);
		predictableSpot.Add (HEALTHY_PREDICTABLE_3);
		predictableSpot.Add (HEALTHY_PREDICTABLE_4);
		predictableSpot.Add (HEALTHY_PREDICTABLE_5);
	}

	IEnumerator DamagedBulletAtk (Vector3 dest1, Vector3 dest2) {
		StartCoroutine (RoamTo (transform.position, dest1, 2, true));
		yield return new WaitForSeconds (2);

//		if ((xDisplacementToPlayer < 0 && transform.localScale.x > 0)||
//			(xDisplacementToPlayer > 0 && transform.localScale.x < 0)) {
//			FlipLocal (false);
//		}

		StartCoroutine (RoamTo (transform.position, dest2, 4, false));
		StartCoroutine (RepeatShoot (3));
		yield return null;
	} 

	void AdvanceHealthState () {
		Debug.Log ("Advancing health stage");

		if (healthStage == Stage.Dying) {
			dead = true;
			anim.SetTrigger ("dead");
		} else {
			healthStage = healthStage + 1;
		}

		if (!dead) {
			StopAllCoroutines ();
			StartCoroutine (RoamTo (transform.position, new Vector3 (0, 0, 0), 2f, true));	
		}

		healthSwitch = false;	// switch of health state is finished
	}

	public void TakeDamage () {
		Debug.Log ("Took damage");
		hp -= 10;
		if (hp == 140f || hp == 70f || hp == 0f) {
			healthSwitch = true;
		}
	}
}

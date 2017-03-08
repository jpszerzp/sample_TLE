using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MidbossManager : MonoBehaviour {

	float atkTime;
	float idleTime;
	const float JUDGE_TIME = 5f;
	//const float IDLE_TIME = 3f;
	const float ROAM_TIME = 2.5F;
	const float COMPLETE_TIME = 3f;

//	readonly Vector3 INITIAL_VEC = new Vector3 (0, 0, 0);
//	Vector3 roamDest;
//	Vector3 lastRoamDest;
//
//	List<Vector3> predictableSpot = new List<Vector3>();

	bool busy = false;
	float timeInState = 0f;

	Animator anim;
	public GameObject midBoss;
	NewMidboss mb;

//	readonly Vector3 HEALTHY_PREDICTABLE_1 = new Vector3 (-10.15f, -0.18f, 0f);
//	readonly Vector3 HEALTHY_PREDICTABLE_2 = new Vector3 (-5.47f, 1.54f, 0f);
//	readonly Vector3 HEALTHY_PREDICTABLE_3 = new Vector3 (0f, 3.44f, 0f);
//	readonly Vector3 HEALTHY_PREDICTABLE_4 = new Vector3 (4.96f, 1.78f, 0f);
//	readonly Vector3 HEALTHY_PREDICTABLE_5 = new Vector3 (9.82f, -1.18f, 0f);

	// Use this for initialization
	void Start () {
		anim = midBoss.GetComponent<Animator> ();
		mb = midBoss.GetComponent<NewMidboss> ();
//		roamDest = INITIAL_VEC;
//
//		PopulatePredictableSpot ();

		Random.seed = (int)System.DateTime.Now.Ticks;
	}
	
	// Update is called once per frame
	void Update () {
		if (!midBoss.GetComponent<NewMidboss> ().Dead) {
			switch (mb.Stat) {
			case State.Judge:
				// Judge behavior is finished momentarily
				mb.Judge ();
				Advance ();
				break;

			case State.Attack:
				if (!busy) {
					atkTime = mb.GetAtkTime (mb.atk);
					mb.AttackAnim (mb.atk);
					busy = true;
					mb.Attacking = true;
				} else {
					//Debug.Log ("In atk state for " + timeInState + " seconds.");
					if (timeInState > atkTime) {
						mb.AtkTakeback (mb.atk);
						busy = false;
						mb.Attacking = false;
						Advance ();
					}
				}
				break;

			case State.Idle:
				if (!busy) {
					busy = true;
					mb.FloatYAnchor = midBoss.transform.position.y;
					idleTime = mb.GetIdleTime ();
				} else {
					//Debug.Log ("In Idle state for " + timeInState + " seconds.");
					if (timeInState > idleTime) {
						busy = false;
						Advance ();
					}
				}
				mb.Idle ();
				break;

			case State.Roam:
				//Vector3 destination = INITIAL_VEC;
				if (!busy) {
					busy = true;

					// pick a predictable spot while healthy
					// the picked position is not same as the last pick - boss always changes spot.
					//				lastRoamDest = roamDest;
					//				while (roamDest == lastRoamDest) {
					//					roamDest = predictableSpot [Random.Range (0, 5)];
					//				}
					//
					//				Debug.Log ("Roam destination just picked is x: " + roamDest.x + ", y: " + roamDest.y + ", z: " + roamDest.z);
					mb.Roam ();
				}

				// roam to destination
				//StartCoroutine (mb.RoamTo (midBoss.transform.position, roamDest, 2));

				else {
					if (timeInState > ROAM_TIME) {
						Debug.Log ("Stayed in roam state for " + timeInState + " seconds.");
						busy = false;
						Advance ();
					}
				}
				break;

			default:
				Debug.LogError ("Boss state ambiguous.");
				break;
			}

			timeInState += Time.deltaTime;
		}
	}

	void Advance () {
		if (mb.Stat == State.Roam) {
			mb.Stat = State.Judge;
		} else {
			mb.Stat = mb.Stat + 1;
		}

		ResetTime ();
	}

	void ResetTime () {
		timeInState = 0f;
	}

//	void PopulatePredictableSpot () {
//		predictableSpot.Add (HEALTHY_PREDICTABLE_1);
//		predictableSpot.Add (HEALTHY_PREDICTABLE_2);
//		predictableSpot.Add (HEALTHY_PREDICTABLE_3);
//		predictableSpot.Add (HEALTHY_PREDICTABLE_4);
//		predictableSpot.Add (HEALTHY_PREDICTABLE_5);
//	}
}

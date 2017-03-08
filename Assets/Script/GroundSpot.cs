using UnityEngine;
using System.Collections;

public class GroundSpot : MonoBehaviour {

    Animator anim;
    float adjustX;
    bool destroy = false;
    float destroyCountdown = 1f;

	// Use this for initialization
	void Start () {
        anim = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
        if (destroy && destroyCountdown > 0)
        {
            destroyCountdown -= Time.deltaTime;
        }
        else if (destroy)
        {
            destroy = false;    // reset bool before destroying gameobject
            Destroy(gameObject);
        }
	}

    void Explode()
    {
        // Some adjustment to position for better appearance
        float newY = transform.position.y + 1.4f;
        float newX = transform.position.x + adjustX;
        transform.position = new Vector3(newX, newY, transform.position.z);

        anim.SetTrigger("explode");
    }

    void ResetPosition(Vector3 newPos)
    {
        transform.position = newPos;

        //print(transform.position);
    }

    void DestroyExplosion()
    {
        destroy = true;
    }

    void AdjustX(float dirX)
    {
        //print(dirX);
        int dir = (dirX >= 0) ? 1 : -1;
        float absDirX = Mathf.Abs(dirX);
        if (absDirX >= 0.1f && absDirX < 0.2f)
        {
            adjustX = dir * 0.1f;
        }

        else if (absDirX >= 0.2f && absDirX < 0.3f)
        {
            adjustX = dir * 0.2f;
        }

        else if (absDirX >= 0.3f && absDirX < 0.4f)
        {
            adjustX = dir * 0.5f;
        }

        else if (absDirX >= 0.4f && absDirX < 0.5f)
        {
            adjustX = dir * 1f;
        }

        else if (absDirX >= 0.5f && absDirX < 0.7f)
        {
            adjustX = dir * 2f;
        }

        else if (absDirX >= 0.7f && absDirX < 0.9f)
        {
            adjustX = dir * 3f;
        }
    }
}

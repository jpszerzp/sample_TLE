using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using System;

/**
 * Midboss position: 1. bullet (10.55, -4.5, 0) and its (nearly) symmetric position
 * 2. rocket: (0, 2.5, 0) 3. lazer: midboss's position is (11.5, -2.5, 0). 
 * Midboss's scale is always (0.4, 0.4, 0.4).
 */

public enum Segment
{
    Left,
    Middle,
    Right
};

public enum Status
{
    Healthy, Damaged, Dying
};

public class Midboss : MonoBehaviour {

    float idleRemaining = 5f;
    //float idleWithinLazerPeriod = 5f;

    private readonly Vector3 DOWNLEFTREC = new Vector3(-9, 0, 0);
    private readonly Vector3 STARTPOS = new Vector3(10.54f, -4.5f, 0);
    private readonly Vector3 ENDPOS = new Vector3(-9.2f, -4.5f, 0);
    private readonly Vector3 LAZERREADYLEFT = new Vector3(-9.2f, 3f, 0);
    private readonly Vector3 LAZERREADYRIGHT = new Vector3(10.54f, 3f, 0);
    private readonly int MIDX = 0;
    private readonly Vector3 UNREACHABLE = new Vector3(100, 100, 100);

    // bendings
    private Vector3 startpos = new Vector3(10.54f, -4.5f, 0);
    private Vector3 endpos = new Vector3(-9.2f, -4.5f, 0);
    float traveltime = 1.9f;    // tested value, need to match the real duration of coroutine as much as possible
    Vector3 bending = new Vector3(0, 3.5f, 0);
    Vector3 bendLeft = new Vector3(-1, 2, 0);
    Vector3 bendRight = new Vector3(1, 2, 0);
    Vector3 collideBendToLeft = new Vector3(2, -1, 0);
    Vector3 collideBendToRight = new Vector3(-2, -1, 0);
    //Vector3 bendRckToBltLeft = new Vector3(-1, 1, 0);
    int bendIndex = 0;

    // stats 
    float hp;
    private readonly float hpconst = 100f;
    private readonly float hpPercentage1 = 0.66f;
    private readonly float hpPercentage2 = 0.33f; 
    float spd = 15f;
    private readonly float spdIncrement = 1.5f;
    private Status stat = Status.Healthy;

    Animator anim;
    Animator lazerAnim;
    Rigidbody2D rb2d;

    float spawnacc = 0f;
    float lazeracc = 0f;
    float rocketacc = 0f;
    float rocketinterval = 0f;
    int firecount = 0;
    int lazercount = 0;
    int rocketcount = 0;
    int rnd;
    bool guard1 = true;
    bool guard2 = true;
    bool guard3 = true;
    bool guard4 = true;
    bool toRck = false;
    bool bulletFireGoingOn = false;
    float bendFlyCountDown = 0;
    float backtoBltCountDown = 0;
    int backAndForthCount = 0;

    // -1 is left, 1 is right. Separated from localscale in editor;
    // used to pass direction info to other components like bullets.
    public int dir = -1;
    public int Dir
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

    public GameObject rocket;
    public GameObject bullets;
    public GameObject bulletsReversed;
    public GameObject player;

    bool lazerProcessStart = false;
    bool guard5 = true;
    float lazerCountDown = 1f;
    bool guard6 = true;
    float randRocketCountDown = 8;
    bool guard7 = false;
    Vector3 rocketPos;
    bool NotYetUpdatedPos = true;
    //bool guard8 = true;
    bool NotYetTranslating = true;
    Vector3 lazerSpawnPos;
    bool idleStarted = true;

    List<Segment> segList = new List<Segment>();

    void Awake ()
    {
        /*
        if (Stage1.instance == null)
        {
            Instantiate(stage1Manager);
        }
        */
    }

    // Initialization
    void Start ()
    {
        hp = hpconst;

        anim = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();

        // assumption: lazer is always FIRST child of midboss gameobject
        lazerAnim = this.gameObject.transform.GetChild(0).GetComponent<Animator>();

        Populate(segList);

        Random.seed = (int)System.DateTime.Now.Ticks;
	}

    // Update is called once per frame
    void Update()
    {

        hp = 55;

        UpdateHealthStatus();

        // Pre: no fire pressed during a complete round of animation
        // Pseudo: boss halt for 2s before starts spawning bullets
        // 1. once fire pressed, start counting for spawn 
        // 2. when spawn is smaller than 2, continue accumulate it and commit no animation
        // 3. when it exceeds 2, trigger the spawn animation, set accumulator to 0
        // 4. after setting to 0, only start accumulate again when another fire is pressed, iterate 1, 2, and 3

        bool fireBullet = Input.GetKeyDown(KeyCode.J);
        bool fireLazer = Input.GetKeyDown(KeyCode.L);
        bool fireRocket = Input.GetKeyDown(KeyCode.K);
        //bool moveBend = Input.GetKeyDown(KeyCode.M);
        //bool one = Input.GetKeyDown(KeyCode.Alpha1);

        if (stat == Status.Healthy && false)
        {
            //Stage1.instance.UpdateStage1();

            // Precondition: player is on left half side of screen
            // Pseudo: Startpos is (10.54, -4.5, 0), move to that place, start firing bullet,
            // move towards the other side (endpos), reverse scale, start firing another round
            // of bullets, fly back to original position. Repeat twice of above movements, fire rocket 
            // to end the entire attacking pattern for Stage 1.

            if (!bulletFireGoingOn && guard1)
            {
                //transform.position = startpos;
                FireBullet2(true);
            }

            // Start bending movement, don't know when this should happen, test using different values in
            // seconds (bendFlyCountDown) to find appropriate start time.
            bendFlyCountDown -= Time.deltaTime;
            backtoBltCountDown = (backtoBltCountDown == 0 ? 0 : backtoBltCountDown - Time.deltaTime);
            //print(backtoBltCountDown);

            if (bendFlyCountDown < 0)
            {
                if (guard2)
                {
                    StartCoroutine(MovetoPosTowards(startpos, endpos, bendIndex));
                    if (bendIndex == 1)
                    {
                        toRck = true;
                    }
                }
                guard2 = false;
            }

            // Flip dir field. Again this field is separated from localscale.
            float closeEnoughDis = Vector3.Distance(transform.position, endpos);
            // Again 0.15 is a tested value that could be adjusted if necessary.
            bool overlapped = closeEnoughDis <= 0.18f;

            if (dir == -1 && overlapped && bendIndex == 0)
            {
                backAndForthCount++;
                dir *= -1;

                // TODO: do this with animation instead
                transform.localScale = new Vector3(-0.4f, 0.4f, 0.4f);

                // boss arriving at endpos, update guard1 to allow further shooting
                guard1 = true;

                // boss will go back and forth 3 times before it launches rockets
                if (backAndForthCount != 3)
                {
                    // swap startpos and endpos
                    Vector3 tempVec = startpos;
                    startpos = endpos;
                    endpos = tempVec;
                }
                else
                {
                    bendIndex = 1;

                    // startpos: previous endpos; endpos: place to launch rockets with a different bending factor 
                    Vector3 tempVec = startpos;
                    startpos = endpos;
                    endpos = new Vector3(0, 2.5f, 0);
                }

                // update guard2 to make sure movetowards happens
                guard2 = true;
            }

            else if (dir == 1 && overlapped && bendIndex == 0)
            {
                dir *= -1;

                // TODO: reverse flip-animation to replace this
                transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

                // TODO: needs refactors
                guard1 = true;
                Vector3 tempVec = startpos;
                startpos = endpos;
                endpos = tempVec;
                guard2 = true;
            }

            else if (toRck)
            {
                // make sure there is no more fire bullet and boss stays at current endpos when it arrives
                guard1 = false;
                guard2 = false;

                if (overlapped && guard3)
                {
                    // starting fire-rocket animation
                    transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                    SyncScaleAndDir();
                    FireRocket(true, 8, transform.position);
                    guard3 = false;

                    // Rocket fire started, prepare back to bullet position; start countdown for backing
                    toRck = !toRck;
                    backtoBltCountDown = 9f;
                }
            }

            // back to bullet position
            else if (!toRck && backtoBltCountDown < 0)
            {
                if (IsPlayerLeft() && guard4)
                {
                    guard4 = false;
                    startpos = endpos;
                    endpos = new Vector3(10.55f, -4.5f, 0);
                    overlapped = Vector3.Distance(transform.position, endpos) <= 0.18f;    // update overlapped info in same frame for later use
                    bendIndex = 2;
                    StartCoroutine(MovetoPosTowards(startpos, endpos, bendIndex));    // TODO: Design of MovetoPosTowards+bendIndex needs refactoring 
                }
                else if (!IsPlayerLeft() && guard4)
                {
                    guard4 = false;
                    startpos = endpos;
                    endpos = new Vector3(-9.2f, -4.5f, 0);
                    overlapped = Vector3.Distance(transform.position, endpos) <= 0.18f;
                    bendIndex = 1;
                    StartCoroutine(MovetoPosTowards(startpos, endpos, bendIndex));
                }

                if (overlapped)
                {
                    // reached endpos, adjust localscale, and reset essential params for next round of attack

                    // TODO: adjustment to scale is unnecssary when same flipped appearance is achieved by animation;
                    // BUT change of dir is needed as it encapsulates info for direction of weapons.
                    float newScaleX = (endpos.x > 0 ? 0.4f : -0.4f);
                    transform.localScale = new Vector3(newScaleX, transform.localScale.y, transform.localScale.z);
                    dir = (endpos.x > 0 ? -1 : 1);

                    backtoBltCountDown = 0;
                    toRck = false;
                    backAndForthCount = 0;
                    bendFlyCountDown = 0;
                    bendIndex = 0;

                    // reset startpos and endpos for new rounds of bullet fire
                    startpos = endpos;
                    endpos = (endpos.x > 0 ? new Vector3(-9.2f, -4.5f, 0) : new Vector3(10.55f, -4.5f, 0));

                    // reset of guards
                    guard1 = true;
                    guard2 = true;
                    guard3 = true;
                    guard4 = true;
                }
            }
        }

        idleRemaining -= Time.deltaTime;
        //print(idleReaining);

        if (stat == Status.Damaged && idleRemaining < 0)
        {
            //print(lazerProcessStart);
            if (!lazerProcessStart)
            {
                // This enables boss to start stage 2 attack on either side. As long as dir and scale are adjusted
                // correctly before entering stage 2, this helps update position variables accordingly.
                // PRE: scale and dir have been adjusted during "transitioning stage"; standard of that should be position
                // of player.
                UpdateTempPos();

                StartCoroutine(SmoothTranslate(transform.position, endpos, 2f));
                lazerProcessStart = true;
            }

            bool overlappedEnd2 = Vector3.Distance(transform.position, endpos) <= 0.18f;
            bool overlappedLazer = Vector3.Distance(transform.position, lazerSpawnPos) <= 0.18f;
            //bool overlappedRck = Vector3.Distance(transform.position, rocketPos) <= 0.18f;
            //bool overlappedEnd1 = Vector3.Distance(transform.position, STARTPOS) <= 0.18f;

            if (overlappedEnd2 && guard5)
            {
                // TODO: replace this with flip animation
                transform.localScale = new Vector3(-1 * transform.localScale.x, transform.localScale.y, transform.localScale.z);
                SyncScaleAndDir();

                guard5 = false;
            }

            if (!guard5)
            {
                lazerCountDown = (lazerCountDown == 0) ? 0 : lazerCountDown - Time.deltaTime;
            }

            //print(lazerCountDown);

            if (lazerCountDown < 0)
            {
                StartCoroutine(SmoothTranslate(transform.position, lazerSpawnPos, 2f));
                lazerCountDown = 0;
            }

            if (overlappedLazer && guard6)
            {
                //print("will fire lazer");
                FireLazer(true);
                guard6 = false;
            }

            if (!guard6)
            {
                //randRocketCountDown = (randRocketCountDown == 0) ? 0 : randRocketCountDown - Time.deltaTime;
                randRocketCountDown -= Time.deltaTime;
            }

            //print(randRocketCountDown);

            if (randRocketCountDown < 0)
            {
                // TODO: replace this and adjust scale based on player position
                transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                SyncScaleAndDir();
               
                if (segList.Count == 0 && guard7)
                {
                    // rocketPos needs to be never approachable
                    rocketPos = UNREACHABLE;

                    // List empty, check position of player and place boss and end position.
                    // Reset params to start next round of attack.
                    //print("seglist is empty!");
                    NotYetTranslating = false;

                    if (IsPlayerLeft())
                    {
                        endpos = STARTPOS;
                        StartCoroutine(SmoothTranslate(transform.position, endpos, 2));

                        // TODO: first check whether flip is needed; if yes commit flip logic, otherwise do nothing.
                        // POST: boss on right side, scale and dir need to respect this fact. Same for left-side-situation.
                        transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                        SyncScaleAndDir();
                    }
                    else
                    {
                        endpos = ENDPOS;
                        StartCoroutine(SmoothTranslate(transform.position, endpos, 2));

                        // TODO: replace with flip animation
                        transform.localScale = new Vector3(-0.4f, 0.4f, 0.4f);
                        SyncScaleAndDir();
                    }
                }
                else if (segList.Count != 0 && NotYetUpdatedPos)
                {
                    rocketPos = UpdateRckPos(segList);
                    NotYetUpdatedPos = false;
                }

                //print(segList.Count);
                bool overlappedRck = Vector3.Distance(transform.position, rocketPos) <= 0.18f;
                //bool overlappedEnd1 = Vector3.Distance(transform.position, STARTPOS) <= 0.18f;

                if (NotYetTranslating)
                {
                    StartCoroutine(SmoothTranslate(transform.position, rocketPos, 1f));
                    NotYetTranslating = false;
                }
                if (overlappedRck)
                {
                    FireRocket(true, 4, transform.position);
                    randRocketCountDown = 5f;
                    NotYetUpdatedPos = true;
                    NotYetTranslating = true;
                   
                    if (segList.Count == 0)
                    {
                        guard7 = true;
                    }
                }
                if (overlappedEnd2)
                {
                    ResetStage2();
                }
            }
        }

        FireBullet2(false);

        FireLazer(false);

        if (stat == Status.Healthy)
        {
            FireRocket(false, 8, transform.position);
        }
        else if (stat == Status.Damaged)
        {
            FireRocket(false, 4, transform.position);
        }
	}

    public void FireBullet2(bool firebullet)
    {
        if (firebullet)
        {
            //stage1Manager.GetComponent<Stage1>().BulletFireGoingOn = true;
            //stage1Manager.GetComponent<Stage1>().Guard1 = false;
            bulletFireGoingOn = true;
            guard1 = false;
            //transform.position = new Vector3(10.55f, -4.5f, 0);
            rnd = Random.Range(0, 2);
            if (rnd == 0)
            {
                firecount = 2;
                anim.SetTrigger("firebullet22");
                bendFlyCountDown = 3.3f;
                //stage1Manager.GetComponent<Stage1>().BendFlyCountDown = 3.3f;    // Adjustable
            }
            else
            {
                // print("bulletatk3 entered");
                firecount = 3;
                anim.SetTrigger("firebullet23");
                bendFlyCountDown = 4.5f;
                //stage1Manager.GetComponent<Stage1>().BendFlyCountDown = 4.5f;    // almost 3.3 + 1
            }
            spawnacc = 1f;
        }

        if (spawnacc > 0f)
        {
            spawnacc -= Time.deltaTime;
        }

        else if (spawnacc <= 0f && firecount > 0)
        {
            if (dir == -1)
            {
                GameObject bulletclone = Instantiate(bullets, bullets.transform.position, new Quaternion(0, 0, 0, 0)) as GameObject;
                //bulletclone.GetComponent<Bullets>().Dir = dir;
            }
            else if (dir == 1)
            {
                GameObject bulletclone = Instantiate(bulletsReversed, bulletsReversed.transform.position, new Quaternion(0, 0, 0, 0)) as GameObject;
                //bulletclone.GetComponent<Bullets>().Dir = dir;
            }

            firecount--;
            spawnacc = (firecount == 0) ? 0f : 1f;    // spawnacc == 0; firecount == 0 when exiting
            bulletFireGoingOn = (firecount == 0) ? false : true;
        }
    }

    void FireBullet(bool firebullet) {

        if (firebullet)
        {
            // bullet fire start
            bulletFireGoingOn = true;
            transform.position = new Vector3(10.55f, -4.5f, 0);
            rnd = Random.Range(0, 2);
            if (rnd == 0)
            {
                firecount = 2;
                anim.SetTrigger("firebullet2");
            }
            else
            {
                // print("bulletatk3 entered");
                firecount = 3;
                anim.SetTrigger("firebullet3");
            }
            spawnacc = 1f;
        }

        if (spawnacc > 0f)
        {
            spawnacc -= Time.deltaTime;
        }

        else if (spawnacc <= 0f && firecount > 0)
        {
            GameObject bulletclone = Instantiate(bullets, bullets.transform.position, new Quaternion(0, 0, 0, 0)) as GameObject;
            firecount--;
            spawnacc = (firecount == 0) ? 0f : 1f;
        }

        if (spawnacc == 0f && firecount == 0)
        {
            // bullet fire done
            anim.SetTrigger("takeback");

            bulletFireGoingOn = false;    
        }
    }

    public void FireLazer(bool firelazer) {
        if (firelazer)
        {
            lazercount++;    // 1
            anim.SetTrigger("lazerstart");
        }

        if (lazeracc < 1f && lazercount > 0)
        {
            lazeracc += Time.deltaTime;
        }

        else if (lazeracc >= 1f)
        {
            lazerAnim.SetTrigger("lazerspawn");
            lazeracc = 0f;
        }

        if (lazeracc == 0f && lazercount > 0)
        {
            anim.SetTrigger("lazertakeback");
            lazercount = 0;
        }
    }

    public void FireRocket(bool firerocket, int count, Vector3 rckPos) {

        if (firerocket)
        {
            //transform.position = new Vector3(0, 2.5f, 0);
            rocketcount++;    // 1
            anim.SetTrigger("rocketstart");
        }

        if (rocketacc < 1f && rocketcount > 0)
        {
            rocketacc += Time.deltaTime;
        }

        else if (rocketacc >= 1f)
        {
            if (rocketinterval > 0.5f && rocketcount < count + 1)
            {
                GameObject rocketclone = Instantiate(rocket, rckPos, new Quaternion(0, 0, 0, 0)) as GameObject;
                rocketcount++;
                rocketinterval = 0;
            }

            else if (rocketcount < count + 1)
            {
                rocketinterval += Time.deltaTime;
            }

            else if (rocketcount == count + 1)
            {
                rocketacc = 0;
                rocketinterval = 0;
            }
        }

        if (rocketacc == 0f && rocketcount > 0)
        {
            // the real "take-back" process should start after rockets' emissions are settled
            anim.SetTrigger("rockettakeback");
            rocketcount = 0;    // rocketcount == rocketacc == rocketinterval == 0 when exiting
        }
    }

    IEnumerator MoveTowardsNoBend (Vector3 startpos, Vector3 endpos)
    {
        float timeStamp = Time.time;

        // travel time is 1 second
        while (Time.time < timeStamp + 1)
        {
            transform.position = Vector3.MoveTowards(transform.position, endpos, spd * Time.deltaTime);
            yield return null;
        }
    }

    /**
     * Firebullet ready
     */
    public IEnumerator MovetoPosTowards(Vector3 startpos, Vector3 endpos, int index)
    {
        float collideTime = 0;
        float timestamp = Time.time;
        float traveldis = Vector3.Distance(startpos, endpos);
        Vector3 prevpos;

        switch (index)
        {
            case 0:
                while (Time.time < timestamp + traveltime)
                {
                    prevpos = transform.position;
                    Vector3 currentpos = Vector3.MoveTowards(transform.position, endpos, spd * Time.deltaTime);
                    float frac = Vector3.Distance(currentpos, prevpos) / traveldis;
                    currentpos.x += bending.x * Mathf.Sin(Mathf.Clamp01(frac) * Mathf.PI);
                    currentpos.y += bending.y * Mathf.Sin(Mathf.Clamp01(frac) * Mathf.PI);
                    currentpos.z += bending.z * Mathf.Sin(Mathf.Clamp01(frac) * Mathf.PI);

                    transform.position = currentpos;

                    yield return null;
                }
                break;

            case 1:
                while (Time.time < timestamp + traveltime)
                {
                    prevpos = transform.position;
                    Vector3 currentpos = Vector3.MoveTowards(transform.position, endpos, spd * Time.deltaTime);
                    float frac = Vector3.Distance(currentpos, prevpos) / traveldis;
                    currentpos.x += bendLeft.x * Mathf.Sin(Mathf.Clamp01(frac) * Mathf.PI);
                    currentpos.y += bendLeft.y * Mathf.Sin(Mathf.Clamp01(frac) * Mathf.PI);
                    currentpos.z += bendLeft.z * Mathf.Sin(Mathf.Clamp01(frac) * Mathf.PI);

                    transform.position = currentpos;

                    yield return null;
                }
                break;

            case 2:
                while (Time.time < timestamp + traveltime)
                {
                    prevpos = transform.position;
                    Vector3 currentpos = Vector3.MoveTowards(transform.position, endpos, spd * Time.deltaTime);
                    float frac = Vector3.Distance(currentpos, prevpos) / traveldis;
                    currentpos.x += bendRight.x * Mathf.Sin(Mathf.Clamp01(frac) * Mathf.PI);
                    currentpos.y += bendRight.y * Mathf.Sin(Mathf.Clamp01(frac) * Mathf.PI);
                    currentpos.z += bendRight.z * Mathf.Sin(Mathf.Clamp01(frac) * Mathf.PI);

                    transform.position = currentpos;

                    yield return null;
                }
                break;

            case 3:
                // obsolete code for bend collision, never use this
                while (collideTime < 1)
                {
                    // finish in 2 seconds, the amount in numerator
                    collideTime += Time.deltaTime / 2;

                    prevpos = transform.position;
                    Vector3 currentpos = Vector3.MoveTowards(prevpos, endpos, Mathf.SmoothStep(0, 1, collideTime));
                    float frac = Vector3.Distance(currentpos, prevpos) / traveldis;
                    currentpos.x += collideBendToLeft.x * Mathf.Sin(Mathf.Clamp01(frac) * Mathf.PI);
                    currentpos.y += collideBendToLeft.y * Mathf.Sin(Mathf.Clamp01(frac) * Mathf.PI);
                    currentpos.z += collideBendToLeft.z * Mathf.Sin(Mathf.Clamp01(frac) * Mathf.PI);

                    // TODO: NAN?
                    transform.position = currentpos;

                    yield return null;
                }
                break;
        }
    }

    IEnumerator Delay(int sec)
    {
        yield return new WaitForSeconds(sec);
    }

    public bool IsPlayerLeft()
    {
        return (MIDX - player.gameObject.transform.position.x) > 0;
    }

    public IEnumerator SmoothTranslate(Vector3 startpos, Vector3 endpos, float sec)
    {
        float time = 0f;

        while (time < 1f)
        {
            time += Time.deltaTime / sec;
            transform.position = Vector3.Lerp(startpos, endpos, Mathf.SmoothStep(0, 1, Mathf.SmoothStep(0, 1, time)));
            yield return null;
        }
    }
    
    IEnumerator SmoothRotate(Vector3 deltaAngle, float sec)
    {
        Quaternion currentAngle = transform.rotation;
        Quaternion toAngle = Quaternion.Euler(transform.eulerAngles + deltaAngle);
        for (float time = 0; time <= 1; time += Time.deltaTime / sec)
        {
            transform.rotation = Quaternion.Lerp(currentAngle, toAngle, Mathf.SmoothStep(0, 1, Mathf.SmoothStep(0, 1, time)));
            yield return null;
        }
    }

    IEnumerator SmoothMovement(Vector3 startpos, Vector3 endpos, Vector3 deltaAngle, float secRotate, float secTranslate)
    {
        float timeTranslate = 0f;
        float timeRotate = 0f;
        Quaternion currentAngle = transform.rotation;
        Quaternion toAngle = Quaternion.Euler(transform.eulerAngles + deltaAngle);

        while (timeTranslate < 1f)
        {
            timeTranslate += Time.deltaTime / secTranslate;
            transform.position = Vector3.Lerp(startpos, endpos, Mathf.SmoothStep(0, 1, Mathf.SmoothStep(0, 1, Mathf.SmoothStep(0, 1, Mathf.SmoothStep(0, 1, timeTranslate)))));
            if (timeRotate < 1f)
            {
                //print("rotation logic entered");
                timeRotate += Time.deltaTime / secRotate;
                transform.rotation = Quaternion.Lerp(currentAngle, toAngle, Mathf.SmoothStep(0, 1, Mathf.SmoothStep(0, 1, timeRotate)));
            }
            yield return null;
        }
    }

    /*
     * Generate rocket randomly in specified rectangular region.
     * Downleft corner of random generator rectangle is (-9, 0, 0). Width is 18 and height is 4.
     * Pre: segs cannot be empty.
     */
    Vector3 UpdateRckPos(List<Segment> segs)
    {
        startpos = transform.position;
        float widthRand = 0;

        int randIndex = Random.Range(0, segs.Count - 1);
        switch (segs[randIndex])
        {
            case Segment.Left:
                widthRand = Random.Range(-9, -2.99f);
                break;

            case Segment.Middle:
                widthRand = Random.Range(-3, 2.99f);
                break;

            case Segment.Right:
                widthRand = Random.Range(3, 9);
                break;
        }
        segs.Remove(segs[randIndex]);
        float heightRand = Random.Range(0, 4);
        //endpos = new Vector3(widthRand, heightRand, 0);

        return new Vector3 (widthRand, heightRand, 0);
    }

    void UpdateHealthStatus ()
    {
        if (hp >= hpconst * hpPercentage1 && hp <= hpconst)
        {
            stat = Status.Healthy;
        }
        else if (hp < hpconst * hpPercentage1 && hp >= hpconst * hpPercentage2)
        {
            stat = Status.Damaged;
        }
        else
        {
            stat = Status.Dying;
        }
    }

    void SyncScaleAndDir ()
    {
        if (transform.localScale.x > 0)
        {
            dir = -1;
        }

        else
        {
            dir = 1;
        }
    }

    void Populate(List <Segment> lst)
    {
        lst.Add(Segment.Left);
        lst.Add(Segment.Middle);
        lst.Add(Segment.Right);
    }

    void ResetStage2 ()
    {
        lazerProcessStart = false;
        lazerCountDown = 1;
        guard5 = true;
        guard6 = true;
        randRocketCountDown = 8;
        Populate(segList);
        NotYetUpdatedPos = true;
        NotYetTranslating = true;
        guard7 = false;
        idleRemaining = 5;
    }

    void UpdateTempPos ()
    {
        if (transform.localScale.x > 0 && dir == -1)
        {
            startpos = STARTPOS; // STARTPOS == transform.position
            endpos = ENDPOS;
            lazerSpawnPos = LAZERREADYLEFT;
        }
        else if (transform.localScale.x < 0 && dir == 1)
        {
            startpos = ENDPOS; // ENDPOS == transform.position
            endpos = STARTPOS;
            lazerSpawnPos = LAZERREADYRIGHT;
        }
    }
}

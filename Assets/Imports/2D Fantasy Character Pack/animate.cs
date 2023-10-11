using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animate : MonoBehaviour {
    private Animator animator;
    public float moveSpeed;
    public Transform waypoint;
    Transform nextWaypoint;
    Vector2 initialPosition;
    public float currentTimeOnPath, time;
    string[] animations;
    // Use this for initialization
    void Start () {
        animator = GetComponent<Animator>();
        initialPosition = transform.position;
        moveSpeed = 0.8f;
        animations = new string[] { "attack", "jump","shoot","dying","run","attack2" };
    }
	
	// Update is called once per frame
	void Update () {
        time += Time.deltaTime;
       
        if (time < 43)
        {
            walk();
        }
        else
        {
            if (time >= 53)
            {
                StartAnimation();
                animator.Play(animations[Random.Range(0,animations.Length - 1)]);
                time = 50;
            }
        }
        

    }
    public void StartAnimation()
    {

    }
    private void look(Vector2 direction)
    {
        if (direction.x >  transform.position.x) // To the right
        {
            transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);

        }
        else if (direction.x < transform.position.x) // To the left
        {
            transform.localScale = new Vector3(-1, transform.localScale.y, transform.localScale.z);
            
        }
    }
    void walk()
    {
        animator.Play("walk");
        Vector3 startPosition = initialPosition;
        Vector3 endPosition = waypoint.transform.GetChild(0).transform.position;
        // 2 
        float pathLength = Vector3.Distance(startPosition, endPosition);
        float totalTimeForPath = pathLength / moveSpeed;
        //float currentTimeOnPath = Time.time - lastWaypointSwitchTime;
        currentTimeOnPath += 1 * Time.deltaTime;
        gameObject.transform.position = Vector3.Lerp(startPosition, endPosition, currentTimeOnPath / totalTimeForPath);
        // 3 
        if (gameObject.transform.position.Equals(endPosition))
        {
            initialPosition = transform.position;
            waypoint = waypoint.transform.GetChild(0);
            currentTimeOnPath = 0;
            look(waypoint.transform.GetChild(0).position);

        }
    }

}

using UnityEngine;
using System.Collections;

public class Actor : MonoBehaviour {


    // "Physics"
    public float walkSpeed = 0.000001f;
    public float SpeedLimit = 1f;
    public float Speed = 10;

    public Transform Sprite;
    public Collider CurrentTrigger;

    public Vector3 Target;

    public Transform NavigationNodes;

    public string TestNode = "";

    // States
    public bool IsUsingLadder = false;

    private float turntarget = 12f;
    private float hintAlpha = 0f;
    private Vector2 actualSize = new Vector2(7f, 7f);

    void Awake()
    {
        turntarget = actualSize.x;
    }

    void FixedUpdate()
    {
       


        if (Target.x < transform.position.x) turntarget = -actualSize.x;
        if (Target.x > transform.position.x) turntarget = actualSize.x;

        //Speed = walkSpeed;
        //rigidbody.velocity = transform.TransformDirection(new Vector3(h, 0, v).normalized) * Speed;
       

        Sprite.localScale = Vector3.Lerp(Sprite.transform.localScale, new Vector3(turntarget, actualSize.y, 1f), 0.25f);


        if (TestNode != "")
        {
            Navigate(TestNode);
        }

    }

    void Navigate(string nodeName)
    {
        
    }

    public void Use()
    {
        if (CurrentTrigger == null) return;

        switch (CurrentTrigger.name)
        {
            case "LadderTop":
                transform.position = CurrentTrigger.transform.position;
                //transform.position = GameObject.Find("LadderBottom").transform.position;
                IsUsingLadder = true;
                StartMoveTo(CurrentTrigger.transform.position, GameObject.Find("LadderBottom").transform.position);

                break;
            case "LadderBottom":
                //transform.position = CurrentTrigger.transform.position;
                IsUsingLadder = true;
                StartMoveTo(CurrentTrigger.transform.position, GameObject.Find("LadderTop").transform.position);

                break;
            case "Stairs1Top":
                //transform.position = CurrentTrigger.transform.position;
                //transform.position = GameObject.Find("Stairs1Bottom").transform.position;
                StartMoveTo(CurrentTrigger.transform.position, GameObject.Find("Stairs1Bottom").transform.position);
                break;
            case "Stairs1Bottom":
                //transform.position = CurrentTrigger.transform.position;
                //transform.position = GameObject.Find("Stairs1Top").transform.position;
                StartMoveTo(CurrentTrigger.transform.position, GameObject.Find("Stairs1Top").transform.position);

                break;
        }
    }

    private void StartMoveTo(Vector3 start, Vector3 end)
    {
        StartCoroutine(MoveToTarget(start, end, 0.01f));
    }

    public void OnTriggerStay(Collider trigger)
    {
        switch (trigger.name)
        {
            case "LadderTop":
            case "LadderBottom":
                
                break;
            case "Stairs1Top":
            case "Stairs1Bottom":
               
                break;
        }

        CurrentTrigger = trigger;
    }

    public void OnTriggerExit(Collider trigger)
    {
        CurrentTrigger = null;
    }

    IEnumerator MoveToTarget(Vector3 source, Vector3 target, float speed)
    {
      
        float cur = 0f;
        while (cur < 1f)
        {
            cur += speed;
            transform.position = Vector3.Lerp(source, target, cur);

            if (transform.position.x < target.x) turntarget = -actualSize.x;
            if (transform.position.x > target.x) turntarget = actualSize.x;

            yield return null;
        }

        IsUsingLadder = false;
    }
}

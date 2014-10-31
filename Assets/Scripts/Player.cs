using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Player : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// "Physics"
    public float walkSpeed = 0.000001f;
    public float SpeedLimit = 1f;
    public float Speed = 10;

    public Transform Sprite;
    public Collider CurrentTrigger;

    // States
    public bool IsUsingLadder = false;
    public bool IsMovingSelf = false;

    private float turntarget = 12f;
    private float hintAlpha = 0f;
    private Vector2 actualSize = new Vector2(7f,7f);

    void Awake()
    {
        turntarget = actualSize.x;        
    }

    void FixedUpdate() {
        //Input
        if (IsMovingSelf)
        {
            //GetComponent<CapsuleCollider>().enabled = false;
        }
        else
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            if (h > 0f) turntarget = -actualSize.x;
            if (h < 0f) turntarget = actualSize.x;

            Speed = walkSpeed;
            rigidbody.velocity = transform.TransformDirection(new Vector3(h, 0, v).normalized) * Speed;
        }

        Sprite.localScale = Vector3.Lerp(Sprite.transform.localScale, new Vector3(turntarget, actualSize.y, 1f), 0.25f);

        if (hintAlpha > 0f) hintAlpha -= 0.1f;
        hintAlpha = Mathf.Clamp(hintAlpha, 0f, 1f);
        transform.FindChild("Hint UI/Hint").GetComponent<Text>().color = Color.white*hintAlpha;

        if (Input.GetButtonDown("Use"))
        {
            Use();
        }
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
        switch(trigger.name)
        {
            case "LadderTop":
            case "LadderBottom":
                HintText("Use Ladder");
                break;
            case "Stairs1Top":
            case "Stairs1Bottom":
                HintText("Use Stairs");
                break;
        }

        CurrentTrigger = trigger;
    }

    public void OnTriggerExit(Collider trigger)
    {
        CurrentTrigger = null;
    }

    public void HintText(string text)
    {
        transform.FindChild("Hint UI/Hint").GetComponent<Text>().text = text;
        hintAlpha +=0.2f;
    }

    IEnumerator MoveToTarget(Vector3 source, Vector3 target, float speed)
    {
        IsMovingSelf = true;

        float cur = 0f;
        while (cur<1f)
        {
            cur += speed;
            transform.position = Vector3.Lerp(source, target, cur);

            if (transform.position.x < target.x) turntarget = -actualSize.x;
            if (transform.position.x > target.x) turntarget = actualSize.x;

            yield return null;
        }

        IsUsingLadder = false;
        IsMovingSelf = false;
    }
}

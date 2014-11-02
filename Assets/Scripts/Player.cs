using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Player : MonoBehaviour {

    public Transform HUD;

	// "Physics"
    public float walkSpeed = 0.000001f;
    public float SpeedLimit = 1f;
    public float Speed = 10;

    public Transform Sprite;
    public Collider CurrentTrigger;

    // States
    public bool IsUsingLadder = false;
    public bool IsMovingSelf = false;
    public bool IsHiding = false;

    // Stats
    public float CaughtAmount = 0f;

    public Dictionary<string, AudioSource> Sounds = new Dictionary<string, AudioSource>();  

    private float turntarget = 12f;
    private float hintAlpha = 0f;
    private Vector2 actualSize = new Vector2(7f,7f);

    private float fstepTime = 0f;

    private float catchTextTimer = 0f;

    private tk2dSpriteAnimator anim;

    void Awake()
    {
        turntarget = actualSize.x;

        anim = Sprite.GetComponent<tk2dSpriteAnimator>();

        GameObject soundsObject = transform.FindChild("Audio").gameObject;
        foreach (AudioSource a in soundsObject.GetComponents<AudioSource>())
        {
            Sounds.Add(a.clip.name, a);
        }
    }

    void FixedUpdate()
    {
        if (CaughtAmount >= 1f)
        {
            GameObject gameOverPanel = HUD.FindChild("GameOverPanel").gameObject;
            gameOverPanel.GetComponent<Image>().color = Color.Lerp(gameOverPanel.GetComponent<Image>().color,Color.black*1f, 0.1f);

            Text gameOverText = HUD.FindChild("GameOverPanel/Text1").GetComponent<Text>();
            gameOverText.color = Color.Lerp(gameOverText.color, Color.red * 1f, 0.1f);
            gameOverText = HUD.FindChild("GameOverPanel/Text2").GetComponent<Text>();
            gameOverText.color = Color.Lerp(gameOverText.color, Color.white * 1f, 0.1f);

            if(Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Use"))
                Application.LoadLevel("HalloweenJam");

            return;
        }

        //Input
        if (IsMovingSelf)
        {
            //GetComponent<CapsuleCollider>().enabled = false;
            if (IsUsingLadder)
            {
                if (!anim.IsPlaying("MonsterLadderTransferOn") && !anim.IsPlaying("MonsterLadderTransferOff"))
                    anim.Play("MonsterLadderClimb");

                if(!Sounds["Ladder"].isPlaying) Sounds["Ladder"].Play();
            }
            else anim.Play("MonsterWalk");

            fstepTime += Time.deltaTime;
            if (fstepTime > 0.1f)
            {
                Sounds["fstep"].pitch = Random.Range(0.5f, 1.5f);
                Sounds["fstep"].Play();
                fstepTime = 0f;
            }
        }
        else if(!IsHiding)
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            if (h > 0f) turntarget = -actualSize.x;
            if (h < 0f) turntarget = actualSize.x;

            Speed = walkSpeed;
            rigidbody.velocity = transform.TransformDirection(new Vector3(h, 0, v).normalized) * Speed;

            if (rigidbody.velocity.magnitude > 0f)
            {
                fstepTime += Time.deltaTime;
                if (fstepTime > 0.1f)
                {
                    Sounds["fstep"].Play();
                    fstepTime = 0f;
                }
            }

            if (!anim.IsPlaying("MonsterLadderTransferOn") && !anim.IsPlaying("MonsterLadderTransferOff"))
            {
                if (rigidbody.velocity.magnitude > 0f) anim.Play("MonsterWalk");
                else anim.Play("MonsterIdle");
            }
        }

        Sprite.localScale = Vector3.Lerp(Sprite.transform.localScale, new Vector3(turntarget, actualSize.y, 1f), 0.25f);

        if (hintAlpha > 0f) hintAlpha -= 0.1f;
        hintAlpha = Mathf.Clamp(hintAlpha, 0f, 1f);
        transform.FindChild("Hint UI/Hint").GetComponent<Text>().color = Color.white*hintAlpha;

        if (Input.GetButtonDown("Use"))
        {
            Use();
        }

        // Hiding
        if (IsHiding)
        {
            Sprite.renderer.enabled = false;
        }
        else
        {
            Sprite.renderer.enabled = true;
        }

        // Being Caught
        int n = NumberOfCatchers();
        CaughtAmount += (float)n*0.001f;
        if (n == 0) CaughtAmount -= 0.001f;
        CaughtAmount = Mathf.Clamp(CaughtAmount, 0f, 1f);

        // HUD
        Scrollbar caughtBar = HUD.FindChild("Panel/CatchMeter").GetComponent<Scrollbar>();
        caughtBar.size = CaughtAmount;
        if (CaughtAmount <= 0f)
        {
            ColorBlock cb = caughtBar.colors;
            cb.disabledColor = Color.Lerp(caughtBar.colors.disabledColor, Color.red*0f, 0.01f);
            caughtBar.colors = cb;
        }
        else
        {
            ColorBlock cb = caughtBar.colors;
            cb.disabledColor = Color.Lerp(caughtBar.colors.disabledColor, Color.red * 1f, 0.01f);
            caughtBar.colors = cb;
        }

        Text caughtText = HUD.FindChild("Panel/CatchText").GetComponent<Text>();
        if (NumberOfCatchers() > 0)
        {
            catchTextTimer += Time.deltaTime;
            if (catchTextTimer > 0.1f)
            {
                caughtText.enabled = !caughtText.enabled;
                catchTextTimer = 0f;
            }
        }
        else caughtText.enabled = false;
        //caughtText.color = Color.Lerp(caughtText.color, Color.red * 1f, 0.01f);

    }

    int NumberOfCatchers()
    {
        int numPeopleCatching = 0;
        foreach (var o in GameObject.FindGameObjectsWithTag("Actor"))
        {
            if (Vector3.Distance(transform.position, o.transform.position) < 3f)
            {
                if (o.GetComponent<Actor>().CanSeeMonster() && (o.GetComponent<Actor>().State == AIState.BeingBrave || o.GetComponent<Actor>().State == AIState.Chasing))
                    numPeopleCatching += 1 + (int)Vector3.Distance(transform.position, o.transform.position);
                
            }
        }

        return numPeopleCatching;
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
                anim.Play("MonsterLadderTransferOn");
                StartMoveTo(CurrentTrigger.transform.position, GameObject.Find("LadderBottom").transform.position);
                
                break;
            case "LadderBottom":
                //transform.position = CurrentTrigger.transform.position;
                IsUsingLadder = true;
                anim.Play("MonsterLadderTransferOn");
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

            case "WardrobeTrigger":
                if (!IsHiding)
                {
                    CurrentTrigger.GetComponentInParent<Wardrobe>().ToggleOpen(true);
                    IsHiding = true;
                    rigidbody.velocity = Vector3.zero;
                }
                else
                {
                    CurrentTrigger.GetComponentInParent<Wardrobe>().ToggleOpen(false);
                    IsHiding = false;
                }
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
            case "WardrobeTrigger":
                if(!IsHiding)
                    HintText("Hide");
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

        if (IsUsingLadder)
        {
            anim.Play("MonsterLadderTransferOff");
        }

        IsUsingLadder = false;
        IsMovingSelf = false;
    }
}

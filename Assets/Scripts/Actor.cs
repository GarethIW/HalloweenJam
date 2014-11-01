using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public enum AIState
{
    Normal,
    RunningAway,
    BeingBrave
}

public class Actor : MonoBehaviour {

    // "Physics"
    public float walkSpeed = 0.000001f;
    public float SpeedLimit = 1f;
    public float Speed = 10;

    public Transform Sprite;
    public Collider CurrentTrigger;

    public Vector3 Target;

    public Transform NavigationNodes;

    public Transform NearestNode;
    public List<Transform> PathNodes;

    public LayerMask LookMask;

    public string AnimName;

    public string TestNode = "";

    // States
    public AIState State = AIState.Normal;
    public bool IsUsingLadder = false;

    // Stats
    public float Bravery = 25f;

    private float turntarget = 12f;
    private float hintAlpha = 0f;
    private int faceDir = 1;
    private Vector2 actualSize = new Vector2(8f, 8f);
    private float statsTick = 0f;

    private tk2dSpriteAnimator anim;

    private Transform monster;

    void Awake()
    {
        turntarget = actualSize.x;

        anim = Sprite.GetComponent<tk2dSpriteAnimator>();
        monster = GameObject.Find("Player").transform;
    }

    void FixedUpdate()
    {
        if (TestNode != "")
        {
            Navigate(TestNode);
        }

        NearestNode = GetNearestNode(transform.position);

        if (PathNodes.Count > 0)
        {
            string debugText = "Going to: " + PathNodes[PathNodes.Count-1].name + " via\n";
            foreach (Transform t in PathNodes)
            {
                debugText += t.name + "\n";
            }
            HintText(debugText);

            
            float dist = Vector3.Distance(transform.position, Target);

            if (dist > 0.2f)
            {
                transform.position = Vector3.MoveTowards(transform.position, Target, State == AIState.RunningAway ? 0.15f : 0.1f);
                if (IsUsingLadder)
                {
                    if (!anim.IsPlaying(AnimName + "LadderTransitionOn") && !anim.IsPlaying(AnimName + "LadderTransitionOff"))
                        anim.Play(AnimName + "LadderClimb");
                }
                else anim.Play(AnimName + "Walk");
            }
            else
            {
                if (PathNodes[0].name == "Ladder Bottom" || PathNodes[0].name == "Ladder Top") anim.Play(AnimName + "LadderTransitionOff");
                else anim.Play(AnimName + "Idle");
                IsUsingLadder = false;

                if (PathNodes.Count > 1)
                {
                    if (PathNodes[0].name == "Ladder Bottom" && PathNodes[1].name == "Ladder Top")
                    {
                        anim.Play(AnimName + "LadderTransitionOn");
                        IsUsingLadder = true;
                    }
                    if (PathNodes[0].name == "Ladder Top" && PathNodes[1].name == "Ladder Bottom")
                    {
                        anim.Play(AnimName + "LadderTransitionOn");
                        IsUsingLadder = true;
                    }
                }

                PathNodes.RemoveAt(0);

                if (PathNodes.Count > 0)
                {
                    Transform targ = PathNodes[0];
                    Vector3 sphere = (Random.insideUnitSphere*
                                      (State == AIState.RunningAway && PathNodes.Count == 1 ? 1f : 0f));
                    sphere.y = 0f;
                    Target = targ.position + sphere;
                }
            }
        }
        else
        {
            HintText("");

            if (State == AIState.RunningAway || State == AIState.BeingBrave) State = AIState.Normal; //StartCoroutine(Helper.WaitThenCallback(3f, () => { State = AIState.Normal; }));
        }

        // AI?
        Bravery = Mathf.Clamp(Bravery, 0f, 100f);

        statsTick += Time.deltaTime;
        if (statsTick >= 1f)
        {
            statsTick = 0f;

            int numPeopleHere = 0;
            foreach (var o in GameObject.FindGameObjectsWithTag("Actor"))
            {
                if (o.transform == transform) continue;

                if (Vector3.Distance(transform.position, o.transform.position) < 3f)
                    numPeopleHere ++;
            }

            Bravery += numPeopleHere;
            if (numPeopleHere == 0) Bravery -= 1f;
        }

        float distanceToMonster = Vector3.Distance(transform.position, monster.position);

        switch (State)
        {
            case AIState.Normal:
                if (PathNodes.Count == 0)
                {
                    if (Random.Range(0, 500) == 0)
                    {
                        Navigate(GetRandomNode().name);
                    }
                }

                if (distanceToMonster < 7f && !IsUsingLadder)
                {

                    //Physics.Raycast(, 5f);
                    if ((faceDir == 1 && monster.position.x > transform.position.x) ||
                        (faceDir == -1 && monster.position.x < transform.position.x))
                    {
                        Ray lookRay = new Ray(transform.position + new Vector3(0f, 2.5f, 0f), ((monster.position + new Vector3(0f, 1.5f, 0f)) - (transform.position + new Vector3(0f, 2.5f, 0f))).normalized);
                        Debug.DrawRay(lookRay.origin, lookRay.direction * 7f, Color.red);
                        RaycastHit hitInfo;
                        if (Physics.Raycast(lookRay, out hitInfo, 7f, LookMask))
                        {
                            if (hitInfo.transform.name == "Player")
                            {
                                HintText("I CAN SEE YOU!\nAnd I am feeling " + (int)Bravery + "% brave!");

                                if (Bravery < 50f)
                                {
                                    State = AIState.RunningAway;
                                    Navigate(GetNearestNode(GetNearestPerson(transform.position).position).name);
                                }
                                else
                                {
                                    State = AIState.BeingBrave;
                                }
                            }
                        }


                    }
                }
                break;
            case AIState.RunningAway:
                //Navigate(GetNearestNode(GetNearestPerson(transform.position).position).name);
                break;
            case AIState.BeingBrave:
                //Target = monster.position;
                break;
        }

        if (!IsUsingLadder)
        {
            if (Target.x > transform.position.x)
            {
                turntarget = -actualSize.x;
                faceDir = 1;
            }
            if (Target.x < transform.position.x)
            {
                turntarget = actualSize.x;
                faceDir = -1;
            }
        }
        else turntarget = actualSize.x;

        Sprite.localScale = Vector3.Lerp(Sprite.transform.localScale, new Vector3(turntarget, actualSize.y, 1f), 0.25f);
    }

    void Navigate(string nodeName)
    {
        Transform destNode = NavigationNodes.FindChild(nodeName);
        List<Transform> pathNodes = new List<Transform>();

        if (destNode == null)
        {
            HintText("");
            return;
        }

        TestNode = "";

        pathNodes.Add(NearestNode);

        List<Transform> path = WalkPath(pathNodes.ToArray(), destNode);

        PathNodes = path;

        if (PathNodes != null && PathNodes.Count > 1)
        {
            if (PathNodes[0].name == "LadderBottom" && PathNodes[1].name == "LadderTop") anim.Play(AnimName + "LadderTransitionOn");

            Transform targ = PathNodes[0];
            Vector3 sphere = (Random.insideUnitSphere *
                              (State == AIState.RunningAway && PathNodes.Count == 1 ? 1f : 0f));
            sphere.y = 0f;
            Target = targ.position + sphere;
        }
    }

    private List<Transform> WalkPath(Transform[] pathNodes, Transform destNode)
    {
        Node n = pathNodes[pathNodes.Length - 1].GetComponent<Node>();

        if (n.transform == destNode) return pathNodes.ToList();

        for (int i = 0; i < n.ConnectedNodes.Count; i++)
        {
            if (pathNodes.Contains(n.ConnectedNodes[i])) continue;

            List<Transform> thisPath = new List<Transform>();
            thisPath.AddRange(pathNodes);
            thisPath.Add(n.ConnectedNodes[i].transform);

            if (n.ConnectedNodes[i] == destNode) return thisPath;
            
            List<Transform> newPath = WalkPath(thisPath.ToArray(), destNode);
            if (newPath != null) return newPath;
        }

        return null;
    }

    Transform GetNearestNode(Vector3 pos)
    {
        float dist = 9999f;
        Transform nearest = null;

        for (int i = 0; i < NavigationNodes.childCount; i++)
        {
            var n = NavigationNodes.GetChild(i);

            if (Vector3.Distance(pos, n.position) < dist)
            {
                dist = Vector3.Distance(pos, n.position);
                nearest = n;
            }
        }

        return nearest;
    }

    Transform GetRandomNode()
    {
        return NavigationNodes.GetChild(Random.Range(0, NavigationNodes.childCount));
    }

    Transform GetNearestPerson(Vector3 pos)
    {
        float dist = 9999f;
        Transform nearest = null;
        foreach (var o in GameObject.FindGameObjectsWithTag("Actor"))
        {
            if (o.transform == transform) continue;

            if (Vector3.Distance(pos, o.transform.position) < dist)
            {
                dist = Vector3.Distance(pos, o.transform.position);
                nearest = o.transform;
            }
        }

        return nearest;
    }

    public void HintText(string text)
    {
        transform.FindChild("Hint UI/Hint").GetComponent<Text>().text = text;
        hintAlpha += 0.2f;
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

using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

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

    public string TestNode = "";

    // States
    public bool IsUsingLadder = false;

    private float turntarget = 12f;
    private float hintAlpha = 0f;
    private Vector2 actualSize = new Vector2(7f, 7f);

    private tk2dSpriteAnimator anim;

    void Awake()
    {
        turntarget = actualSize.x;

        //anim = GetComponent<tk2dSpriteAnimator>();
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

        NearestNode = GetNearestNode();

        if (PathNodes.Count > 0)
        {
            Transform targ = PathNodes[0];
            Target = targ.position;
            float dist = Vector3.Distance(transform.position, targ.position);

            if (dist > 0.2f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targ.position, 0.1f);
            }
            else
            {
                PathNodes.RemoveAt(0);
            }
        }
        else
        {
            if (Random.Range(0, 500) == 0)
            {
                Navigate(GetRandomNode().name);
            }
        }
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

        string debugText = "Going to: " + destNode.name + " via ";

        pathNodes.Add(NearestNode);

        List<Transform> path = WalkPath(pathNodes.ToArray(), destNode);

        if (path != null)
        {
            foreach (Transform t in path)
            {
                debugText += t.name + ", ";
            }
        }

        HintText(debugText);

        PathNodes = path;
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

    Transform GetNearestNode()
    {
        float dist = 9999f;
        Transform nearest = null;

        for (int i = 0; i < NavigationNodes.childCount; i++)
        {
            var n = NavigationNodes.GetChild(i);

            if (Vector3.Distance(transform.position, n.position) < dist)
            {
                dist = Vector3.Distance(transform.position, n.position);
                nearest = n;
            }
        }

        return nearest;
    }

    Transform GetRandomNode()
    {
        return NavigationNodes.GetChild(Random.Range(0, NavigationNodes.childCount - 1));
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

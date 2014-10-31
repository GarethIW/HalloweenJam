using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MainCamera : MonoBehaviour
{
    public Transform Player;

    public float MinX;
    public float MaxX;
    public float Z;

    public Vector3 Offset;

    public Vector3 Target;
    public float Speed;

    public Vector3 ZoomedOutTarget;

	// Use this for initialization
	void Start ()
	{
        Target = Player.position + Offset;
	    Target.z = Z;
	    transform.position = Target;
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
	    if (!Input.GetKey(KeyCode.Space))
	    {
	        Target = Player.position;

	        Target.x = Mathf.Clamp(Target.x, MinX, MaxX);
	        Target.z = Z;

	        transform.position = Vector3.Lerp(transform.position, Target + Offset, Speed);
	    }
	    else
	    {
	        Target = ZoomedOutTarget;
            transform.position = Vector3.Lerp(transform.position, Target, Speed);
	    }

	}
}

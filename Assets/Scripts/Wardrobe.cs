using UnityEngine;
using System.Collections;

public class Wardrobe : MonoBehaviour
{

    public bool IsOpen = false;

	// Use this for initialization
	void Start ()
	{
	}
	
	// Update is called once per frame
	void Update ()
	{
	    ToggleOpen();
	}

    void ToggleOpen()
    {
        transform.FindChild("DoorOpen").GetComponent<SpriteRenderer>().enabled = false;
        transform.FindChild("DoorClosed").GetComponent<SpriteRenderer>().enabled = false;

        if (IsOpen)
            transform.FindChild("DoorOpen").GetComponent<SpriteRenderer>().enabled = true;
        else
            transform.FindChild("DoorClosed").GetComponent<SpriteRenderer>().enabled = true;
    }
}

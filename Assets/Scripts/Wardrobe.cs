using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class Wardrobe : MonoBehaviour
{

    public bool IsOpen = false;

    public Dictionary<string, AudioSource> Sounds = new Dictionary<string, AudioSource>();  

	// Use this for initialization
	void Start ()
	{
        GameObject soundsObject = transform.FindChild("Audio").gameObject;
        foreach (AudioSource a in soundsObject.GetComponents<AudioSource>())
        {
            Sounds.Add(a.clip.name, a);
        }

	}
	
	// Update is called once per frame
	void Update ()
	{
	}

    public void ToggleOpen(bool open)
    {
        transform.FindChild("DoorOpen").GetComponent<SpriteRenderer>().enabled = false;
        transform.FindChild("DoorClosed").GetComponent<SpriteRenderer>().enabled = false;

        if (open)
        {
            IsOpen = true;
            transform.FindChild("DoorOpen").GetComponent<SpriteRenderer>().enabled = true;
            Sounds["Cupboard-Open"].Play();
        }
        else
        {
            IsOpen = false;
            transform.FindChild("DoorClosed").GetComponent<SpriteRenderer>().enabled = true;
            Sounds["Cupboard-Close"].Play();
        }
    }


}

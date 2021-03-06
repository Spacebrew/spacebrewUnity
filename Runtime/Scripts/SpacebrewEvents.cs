﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpacebrewEvents : MonoBehaviour
{

    public SpacebrewClient sbClient;

    // Use this for initialization
    public void Start()
    {
        if (sbClient == null)
        {
            GameObject go = GameObject.Find("SpacebrewObject"); // the name of your client object
            sbClient = go.GetComponent<SpacebrewClient>();
        }

        if (sbClient != null)
        {
            // register an event with the client and a callback function here.
            // COMMON GOTCHA: THIS MUST MATCH THE NAME VALUE YOU TYPED IN THE EDITOR!!
            sbClient.AddListenerTo("mystring", "string", OnSpacebrewEvent);
        }
    }

    // Update is called once per frame
    public void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            print("Sending Spacebrew Message");
            // name, type, value
            // COMMON GOTCHA: THIS MUST MATCH THE NAME VALUE YOU TYPED IN THE EDITOR!!
            sbClient.sendMessage("mybool", "boolean", "true");
        }
    }

    public void OnSpacebrewEvent(SpacebrewClient.SpacebrewMessage _msg)
    {
        print("Received Spacebrew Message");
        print(_msg.value);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomEventHandler : MonoBehaviour {

	public SpacebrewClient sbClient;

	// Use this for initialization
	void Start () {
		
	}
	
	public void OnMessage(SpacebrewClient.SpacebrewMessage message){
    Debug.Log(message.value);
  }
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown ("space")) {
			print ("Sending Spacebrew Message");
			// name, type, value
			// COMMON GOTCHA: THIS MUST MATCH THE NAME VALUE YOU TYPED IN THE EDITOR!!
			sbClient.sendMessage("mybool", "boolean", "true");
}
	}
}

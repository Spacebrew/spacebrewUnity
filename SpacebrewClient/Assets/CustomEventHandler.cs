using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomEventHandler : MonoBehaviour {

	public SpacebrewClient sbClient;

	// Use this for initialization
	void Start () {
		
	}
	
	public void OnMyMessage(SpacebrewClient.SpacebrewMessage message){
    Debug.LogFormat(
			"[OnMyMessage] {0}({1}): {2}",
			message.name,
			message.type,
			message.value);
  }

  public void OnRangeMessage(SpacebrewClient.SpacebrewMessage message)
  {
    if (message.type == "range")
    {
      Debug.LogFormat(
        "[OnSomeMessage] {0}({1}): {2}",
        message.name,
        message.type,
        message.valueNode.AsInt);
    }
  }

  //Update is called once per frame
  void Update () {
		if (Input.GetKeyDown ("space")) {
			print ("Sending Spacebrew Message");
			// name, type, value
			// COMMON GOTCHA: THIS MUST MATCH THE NAME VALUE YOU TYPED IN THE EDITOR!!
			sbClient.sendMessage("mybool", "boolean", "true");
		}
	}
}

spacebrewUnity
==============

A Spacebrew client library for Unity3D. 

## Usage

This is a Unity Package which can be imported into your Unity project. Use the Unity Package Manager to import this git repo into your project ([Detailed instructions](#import-details))

__SpacebrewClient__: This is where you configure your Spacebrew client to point to your desired server and setup your publishers and subscribers.

![Alt text](/screenshots/Capture.PNG "Spacebrew Client")

__SpacebrewEvents__: This is what you attach to your game object to have it send and receive Spacebrew events.


	SpacebrewClient sbClient;

	// Use this for initialization
	void Start () {
		GameObject go = GameObject.Find ("SpacebrewObject"); // the name of your client object
		sbClient = go.GetComponent <SpacebrewClient> ();

		// register an event with the client and a callback function here.
		// COMMON GOTCHA: THIS MUST MATCH THE NAME VALUE YOU TYPED IN THE EDITOR!!
		sbClient.addEventListener (this.gameObject, "mystring");
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

	public void OnSpacebrewEvent(SpacebrewClient.SpacebrewMessage _msg) {
		print ("Received Spacebrew Message");
		print (_msg.name);
		print (_msg.value);
	}


### Import Details

Once you have your Unity project open:

* go to _Window -> Package Manager_ 
* click on the __+__ in the upper left corner of the Package Manager window 
* select _Add package from git URL_
* Enter `https://github.com/spacebrew/spacebrewUnity.git` into the  prompt and click _Add_
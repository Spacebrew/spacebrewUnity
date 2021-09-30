using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WebSocketSharp;
using System;
using System.Reflection;
using SimpleJSON;
using UnityEngine.Events;

public class SpacebrewClient : MonoBehaviour
{

    public enum type
    {
        BOOLEAN = 0,
        STRING = 1,
        RANGE = 2,
        CUSTOM = 3
    };

    [Serializable]
    public class SpacebrewMessageEvent : UnityEvent<SpacebrewClient.SpacebrewMessage> { }

    [Serializable]
    public class Publisher
    {
        public string name;
        public type pubType;
#if UNITY_EDITOR
        [SBCustomType("pubType", false)]
#endif
        public string customType;
        //public string defaultValue;

        private string _typeString;

        public string typeString
        {
            get
            {
                if (_typeString == null)
                {
                    _typeString = TypeToString(pubType, customType);
                }
                return _typeString;
            }
        }
    }

    [Serializable]
    public class Subscriber
    {
        public string name;
        public type subType;
#if UNITY_EDITOR
        [SBCustomType("subType", false)]
#endif
        public string customType;
        public SpacebrewMessageEvent onReceived;

        private string _typeString;

        public string typeString
        {
            get
            {
                if (_typeString == null)
                {
                    _typeString = TypeToString(subType, customType);
                }
                return _typeString;
            }
        }
    }

    public class SpacebrewMessage
    {
        public string name;
        public string type;
        public string value;
        public JSONNode valueNode;
        public string clientName;
    }

    public class SpacebrewEvent
    {
        public GameObject sbGo;
        public string sbEvent;
    }

    public WebSocket conn;
    public bool debugLogging = false;
    public bool autoconnect = false;
    public string serverAddress; // you can include the port number so ws://192.168.7.2:9000
    public Publisher[] publishers;
    public Subscriber[] subscribers;
    public string clientName;
    public string descriptionText;
    public ArrayList SpacebrewEvents;
    Queue<SpacebrewMessage> spacebrewMsgs = new Queue<SpacebrewMessage>();

    private bool attemptingReconnect;

    private static string TypeToString(type type, string customType)
    {
        switch (type)
        {
            case type.BOOLEAN:
                return "boolean";
            case type.STRING:
                return "string";
            case type.RANGE:
                return "range";
            case type.CUSTOM:
                if (string.IsNullOrEmpty(customType))
                {
                    Debug.LogWarning("[Subscriber.typeString] "
                        + "\"Custom Type\" field must be filled in "
                        + "when CUSTOM is selected from type list");
                    return "";
                }
                else
                {
                    return customType;
                }
            default:
                Debug.LogWarning("[Subscriber.typeString] "
                    + "unrecognized type: " + type);
                return "";
        }
    }

    void Awake()
    {
        if (autoconnect)
        {
            Connect();
        }
    }

    public void Connect()
    {
        if (conn == null)
        {
            conn = new WebSocket(serverAddress); // removed WebSocket on begin
            conn.OnOpen += (sender, e) =>
            {
                if (debugLogging)
                {
                    Debug.Log("Attempting to open socket");
                }
            };

            conn.OnMessage += (sender, e) =>
            {
                if (debugLogging)
                {
                    print(e.Data);
                }

                // parse the incoming json message from spacebrew
                var N = JSON.Parse(e.Data);
                var cMsg = new SpacebrewMessage();
                cMsg.name = N["message"]["name"];
                cMsg.type = N["message"]["type"];
                cMsg.value = N["message"]["value"];
                cMsg.valueNode = N["message"]["value"];
                cMsg.clientName = N["message"]["clientName"];

                //var cMsg = JsonUtility.FromJson<SpacebrewMessage>(e.Data);

                if (debugLogging)
                {
                    print(cMsg);
                }
                spacebrewMsgs.Enqueue(cMsg);
                //ProcessSpacebrewMessage(cMsg);

                //			if (e.Type == Opcode.Text) {
                //				// Do something with e.Data
                //				print (e);
                //				print (e.Data);
                //				return;
                //			}
                //			
                //			if (e.Type == Opcode.Binary) {
                //				// Do something with e.RawData
                //				return;
                //			}

            };

            conn.OnError += (sender, e) =>
            {
                Debug.LogWarningFormat(
                    "THERE WAS AN ERROR CONNECTING {0}",
                    e.Message);
            };

            conn.OnClose += (sender, e) =>
            {
                print("Connection closed");
            };

            if (debugLogging)
            {
                print("Attemping to connect to " + serverAddress);
            }
            conn.Connect();

            // Connect and send the configuration for the app to Spacebrew
            if (conn.ReadyState == WebSocketState.OPEN)
            {
                conn.Send(makeConfig());
            }
        }
    }

    // You can use these to programatically add publisher and subsribers
    // otherwise you should do it through the editor interface.
    void addPublisher(string _name, string _type, string _default)
    {
        //var P = new JSONObject();
        //P ["name"] = _name;
        //P ["type"] = _type;
        //		if (_default != "") {
        //			P ["default"] = _default;
        //		}
        //publishers.Add(P);
    }

    void addSubscriber(string _name, string _type)
    {
        //var S = new JSONObject();
        //S ["name"] = _name;
        //S ["type"] = _type;
        //subscribers.Add(S);
    }

    private Subscriber GetSubscriber(string name, string type)
    {
        for (int i = subscribers.Length - 1; i >= 0; i--)
        {
            if (subscribers[i].name == name && subscribers[i].typeString == type)
            {
                return subscribers[i];
            }
        }
        return null;
    }

    public void AddListenerTo(string name, string type, UnityAction<SpacebrewMessage> callback)
    {
        Subscriber sub = GetSubscriber(name, type);
        if (sub != null)
        {
            sub.onReceived.AddListener(callback);
        }
        else
        {
            Debug.LogWarningFormat(
                "[SpacebrewClient.AddListenerTo] Did not find {0}, {1}",
                name,
                type);
        }
    }

    public void RemoveListenerFrom(string name, string type, UnityAction<SpacebrewMessage> callback)
    {
        Subscriber sub = GetSubscriber(name, type);
        if (sub != null)
        {
            sub.onReceived.RemoveListener(callback);
        }
    }

    // making the json into a sring
    private String makeConfig()
    {
        // Begin the JSON config
        var I = new JSONObject();
        I["name"] = clientName;
        I["description"] = descriptionText;

        // Add all the publishers
        for (int i = 0; i < publishers.Length; i++) // Loop through List with for
        {
            if (string.IsNullOrEmpty(publishers[i].typeString))
            {
                continue;
            }
            else
            {
                var O = new JSONObject();
                O["name"] = publishers[i].name;
                O["type"] = publishers[i].typeString;
                O["default"] = "";
                I["publish"]["messages"][-1] = O;
            }
        }

        // Add all the subscribers
        for (int i = 0; i < subscribers.Length; i++) // Loop through List with for
        {
            if (string.IsNullOrEmpty(subscribers[i].typeString))
            {
                continue;
            }
            else
            {
                var Q = new JSONObject();
                Q["name"] = subscribers[i].name;
                Q["type"] = subscribers[i].typeString;
                I["subscribe"]["messages"][-1] = Q;
            }
        }


        // Add everything to config
        var C = new JSONObject();
        C["config"] = I;

        if (debugLogging)
        {
            Debug.LogFormat("Connection: {0}", C.ToString());
        }

        return C.ToString();
    }

    void onOpen()
    {

    }

    void onClose()
    {

    }

    public void sendMessage(string _name, string _type, object _value)
    {
        string valueText = JsonUtility.ToJson(_value);
        JSONNode valueNode = JSON.Parse(valueText);
        sendMessage(_name, _type, valueNode);
    }

    public void sendMessage(string _name, string _type, JSONNode _value)
    {
        var M = new JSONObject();
        M["clientName"] = clientName;
        M["name"] = _name;
        M["type"] = _type;
        M["value"] = _value;

        var MS = new JSONObject();
        MS["message"] = M;
        conn.Send(MS.ToString());
    }

    public void sendMessage(string _name, bool _value)
    {
        var M = new JSONObject();
        M["clientName"] = clientName;
        M["name"] = _name;
        M["type"] = "boolean";
        M["value"] = _value;

        var MS = new JSONObject();
        MS["message"] = M;
        conn.Send(MS.ToString());
        //        conn.Send (makeConfig().ToString());

        //       {
        //         "message":{
        //           "clientName":"CLIENT NAME (Must match the name in the config statement)",
        //           "name":"PUBLISHER NAME (outgoing messages), SUBSCRIBER NAME (incoming messages)",
        //           "type":"DATA TYPE",
        //           "value":"VALUE",
        //       }
        //   }
    }

    public void sendMessage(string _name, string _type, string _value)
    {
        var M = new JSONObject();
        M["clientName"] = clientName;
        M["name"] = _name;
        M["type"] = _type;
        M["value"] = _value;

        var MS = new JSONObject();
        MS["message"] = M;
        conn.Send(MS.ToString());
        //        conn.Send (makeConfig().ToString());

        //       {
        //         "message":{
        //           "clientName":"CLIENT NAME (Must match the name in the config statement)",
        //           "name":"PUBLISHER NAME (outgoing messages), SUBSCRIBER NAME (incoming messages)",
        //           "type":"DATA TYPE",
        //           "value":"VALUE",
        //       }
        //   }

    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        // go through new messages
        while (spacebrewMsgs.Count != 0)
        {
            SpacebrewMessage element = spacebrewMsgs.Dequeue();
            try
            {
                Subscriber sub = GetSubscriber(element.name, element.type);
                if (sub != null)
                {
                    sub.onReceived.Invoke(element);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[SpacebrewClient.Update] exception while processing Spacebrew message: " + e.Message + " " + e.StackTrace);
            }
        }

        //check to see if connection has died, connect if so
        if (conn != null && conn.ReadyState != WebSocketState.OPEN && !attemptingReconnect)
        {
            StartCoroutine("AttemptWebsocketReconnect");
        }
    }

    private IEnumerator AttemptWebsocketReconnect()
    {
        attemptingReconnect = true;

        float timer = 0.1f;
        float maxInterval = 3.0f;
        while (conn.ReadyState != WebSocketState.OPEN)
        {
            Debug.LogWarning("Attempting to Reconnect");
            conn.ConnectAsync();
            yield return new WaitForSeconds(timer);
            if (timer < maxInterval)
            {
                timer *= 2.0f; // exponential backoff
            }
            else
            {
                timer = maxInterval;
            }
        }
        conn.Send(makeConfig().ToString());

        attemptingReconnect = false;
    }

    private void OnDestroy()
    {
        if (conn != null)
        {
            conn.CloseAsync();
        }
    }
}


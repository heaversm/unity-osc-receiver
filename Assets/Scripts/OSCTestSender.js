

private var oscHandler: Osc = null;
public var controller : Transform;
public var gameReceiver = "Cube"; //the tag of the object on stage that you want to manipulate
public var remoteIp : String;
public var sendToPort : int;
public var listenerPort : int;
private var yRot : int = 0; //the rotation around the y axis



// Start is called just before any of the Update methods is called the first time.
public function Start()
{

    var udp:UDPPacketIO  = GetComponent("UDPPacketIO");
    udp.init(remoteIp, sendToPort, listenerPort);

    oscHandler = GetComponent("Osc");
    oscHandler.init(udp);

    //oscHandler.SetAddressHandler("/1/push1", Example);
}

public function Example(m)
{
    Debug.Log("--------------> OSC example message received: ("+m+")");
}

// Update is called every frame, if the MonoBehaviour is enabled.
function Update()
{
    //Debug.LogWarning("time = " + Time.time);

    var oscM : OscMessage = null;

    if (Input.GetKey(KeyCode.W))
    {
        oscM = Osc.StringToOscMessage("/1/push1");
        oscHandler.Send(oscM);

        yRot = 1;
    } else if (Input.GetKey(KeyCode.S)){
        oscM = Osc.StringToOscMessage("/1/push4");
        oscHandler.Send(oscM);
        yRot = -1;
    } else {
        yRot = 0;
    }

    var go = GameObject.Find(gameReceiver);
    go.transform.Rotate(0, yRot, 0);

}

function OnDisable()
{
    // close OSC UDP socket
    Debug.Log("closing OSC UDP socket in OnDisable");
    oscHandler.Cancel();
    oscHandler = null;
}



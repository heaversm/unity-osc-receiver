

private var oscHandler: Osc = null;
public var controller : Transform;
public var gameReceiver = "Cube"; //the tag of the object on stage that you want to manipulate
public var remoteIp : String = "127.0.0.1";
public var sendToPort : int = 8000;
public var listenerPort : int = 0;



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
    var go = GameObject.Find(gameReceiver);

    if (Input.GetKey(KeyCode.A))
    {
        oscM = Osc.StringToOscMessage("/1/push1");
        oscHandler.Send(oscM);
        go.transform.Rotate(0, 1, 0);
    } else if (Input.GetKey(KeyCode.D)){
        oscM = Osc.StringToOscMessage("/1/push2");
        oscHandler.Send(oscM);
        go.transform.Rotate(0, -1, 0);
    } else if (Input.GetKey(KeyCode.W)){
        oscM = Osc.StringToOscMessage("/1/push3");
        oscHandler.Send(oscM);
        go.transform.Rotate(1, 0, 0);
    } else if (Input.GetKey(KeyCode.S)){
        oscM = Osc.StringToOscMessage("/1/push4");
        oscHandler.Send(oscM);
        go.transform.Rotate(-1, 0, 0);
    } else if (Input.GetKey(KeyCode.Z)){
        oscM = Osc.StringToOscMessage("/1/push5");
        oscHandler.Send(oscM);
        go.transform.Rotate(0, 0, -1);
    } else if (Input.GetKey(KeyCode.X)){
        oscM = Osc.StringToOscMessage("/1/push6");
        oscHandler.Send(oscM);
        go.transform.Rotate(0, 0, 1);
    }  else if (Input.GetKey(KeyCode.Q)){
        oscM = Osc.StringToOscMessage("/1/push7");
        oscHandler.Send(oscM);
        go.transform.Translate(1,0,0);
    } else if (Input.GetKey(KeyCode.E)){
        oscM = Osc.StringToOscMessage("/1/push8");
        oscHandler.Send(oscM);
        go.transform.Translate(-1,0,0);
    } else if (Input.GetKey(KeyCode.C)){
        oscM = Osc.StringToOscMessage("/1/push9");
        oscHandler.Send(oscM);
        go.transform.localScale += new Vector3(.01F,.01F,.01F);
    } else {
        //
    }


}

function OnDisable()
{
    // close OSC UDP socket
    Debug.Log("closing OSC UDP socket in OnDisable");
    oscHandler.Cancel();
    oscHandler = null;
}



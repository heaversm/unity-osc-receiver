
public var RemoteIP : String = "127.0.0.1"; //127.0.0.1 signifies a local host (if testing locally
public var SendToPort : int = 9000; //the port you will be sending from
public var ListenerPort : int = 8000; //the port you will be listening on
public var controller : Transform;
public var gameReceiver = "Cube"; //the tag of the object on stage that you want to manipulate
public var gameReceiver2 = "Sphere";
private var handler : Osc;

//VARIABLES YOU WANT TO BE ANIMATED
private var xRot : int = 0; //the rotation around the x axis
private var yRot : int = 0; //the rotation around the y axis
private var zRot : int = 0; //the rotation around the z axis
private var scaleVal : float = 1;
private var xVal: float = 0;

private var scaleValX: int = 1;
private var scaleValY: int = 1;
private var scaleValZ: int = 1;

public function Start ()
{
	//Initializes on start up to listen for messages
	//make sure this game object has both UDPPackIO and OSC script attached

	var udp : UDPPacketIO = GetComponent("UDPPacketIO");
	udp.init(RemoteIP, SendToPort, ListenerPort);
	handler = GetComponent("Osc");
	handler.init(udp);
	handler.SetAllMessageHandler(AllMessageHandler);

}
Debug.Log("Running");

function Update () {
	var go = GameObject.Find(gameReceiver);
	var go2 = GameObject.Find(gameReceiver2);
	go.transform.Rotate(xRot, yRot, zRot);
	go.transform.localScale = new Vector3(scaleVal, scaleVal, scaleVal);
	go.transform.Translate(xVal,0,0,Space.World);
	go2.transform.localScale = new Vector3(scaleValX,scaleValY,scaleValZ);
}

//These functions are called when messages are received
//Access values via: oscMessage.Values[0], oscMessage.Values[1], etc

public function AllMessageHandler(oscMessage: OscMessage){


	var msgString = Osc.OscMessageToString(oscMessage); //the message and value combined
	var msgAddress = oscMessage.Address; //the message parameters
	var msgValue = oscMessage.Values[0]; //the message value
	Debug.Log(msgAddress); //log the message and values coming from OSC

	//FUNCTIONS YOU WANT CALLED WHEN A SPECIFIC MESSAGE IS RECEIVED
	switch (msgAddress){
		case "/1/push1":
			xRot = msgValue;
			break;
		case "/1/push2":
			yRot = msgValue;
			break;
		case "/1/push3":
			zRot = msgValue;
			break;
		case "/1/fader1":
			scaleVal = 1+msgValue;
			break;
		case "/1/fader2":
			xVal = -.5+msgValue;
			break;
		case "/1/push7":
			scaleValX = 1+msgValue;
			break;
		case "/1/push8":
			scaleValY = 1+msgValue;
			break;
		case "/1/push9":
			scaleValZ = 1+msgValue;
			break;
		default:
			//
			break;
	}

}


//FUNCTIONS CALLED BY MATCHING A SPECIFIC MESSAGE IN THE ALLMESSAGEHANDLER FUNCTION
public function Rotate(msgValue) : void //rotate the cube around its axis
{
	yRot = msgValue;
}


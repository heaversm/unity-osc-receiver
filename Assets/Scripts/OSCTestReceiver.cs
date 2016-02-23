using UnityEngine;
using System.Collections;
using System.Collections.Generic;



// Simple OSC test communication script
[AddComponentMenu("Scripts/OSCTestReceiver")]
public class OSCTestReceiver : MonoBehaviour
{

    private Osc oscHandler;

	public string remoteIp;
    public int sendToPort;
    public int listenerPort;


    ~OSCTestReceiver()
    {
        if (oscHandler != null)
        {
            oscHandler.Cancel();
        }

        // speed up finalization
        oscHandler = null;
        System.GC.Collect();
    }

    // Update is called every frame, if the MonoBehaviour is enabled.
    void Update()
    {
 
    }


    void Awake()
    {

    }

    void OnDisable()
    {
        // close OSC UDP socket
        Debug.Log("closing OSC UDP socket in OnDisable");
        oscHandler.Cancel();
        oscHandler = null;
    }


    // Start is called just before any of the Update methods is called the first time.
    void Start()
    {

        UDPPacketIO udp = GetComponent<UDPPacketIO>();
        udp.init(remoteIp, sendToPort, listenerPort);

	    oscHandler = GetComponent<Osc>();
        oscHandler.init(udp);

        oscHandler.SetAllMessageHandler(AllMessageHandler);
    }
		
	public static void AllMessageHandler(OscMessage m){
		print(m);
	}
}

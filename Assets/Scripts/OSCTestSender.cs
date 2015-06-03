using UnityEngine;
using System.Collections;
using System.Collections.Generic;



// Simple OSC test communication script
[AddComponentMenu("Scripts/OSCTestSender")]
public class OSCTestSender : MonoBehaviour
{

    private Osc oscHandler;

    public string remoteIp;
    public int sendToPort;
    public int listenerPort;


    ~OSCTestSender()
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
        //Debug.LogWarning("time = " + Time.time);

        OscMessage oscM = Osc.StringToOscMessage("/1/push1");
        oscHandler.Send(oscM);
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

        oscHandler.SetAddressHandler("/hand1", Example);
    }

    public static void Example(OscMessage m)
    {
        Debug.Log("--------------> OSC example message received: ("+m+")");
    }



}

using System;
using System.IO;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;


  // UdpPacket provides packetIO over UDP
  public class UDPPacketIO : MonoBehaviour
  {
    private UdpClient Sender;
    private UdpClient Receiver;
    private bool socketsOpen;
    private string remoteHostName;
    private int remotePort;
    private int localPort;

    void Start()
    {
        //do nothing. init must be called
    }

  	public void init(string hostIP, int remotePort, int localPort){
        RemoteHostName = hostIP;
        RemotePort = remotePort;
        LocalPort = localPort;
        socketsOpen = false;
  	}


    ~UDPPacketIO()
    {
        // latest time for this socket to be closed
        if (IsOpen())
            Close();
    }


    // Open a UDP socket and create a UDP sender.

    //returns - True on success, false on failure.</returns>
    public bool Open()
    {
        try
        {
            Sender = new UdpClient();
            //Debug.Log("opening udpclient listener on port " + localPort);

            IPEndPoint listenerIp = new IPEndPoint(IPAddress.Any, localPort);
            Receiver = new UdpClient(listenerIp);
            socketsOpen = true;

            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("cannot open udp client interface at port "+localPort);
            Debug.LogWarning(e);
        }

        return false;
    }

    // Close the socket currently listening, and destroy the UDP sender device.
    public void Close()
    {
        if(Sender != null)
            Sender.Close();

        if (Receiver != null)
        {
            Receiver.Close();
            // Debug.Log("UDP receiver closed");
        }
        Receiver = null;
        socketsOpen = false;

    }

    public void OnDisable()
    {
        Close();
    }

    // Query the open state of the UDP socket.
    // returns - True if open, false if closed.
    public bool IsOpen()
    {
      return socketsOpen;
    }

    // Send a packet of bytes out via UDP.
    // packet - The packet of bytes to be sent.
    // length - The length of the packet of bytes to be sent.
    public void SendPacket(byte[] packet, int length)
    {
        if (!IsOpen())
            Open();
        if (!IsOpen())
            return;

        Sender.Send(packet, length, remoteHostName, remotePort);
        //Debug.Log("osc message sent to "+remoteHostName+" port "+remotePort+" len="+length);
    }


    // Receive a packet of bytes over UDP.

    // buffer - The buffer to be read into.
    // returns - The number of bytes read, or 0 on failure
    public int ReceivePacket(byte[] buffer)
    {
        if (!IsOpen())
            Open();
        if (!IsOpen())
            return 0;

      IPEndPoint iep = new IPEndPoint(IPAddress.Any, localPort);
      byte[] incoming = Receiver.Receive( ref iep );
      int count = Math.Min(buffer.Length, incoming.Length);
      System.Array.Copy(incoming, buffer, count);
      return count;
    }

    // The address of the board that you're sending to.
    public string RemoteHostName
    {
      get
      {
        return remoteHostName;
      }
      set
      {
        remoteHostName = value;
      }
    }

    // The remote port that you're sending to.
    public int RemotePort
    {
      get
      {
        return remotePort;
      }
      set
      {
        remotePort = value;
      }
    }

    // The local port you're listening on.
    public int LocalPort
    {
      get
      {
        return localPort;
      }
      set
      {
        localPort = value;
      }
    }
}

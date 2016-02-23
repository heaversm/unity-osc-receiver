using UnityEngine;
using System.Threading;
using System.Text;
using System.Collections;
using System.IO;
using System;

  //The OscMessage class is a data structure that represents an OSC address and an arbitrary number of values to be sent to that address.
  public class OscMessage
  {

   // The OSC address of the message as a string.
   public string Address;

   // The list of values to be delivered to the Address.
   public ArrayList Values;
    public OscMessage()
    {
      Values = new ArrayList();
    }
  }

  public delegate void OscMessageHandler( OscMessage oscM );

  public class Osc : MonoBehaviour
  {
      private UDPPacketIO OscPacketIO;
      Thread ReadThread;
      private bool ReaderRunning;
      private OscMessageHandler AllMessageHandler;
      Hashtable AddressTable;


	void Start() {
		//do nothing, init must be called
	}

	public void init(UDPPacketIO oscPacketIO){
	  OscPacketIO = oscPacketIO;

      // Create the hashtable for the address lookup mechanism
      AddressTable = new Hashtable();

      ReadThread = new Thread(Read);
      ReaderRunning = true;
      ReadThread.IsBackground = true;
      ReadThread.Start();
	}


    // Make sure the PacketExchange is closed.
    ~Osc()
    {
    	if (ReaderRunning) Cancel();
        //Debug.LogError("~Osc");
    }

    public void Cancel()
    {
        //Debug.Log("Osc Cancel start");
        if (ReaderRunning)
        {
            ReaderRunning = false;
            ReadThread.Abort();
        }
        if (OscPacketIO != null && OscPacketIO.IsOpen())
        {
            OscPacketIO.Close();
            OscPacketIO = null;
        }
        //Debug.Log("Osc Cancel finished");
    }


    // Read Thread.  Loops waiting for packets.  When a packet is received, it is
    // dispatched to any waiting All Message Handler.  Also, the address is looked up and
    // any matching handler is called.
    private void Read()
    {
        try
        {
            while (ReaderRunning)
            {
                byte[] buffer = new byte[1000];
                int length = OscPacketIO.ReceivePacket(buffer);
                //Debug.Log("received packed of len=" + length);
                if (length > 0)
                {
                    ArrayList messages = Osc.PacketToOscMessages(buffer, length);
                    foreach (OscMessage om in messages)
                    {
                        if (AllMessageHandler != null)
                            AllMessageHandler(om);
                        OscMessageHandler h = (OscMessageHandler)Hashtable.Synchronized(AddressTable)[om.Address];
                        if (h != null)
                            h(om);
                    }
                }
                else
                    Thread.Sleep(20);
            }
        }
        catch (Exception e)
        {
            //Debug.Log("ThreadAbortException"+e);
        }
        finally
        {
            //Debug.Log("terminating thread - clearing handlers");
            //Cancel();
            //Hashtable.Synchronized(AddressTable).Clear();
        }

    }


    // Send an individual OSC message.  Internally takes the OscMessage object and
    // serializes it into a byte[] suitable for sending to the PacketIO.
    public void Send( OscMessage oscMessage )
    {
      byte[] packet = new byte[1000];
      int length = Osc.OscMessageToPacket( oscMessage, packet, 1000 );
      OscPacketIO.SendPacket( packet, length);
    }


    // Sends a list of OSC Messages.  Internally takes the OscMessage objects and
    // serializes them into a byte[] suitable for sending to the PacketExchange.

    //oms - The OSC Message to send.
    public void Send(ArrayList oms)
    {
      byte[] packet = new byte[1000];
      int length = Osc.OscMessagesToPacket(oms, packet, 1000);
      OscPacketIO.SendPacket(packet, length);
    }


    // Set the method to call back on when any message is received.
    // The method needs to have the OscMessageHandler signature - i.e. void amh( OscMessage oscM )

    // amh - The method to call back on.
    public void SetAllMessageHandler(OscMessageHandler amh)
    {
      AllMessageHandler = amh;
    }


    // Set the method to call back on when a message with the specified
    // address is received.  The method needs to have the OscMessageHandler signature - i.e.
    // void amh( OscMessage oscM )

    // key - Address string to be matched
    // ah - he method to call back on.
    public void SetAddressHandler(string key, OscMessageHandler ah)
    {
      Hashtable.Synchronized(AddressTable).Add(key, ah);
    }
		

    // General static helper that returns a string suitable for printing representing the supplied
    // OscMessage.

    //  message - The OscMessage to be stringified
    // returns The OscMessage as a string.
    public static string OscMessageToString(OscMessage message)
    {
      StringBuilder s = new StringBuilder();
      s.Append(message.Address);
      foreach( object o in message.Values )
      {
        s.Append(" ");
        s.Append(o.ToString());
      }
      return s.ToString();
    }


    // Creates an OscMessage from a string - extracts the address and determines each of the values.

    // message - The string to be turned into an OscMessage
    // returns - The OscMessage
    public static OscMessage StringToOscMessage(string message)
    {
      OscMessage oM = new OscMessage();
      // Console.WriteLine("Splitting " + message);
      string[] ss = message.Split(new char[] { ' ' });
      IEnumerator sE = ss.GetEnumerator();
      if (sE.MoveNext())
        oM.Address = (string)sE.Current;
      while ( sE.MoveNext() )
      {
        string s = (string)sE.Current;
        // Console.WriteLine("  <" + s + ">");
        if (s.StartsWith("\""))
        {
          StringBuilder quoted = new StringBuilder();
          bool looped = false;
          if (s.Length > 1)
            quoted.Append(s.Substring(1));
          else
            looped = true;
          while (sE.MoveNext())
          {
            string a = (string)sE.Current;
            // Console.WriteLine("    q:<" + a + ">");
            if (looped)
              quoted.Append(" ");
            if (a.EndsWith("\""))
            {
              quoted.Append(a.Substring(0, a.Length - 1));
              break;
            }
            else
            {
              if (a.Length == 0)
                quoted.Append(" ");
              else
                quoted.Append(a);
            }
            looped = true;
          }
          oM.Values.Add(quoted.ToString());
        }
        else
        {
          if (s.Length > 0)
          {
            try
            {
              int i = int.Parse(s);
              // Console.WriteLine("  i:" + i);
              oM.Values.Add(i);
            }
            catch
            {
              try
              {
                float f = float.Parse(s);
                // Console.WriteLine("  f:" + f);
                oM.Values.Add(f);
              }
              catch
              {
                // Console.WriteLine("  s:" + s);
                oM.Values.Add(s);
              }
            }

          }
        }
      }
      return oM;
    }


    // Takes a packet (byte[]) and turns it into a list of OscMessages.

    //packet - The packet to be parsed
    // length - The length of the packet.
    // returns - An ArrayList of OscMessages.
    public static ArrayList PacketToOscMessages(byte[] packet, int length)
    {
      ArrayList messages = new ArrayList();
      ExtractMessages(messages, packet, 0, length);
      return messages;
    }


    // Puts an array of OscMessages into a packet (byte[]).
    public static int OscMessagesToPacket(ArrayList messages, byte[] packet, int length)
    {
      int index = 0;
      if (messages.Count == 1)
        index = OscMessageToPacket((OscMessage)messages[0], packet, 0, length);
      else
      {
        // Write the first bundle bit
        index = InsertString("#bundle", packet, index, length);
        // Write a null timestamp (another 8bytes)
        int c = 8;
        while (( c-- )>0)
          packet[index++]++;
        // Now, put each message preceded by it's length
        foreach (OscMessage oscM in messages)
        {
          int lengthIndex = index;
          index += 4;
          int packetStart = index;
          index = OscMessageToPacket(oscM, packet, index, length);
          int packetSize = index - packetStart;
          packet[lengthIndex++] = (byte)((packetSize >> 24) & 0xFF);
          packet[lengthIndex++] = (byte)((packetSize >> 16) & 0xFF);
          packet[lengthIndex++] = (byte)((packetSize >> 8) & 0xFF);
          packet[lengthIndex++] = (byte)((packetSize) & 0xFF);
        }
      }
      return index;
    }


    // Creates a packet (an array of bytes) from a single OscMessage.

    // oscM - The OscMessage to be returned as a packet.
    //packet - The packet to be populated with the OscMessage.
    //length - The usable size of the array of bytes.
    //returns - The length of the packet
    public static int OscMessageToPacket(OscMessage oscM, byte[] packet, int length)
    {
      return OscMessageToPacket(oscM, packet, 0, length);
    }


    // Creates an array of bytes from a single OscMessage.  Used internally.
    private static int OscMessageToPacket(OscMessage oscM, byte[] packet, int start, int length)
    {
      int index = start;
      index = InsertString(oscM.Address, packet, index, length);
      //if (oscM.Values.Count > 0)
      {
        StringBuilder tag = new StringBuilder();
        tag.Append(",");
        int tagIndex = index;
        index += PadSize(2 + oscM.Values.Count);

        foreach (object o in oscM.Values)
        {
          if (o is int)
          {
            int i = (int)o;
            tag.Append("i");
            packet[index++] = (byte)((i >> 24) & 0xFF);
            packet[index++] = (byte)((i >> 16) & 0xFF);
            packet[index++] = (byte)((i >> 8) & 0xFF);
            packet[index++] = (byte)((i) & 0xFF);
          }
          else
          {
            if (o is float)
            {
              float f = (float)o;
              tag.Append("f");
              byte[] buffer = new byte[4];
              MemoryStream ms = new MemoryStream(buffer);
              BinaryWriter bw = new BinaryWriter(ms);
              bw.Write(f);
              packet[index++] = buffer[3];
              packet[index++] = buffer[2];
              packet[index++] = buffer[1];
              packet[index++] = buffer[0];
            }
            else
            {
              if (o is string)
              {
                tag.Append("s");
                index = InsertString(o.ToString(), packet, index, length);
              }
              else
              {
                tag.Append("?");
              }
            }
          }
        }
        InsertString(tag.ToString(), packet, tagIndex, length);
      }
      return index;
    }


    // Receive a raw packet of bytes and extract OscMessages from it.  Used internally.
    private static int ExtractMessages(ArrayList messages, byte[] packet, int start, int length)
    {
      int index = start;
      switch ( (char)packet[ start ] )
      {
        case '/':
          index = ExtractMessage( messages, packet, index, length );
          break;
        case '#':
          string bundleString = ExtractString(packet, start, length);
          if ( bundleString == "#bundle" )
          {
            // skip the "bundle" and the timestamp
            index+=16;
            while ( index < length )
            {
              int messageSize = ( packet[index++] << 24 ) + ( packet[index++] << 16 ) + ( packet[index++] << 8 ) + packet[index++];
              /*int newIndex = */ExtractMessages( messages, packet, index, length );
              index += messageSize;
            }
          }
          break;
      }
      return index;
    }


    // Extracts a messages from a packet.
    private static int ExtractMessage(ArrayList messages, byte[] packet, int start, int length)
    {
      OscMessage oscM = new OscMessage();
      oscM.Address = ExtractString(packet, start, length);
      int index = start + PadSize(oscM.Address.Length+1);
      string typeTag = ExtractString(packet, index, length);
      index += PadSize(typeTag.Length + 1);
      //oscM.Values.Add(typeTag);
      foreach (char c in typeTag)
      {
        switch (c)
        {
          case ',':
            break;
          case 's':
            {
              string s = ExtractString(packet, index, length);
              index += PadSize(s.Length + 1);
              oscM.Values.Add(s);
              break;
            }
          case 'i':
            {
              int i = ( packet[index++] << 24 ) + ( packet[index++] << 16 ) + ( packet[index++] << 8 ) + packet[index++];
              oscM.Values.Add(i);
              break;
            }
          case 'f':
            {
              byte[] buffer = new byte[4];
              buffer[3] = packet[index++];
              buffer[2] = packet[index++];
              buffer[1] = packet[index++];
              buffer[0] = packet[index++];
              MemoryStream ms = new MemoryStream(buffer);
              BinaryReader br = new BinaryReader(ms);
              float f = br.ReadSingle();
              oscM.Values.Add(f);
              break;
            }
        }
      }
      messages.Add( oscM );
      return index;
    }


    // Removes a string from a packet.  Used internally.
    private static string ExtractString(byte[] packet, int start, int length)
    {
      StringBuilder sb = new StringBuilder();
      int index = start;
      while (packet[index] != 0 && index < length)
        sb.Append((char)packet[index++]);
      return sb.ToString();
    }

    private static string Dump(byte[] packet, int start, int length)
    {
      StringBuilder sb = new StringBuilder();
      int index = start;
      while (index < length)
        sb.Append(packet[index++]+"|");
      return sb.ToString();
    }

    // Inserts a string, correctly padded into a packet.  Used internally.
    private static int InsertString(string s, byte[] packet, int start, int length)
    {
      int index = start;
      foreach (char c in s)
      {
        packet[index++] = (byte)c;
        if (index == length)
          return index;
      }
      packet[index++] = 0;
      int pad = (s.Length+1) % 4;
      if (pad != 0)
      {
        pad = 4 - pad;
        while (pad-- > 0)
          packet[index++] = 0;
      }
      return index;
    }

    // Takes a length and returns what it would be if padded to the nearest 4 bytes.
    private static int PadSize(int rawSize)
    {
      int pad = rawSize % 4;
      if (pad == 0)
        return rawSize;
      else
        return rawSize + (4 - pad);
    }
  }


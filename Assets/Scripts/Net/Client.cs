using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class Client : MonoBehaviour
{
    #region Singleton implementation
    public static Client Instance { get; set; }
    private void Awake()
    {
        Instance = this;
    }
    #endregion

    public NetworkDriver driver;
    private NetworkConnection connection;
    private bool isActive = false;
    public Action connectionDropped;

    //Methodes :
    public void Init(string ip, ushort port)
    {
        driver = NetworkDriver.Create();
        // connect to the server with a specific ip and port :
        NetworkEndPoint endPoint = NetworkEndPoint.Parse(ip,port);
        // Connect to server :
        connection = driver.Connect(endPoint);
        isActive = true;
        RegisterToEvent();
    }
    public void Shutdown()
    {
        if (isActive)
        {
            UnregisterToEvent();
            driver.Dispose();
            isActive = false;
            connection = default(NetworkConnection);
        }
    }
    public void OnDestroy()
    {
        Shutdown();
    }

    public void Update()
    {
        if (!isActive) return;
        
        driver.ScheduleUpdate().Complete();
        CheckAlive();
        
      
        UpdateMessagePump();
    }
    private void CheckAlive()
    {
        if (!connection.IsCreated && isActive)
        {
            Debug.Log("Something went wrong, lost connection to server");
            connectionDropped?.Invoke();
            Shutdown();
        }
    }
    
    
    private void UpdateMessagePump()
    {
        DataStreamReader Stream;
        NetworkEvent.Type cmd;
        while ((cmd = connection.PopEvent(driver, out Stream)) != NetworkEvent.Type.Empty)
        {
           if (cmd == NetworkEvent.Type.Connect)
           {
                SendToServer(new NetWelcome());
                Debug.Log("Connected to Server Successfully !");
           }
           else if (cmd == NetworkEvent.Type.Data)
           {
              NetUtility.OnData(Stream, default(NetworkConnection));
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got diconnected from Server !");
                connection = default(NetworkConnection);
                connectionDropped?.Invoke();

            }
        }
    }
    public void SendToServer(NetMessage msg)
    {
        DataStreamWriter writer;
        driver.BeginSend(connection, out writer);
        msg.Serialize(ref writer);
        driver.EndSend(writer);
    }
    // Event Parsing :
    private void RegisterToEvent()
    {
        NetUtility.C_KEEP_ALIVE += OnKeepAlive;
    }
    private void UnregisterToEvent()
    {
        NetUtility.C_KEEP_ALIVE -= OnKeepAlive;
    }
    private void OnKeepAlive(NetMessage msg)
    {
        // Send a msg back to keep both server and client connected :
        SendToServer(msg);
    }
}

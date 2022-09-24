using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class Server : MonoBehaviour
{
    #region Singleton implementation
    public static Server Instance { get; set; }
    private void Awake()
    {
        Instance = this;
    }
    #endregion
    public NetworkDriver driver;
    private NativeList<NetworkConnection> connections;
    private bool isActive = false;
    private const float keepAliveTickRate = 20.0f;
    private float lastKeepAlive;
    public Action connectionDropped;
    //Methodes :
    public void Init(ushort port)
    {
        driver = NetworkDriver.Create();
        // Let any address connect to the server :
        // We can also use the "NetworkEndPoint.LoopbackIpv4" to make only your PC able to connectto the server (a Localhost only server)
        NetworkEndPoint endPoint = NetworkEndPoint.AnyIpv4;
        endPoint.Port = port;
        // If no one was able to connect to the server 
        if (driver.Bind(endPoint)!= 0)
        {
            Debug.Log("Unable to connect to port : " + endPoint.Port);
            return;
        }
        else
        // then keep searching  
        {
            driver.Listen();
            Debug.Log("Currently listening on port : " + endPoint.Port);
        }
        connections = new NativeList<NetworkConnection>(2 // the max number of players connected
                                                       ,Allocator.Persistent); // appliying the alocations of the players
        isActive = true;
    }
    public void Shutdown()
    {
        if (isActive)
        {
            driver.Dispose();
            connections.Dispose();
            isActive = false;
        }
    }
    public void OnDestroy()
    {
        Shutdown();
    }
    public void Update()
    {
        if (!isActive) return;
        KeepAlive();
        driver.ScheduleUpdate().Complete();
        CleanUpConnections();
        AcceptNewConnections();
        UpdateMessagePump();
    }
    private void KeepAlive()
    {
        if (Time.time - lastKeepAlive>keepAliveTickRate)
        {
            lastKeepAlive = Time.time;
            BroadCast(new NetKeepAlive());
        }
    }
    private void CleanUpConnections()
    {
        for (int i = 0; i < connections.Length; i++)
        {
            if (!connections[i].IsCreated)
            {
                connections.RemoveAtSwapBack(i);
                --i;
            }
        }
    }
    private void AcceptNewConnections()
    {
        // Accept new connection :
        NetworkConnection cn;
        while ((cn=driver.Accept())!=default(NetworkConnection))
        {
            connections.Add(cn);
        }
    }
    private void UpdateMessagePump()
    {
        DataStreamReader Stream;
        for (int i = 0; i < connections.Length; i++)
        {
            NetworkEvent.Type cmd;
            while ((cmd=driver.PopEventForConnection(connections[i],out Stream))!=NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    NetUtility.OnData(Stream, connections[i], this);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client Disconected from server !");
                    connections[i] = default(NetworkConnection);
                    connectionDropped?.Invoke();
                    Shutdown();
                }
            }
        }
    }
    // Server Specific :
    public void SendToClient(NetworkConnection conection,NetMessage msg)
    {
        DataStreamWriter writer;
        driver.BeginSend(conection, out writer);
        msg.Serialize(ref writer);
        driver.EndSend(writer);
    }
    public void BroadCast(NetMessage msg)
    {
        for (int i = 0; i < connections.Length; i++)
        {
            if (connections[i].IsCreated)
            {
                Debug.Log($"Sending {msg.code} to : {connections[i].InternalId}");
                SendToClient(connections[i],msg);
            }
        }
    }
}

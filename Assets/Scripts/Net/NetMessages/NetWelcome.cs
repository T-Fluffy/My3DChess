using Unity.Networking.Transport;
using UnityEngine;

public class NetWelcome : NetMessage
{
    public int AssignedTeam { get; set; }
    public NetWelcome()
    {
        code = OpCode.WELCOME;
    }
    public NetWelcome(DataStreamReader reader)
    {
        code = OpCode.WELCOME;
        DeSerialize(reader);
    }
    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte) code);
        writer.WriteInt(AssignedTeam);
    }
    public override void DeSerialize(DataStreamReader reader)
    {
       // We already read the byte in the NetUtility::OnData  
       AssignedTeam=reader.ReadInt();
    }
    public override void ReceivedOnClient()
    {
        NetUtility.C_WELCOME?.Invoke(this);
    }
    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_WELCOME?.Invoke(this,cnn);
    }
}


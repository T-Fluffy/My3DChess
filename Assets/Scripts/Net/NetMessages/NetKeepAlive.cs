using Unity.Networking.Transport;

public class NetKeepAlive : NetMessage
{
    public NetKeepAlive() // <-- Making the box  (paking a keepALive message to send) 
    {
        code = OpCode.KEEP_ALIVE;
    }
    public NetKeepAlive(DataStreamReader reader)// <-- Rceiving a box (Receive a a KeepAlive message from a server)
    {
        code = OpCode.KEEP_ALIVE;
        DeSerialize(reader);
    }
    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte) code);
    }
    public override void DeSerialize(DataStreamReader reader)
    {
       
    }
    public override void ReceivedOnClient()
    {
        NetUtility.C_KEEP_ALIVE?.Invoke(this);
    }
    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_KEEP_ALIVE?.Invoke(this,cnn);
    }
}

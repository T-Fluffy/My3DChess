using Unity.Networking.Transport;


public class NetMakeMove : NetMessage
{
    public int originalX, originalY, destinationX, destinationY, teamID;
    public NetMakeMove()
    {
        code = OpCode.MAKE_MOVE;
    }
    public NetMakeMove(DataStreamReader reader)
    {
        code = OpCode.MAKE_MOVE;
        DeSerialize(reader);
    }
    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)code);
        writer.WriteInt(originalX);
        writer.WriteInt(originalY);
        writer.WriteInt(destinationX);
        writer.WriteInt(destinationY);
        writer.WriteInt(teamID);
    }
    public override void DeSerialize(DataStreamReader reader)
    {
        originalX = reader.ReadInt();
        originalY = reader.ReadInt();
        destinationX = reader.ReadInt();
        destinationY = reader.ReadInt();
        teamID = reader.ReadInt();
    }
    public override void ReceivedOnClient()
    {
        NetUtility.C_MAKE_MOVE?.Invoke(this);
    }
    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_MAKE_MOVE?.Invoke(this, cnn);
    }
}

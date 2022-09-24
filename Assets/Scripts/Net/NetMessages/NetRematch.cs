
using Unity.Networking.Transport;

public class NetRematch : NetMessage
{
    public int  teamID;
    public byte wanteRematch;
    public NetRematch()
    {
        code = OpCode.MAKE_MOVE;
    }
    public NetRematch(DataStreamReader reader)
    {
        code = OpCode.REMATCH;
        DeSerialize(reader);
    }
    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)code);
        writer.WriteInt(teamID);
        writer.WriteByte(wanteRematch);
    }
    public override void DeSerialize(DataStreamReader reader)
    {
       teamID = reader.ReadInt();
       wanteRematch = reader.ReadByte();
    }
    public override void ReceivedOnClient()
    {
        NetUtility.C_REMATCH?.Invoke(this);
    }
    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_REMATCH?.Invoke(this, cnn);
    }
}

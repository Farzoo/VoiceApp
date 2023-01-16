using System.Runtime.Serialization;

namespace Core.Packets.Serializer;

[Serializable]
public class PacketSerializationException : Exception
{
    public PacketSerializationException(string message) : base(message)
    {
    }
    
    public PacketSerializationException(string message, Exception innerException) : base(message, innerException)
    {
    }
    
    protected PacketSerializationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}

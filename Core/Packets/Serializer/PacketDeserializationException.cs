using System.Runtime.Serialization;

namespace Core.Packets.Serializer;

[Serializable]
public class PacketDeserializationException : Exception
{
    public PacketDeserializationException(string message) : base(message)
    {
    }
    
    public PacketDeserializationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected PacketDeserializationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
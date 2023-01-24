using System.Reflection;
using NetLib.Packets;
using ProtoBuf.Meta;

namespace Core.Packets.Serializer;

public class PacketSerializer : IPacketSerializer
{
    private IPacketMapper<ushort> Mapper { get; }

    public PacketSerializer(IPacketMapper<ushort> mapper)
    {
        this.Mapper = mapper;
        this.Mapper.GetTypes().ForEach(t =>
        {
            this.CheckType(t);
            this.RegisterTypeForBaseType(t);
        });
    }

    private void CheckType(Type type)
    {
        bool hasParameterlessConstructor = type.GetConstructors().Any(c => c.GetParameters().Length == 0);
        if(!hasParameterlessConstructor)
            throw new ArgumentException("BasePacket type must have a parameterless constructor");
    }

    private void RegisterTypeForBaseType(Type type)
    {
        if(type.BaseType != typeof(BasePacket))
            throw new ArgumentException($"Cannot register {type.Name} : {type.BaseType} for serializer. It must inherit from {nameof(BasePacket)}");
        PacketInfo? packetInfo = type.GetCustomAttribute<PacketInfo>(false);
        if (packetInfo == null)
            throw new ArgumentException($"{type.Name} must have a PacketInfo");
        MetaType thisType = RuntimeTypeModel.Default[type];
        MetaType baseType = RuntimeTypeModel.Default[type.BaseType];
        if (baseType.GetSubtypes().All(s => s.DerivedType != thisType))
        {
            if(baseType.GetSubtypes().Any(s => s.FieldNumber == packetInfo.Id + 1))
                throw new ArgumentException($"BasePacket id {type.Name} already registered for {baseType.GetSubtypes().First(s => s.FieldNumber == packetInfo.Id).DerivedType.Name}");
            Console.WriteLine($"Registering {type.Name} with id {packetInfo.Id+1}");
            baseType.AddSubType(packetInfo.Id + 1, type);
        }
    }

    public BasePacket Deserialize(byte[] data)
    {
        using MemoryStream stream = new MemoryStream(data);
        using BinaryReader reader = new BinaryReader(stream);
        
        ushort length = reader.ReadUInt16();
        ushort id = reader.ReadUInt16();
        
        this.Mapper.TryGetType(id, out var type);

        if (type == null)
        {
            throw new PacketDeserializationException($"Invalid basePacket id {id} bytes: {BitConverter.ToString(data)}");
        }
        
        if(length != data.Length)
        {
            throw new PacketDeserializationException($"Invalid basePacket length {length} bytes: {BitConverter.ToString(data)}");
        }
        
        reader.BaseStream.SetLength(length);
        reader.BaseStream.Seek(sizeof(ushort)*2, SeekOrigin.Begin);
        return ProtoBuf.Serializer.NonGeneric.Deserialize(type, stream) as BasePacket ?? throw new PacketDeserializationException($"Failed to deserialize packet {id}");
    }

    public byte[] Serialize(BasePacket basePacket)
    {
        using MemoryStream stream = new MemoryStream();
        using BinaryWriter writer = new BinaryWriter(stream);
        writer.Seek(sizeof(ushort), SeekOrigin.Begin);
        
        if(!this.Mapper.TryGetId(basePacket.GetType(), out var id)) throw new PacketSerializationException($"Failed to get id for {basePacket.GetType().Name}");
        
        writer.Write(id);
        ProtoBuf.Serializer.Serialize(writer.BaseStream, basePacket);
        writer.Seek(0, SeekOrigin.Begin);
        writer.Write((ushort)writer.BaseStream.Length);
        
        return stream.ToArray();
    }
}
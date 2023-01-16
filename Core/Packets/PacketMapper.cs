using System.Reflection;
using NetLib.Packets;

namespace Core.Packets;

public class PacketMapper : IPacketMapper<ushort>
{
    private IDictionary<ushort, Type> IdToType { get; } = new Dictionary<ushort, Type>();
    private IDictionary<Type, ushort> TypeToId { get; } = new Dictionary<Type, ushort>();

    public IPacketMapper<ushort> Register<TPacket>() where TPacket : BasePacket, new()
    {
        if (this.TypeToId.ContainsKey(typeof(TPacket))) throw new ArgumentException($"Type {typeof(TPacket).Name} is already registered");
        if (!GetId<TPacket>(out var id)) throw new ArgumentException($"{typeof(TPacket).Name} does not have a PacketInfo attribute.");
        if (this.IdToType.ContainsKey((ushort) id!)) throw new ArgumentException($"Trying to register {typeof(TPacket).Name} with id {id}. Id {id} is already registered as {this.IdToType[(ushort) id].Name}");
        
        this.IdToType.TryAdd(id.Value, typeof(TPacket));
        this.TypeToId.Add(typeof(TPacket), id.Value);
        
        return this;
    }

    public bool TryGetId<TPacket>(out ushort id) where TPacket : BasePacket, new()
    {
        return this.TypeToId.TryGetValue(typeof(TPacket), out id);
    }
    
    public bool TryGetId(Type type, out ushort id)
    {
        return this.TypeToId.TryGetValue(type, out id);
    }

    public bool TryGetType(ushort id, out Type? type)
    {
        return this.IdToType.TryGetValue(id, out type);
    }

    public Type? GetType(ushort id)
    {
        return this.IdToType.TryGetValue(id, out var type) ? type : null;
    }

    public List<Type> GetTypes()
    {
        return this.IdToType.Values.ToList();
    }

    private static bool GetId<T>(out ushort? id) where T : BasePacket, new()
    {
        PacketInfo? packetInfo = typeof(T).GetCustomAttribute<PacketInfo>(false);
        id = packetInfo?.Id;
        return packetInfo != null;
    }
}
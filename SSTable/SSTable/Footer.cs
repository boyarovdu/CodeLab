using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SSTable;

public class Footer
{
    public required long BlockIndexOffset { get; init; }
    public required long BlockIndexLength { get; init; }
    public required CompressionType BlockCompressionType { get; init; }
    public required SerializationType BlockSerializationType { get; init; }
}

public interface IFooterConverter
{
    public Version Version { get; }
    public int FooterLength { get; }
    public Footer ToFooter(byte[] bytes);
    public byte[] GetBytes(Footer footer);
}

public class FooterConverter : IFooterConverter
{
    public Version Version { get; } = new Version(0, 0, 1);

    /// <summary>
    /// Length of the serialized footer in bytes
    /// </summary>
    public int FooterLength => 36;

    public Footer ToFooter(byte[] bytes)
    {
        var span = bytes.AsSpan();

        var version = Version.FromBytes(span.Slice(24, sizeof(uint) * 3));
        if (!Equals(version, Version))
            throw new Exception(
                $"Cannot convert given bytes into Footer object. Given footer format of version {version} but expected version is {Version}.");

        var position = 0;
        var blockIndexOffest = BitConverter.ToInt64(span.Slice(sizeof(long), sizeof(long)));
        position += sizeof(long);
        var blockIndexLength = BitConverter.ToInt64(span.Slice(position, sizeof(long)));
        position += sizeof(long);
        var blockCompressionType = (CompressionType)BitConverter.ToInt32(span.Slice(position, sizeof(int)));
        position += sizeof(int);
        var blockSerializationType = (SerializationType)BitConverter.ToInt32(span.Slice(position, sizeof(int)));
        position += sizeof(int);

        return new Footer
        {
            BlockIndexOffset = blockIndexOffest,
            BlockIndexLength = blockIndexLength,
            BlockCompressionType = blockCompressionType,
            BlockSerializationType = blockSerializationType
        };
    }

    public byte[] GetBytes(Footer footer)
    {
        var result = new List<byte>(FooterLength);

        result.AddRange(BitConverter.GetBytes(footer.BlockIndexOffset)); // 8 bytes
        result.AddRange(BitConverter.GetBytes(footer.BlockIndexLength)); // 8 bytes
        result.AddRange(BitConverter.GetBytes((int)footer.BlockCompressionType)); // 4 bytes
        result.AddRange(BitConverter.GetBytes((int)footer.BlockSerializationType)); // 4 bytes
        result.AddRange(Version.GetBytes()); // 4 bytes * 3 = 12 bytes

        return result.ToArray();
    }
}

public class Xxx
{
    public List<Tuple<PropertyInfo, Delegate, Delegate>> Properties { get; set; }

    private int? _footerLength = null;

    public int FooterLength
    {
        get
        {
            if (_footerLength.HasValue)
                return _footerLength.Value;
            return (_footerLength = CalculateFooterLength()).Value;
        }
    }

    private int CalculateFooterLength()
    {
        var length = 0;
        foreach (var property in Properties)
        {
            length += Marshal.SizeOf(property.Item1.PropertyType);
        }

        return length;
    }

    public byte[] GetBytes(Footer footer)
    {
        var result = new List<byte>(FooterLength);
        foreach (var property in Properties)
        {
            var value = property.Item1.GetValue(footer);
            var bytes = property.Item2.DynamicInvoke(value) as byte[];
            if (bytes == null)
                throw new Exception(
                    $"Property '{property.Item1.Name}' of the footer cannot be serialized. Serializer returns null.");

            result.AddRange(bytes);
        }

        return result.ToArray();
    }
    
    public Footer ToFooter(byte[] bytes)
    {
        var memory = bytes.AsMemory();
        var position = 0;
        foreach (var property in Properties)
        {
            var propLength = Marshal.SizeOf(property.Item1.PropertyType);
            var propBytes = memory.Slice(position, propLength);
            property.Item1.SetValue(this, property.Item3.DynamicInvoke(propBytes));
        }

        return null;
    }
}

public class XxxBuilder
{
    private readonly Xxx _xxx = new Xxx();

    public XxxBuilder ConfigureSerializer<T>(Expression<Func<Footer, T>> expression, Func<T, byte[]> toBytes, Func<byte[],T> fromBytes)
    {
        var member = expression.Body as MemberExpression;
        if (member == null)
            throw new Exception("Expression must be a member expression.");
        
        var property = _xxx.GetType().GetProperty(member.Member.Name);
        if (property == null)
            throw new Exception($"Property '{member.Member.Name}' not found on type '{nameof(Xxx)}'.");

        if (property.PropertyType.IsPrimitive)
            throw new Exception($"Property '{member.Member.Name}' must be a primitive type");

        _xxx.Properties.Add(new Tuple<PropertyInfo, Delegate, Delegate>(property, toBytes, fromBytes));
        return this;
    }

    public Xxx Build()
    {
        // Any validations?
        return _xxx;
    }
}
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SSTable;

public class Footer
{
    public long BlockIndexOffset { get; set; }
    public long BlockIndexLength { get; set; }
    public CompressionType BlockCompressionType { get; set; }
    public SerializationType BlockSerializationType { get; set; }
}

public class FooterConverter
{
    private readonly Tuple<PropertyInfo, Delegate, Delegate>[] _propertyConverters;

    private readonly Version _version;

    internal FooterConverter(Version version, Tuple<PropertyInfo, Delegate, Delegate>[] propertyConverters)
    {
        _version = version;
        _propertyConverters = propertyConverters;
    }

    private int GetSize(bool includeVersion = true)
    {
        return _propertyConverters.Sum(property => Marshal.SizeOf(property.Item1.PropertyType))
               + (includeVersion ? Version.SizeOf() : 0);
    }

    public byte[] GetBytes(Footer footer)
    {
        var result = new List<byte>(GetSize());

        result.AddRange(_version.GetBytes());
        foreach (var property in _propertyConverters)
            result.AddRange(SerializeProperty(footer, property)!);

        return result.ToArray();
    }

    private static byte[]? SerializeProperty(Footer footer, Tuple<PropertyInfo, Delegate, Delegate> property)
    {
        var val = property.Item1.GetValue(footer);
        var bytes = property.Item2.DynamicInvoke(val) as byte[];

        if (bytes == null || bytes.Length == 0)
            throw new Exception(
                $"Property '{property.Item1.Name}' of the footer cannot be serialized. Serializer returns null or empty bytes array.");

        return bytes;
    }
    
    public Footer ToFooter(byte[] bytes)
    {
        var footer = new Footer();

        ValidateVersion(GetVersion(bytes));

        var span = bytes.AsSpan().Slice(Version.SizeOf());
        var offset = 0;
        foreach (var property in _propertyConverters)
        {
            offset += DeserializeProperty(property, span.Slice(offset), footer);
        }

        return footer;
    }

    private static int DeserializeProperty(Tuple<PropertyInfo, Delegate, Delegate> property, Span<byte> span,
        Footer footer)
    {
        var propLength = Marshal.SizeOf(property.Item1.PropertyType);
        property.Item1.SetValue(footer, property.Item3.DynamicInvoke(span.Slice(0, propLength).ToArray()));
        return propLength;
    }

    private Version GetVersion(byte[] bytes)
    {
        return Version.FromBytes(bytes.Take(Version.SizeOf()).ToArray());
    }

    private void ValidateVersion(Version version)
    {
        // TODO: make footer versions backward-compatible 
        if (!version.Equals(_version))
            throw new Exception(
                $"Cannot convert given bytes into Footer object. Given footer format of version {version} but expected version is {_version}.");
    }
}

public class FooterConverterBuilder
{
    private readonly List<Tuple<PropertyInfo, Delegate, Delegate>> _propConverters = new();

    private Version _version = new Version(0, 0, 1);

    public FooterConverterBuilder ConfigureSerializer<T>(Expression<Func<Footer, T>> expression,
        Func<T, byte[]> toBytes, Func<byte[], T> fromBytes)
    {
        var member = expression.Body as MemberExpression;
        if (member == null)
            throw new Exception("Expression must be a member expression.");

        var property = typeof(Footer).GetProperty(member.Member.Name);
        if (property == null)
            throw new Exception($"Property '{member.Member.Name}' not found on type '{nameof(FooterConverter)}'.");

        if (property.PropertyType.IsPrimitive)
            throw new Exception($"Property '{member.Member.Name}' must be a primitive type");

        _propConverters.Add(new Tuple<PropertyInfo, Delegate, Delegate>(property, toBytes, fromBytes));
        return this;
    }

    public FooterConverterBuilder ConfigureVersion(Version version)
    {
        _version = version;
        return this;
    }

    public FooterConverter Build()
    {
        var length = _propConverters.Sum(property => Marshal.SizeOf(property.Item1.PropertyType));

        if (length > 64_000)
            throw new Exception("Footer length cannot be greater than 64kb");

        return new FooterConverter(_version, _propConverters.ToArray());
    }
}
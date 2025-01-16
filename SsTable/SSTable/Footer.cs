using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SSTable;

public class Footer
{
    public long BlockIndexOffset { get; set; }
    public long BlockIndexLength { get; set; }
}

public interface IFooterConverter
{
    public byte[] GetBytes(Footer footer);
    public Footer ToFooter(byte[] bytes);
    public int SizeOf();
}

public class FooterConverter : IFooterConverter
{
    private readonly Tuple<PropertyInfo, Delegate, Delegate>[] _propertyConverters;

    private readonly Version _version;

    internal FooterConverter(Version version, Tuple<PropertyInfo, Delegate, Delegate>[] propertyConverters)
    {
        _version = version;
        _propertyConverters = propertyConverters;
    }

    public int SizeOf() => _propertyConverters.Sum(property => Marshal.SizeOf(property.Item1.PropertyType));

    public byte[] GetBytes(Footer footer)
    {
        var result = new List<byte>(SizeOf());
        foreach (var property in _propertyConverters)
        {
            result.AddRange(SerializeProperty(footer, property));
        }

        return result.ToArray();
    }

    private static byte[] SerializeProperty(Footer footer, Tuple<PropertyInfo, Delegate, Delegate> property)
    {
        var val = property.Item1.GetValue(footer);
        var bytes = property.Item2.DynamicInvoke(val) as byte[];

        ValidatePropertyPayload(property.Item1, bytes);

        return bytes!;
    }

    public Footer ToFooter(byte[] payload)
    {
        var footer = new Footer();
        
        var propOffset = 0;
        foreach (var property in _propertyConverters)
        {
            propOffset += DeserializeProperty(property, payload[propOffset..], footer);
        }

        return footer;
    }

    private static int DeserializeProperty(Tuple<PropertyInfo, Delegate, Delegate> property, byte[] bytes,
        Footer footer)
    {
        var propLength = Marshal.SizeOf(property.Item1.PropertyType);
        property.Item1.SetValue(footer, property.Item3.DynamicInvoke(bytes[..propLength]));
        return propLength;
    }
    
    private static void ValidatePropertyPayload(PropertyInfo prop, byte[]? bytes)
    {
        if (bytes == null || bytes.Length == 0)
            throw new Exception(
                $"Property '{prop.Name}' of the footer cannot be serialized. Property serializer returns null or empty bytes array.");
    }
}

public class FooterConverterBuilder
{
    private readonly List<Tuple<PropertyInfo, Delegate, Delegate>> _propConverters = new();

    private Version _version = new Version(0, 0, 1);

    private const int FooterPayloadSize = 64_000;

    public FooterConverterBuilder ConfigureSerializer<T>(Expression<Func<Footer, T>> expression,
        Func<T, byte[]> toBytes, Func<byte[], T> fromBytes)
    {
        var member = expression.Body as MemberExpression;
        if (member == null)
            throw new Exception("Expression must be a member expression.");

        var property = typeof(Footer).GetProperty(member.Member.Name);
        if (property == null)
            throw new Exception($"Property '{member.Member.Name}' not found on type '{nameof(FooterConverter)}'.");

        if (!property.PropertyType.IsPrimitive)
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

        if (length > FooterPayloadSize)
            throw new Exception($"Footer payload length must not be greater than {FooterPayloadSize / 1000}kb. Actual length is {length}.");

        return new FooterConverter(_version, _propConverters.ToArray());
    }
}
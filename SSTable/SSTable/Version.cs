namespace SSTable;


public class Version(uint major, uint minor, uint patch)
{
    public uint MajorVersion { get; } = major;
    public uint MinorVersion { get; } = minor;
    public uint PatchVersion { get; } = patch;

    private const int VersionComponentLength = sizeof(uint);

    public byte[] GetBytes()
    {
        var result = new List<byte>(VersionComponentLength * 3);

        result.AddRange(BitConverter.GetBytes(MajorVersion));
        result.AddRange(BitConverter.GetBytes(MinorVersion));
        result.AddRange(BitConverter.GetBytes(PatchVersion));

        return result.ToArray();
    }

    public static Version FromBytes(byte[] bytes)
    {
        return FromBytes(bytes.AsSpan());
    }

    public static Version FromBytes(ReadOnlySpan<byte> bytes)
    {
        var vcl = VersionComponentLength;
        var major = BitConverter.ToUInt32(bytes.Slice(vcl * 0, vcl));
        var minor = BitConverter.ToUInt32(bytes.Slice(vcl * 1, vcl));
        var patch = BitConverter.ToUInt32(bytes.Slice(vcl * 2, vcl));

        return new Version(major, minor, patch);
    }

    public override string ToString() => $"{MajorVersion}.{MinorVersion}.{PatchVersion}";

    public override bool Equals(object? @object)
    {
        var other = @object as Version;

        if (other == null)
            return false;

        return MajorVersion == other.MajorVersion &&
               MinorVersion == other.MinorVersion &&
               PatchVersion == other.PatchVersion;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(MajorVersion, MinorVersion, PatchVersion);
    }
}

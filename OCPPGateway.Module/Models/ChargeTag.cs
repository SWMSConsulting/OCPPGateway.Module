#nullable disable

using System.Buffers.Binary;

namespace OCPPGateway.Module.Models;

public partial class ChargeTag
{
    public string TagId { get; set; }
    public string TagName { get; set; }
    public string ParentTagId { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool? Blocked { get; set; }

    public override string ToString()
    {
        if (!string.IsNullOrEmpty(TagName))
            return TagName;
        else
            return TagId;
    }


    public static string ToHexString(long tagId, bool reverseEndians)
    {
        var hexTag = tagId.ToString("X");
        if (reverseEndians)
        {
            hexTag = BinaryPrimitives.ReverseEndianness((uint)tagId).ToString("X");
        }

        while (hexTag.Length < 8)
        {
            hexTag = "0" + hexTag;
        }

        return hexTag;
    }

    public static long? FromHexString(string tagId, bool reverseEndians)
    {
        if (string.IsNullOrEmpty(tagId))
            return null;

        if (tagId.Count() > 8 && long.TryParse(tagId, out long result))
        {
            return result;
        }

        try
        {
            if (reverseEndians)
                return BinaryPrimitives.ReverseEndianness(Convert.ToUInt32(tagId, 16));
            else
                return Convert.ToUInt32(tagId, 16);
        }
        catch
        {
            return null;
        }
    }

}

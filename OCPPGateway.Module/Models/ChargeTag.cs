#nullable disable

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
}

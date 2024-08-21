
namespace OCPPGateway.Module.Models;
public class TransactionExtended : Transaction
{
    public virtual string StartTagName { get; set; }
    public virtual string StartTagParentId { get; set; }
    public virtual string StopTagName { get; set; }
    public virtual string StopTagParentId { get; set; }
}

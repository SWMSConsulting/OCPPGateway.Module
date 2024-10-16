using System.ComponentModel.DataAnnotations;

namespace OCPPGateway.Module.BusinessObjects;

public interface IChargeTagGroup
{
    public string Name { get; }


    [StringLength(20)]
    public string Identifier { get; }

    public IList<OCPPChargeTag> ChargeTags { get; }
}

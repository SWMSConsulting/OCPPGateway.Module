namespace OCPPGateway.Module.BusinessObjects;

public interface IChargeTagGroup
{
    public string Name { get; set; }
    
    public string Identifier { get; set; }

    public IList<OCPPChargeTag> ChargeTags { get; set; }
}

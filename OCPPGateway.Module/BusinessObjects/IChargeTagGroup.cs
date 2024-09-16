namespace OCPPGateway.Module.BusinessObjects;

public interface IChargeTagGroup
{
    public string Name { get; }
    
    public string Identifier { get; }

    public IList<OCPPChargeTag> ChargeTags { get; }
}

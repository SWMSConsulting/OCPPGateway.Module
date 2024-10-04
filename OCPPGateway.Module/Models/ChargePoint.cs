#nullable disable

using Newtonsoft.Json;

namespace OCPPGateway.Module.Models;

public partial class ChargePoint
{
    public ChargePoint()
    {
        Transactions = new HashSet<Transaction>();
    }

    public string ChargePointId { get; set; }
    public int NumberOfConnectors { get; set; }
    public string Name { get; set; }
    public string Comment { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string ClientCertThumb { get; set; }

    [JsonIgnore]
    public virtual ICollection<Transaction> Transactions { get; set; }
}

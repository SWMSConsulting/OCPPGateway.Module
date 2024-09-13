using Microsoft.EntityFrameworkCore;

namespace OCPPGateway.Module.BusinessObjects;

public static class OCPPDbContextExtention
{
    public static void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UnknownOCPPChargePoint>();
        modelBuilder.Entity<UnknownOCPPChargeTag>();

        // abstract classes
        modelBuilder.Entity<OCPPChargeTag>();
    }
}

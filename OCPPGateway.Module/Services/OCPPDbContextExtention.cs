using Microsoft.EntityFrameworkCore;
using OCPPGateway.Module.BusinessObjects;

namespace OCPPGateway.Module.Services;

public static class OCPPDbContextExtention
{ 
    public static void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UnknownOCPPChargePoint>();
        modelBuilder.Entity<UnknownOCPPChargeTag>();

        // abstract classes
        modelBuilder.Entity<OCPPChargePoint>();
        modelBuilder.Entity<OCPPChargeTag>();
    }
}

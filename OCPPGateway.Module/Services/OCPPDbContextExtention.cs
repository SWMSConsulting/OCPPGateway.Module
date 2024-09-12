using Microsoft.EntityFrameworkCore;
using OCPPGateway.Module.BusinessObjects;

namespace OCPPGateway.Module.Services;

public static class OCPPDbContextExtention
{ 
    public static void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UnknownChargePoint>();
        modelBuilder.Entity<UnknownChargeTag>();
    }
}

﻿using Microsoft.EntityFrameworkCore;
using OCPPGateway.Module.BusinessObjects.Events;
using SWMS.Influx.Module.BusinessObjects;

namespace OCPPGateway.Module.BusinessObjects;

public static class OCPPDbContextExtention
{
    public static void OnModelCreating(ModelBuilder modelBuilder)
    {
        InfluxDbContextExtention.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UnknownOCPPChargePoint>();
        modelBuilder.Entity<UnknownOCPPChargeTag>();

        // abstract classes
        modelBuilder.Entity<OCPPTransaction>();
        modelBuilder.Entity<OCPPChargeTag>();

        modelBuilder.Entity<OCPPChargePoint>()
            .HasMany(c => c.Connectors)
            .WithOne(c => c.ChargePoint)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OCPPChargePointConnector>();

        modelBuilder.Entity<OCPPMessageCallbackLink>();

        modelBuilder.Entity<OCPPMessageLogEvent>();
    }
}

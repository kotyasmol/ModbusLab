using Microsoft.EntityFrameworkCore;
using ModbusLab.Domain.Devices;
using ModbusLab.Domain.Logs;
using ModbusLab.Domain.Registers;

namespace ModbusLab.Infrastructure.Persistence;

public sealed class ModbusLabDbContext : DbContext
{
    public ModbusLabDbContext(DbContextOptions<ModbusLabDbContext> options)
        : base(options)
    {
    }

    public DbSet<DeviceType> DeviceTypes => Set<DeviceType>();

    public DbSet<SlaveDevice> SlaveDevices => Set<SlaveDevice>();

    public DbSet<RegisterDefinition> RegisterDefinitions => Set<RegisterDefinition>();

    public DbSet<RegisterValue> RegisterValues => Set<RegisterValue>();

    public DbSet<ModbusLogEntry> ModbusLogs => Set<ModbusLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DeviceType>(entity =>
        {
            entity.ToTable("device_types");

            entity.HasKey(deviceType => deviceType.Id);

            entity.Property(deviceType => deviceType.Name)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(deviceType => deviceType.Description)
                .HasMaxLength(512);
        });

        modelBuilder.Entity<SlaveDevice>(entity =>
        {
            entity.ToTable("slave_devices");

            entity.HasKey(device => device.Id);

            entity.Property(device => device.Name)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(device => device.SlaveAddress)
                .IsRequired();

            entity.Property(device => device.DeviceTypeId)
                .IsRequired();

            entity.Property(device => device.IsEnabled)
                .IsRequired();

            entity.HasIndex(device => device.SlaveAddress)
                .IsUnique();
        });

        modelBuilder.Entity<RegisterDefinition>(entity =>
        {
            entity.ToTable("register_definitions");

            entity.HasKey(register => register.Id);

            entity.Property(register => register.DeviceTypeId)
                .IsRequired();

            entity.Property(register => register.Address)
                .IsRequired();

            entity.Property(register => register.Name)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(register => register.AccessMode)
                .IsRequired();

            entity.Property(register => register.Unit)
                .HasMaxLength(32);

            entity.Property(register => register.Description)
                .HasMaxLength(512);

            entity.HasIndex(register => new
            {
                register.DeviceTypeId,
                register.Address
            }).IsUnique();
        });

        modelBuilder.Entity<RegisterValue>(entity =>
        {
            entity.ToTable("register_values");

            entity.HasKey(registerValue => registerValue.Id);

            entity.Property(registerValue => registerValue.SlaveDeviceId)
                .IsRequired();

            entity.Property(registerValue => registerValue.RegisterDefinitionId)
                .IsRequired();

            entity.Property(registerValue => registerValue.Value)
                .IsRequired();

            entity.Property(registerValue => registerValue.UpdatedAtUtc)
                .IsRequired();

            entity.HasIndex(registerValue => new
            {
                registerValue.SlaveDeviceId,
                registerValue.RegisterDefinitionId
            }).IsUnique();
        });

        modelBuilder.Entity<ModbusLogEntry>(entity =>
        {
            entity.ToTable("modbus_logs");

            entity.HasKey(log => log.Id);

            entity.Property(log => log.TimestampUtc)
                .IsRequired();

            entity.Property(log => log.SlaveAddress)
                .IsRequired();

            entity.Property(log => log.FunctionCode)
                .IsRequired();

            entity.Property(log => log.RegisterAddress)
                .IsRequired();

            entity.Property(log => log.Status)
                .IsRequired();

            entity.Property(log => log.Message)
                .HasMaxLength(512)
                .IsRequired();

            entity.HasIndex(log => log.TimestampUtc);
            entity.HasIndex(log => log.SlaveAddress);
        });
    }
}
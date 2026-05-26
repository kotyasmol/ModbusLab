using Microsoft.EntityFrameworkCore;
using ModbusLab.Domain.Devices;
using ModbusLab.Domain.Logs;
using ModbusLab.Domain.Registers;
using ModbusLab.Domain.Testing;
using ModbusLab.Domain.Users;

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

    public DbSet<TestProfile> TestProfiles => Set<TestProfile>();

    public DbSet<TestStep> TestSteps => Set<TestStep>();

    public DbSet<TestRun> TestRuns => Set<TestRun>();

    public DbSet<TestStepResult> TestStepResults => Set<TestStepResult>();

    public DbSet<AppUser> AppUsers => Set<AppUser>();

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

        modelBuilder.Entity<TestProfile>(entity =>
        {
            entity.ToTable("test_profiles");

            entity.HasKey(profile => profile.Id);

            entity.Property(profile => profile.Name)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(profile => profile.Description)
                .HasMaxLength(512);

            entity.Property(profile => profile.IsEnabled)
                .IsRequired();

            entity.Property(profile => profile.CreatedAtUtc)
                .IsRequired();

            entity.HasIndex(profile => profile.Name)
                .IsUnique();
        });

        modelBuilder.Entity<TestStep>(entity =>
        {
            entity.ToTable("test_steps");

            entity.HasKey(step => step.Id);

            entity.Property(step => step.TestProfileId)
                .IsRequired();

            entity.Property(step => step.OrderIndex)
                .IsRequired();

            entity.Property(step => step.Name)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(step => step.Type)
                .IsRequired();

            entity.HasIndex(step => new
            {
                step.TestProfileId,
                step.OrderIndex
            }).IsUnique();

            entity.HasOne<TestProfile>()
                .WithMany()
                .HasForeignKey(step => step.TestProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TestRun>(entity =>
        {
            entity.ToTable("test_runs");

            entity.HasKey(run => run.Id);

            entity.Property(run => run.TestProfileId)
                .IsRequired();

            entity.Property(run => run.ProfileName)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(run => run.Status)
                .IsRequired();

            entity.Property(run => run.StartedAtUtc)
                .IsRequired();

            entity.Property(run => run.Summary)
                .HasMaxLength(512);

            entity.HasIndex(run => run.StartedAtUtc);
            entity.HasIndex(run => run.TestProfileId);
        });

        modelBuilder.Entity<TestStepResult>(entity =>
        {
            entity.ToTable("test_step_results");

            entity.HasKey(result => result.Id);

            entity.Property(result => result.TestRunId)
                .IsRequired();

            entity.Property(result => result.OrderIndex)
                .IsRequired();

            entity.Property(result => result.StepName)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(result => result.StepType)
                .IsRequired();

            entity.Property(result => result.Status)
                .IsRequired();

            entity.Property(result => result.Message)
                .HasMaxLength(512)
                .IsRequired();

            entity.Property(result => result.StartedAtUtc)
                .IsRequired();

            entity.Property(result => result.FinishedAtUtc)
                .IsRequired();

            entity.HasIndex(result => new
            {
                result.TestRunId,
                result.OrderIndex
            }).IsUnique();

            entity.HasOne<TestRun>()
                .WithMany()
                .HasForeignKey(result => result.TestRunId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("app_users");

            entity.HasKey(user => user.Id);

            entity.Property(user => user.UserName)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(user => user.Email)
                .HasMaxLength(256);

            entity.Property(user => user.PasswordHash)
                .HasMaxLength(512)
                .IsRequired();

            entity.Property(user => user.Role)
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(user => user.CreatedAtUtc)
                .IsRequired();

            entity.HasIndex(user => user.UserName)
                .IsUnique();

            entity.HasIndex(user => user.Email)
                .IsUnique();
        });
    }
}

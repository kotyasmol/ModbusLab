using ModbusLab.Api.Audit;
using ModbusLab.Api.Auth;
using ModbusLab.Api.Devices;

namespace ModbusLab.Api.Endpoints;

public static class DeviceManagementEndpoints
{
    public static IEndpointRouteBuilder MapDeviceManagementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/device-management")
            .RequireAuthorization(AuthPolicies.RequireEngineer)
            .WithTags("Device management");

        group.MapGet("/types", async (
            DeviceManagementService deviceManagementService,
            CancellationToken cancellationToken) =>
        {
            var types = await deviceManagementService.GetDeviceTypesAsync(cancellationToken);
            return Results.Ok(types);
        })
        .WithSummary("Get device types");

        group.MapPost("/types", async (
            CreateDeviceTypeRequest request,
            DeviceManagementService deviceManagementService,
            AuditLogService auditLogService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var type = await deviceManagementService.CreateDeviceTypeAsync(request, cancellationToken);
                await auditLogService.LogAsync(
                    "devices.create_type",
                    isSuccess: true,
                    entityType: "DeviceType",
                    entityId: type.Id.ToString(),
                    details: $"Created device type '{type.Name}'.",
                    cancellationToken: cancellationToken);

                return Results.Created($"/api/device-management/types/{type.Id}", type);
            }
            catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
            {
                await auditLogService.LogAsync(
                    "devices.create_type",
                    isSuccess: false,
                    entityType: "DeviceType",
                    details: exception.Message,
                    cancellationToken: cancellationToken);

                return Results.BadRequest(new { message = exception.Message });
            }
        })
        .WithSummary("Create device type");

        group.MapPost("/devices", async (
            CreateDeviceRequest request,
            DeviceManagementService deviceManagementService,
            AuditLogService auditLogService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var device = await deviceManagementService.CreateDeviceAsync(request, cancellationToken);
                await auditLogService.LogAsync(
                    "devices.create_device",
                    isSuccess: true,
                    entityType: "SlaveDevice",
                    entityId: device.Id.ToString(),
                    details: $"Created device '{device.Name}' at slave address {device.SlaveAddress}.",
                    cancellationToken: cancellationToken);

                return Results.Created($"/api/devices/{device.Id}", device);
            }
            catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
            {
                await auditLogService.LogAsync(
                    "devices.create_device",
                    isSuccess: false,
                    entityType: "SlaveDevice",
                    details: exception.Message,
                    cancellationToken: cancellationToken);

                return Results.BadRequest(new { message = exception.Message });
            }
        })
        .WithSummary("Create slave device");

        group.MapPatch("/devices/{deviceId:guid}/status", async (
            Guid deviceId,
            ChangeDeviceStatusRequest request,
            DeviceManagementService deviceManagementService,
            AuditLogService auditLogService,
            CancellationToken cancellationToken) =>
        {
            var device = await deviceManagementService.ChangeDeviceStatusAsync(
                deviceId,
                request,
                cancellationToken);

            if (device is null)
                return Results.NotFound();

            await auditLogService.LogAsync(
                "devices.change_status",
                isSuccess: true,
                entityType: "SlaveDevice",
                entityId: device.Id.ToString(),
                details: $"Changed device '{device.Name}' status to '{(device.IsEnabled ? "enabled" : "disabled")}'.",
                cancellationToken: cancellationToken);

            return Results.Ok(device);
        })
        .WithSummary("Enable or disable slave device");

        group.MapPost("/registers", async (
            CreateRegisterDefinitionRequest request,
            DeviceManagementService deviceManagementService,
            AuditLogService auditLogService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var register = await deviceManagementService.CreateRegisterDefinitionAsync(
                    request,
                    cancellationToken);

                await auditLogService.LogAsync(
                    "devices.create_register",
                    isSuccess: true,
                    entityType: "RegisterDefinition",
                    entityId: register.Id.ToString(),
                    details: $"Created register '{register.Name}' at address {register.Address}.",
                    cancellationToken: cancellationToken);

                return Results.Created($"/api/device-management/registers/{register.Id}", register);
            }
            catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
            {
                await auditLogService.LogAsync(
                    "devices.create_register",
                    isSuccess: false,
                    entityType: "RegisterDefinition",
                    details: exception.Message,
                    cancellationToken: cancellationToken);

                return Results.BadRequest(new { message = exception.Message });
            }
        })
        .WithSummary("Create register definition");

        return app;
    }
}

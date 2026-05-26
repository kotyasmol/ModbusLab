using ModbusLab.Application.Devices;

namespace ModbusLab.Api.Endpoints;

public static class DeviceEndpoints
{
    public static IEndpointRouteBuilder MapDeviceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/devices")
            .WithTags("Devices")
            .RequireAuthorization();

        group.MapGet("/", async (
            DeviceQueryService deviceQueryService,
            CancellationToken cancellationToken) =>
        {
            var devices = await deviceQueryService.GetDevicesAsync(cancellationToken);

            return Results.Ok(devices);
        })
        .WithSummary("Get all Modbus slave devices");

        group.MapGet("/{deviceId:guid}/registers", async (
            Guid deviceId,
            DeviceQueryService deviceQueryService,
            CancellationToken cancellationToken) =>
        {
            var registers = await deviceQueryService.GetDeviceRegistersAsync(
                deviceId,
                cancellationToken);

            return Results.Ok(registers);
        })
        .WithSummary("Get registers for selected device");

        return app;
    }
}

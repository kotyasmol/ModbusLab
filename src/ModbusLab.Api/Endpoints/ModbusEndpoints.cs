using ModbusLab.Application.Abstractions;
using ModbusLab.Application.Modbus;

namespace ModbusLab.Api.Endpoints;

public static class ModbusEndpoints
{
    public static IEndpointRouteBuilder MapModbusEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/modbus")
            .WithTags("Modbus")
            .RequireAuthorization();

        group.MapPost("/read", async (
            ReadRegisterRequest request,
            ModbusRegisterService modbusRegisterService,
            CancellationToken cancellationToken) =>
        {
            var result = await modbusRegisterService.ReadAsync(
                request,
                cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        })
        .WithSummary("Read Modbus register");

        group.MapPost("/write", async (
            WriteRegisterRequest request,
            ModbusRegisterService modbusRegisterService,
            CancellationToken cancellationToken) =>
        {
            var result = await modbusRegisterService.WriteAsync(
                request,
                cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        })
        .WithSummary("Write Modbus register");

        group.MapGet("/logs", async (
            IModbusLogRepository logRepository,
            CancellationToken cancellationToken) =>
        {
            var logs = await logRepository.GetLatestAsync(
                count: 100,
                cancellationToken);

            return Results.Ok(logs);
        })
        .WithSummary("Get latest Modbus operation logs");

        return app;
    }
}

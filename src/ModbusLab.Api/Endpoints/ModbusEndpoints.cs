using ModbusLab.Api.Audit;
using ModbusLab.Api.Auth;
using ModbusLab.Application.Abstractions;
using ModbusLab.Application.Modbus;

namespace ModbusLab.Api.Endpoints;

public static class ModbusEndpoints
{
    public static IEndpointRouteBuilder MapModbusEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/modbus")
            .WithTags("Modbus");

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
        .RequireAuthorization(AuthPolicies.RequireViewer)
        .WithSummary("Read Modbus register");

        group.MapPost("/write", async (
            WriteRegisterRequest request,
            ModbusRegisterService modbusRegisterService,
            AuditLogService auditLogService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await modbusRegisterService.WriteAsync(
                    request,
                    cancellationToken);

                await auditLogService.LogAsync(
                    "modbus.write_register",
                    result.IsSuccess,
                    entityType: "Register",
                    entityId: $"{request.SlaveAddress}:{request.RegisterAddress}",
                    details: $"Value={request.Value}; Message={result.Message}",
                    cancellationToken: cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(result)
                    : Results.BadRequest(result);
            }
            catch (Exception exception)
            {
                await auditLogService.LogAsync(
                    "modbus.write_register",
                    isSuccess: false,
                    entityType: "Register",
                    entityId: $"{request.SlaveAddress}:{request.RegisterAddress}",
                    details: exception.Message,
                    cancellationToken: cancellationToken);

                throw;
            }
        })
        .RequireAuthorization(AuthPolicies.RequireEngineer)
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
        .RequireAuthorization(AuthPolicies.RequireViewer)
        .WithSummary("Get latest Modbus operation logs");

        return app;
    }
}

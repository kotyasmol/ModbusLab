using ModbusLab.Domain.Logs;

namespace ModbusLab.Application.Modbus;

public sealed record RegisterOperationResult(
    bool IsSuccess,
    ModbusOperationStatus Status,
    int? Value,
    string Message)
{
    public static RegisterOperationResult Success(int? value, string message)
    {
        return new RegisterOperationResult(
            true,
            ModbusOperationStatus.Success,
            value,
            message);
    }

    public static RegisterOperationResult Failed(string message)
    {
        return new RegisterOperationResult(
            false,
            ModbusOperationStatus.Failed,
            null,
            message);
    }

    public static RegisterOperationResult Rejected(string message)
    {
        return new RegisterOperationResult(
            false,
            ModbusOperationStatus.Rejected,
            null,
            message);
    }
}
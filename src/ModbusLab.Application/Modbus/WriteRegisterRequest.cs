namespace ModbusLab.Application.Modbus;

public sealed record WriteRegisterRequest(
    int SlaveAddress,
    int RegisterAddress,
    int Value);
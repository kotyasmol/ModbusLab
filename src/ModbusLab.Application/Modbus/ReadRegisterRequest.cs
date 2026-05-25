namespace ModbusLab.Application.Modbus;

public sealed record ReadRegisterRequest(
    int SlaveAddress,
    int RegisterAddress);

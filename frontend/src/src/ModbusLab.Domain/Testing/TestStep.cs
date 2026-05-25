namespace ModbusLab.Domain.Testing;

public sealed class TestStep
{
    public Guid Id { get; private set; }

    public Guid TestProfileId { get; private set; }

    public int OrderIndex { get; private set; }

    public string Name { get; private set; }

    public TestStepType Type { get; private set; }

    public int? SlaveAddress { get; private set; }

    public int? RegisterAddress { get; private set; }

    public int? Value { get; private set; }

    public int? MinValue { get; private set; }

    public int? MaxValue { get; private set; }

    public int? DelayMs { get; private set; }

    private TestStep()
    {
        Name = string.Empty;
    }

    private TestStep(
        Guid testProfileId,
        int orderIndex,
        string name,
        TestStepType type,
        int? slaveAddress = null,
        int? registerAddress = null,
        int? value = null,
        int? minValue = null,
        int? maxValue = null,
        int? delayMs = null)
    {
        if (testProfileId == Guid.Empty)
            throw new ArgumentException("Test profile id cannot be empty.", nameof(testProfileId));

        if (orderIndex <= 0)
            throw new ArgumentOutOfRangeException(nameof(orderIndex), "Order index must be positive.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Test step name cannot be empty.", nameof(name));

        if (slaveAddress.HasValue && slaveAddress is < 1 or > 247)
            throw new ArgumentOutOfRangeException(nameof(slaveAddress), "Modbus slave address must be between 1 and 247.");

        if (registerAddress.HasValue && registerAddress < 0)
            throw new ArgumentOutOfRangeException(nameof(registerAddress), "Register address cannot be negative.");

        if (minValue.HasValue && maxValue.HasValue && minValue.Value > maxValue.Value)
            throw new ArgumentException("Min value cannot be greater than max value.");

        if (delayMs.HasValue && delayMs <= 0)
            throw new ArgumentOutOfRangeException(nameof(delayMs), "Delay must be positive.");

        Id = Guid.NewGuid();
        TestProfileId = testProfileId;
        OrderIndex = orderIndex;
        Name = name.Trim();
        Type = type;
        SlaveAddress = slaveAddress;
        RegisterAddress = registerAddress;
        Value = value;
        MinValue = minValue;
        MaxValue = maxValue;
        DelayMs = delayMs;
    }

    public static TestStep CreateWriteRegister(
        Guid testProfileId,
        int orderIndex,
        string name,
        int slaveAddress,
        int registerAddress,
        int value)
    {
        return new TestStep(
            testProfileId,
            orderIndex,
            name,
            TestStepType.WriteRegister,
            slaveAddress,
            registerAddress,
            value);
    }

    public static TestStep CreateDelay(
        Guid testProfileId,
        int orderIndex,
        string name,
        int delayMs)
    {
        return new TestStep(
            testProfileId,
            orderIndex,
            name,
            TestStepType.Delay,
            delayMs: delayMs);
    }

    public static TestStep CreateCheckRegisterRange(
        Guid testProfileId,
        int orderIndex,
        string name,
        int slaveAddress,
        int registerAddress,
        int minValue,
        int maxValue)
    {
        return new TestStep(
            testProfileId,
            orderIndex,
            name,
            TestStepType.CheckRegisterRange,
            slaveAddress,
            registerAddress,
            minValue: minValue,
            maxValue: maxValue);
    }
}

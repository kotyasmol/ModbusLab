import { type FormEvent, useMemo, useState } from "react";
import { useDeviceManagement } from "../features/deviceManagement/useDeviceManagement";

function getErrorMessage(error: unknown): string {
  if (error instanceof Error) return error.message;

  if (typeof error === "object" && error !== null && "message" in error) {
    const message = (error as { message?: unknown }).message;
    if (typeof message === "string") return message;
  }

  return "Request failed.";
}

function toNullableNumber(value: string): number | null {
  if (!value.trim()) return null;
  const parsed = Number(value);
  return Number.isNaN(parsed) ? null : parsed;
}

export function DeviceManagementPage() {
  const {
    deviceTypesQuery,
    devicesQuery,
    createDeviceType,
    creatingDeviceType,
    createDevice,
    creatingDevice,
    changeDeviceStatus,
    changingDeviceStatus,
    createRegisterDefinition,
    creatingRegisterDefinition,
  } = useDeviceManagement();

  const [typeName, setTypeName] = useState("");
  const [typeDescription, setTypeDescription] = useState("");
  const [deviceName, setDeviceName] = useState("");
  const [slaveAddress, setSlaveAddress] = useState("");
  const [deviceTypeId, setDeviceTypeId] = useState("");
  const [registerTypeId, setRegisterTypeId] = useState("");
  const [registerAddress, setRegisterAddress] = useState("");
  const [registerName, setRegisterName] = useState("");
  const [accessMode, setAccessMode] = useState("ReadOnly");
  const [unit, setUnit] = useState("");
  const [minValue, setMinValue] = useState("");
  const [maxValue, setMaxValue] = useState("");
  const [initialValue, setInitialValue] = useState("0");
  const [error, setError] = useState<string | null>(null);

  const deviceTypes = useMemo(() => deviceTypesQuery.data ?? [], [deviceTypesQuery.data]);
  const typeById = useMemo(
    () => new Map(deviceTypes.map((type) => [type.id, type.name])),
    [deviceTypes]
  );

  const selectedDeviceTypeId = deviceTypeId || deviceTypes[0]?.id || "";
  const selectedRegisterTypeId = registerTypeId || deviceTypes[0]?.id || "";

  const handleCreateType = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);

    try {
      const created = await createDeviceType({
        name: typeName,
        description: typeDescription.trim() || null,
      });
      setTypeName("");
      setTypeDescription("");
      setDeviceTypeId(created.id);
      setRegisterTypeId(created.id);
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    }
  };

  const handleCreateDevice = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);

    try {
      await createDevice({
        name: deviceName,
        slaveAddress: Number(slaveAddress),
        deviceTypeId: selectedDeviceTypeId,
      });
      setDeviceName("");
      setSlaveAddress("");
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    }
  };

  const handleCreateRegister = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError(null);

    try {
      await createRegisterDefinition({
        deviceTypeId: selectedRegisterTypeId,
        address: Number(registerAddress),
        name: registerName,
        accessMode,
        unit: unit.trim() || null,
        minValue: toNullableNumber(minValue),
        maxValue: toNullableNumber(maxValue),
        description: null,
        initialValue: toNullableNumber(initialValue),
      });
      setRegisterAddress("");
      setRegisterName("");
      setUnit("");
      setMinValue("");
      setMaxValue("");
      setInitialValue("0");
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    }
  };

  const handleStatusChange = async (deviceId: string, isEnabled: boolean) => {
    setError(null);

    try {
      await changeDeviceStatus(deviceId, isEnabled);
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    }
  };

  return (
    <section className="device-management-layout">
      <div className="panel">
        <div className="panel-header">
          <h2>Device management</h2>
          <span className="panel-counter">{devicesQuery.data?.length ?? 0}</span>
        </div>

        {error && <div className="form-error">{error}</div>}
        {devicesQuery.isError && <div className="form-error">Failed to load devices.</div>}

        <div className="table-wrapper">
          <table>
            <thead>
              <tr>
                <th>Name</th>
                <th>Type</th>
                <th>Slave</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {(devicesQuery.data ?? []).map((device) => (
                <tr key={device.id}>
                  <td>{device.name}</td>
                  <td>{typeById.get(device.deviceTypeId) ?? device.deviceTypeId}</td>
                  <td>{device.slaveAddress}</td>
                  <td>
                    <label className="inline-toggle">
                      <input
                        type="checkbox"
                        checked={device.isEnabled}
                        disabled={changingDeviceStatus}
                        onChange={(event) =>
                          void handleStatusChange(device.id, event.target.checked)
                        }
                      />
                      <span className={device.isEnabled ? "log-status success" : "log-status failed"}>
                        {device.isEnabled ? "Enabled" : "Disabled"}
                      </span>
                    </label>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      <aside className="panel form-stack">
        <form className="auth-form" onSubmit={handleCreateType}>
          <h3>Create device type</h3>
          <label>
            Name
            <input value={typeName} onChange={(event) => setTypeName(event.target.value)} />
          </label>
          <label>
            Description
            <textarea
              value={typeDescription}
              onChange={(event) => setTypeDescription(event.target.value)}
            />
          </label>
          <button className="primary-button" disabled={creatingDeviceType}>
            {creatingDeviceType ? "Creating..." : "Create type"}
          </button>
        </form>

        <form className="auth-form" onSubmit={handleCreateDevice}>
          <h3>Create device</h3>
          <label>
            Name
            <input value={deviceName} onChange={(event) => setDeviceName(event.target.value)} />
          </label>
          <label>
            Device type
            <select
              value={selectedDeviceTypeId}
              onChange={(event) => setDeviceTypeId(event.target.value)}
            >
              {deviceTypes.map((type) => (
                <option key={type.id} value={type.id}>
                  {type.name}
                </option>
              ))}
            </select>
          </label>
          <label>
            Slave address
            <input
              type="number"
              value={slaveAddress}
              onChange={(event) => setSlaveAddress(event.target.value)}
            />
          </label>
          <button className="primary-button" disabled={creatingDevice || deviceTypes.length === 0}>
            {creatingDevice ? "Creating..." : "Create device"}
          </button>
        </form>

        <form className="auth-form" onSubmit={handleCreateRegister}>
          <h3>Create register</h3>
          <label>
            Device type
            <select
              value={selectedRegisterTypeId}
              onChange={(event) => setRegisterTypeId(event.target.value)}
            >
              {deviceTypes.map((type) => (
                <option key={type.id} value={type.id}>
                  {type.name}
                </option>
              ))}
            </select>
          </label>
          <label>
            Address
            <input
              type="number"
              value={registerAddress}
              onChange={(event) => setRegisterAddress(event.target.value)}
            />
          </label>
          <label>
            Name
            <input value={registerName} onChange={(event) => setRegisterName(event.target.value)} />
          </label>
          <label>
            Access
            <select value={accessMode} onChange={(event) => setAccessMode(event.target.value)}>
              <option value="ReadOnly">ReadOnly</option>
              <option value="ReadWrite">ReadWrite</option>
            </select>
          </label>
          <div className="compact-form-row">
            <label>
              Unit
              <input value={unit} onChange={(event) => setUnit(event.target.value)} />
            </label>
            <label>
              Initial
              <input
                type="number"
                value={initialValue}
                onChange={(event) => setInitialValue(event.target.value)}
              />
            </label>
          </div>
          <div className="compact-form-row">
            <label>
              Min
              <input
                type="number"
                value={minValue}
                onChange={(event) => setMinValue(event.target.value)}
              />
            </label>
            <label>
              Max
              <input
                type="number"
                value={maxValue}
                onChange={(event) => setMaxValue(event.target.value)}
              />
            </label>
          </div>
          <button
            className="primary-button"
            disabled={creatingRegisterDefinition || deviceTypes.length === 0}
          >
            {creatingRegisterDefinition ? "Creating..." : "Create register"}
          </button>
        </form>
      </aside>
    </section>
  );
}

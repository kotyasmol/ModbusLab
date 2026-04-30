import * as signalR from "@microsoft/signalr";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export function createModbusHubConnection() {
  return new signalR.HubConnectionBuilder()
    .withUrl(`${API_BASE_URL}/hubs/modbus`)
    .withAutomaticReconnect()
    .build();
}
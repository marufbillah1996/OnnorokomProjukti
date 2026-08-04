import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { getAccessToken } from "@/shared/lib/auth-storage";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5000/api";
// Strip the trailing "/api" — the hub is mapped at the API host's root ("/hubs/notifications"),
// not under the "/api" prefix (see backend Program.cs: app.MapHub<NotificationsHub>("/hubs/notifications")).
const API_HOST = API_BASE_URL.replace(/\/api\/?$/, "");

let connection: HubConnection | null = null;

/**
 * Lazily creates (or reuses) the single SignalR connection for the current session. The access
 * token is read fresh on every (re)connection attempt via accessTokenFactory, since SignalR
 * cannot attach an Authorization header to the WebSocket upgrade — the backend's JWT bearer
 * handler instead reads it from "?access_token=" for requests under "/hubs" (see
 * backend Api/Extensions/AuthServiceExtensions.cs).
 */
export function getNotificationsConnection(): HubConnection {
  if (connection) {
    return connection;
  }

  connection = new HubConnectionBuilder()
    .withUrl(`${API_HOST}/hubs/notifications`, {
      accessTokenFactory: () => getAccessToken() ?? "",
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build();

  return connection;
}

export async function stopNotificationsConnection(): Promise<void> {
  if (connection) {
    await connection.stop();
    connection = null;
  }
}

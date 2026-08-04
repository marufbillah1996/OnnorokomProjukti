import { apiClient } from "@/shared/lib/api-client";
import type { NotificationDto } from "./types";

export async function getNotifications(unreadOnly: boolean): Promise<NotificationDto[]> {
  const { data } = await apiClient.get<NotificationDto[]>("/notifications", {
    params: { unreadOnly },
  });
  return data;
}

export async function markNotificationAsRead(id: string): Promise<void> {
  await apiClient.put<void>(`/notifications/${id}/read`);
}

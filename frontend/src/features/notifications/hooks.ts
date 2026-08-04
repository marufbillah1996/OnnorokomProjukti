"use client";

import { useEffect } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  getNotificationsConnection,
  stopNotificationsConnection,
} from "@/shared/lib/signalr-client";
import { getNotifications, markNotificationAsRead } from "./api";

export function useNotifications(unreadOnly = false, options?: { enabled?: boolean }) {
  return useQuery({
    queryKey: ["notifications", unreadOnly],
    queryFn: () => getNotifications(unreadOnly),
    enabled: options?.enabled,
  });
}

export function useMarkNotificationAsRead() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => markNotificationAsRead(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["notifications"] });
    },
  });
}

/**
 * Connects to the notifications SignalR hub once per mount and invalidates this feature's React
 * Query cache whenever a live push arrives, so the bell's unread list refetches and picks up the
 * newly persisted Notification row (the backend writes to the database before pushing, so
 * GET /notifications is already up to date by the time the push arrives).
 *
 * No return value — this is a side-effect-only hook. Call it exactly once (from
 * ToastListener, which every role layout mounts alongside the bell).
 */
export function useNotificationSocket(): void {
  const queryClient = useQueryClient();

  useEffect(() => {
    const connection = getNotificationsConnection();

    // A failed/optional realtime connection must never crash the app.
    connection.start().catch(console.error);

    const invalidate = () => {
      queryClient.invalidateQueries({ queryKey: ["notifications"] });
    };

    connection.on("AssignmentPublished", invalidate);
    connection.on("SubmissionGraded", invalidate);

    return () => {
      connection.off("AssignmentPublished", invalidate);
      connection.off("SubmissionGraded", invalidate);
      stopNotificationsConnection().catch(console.error);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps -- connect once per mount, by design.
  }, []);
}

"use client";

import { useEffect, useState } from "react";
import { getNotificationsConnection } from "@/shared/lib/signalr-client";
import { useNotificationSocket } from "../hooks";
import type { AssignmentPublishedPayload } from "../types";

/**
 * Live SignalR push payload for "SubmissionGraded" — note this is DIFFERENT from the persisted
 * NotificationDto's parsed payload shape (which uses "marks", see types.ts). The hub pushes
 * "marksAwarded" directly as the event argument.
 */
interface SubmissionGradedPushPayload {
  submissionId: string;
  marksAwarded: number | null;
}

/**
 * Registers a second set of listeners on the (already-connected, singleton) notifications hub
 * connection purely to surface an ephemeral toast — kept separate from useNotificationSocket so
 * that hook stays focused on cache invalidation only. Self-dismisses after 3 seconds.
 */
function useNotificationToasts(): string | null {
  const [toast, setToast] = useState<string | null>(null);

  useEffect(() => {
    const connection = getNotificationsConnection();

    const handleAssignmentPublished = (data: AssignmentPublishedPayload) => {
      setToast(`New assignment published: ${data.assignmentTitle}`);
    };

    const handleSubmissionGraded = (data: SubmissionGradedPushPayload) => {
      setToast(`Your submission was graded: ${data.marksAwarded ?? "—"} marks`);
    };

    connection.on("AssignmentPublished", handleAssignmentPublished);
    connection.on("SubmissionGraded", handleSubmissionGraded);

    return () => {
      connection.off("AssignmentPublished", handleAssignmentPublished);
      connection.off("SubmissionGraded", handleSubmissionGraded);
    };
  }, []);

  useEffect(() => {
    if (!toast) return;
    const timer = setTimeout(() => setToast(null), 3000);
    return () => clearTimeout(timer);
  }, [toast]);

  return toast;
}

/**
 * Mount once per role layout, alongside the NotificationBell. Owns the single call to
 * useNotificationSocket() (which connects the hub + invalidates the notifications cache on
 * every push) and renders a lightweight self-dismissing toast whenever a new
 * "AssignmentPublished" or "SubmissionGraded" event arrives. Renders nothing when idle.
 */
export function ToastListener() {
  useNotificationSocket();
  const toast = useNotificationToasts();

  if (!toast) return null;

  return (
    <div className="fixed bottom-4 right-4 z-[60] max-w-sm rounded-lg border border-border bg-surface px-4 py-3 text-sm text-foreground shadow-xl">
      {toast}
    </div>
  );
}

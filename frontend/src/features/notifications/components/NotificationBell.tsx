"use client";

import { useState } from "react";
import clsx from "clsx";
import { formatRelativeToNow } from "@/shared/lib/date";
import { SkeletonRows } from "@/shared/components/ui/Skeleton";
import { useMarkNotificationAsRead, useNotifications } from "../hooks";
import type { AssignmentPublishedPayload, NotificationDto, SubmissionGradedPayload } from "../types";

/**
 * Parses a NotificationDto's JSON-encoded "payload" string into a one-line human description.
 * Guards defensively against malformed payloads: logs to console.error and returns null so the
 * caller can skip rendering that notification's detail line instead of crashing the bell.
 */
function describeNotification(notification: NotificationDto): string | null {
  try {
    const parsed = JSON.parse(notification.payload) as unknown;

    if (notification.type === "AssignmentPublished") {
      const { assignmentTitle } = parsed as AssignmentPublishedPayload;
      return `New assignment published: ${assignmentTitle}`;
    }

    if (notification.type === "SubmissionGraded") {
      const { marks } = parsed as SubmissionGradedPayload;
      return `Your submission was graded: ${marks ?? "—"} marks`;
    }

    return null;
  } catch (error) {
    console.error("Failed to parse notification payload for notification", notification.id, error);
    return null;
  }
}

export function NotificationBell() {
  const [isOpen, setIsOpen] = useState(false);

  const unreadQuery = useNotifications(true);
  const listQuery = useNotifications(false, { enabled: isOpen });
  const markAsRead = useMarkNotificationAsRead();

  const unreadCount = unreadQuery.data?.length ?? 0;

  function handleItemClick(notification: NotificationDto) {
    if (!notification.isRead) {
      markAsRead.mutate(notification.id);
    }
  }

  return (
    <div className="relative">
      <button
        type="button"
        onClick={() => setIsOpen((open) => !open)}
        aria-label="Notifications"
        aria-expanded={isOpen}
        className="relative inline-flex h-9 w-9 items-center justify-center rounded-full text-lg hover:bg-surface-muted"
      >
        <span aria-hidden="true">🔔</span>
        {unreadCount > 0 && (
          <span
            className="absolute -right-0.5 -top-0.5 inline-flex h-4 min-w-4 items-center justify-center rounded-full bg-danger-500 px-1 text-[10px] font-semibold leading-none text-white"
            aria-label={`${unreadCount} unread notifications`}
          >
            {unreadCount > 9 ? "9+" : unreadCount}
          </span>
        )}
      </button>

      {isOpen && (
        <>
          {/* Backdrop: click-away to close, keeps the dropdown implementation to a plain div. */}
          <div className="fixed inset-0 z-40" onClick={() => setIsOpen(false)} aria-hidden="true" />
          <div className="absolute right-0 z-50 mt-2 w-80 rounded-lg border border-border bg-surface shadow-xl">
            <div className="border-b border-border px-4 py-3">
              <h3 className="text-sm font-semibold text-foreground">Notifications</h3>
            </div>
            <div className="max-h-96 overflow-y-auto">
              {listQuery.isLoading && (
                <div className="p-4">
                  <SkeletonRows rows={3} />
                </div>
              )}

              {!listQuery.isLoading && (listQuery.data?.length ?? 0) === 0 && (
                <p className="px-4 py-6 text-center text-sm text-foreground/60">No notifications yet.</p>
              )}

              {!listQuery.isLoading && listQuery.data && listQuery.data.length > 0 && (
                <ul>
                  {listQuery.data.map((notification) => {
                    const description = describeNotification(notification);
                    return (
                      <li key={notification.id}>
                        <button
                          type="button"
                          onClick={() => handleItemClick(notification)}
                          className={clsx(
                            "flex w-full flex-col gap-1 border-b border-border px-4 py-3 text-left last:border-b-0 hover:bg-surface-muted",
                            !notification.isRead && "bg-brand-100/40",
                          )}
                        >
                          <span className="flex items-center gap-2 text-sm text-foreground">
                            {!notification.isRead && (
                              <span
                                className="inline-block h-1.5 w-1.5 shrink-0 rounded-full bg-brand-600"
                                aria-hidden="true"
                              />
                            )}
                            {description ?? "Notification"}
                          </span>
                          <span className="text-xs text-foreground/60">
                            {formatRelativeToNow(notification.createdAt)}
                          </span>
                        </button>
                      </li>
                    );
                  })}
                </ul>
              )}
            </div>
          </div>
        </>
      )}
    </div>
  );
}

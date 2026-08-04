/**
 * Every deadline/timestamp from the API is UTC (ISO 8601 with a "Z" suffix — see
 * DocsAndPlan/IMPLEMENTATION_PLAN.md section 10.4). This module is the ONLY place that converts
 * to the viewer's local timezone for display. The authoritative "is it past the deadline?" check
 * always happens on the server — nothing here is ever used to gate a submission client-side,
 * only to render a human-readable date/countdown.
 */

export function toLocalDateTime(utcIso: string): Date {
  return new Date(utcIso);
}

export function formatDateTime(utcIso: string): string {
  return toLocalDateTime(utcIso).toLocaleString(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  });
}

export function formatDate(utcIso: string): string {
  return toLocalDateTime(utcIso).toLocaleDateString(undefined, {
    dateStyle: "medium",
  });
}

export function isPastDeadline(deadlineUtcIso: string, now: Date = new Date()): boolean {
  return now.getTime() > toLocalDateTime(deadlineUtcIso).getTime();
}

/** Human-friendly "in 3 days" / "2 hours ago" style label for dashboard deadline widgets. */
export function formatRelativeToNow(utcIso: string, now: Date = new Date()): string {
  const targetMs = toLocalDateTime(utcIso).getTime();
  const diffMs = targetMs - now.getTime();
  const diffMinutes = Math.round(diffMs / 60000);

  const formatter = new Intl.RelativeTimeFormat(undefined, { numeric: "auto" });

  const absMinutes = Math.abs(diffMinutes);
  if (absMinutes < 60) {
    return formatter.format(diffMinutes, "minute");
  }

  const diffHours = Math.round(diffMinutes / 60);
  if (Math.abs(diffHours) < 24) {
    return formatter.format(diffHours, "hour");
  }

  const diffDays = Math.round(diffHours / 24);
  return formatter.format(diffDays, "day");
}

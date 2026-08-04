"use client";

import { Badge } from "@/shared/components/ui/Badge";
import { formatRelativeToNow, isPastDeadline } from "@/shared/lib/date";

export interface DeadlineCountdownProps {
  deadlineUtc: string;
}

/** Small relative-time chip for a deadline — warning tone once it's passed, neutral otherwise. */
export function DeadlineCountdown({ deadlineUtc }: DeadlineCountdownProps) {
  const overdue = isPastDeadline(deadlineUtc);

  return (
    <Badge tone={overdue ? "warning" : "neutral"}>
      {overdue ? "Deadline passed" : `Due ${formatRelativeToNow(deadlineUtc)}`}
    </Badge>
  );
}

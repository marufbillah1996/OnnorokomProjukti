import clsx from "clsx";

type Tone = "neutral" | "brand" | "success" | "warning" | "danger";

const toneClasses: Record<Tone, string> = {
  neutral: "bg-surface-muted text-foreground",
  brand: "bg-brand-100 text-brand-700",
  success: "bg-green-100 text-success-500",
  warning: "bg-amber-100 text-warning-500",
  danger: "bg-red-100 text-danger-500",
};

/** Maps the common domain statuses (assignment/submission) to a sensible visual tone. */
const STATUS_TONE: Record<string, Tone> = {
  Draft: "neutral",
  Published: "brand",
  Pending: "warning",
  Graded: "success",
  Returned: "danger",
};

export function Badge({ tone = "neutral", children }: { tone?: Tone; children: React.ReactNode }) {
  return (
    <span
      className={clsx(
        "inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium",
        toneClasses[tone],
      )}
    >
      {children}
    </span>
  );
}

/** Convenience wrapper: <StatusBadge status={assignment.status} /> picks the right tone automatically. */
export function StatusBadge({ status }: { status: string }) {
  return <Badge tone={STATUS_TONE[status] ?? "neutral"}>{status}</Badge>;
}

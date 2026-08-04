import { StatusBadge } from "@/shared/components/ui/Badge";
import { Card, CardBody, CardHeader, CardTitle } from "@/shared/components/ui/Card";
import { formatDateTime } from "@/shared/lib/date";
import { DeadlineCountdown } from "./DeadlineCountdown";
import type { AssignmentDto } from "../types";

export interface AssignmentDetailCardProps {
  assignment: AssignmentDto;
  /** Purely a framing hint — a student never needs to be told an assignment is "Draft" (they
   *  can't see it until it's Published anyway), so the student variant leads with the deadline
   *  countdown instead of the status badge. Same data either way. */
  variant?: "teacher" | "student";
}

/** Read-only detail view of a single assignment — shared by the teacher detail page and the
 *  student assignment detail page. */
export function AssignmentDetailCard({ assignment, variant = "teacher" }: AssignmentDetailCardProps) {
  return (
    <Card>
      <CardHeader className="flex flex-col gap-2">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <CardTitle>{assignment.title}</CardTitle>
          {variant === "teacher" ? (
            <StatusBadge status={assignment.status} />
          ) : (
            <DeadlineCountdown deadlineUtc={assignment.deadlineUtc} />
          )}
        </div>
        <p className="text-sm text-foreground/60">
          {assignment.className} · {assignment.subjectName}
        </p>
      </CardHeader>
      <CardBody className="flex flex-col gap-4">
        <p className="whitespace-pre-wrap text-sm text-foreground">{assignment.description}</p>

        <dl className="grid grid-cols-2 gap-4 text-sm sm:grid-cols-3">
          <div>
            <dt className="text-foreground/60">Deadline</dt>
            <dd className="font-medium text-foreground">{formatDateTime(assignment.deadlineUtc)}</dd>
          </div>
          <div>
            <dt className="text-foreground/60">Max marks</dt>
            <dd className="font-medium text-foreground">{assignment.maxMarks}</dd>
          </div>
          <div>
            <dt className="text-foreground/60">Status</dt>
            <dd className="font-medium text-foreground">{assignment.status}</dd>
          </div>
          <div>
            <dt className="text-foreground/60">Teacher</dt>
            <dd className="font-medium text-foreground">{assignment.createdByTeacherName}</dd>
          </div>
          <div>
            <dt className="text-foreground/60">Created</dt>
            <dd className="font-medium text-foreground">{formatDateTime(assignment.createdAt)}</dd>
          </div>
        </dl>
      </CardBody>
    </Card>
  );
}

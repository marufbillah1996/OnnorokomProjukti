"use client";

import Link from "next/link";
import { Card, CardBody, CardHeader, CardTitle } from "@/shared/components/ui/Card";
import { SkeletonRows } from "@/shared/components/ui/Skeleton";
import { StatusBadge } from "@/shared/components/ui/Badge";
import { formatDateTime } from "@/shared/lib/date";
import { usePublishedAssignments } from "@/features/assignments/hooks";
import { DeadlineCountdown } from "@/features/assignments/components/DeadlineCountdown";
import { useMySubmissions } from "@/features/submissions/hooks";

export default function StudentDashboardPage() {
  const upcomingQuery = usePublishedAssignments({ pageSize: 10, sortBy: "deadline" });
  const submissionsQuery = useMySubmissions({ pageSize: 5 });

  const upcoming = upcomingQuery.data?.items ?? [];
  const recentSubmissions = submissionsQuery.data?.items ?? [];

  return (
    <div className="flex flex-col gap-6">
      <Card>
        <CardHeader>
          <CardTitle>Upcoming deadlines</CardTitle>
        </CardHeader>
        <CardBody>
          {upcomingQuery.isLoading && <SkeletonRows rows={4} />}

          {!upcomingQuery.isLoading && upcoming.length === 0 && (
            <p className="text-sm text-foreground/60">No published assignments right now.</p>
          )}

          {!upcomingQuery.isLoading && upcoming.length > 0 && (
            <ul className="flex flex-col gap-3">
              {upcoming.map((assignment) => (
                <li key={assignment.id}>
                  <Link
                    href={`/student/assignments/${assignment.id}`}
                    className="flex flex-wrap items-center justify-between gap-2 rounded-md border border-border p-3 hover:bg-surface-muted"
                  >
                    <div>
                      <p className="text-sm font-medium text-foreground">{assignment.title}</p>
                      <p className="text-xs text-foreground/60">
                        {assignment.className} · {assignment.subjectName}
                      </p>
                    </div>
                    <DeadlineCountdown deadlineUtc={assignment.deadlineUtc} />
                  </Link>
                </li>
              ))}
            </ul>
          )}
        </CardBody>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Recent submissions</CardTitle>
        </CardHeader>
        <CardBody>
          {submissionsQuery.isLoading && <SkeletonRows rows={3} />}

          {!submissionsQuery.isLoading && recentSubmissions.length === 0 && (
            <p className="text-sm text-foreground/60">You haven&apos;t submitted anything yet.</p>
          )}

          {!submissionsQuery.isLoading && recentSubmissions.length > 0 && (
            <ul className="flex flex-col gap-3">
              {recentSubmissions.map((submission) => (
                <li
                  key={submission.id}
                  className="flex flex-wrap items-center justify-between gap-2 rounded-md border border-border p-3"
                >
                  <div>
                    <p className="text-sm font-medium text-foreground">{submission.assignmentTitle}</p>
                    <p className="text-xs text-foreground/60">
                      Submitted {formatDateTime(submission.submittedAt)}
                    </p>
                  </div>
                  <div className="flex items-center gap-3">
                    {submission.status === "Graded" && (
                      <span className="text-sm font-medium text-foreground">
                        {submission.marksAwarded ?? "—"} / {submission.maxMarks}
                      </span>
                    )}
                    <StatusBadge status={submission.status} />
                  </div>
                </li>
              ))}
            </ul>
          )}
        </CardBody>
      </Card>
    </div>
  );
}

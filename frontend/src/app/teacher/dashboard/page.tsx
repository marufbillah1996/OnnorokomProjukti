"use client";

import Link from "next/link";
import { useMyAssignments } from "@/features/assignments/hooks";
import { Card, CardBody, CardHeader, CardTitle } from "@/shared/components/ui/Card";
import { StatusBadge } from "@/shared/components/ui/Badge";
import { SkeletonRows } from "@/shared/components/ui/Skeleton";
import { formatDateTime } from "@/shared/lib/date";

export default function TeacherDashboardPage() {
  const { data, isLoading } = useMyAssignments({ pageSize: 5, sortBy: "deadline" });

  const assignments = data?.items ?? [];
  const draftCount = assignments.filter((assignment) => assignment.status === "Draft").length;
  const publishedCount = assignments.filter((assignment) => assignment.status === "Published").length;

  return (
    <div className="flex flex-col gap-6">
      <h2 className="text-xl font-semibold text-foreground">Dashboard</h2>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Card>
          <CardBody className="flex flex-col gap-1">
            <p className="text-sm text-foreground/60">Draft</p>
            <p className="text-2xl font-semibold text-foreground">{draftCount}</p>
          </CardBody>
        </Card>
        <Card>
          <CardBody className="flex flex-col gap-1">
            <p className="text-sm text-foreground/60">Published</p>
            <p className="text-2xl font-semibold text-foreground">{publishedCount}</p>
          </CardBody>
        </Card>
      </div>

      <Card>
        <CardHeader className="flex items-center justify-between">
          <CardTitle>Upcoming assignments</CardTitle>
          <Link href="/teacher/assignments" className="text-sm font-medium text-brand-600 hover:text-brand-700">
            View all
          </Link>
        </CardHeader>
        <CardBody>
          {isLoading && <SkeletonRows rows={5} />}

          {!isLoading && assignments.length === 0 && (
            <p className="py-6 text-center text-sm text-foreground/60">No assignments yet.</p>
          )}

          {!isLoading && assignments.length > 0 && (
            <ul className="flex flex-col divide-y divide-border">
              {assignments.map((assignment) => (
                <li key={assignment.id} className="flex items-center justify-between gap-4 py-3">
                  <div>
                    <p className="text-sm font-medium text-foreground">{assignment.title}</p>
                    <p className="text-xs text-foreground/60">
                      {assignment.className} · {assignment.subjectName} · Due{" "}
                      {formatDateTime(assignment.deadlineUtc)}
                    </p>
                  </div>
                  <StatusBadge status={assignment.status} />
                </li>
              ))}
            </ul>
          )}
        </CardBody>
      </Card>
    </div>
  );
}

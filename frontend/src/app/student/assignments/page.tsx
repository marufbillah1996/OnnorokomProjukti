"use client";

import Link from "next/link";
import { Card, CardBody, CardHeader, CardTitle } from "@/shared/components/ui/Card";
import { Input } from "@/shared/components/ui/Input";
import { Pagination } from "@/shared/components/ui/Pagination";
import { SkeletonRows } from "@/shared/components/ui/Skeleton";
import { StatusBadge } from "@/shared/components/ui/Badge";
import { Button } from "@/shared/components/ui/Button";
import { usePagination } from "@/shared/hooks/usePagination";
import { usePublishedAssignments } from "@/features/assignments/hooks";
import { DeadlineCountdown } from "@/features/assignments/components/DeadlineCountdown";

export default function StudentAssignmentsPage() {
  const { search, updateSearch, query, setPage } = usePagination();
  const assignmentsQuery = usePublishedAssignments(query);

  const assignments = assignmentsQuery.data?.items ?? [];

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h2 className="text-xl font-semibold text-foreground">Assignments</h2>
        <Input
          placeholder="Search assignments..."
          value={search}
          onChange={(event) => updateSearch(event.target.value)}
          className="w-full sm:w-72"
        />
      </div>

      {assignmentsQuery.isLoading && (
        <Card>
          <CardBody>
            <SkeletonRows rows={5} />
          </CardBody>
        </Card>
      )}

      {!assignmentsQuery.isLoading && assignments.length === 0 && (
        <Card>
          <CardBody>
            <p className="text-sm text-foreground/60">No published assignments found.</p>
          </CardBody>
        </Card>
      )}

      {!assignmentsQuery.isLoading && assignments.length > 0 && (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {assignments.map((assignment) => (
            <Card key={assignment.id}>
              <CardHeader className="flex flex-col gap-2">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <CardTitle>{assignment.title}</CardTitle>
                  <StatusBadge status={assignment.status} />
                </div>
                <p className="text-sm text-foreground/60">
                  {assignment.className} · {assignment.subjectName}
                </p>
              </CardHeader>
              <CardBody className="flex flex-col gap-4">
                <DeadlineCountdown deadlineUtc={assignment.deadlineUtc} />
                <Link href={`/student/assignments/${assignment.id}`}>
                  <Button variant="secondary" size="sm" className="w-full">
                    View
                  </Button>
                </Link>
              </CardBody>
            </Card>
          ))}
        </div>
      )}

      {assignmentsQuery.data && (
        <Pagination
          page={assignmentsQuery.data.page}
          totalPages={assignmentsQuery.data.totalPages}
          hasPreviousPage={assignmentsQuery.data.hasPreviousPage}
          hasNextPage={assignmentsQuery.data.hasNextPage}
          onPageChange={setPage}
        />
      )}
    </div>
  );
}

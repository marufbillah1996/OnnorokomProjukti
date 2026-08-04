"use client";

import { Card, CardBody, CardHeader, CardTitle } from "@/shared/components/ui/Card";
import { Pagination } from "@/shared/components/ui/Pagination";
import { usePagination } from "@/shared/hooks/usePagination";
import { AssignmentGrid } from "@/features/assignments/components/AssignmentGrid";
import { useAdminAssignments } from "@/features/assignments/hooks";
import { SubmissionHistoryTable } from "@/features/submissions/components/SubmissionHistoryTable";
import { useAdminSubmissions } from "@/features/submissions/hooks";

export default function AdminMonitoringPage() {
  const assignmentsPagination = usePagination();
  const submissionsPagination = usePagination();

  const assignmentsQuery = useAdminAssignments(assignmentsPagination.query);
  const submissionsQuery = useAdminSubmissions(submissionsPagination.query);

  return (
    <div className="flex flex-col gap-6">
      <h2 className="text-xl font-semibold text-foreground">Monitoring</h2>

      <Card>
        <CardHeader>
          <CardTitle>All assignments</CardTitle>
        </CardHeader>
        <CardBody className="p-0">
          <AssignmentGrid
            assignments={assignmentsQuery.data?.items ?? []}
            isLoading={assignmentsQuery.isLoading}
          />
          {assignmentsQuery.data && (
            <Pagination
              page={assignmentsPagination.page}
              totalPages={assignmentsQuery.data.totalPages}
              hasPreviousPage={assignmentsQuery.data.hasPreviousPage}
              hasNextPage={assignmentsQuery.data.hasNextPage}
              onPageChange={assignmentsPagination.setPage}
            />
          )}
        </CardBody>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>All submissions</CardTitle>
        </CardHeader>
        <CardBody className="p-0">
          <SubmissionHistoryTable
            submissions={submissionsQuery.data?.items ?? []}
            isLoading={submissionsQuery.isLoading}
          />
          {submissionsQuery.data && (
            <Pagination
              page={submissionsPagination.page}
              totalPages={submissionsQuery.data.totalPages}
              hasPreviousPage={submissionsQuery.data.hasPreviousPage}
              hasNextPage={submissionsQuery.data.hasNextPage}
              onPageChange={submissionsPagination.setPage}
            />
          )}
        </CardBody>
      </Card>
    </div>
  );
}

"use client";

import { use, useState } from "react";
import { useAssignment } from "@/features/assignments/hooks";
import { AssignmentDetailCard } from "@/features/assignments/components/AssignmentDetailCard";
import { useSubmissionsForAssignment } from "@/features/submissions/hooks";
import { GradingPanel } from "@/features/submissions/components/GradingPanel";
import type { SubmissionDto } from "@/features/submissions/types";
import { usePagination } from "@/shared/hooks/usePagination";
import { Button } from "@/shared/components/ui/Button";
import { StatusBadge } from "@/shared/components/ui/Badge";
import { Card, CardBody, CardHeader, CardTitle } from "@/shared/components/ui/Card";
import { DataTable, type DataTableColumn } from "@/shared/components/ui/DataTable";
import { Modal } from "@/shared/components/ui/Modal";
import { Pagination } from "@/shared/components/ui/Pagination";
import { Skeleton } from "@/shared/components/ui/Skeleton";
import { formatDateTime } from "@/shared/lib/date";

export default function TeacherAssignmentSubmissionsPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);

  const { data: assignment, isLoading: isLoadingAssignment } = useAssignment(id);

  const { setPage, query } = usePagination();
  const { data, isLoading } = useSubmissionsForAssignment(id, query);

  const [gradingSubmission, setGradingSubmission] = useState<SubmissionDto | undefined>(undefined);

  const columns: DataTableColumn<SubmissionDto>[] = [
    { key: "studentName", header: "Student", render: (row) => row.studentName },
    { key: "status", header: "Status", render: (row) => <StatusBadge status={row.status} /> },
    {
      key: "marks",
      header: "Marks",
      render: (row) => (row.marksAwarded === null ? "—" : `${row.marksAwarded} / ${row.maxMarks}`),
    },
    { key: "submittedAt", header: "Submitted", render: (row) => formatDateTime(row.submittedAt) },
    {
      key: "actions",
      header: "Actions",
      render: (row) => (
        <Button variant="secondary" size="sm" onClick={() => setGradingSubmission(row)}>
          Grade
        </Button>
      ),
    },
  ];

  return (
    <div className="flex flex-col gap-6">
      {isLoadingAssignment && <Skeleton className="h-40 w-full" />}
      {!isLoadingAssignment && assignment && (
        <AssignmentDetailCard assignment={assignment} variant="teacher" />
      )}

      <Card>
        <CardHeader>
          <CardTitle>Submissions</CardTitle>
        </CardHeader>
        <CardBody className="p-0">
          <DataTable
            columns={columns}
            data={data?.items ?? []}
            keyExtractor={(row) => row.id}
            isLoading={isLoading}
            emptyMessage="No submissions yet."
          />
          {data && (
            <Pagination
              page={data.page}
              totalPages={data.totalPages}
              hasPreviousPage={data.hasPreviousPage}
              hasNextPage={data.hasNextPage}
              onPageChange={setPage}
            />
          )}
        </CardBody>
      </Card>

      <Modal
        isOpen={Boolean(gradingSubmission)}
        onClose={() => setGradingSubmission(undefined)}
        title={gradingSubmission ? `Grade ${gradingSubmission.studentName}'s submission` : "Grade submission"}
      >
        {gradingSubmission && <GradingPanel submission={gradingSubmission} />}
      </Modal>
    </div>
  );
}

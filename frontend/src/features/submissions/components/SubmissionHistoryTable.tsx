"use client";

import { DataTable, type DataTableColumn } from "@/shared/components/ui/DataTable";
import { StatusBadge } from "@/shared/components/ui/Badge";
import { formatDateTime } from "@/shared/lib/date";
import type { SubmissionDto } from "../types";

export interface SubmissionHistoryTableProps {
  submissions: SubmissionDto[];
  isLoading?: boolean;
}

const FEEDBACK_TRUNCATE_LENGTH = 80;

function truncate(text: string, maxLength: number): string {
  return text.length > maxLength ? `${text.slice(0, maxLength).trimEnd()}…` : text;
}

const columns: DataTableColumn<SubmissionDto>[] = [
  {
    key: "assignmentTitle",
    header: "Assignment",
    render: (row) => row.assignmentTitle,
  },
  {
    key: "submittedAt",
    header: "Submitted",
    render: (row) => formatDateTime(row.submittedAt),
  },
  {
    key: "status",
    header: "Status",
    render: (row) => <StatusBadge status={row.status} />,
  },
  {
    key: "marks",
    header: "Marks",
    render: (row) => (row.marksAwarded === null ? "—" : `${row.marksAwarded} / ${row.maxMarks}`),
  },
  {
    key: "feedback",
    header: "Feedback",
    render: (row) => (row.feedback ? truncate(row.feedback, FEEDBACK_TRUNCATE_LENGTH) : "—"),
  },
];

export function SubmissionHistoryTable({ submissions, isLoading }: SubmissionHistoryTableProps) {
  return (
    <DataTable
      columns={columns}
      data={submissions}
      keyExtractor={(row) => row.id}
      isLoading={isLoading}
      emptyMessage="No submissions yet."
    />
  );
}

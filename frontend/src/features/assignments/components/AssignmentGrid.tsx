"use client";

import { Button } from "@/shared/components/ui/Button";
import { StatusBadge } from "@/shared/components/ui/Badge";
import { DataTable, type DataTableColumn } from "@/shared/components/ui/DataTable";
import { formatDateTime } from "@/shared/lib/date";
import type { AssignmentDto } from "../types";

export interface AssignmentGridProps {
  assignments: AssignmentDto[];
  isLoading?: boolean;
  /** Any of these left undefined simply drops that action from the row (and the whole Actions
   *  column disappears if none are given) — this lets the same grid serve the teacher's own
   *  list (all four) and a read-only admin oversight list (none). */
  onEdit?: (assignment: AssignmentDto) => void;
  onDelete?: (assignment: AssignmentDto) => void;
  onPublish?: (assignment: AssignmentDto) => void;
  onViewSubmissions?: (assignment: AssignmentDto) => void;
}

export function AssignmentGrid({
  assignments,
  isLoading,
  onEdit,
  onDelete,
  onPublish,
  onViewSubmissions,
}: AssignmentGridProps) {
  const hasActions = Boolean(onEdit || onDelete || onPublish || onViewSubmissions);

  const allColumns: DataTableColumn<AssignmentDto>[] = [
    { key: "title", header: "Title", render: (row) => row.title },
    {
      key: "classSubject",
      header: "Class / Subject",
      render: (row) => `${row.className} · ${row.subjectName}`,
    },
    { key: "deadline", header: "Deadline", render: (row) => formatDateTime(row.deadlineUtc) },
    { key: "maxMarks", header: "Max marks", render: (row) => row.maxMarks },
    { key: "status", header: "Status", render: (row) => <StatusBadge status={row.status} /> },
    {
      key: "actions",
      header: "Actions",
      render: (row) => (
        <div className="flex flex-wrap gap-2">
          {onPublish && row.status === "Draft" && (
            <Button variant="primary" size="sm" onClick={() => onPublish(row)}>
              Publish
            </Button>
          )}
          {onViewSubmissions && (
            <Button variant="secondary" size="sm" onClick={() => onViewSubmissions(row)}>
              Submissions
            </Button>
          )}
          {onEdit && (
            <Button variant="secondary" size="sm" onClick={() => onEdit(row)}>
              Edit
            </Button>
          )}
          {onDelete && (
            <Button variant="danger" size="sm" onClick={() => onDelete(row)}>
              Delete
            </Button>
          )}
        </div>
      ),
    },
  ];

  const columns = hasActions ? allColumns : allColumns.filter((column) => column.key !== "actions");

  return (
    <DataTable
      columns={columns}
      data={assignments}
      keyExtractor={(row) => row.id}
      isLoading={isLoading}
      emptyMessage="No assignments yet."
    />
  );
}

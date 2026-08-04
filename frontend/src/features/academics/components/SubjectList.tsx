"use client";

import { Button } from "@/shared/components/ui/Button";
import { DataTable, type DataTableColumn } from "@/shared/components/ui/DataTable";
import type { SubjectDto } from "../types";

export interface SubjectListProps {
  subjects: SubjectDto[];
  isLoading?: boolean;
  onEdit: (subject: SubjectDto) => void;
  onDelete: (subject: SubjectDto) => void;
}

export function SubjectList({ subjects, isLoading, onEdit, onDelete }: SubjectListProps) {
  const columns: DataTableColumn<SubjectDto>[] = [
    { key: "name", header: "Name", render: (row) => row.name },
    { key: "code", header: "Code", render: (row) => row.code },
    {
      key: "actions",
      header: "Actions",
      render: (row) => (
        <div className="flex gap-2">
          <Button variant="secondary" size="sm" onClick={() => onEdit(row)}>
            Edit
          </Button>
          <Button variant="danger" size="sm" onClick={() => onDelete(row)}>
            Delete
          </Button>
        </div>
      ),
    },
  ];

  return (
    <DataTable
      columns={columns}
      data={subjects}
      keyExtractor={(row) => row.id}
      isLoading={isLoading}
      emptyMessage="No subjects yet."
    />
  );
}

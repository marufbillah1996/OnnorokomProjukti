"use client";

import { Button } from "@/shared/components/ui/Button";
import { DataTable, type DataTableColumn } from "@/shared/components/ui/DataTable";
import type { ClassDto } from "../types";

export interface ClassListProps {
  classes: ClassDto[];
  isLoading?: boolean;
  onEdit: (klass: ClassDto) => void;
  onDelete: (klass: ClassDto) => void;
}

export function ClassList({ classes, isLoading, onEdit, onDelete }: ClassListProps) {
  const columns: DataTableColumn<ClassDto>[] = [
    { key: "name", header: "Name", render: (row) => row.name },
    { key: "description", header: "Description", render: (row) => row.description ?? "—" },
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
      data={classes}
      keyExtractor={(row) => row.id}
      isLoading={isLoading}
      emptyMessage="No classes yet."
    />
  );
}

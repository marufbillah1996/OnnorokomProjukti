"use client";

import { Badge } from "@/shared/components/ui/Badge";
import { Button } from "@/shared/components/ui/Button";
import { DataTable, type DataTableColumn } from "@/shared/components/ui/DataTable";
import { formatDate } from "@/shared/lib/date";
import type { UserDto } from "../types";

export interface UserTableProps {
  users: UserDto[];
  isLoading: boolean;
  onEdit: (user: UserDto) => void;
  onDelete: (id: string) => void;
}

export function UserTable({ users, isLoading, onEdit, onDelete }: UserTableProps) {
  const columns: DataTableColumn<UserDto>[] = [
    {
      key: "name",
      header: "Name",
      render: (user) => user.name,
    },
    {
      key: "email",
      header: "Email",
      render: (user) => user.email,
    },
    {
      key: "role",
      header: "Role",
      render: (user) => <Badge tone="brand">{user.role}</Badge>,
    },
    {
      key: "isActive",
      header: "Active",
      render: (user) => (
        <Badge tone={user.isActive ? "success" : "neutral"}>
          {user.isActive ? "Active" : "Inactive"}
        </Badge>
      ),
    },
    {
      key: "createdAt",
      header: "Created",
      render: (user) => formatDate(user.createdAt),
    },
    {
      key: "actions",
      header: "Actions",
      render: (user) => (
        <div className="flex gap-2">
          <Button variant="secondary" size="sm" onClick={() => onEdit(user)}>
            Edit
          </Button>
          <Button variant="danger" size="sm" onClick={() => onDelete(user.id)}>
            Deactivate
          </Button>
        </div>
      ),
    },
  ];

  return (
    <DataTable
      columns={columns}
      data={users}
      keyExtractor={(user) => user.id}
      isLoading={isLoading}
      emptyMessage="No users found."
    />
  );
}

"use client";

import { useState } from "react";
import { Button } from "@/shared/components/ui/Button";
import { Card, CardBody, CardHeader, CardTitle } from "@/shared/components/ui/Card";
import { Input } from "@/shared/components/ui/Input";
import { Pagination } from "@/shared/components/ui/Pagination";
import { usePagination } from "@/shared/hooks/usePagination";
import type { ApiError } from "@/shared/types/api-error";
import { UserTable } from "@/features/users/components/UserTable";
import { UserFormModal } from "@/features/users/components/UserFormModal";
import { useDeleteUser, useUsers } from "@/features/users/hooks";
import type { UserDto } from "@/features/users/types";

export default function AdminUsersPage() {
  const pagination = usePagination();
  const usersQuery = useUsers(pagination.query);
  const deleteUser = useDeleteUser();

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingUser, setEditingUser] = useState<UserDto | undefined>(undefined);

  function openCreateModal() {
    setEditingUser(undefined);
    setIsModalOpen(true);
  }

  function openEditModal(user: UserDto) {
    setEditingUser(user);
    setIsModalOpen(true);
  }

  function closeModal() {
    setIsModalOpen(false);
    setEditingUser(undefined);
  }

  function handleDelete(id: string) {
    if (window.confirm("Are you sure you want to deactivate this user?")) {
      deleteUser.mutate(id);
    }
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold text-foreground">Users</h2>
        <Button onClick={openCreateModal}>New User</Button>
      </div>

      <Card>
        <CardHeader className="flex items-center justify-between gap-4">
          <CardTitle>All users</CardTitle>
          <Input
            placeholder="Search users…"
            value={pagination.search}
            onChange={(event) => pagination.updateSearch(event.target.value)}
            className="max-w-xs"
          />
        </CardHeader>
        <CardBody className="p-0">
          <UserTable
            users={usersQuery.data?.items ?? []}
            isLoading={usersQuery.isLoading}
            onEdit={openEditModal}
            onDelete={handleDelete}
          />
          {deleteUser.isError && (
            <p className="px-5 py-3 text-sm text-danger-500">{(deleteUser.error as ApiError).message}</p>
          )}
          {usersQuery.data && (
            <Pagination
              page={pagination.page}
              totalPages={usersQuery.data.totalPages}
              hasPreviousPage={usersQuery.data.hasPreviousPage}
              hasNextPage={usersQuery.data.hasNextPage}
              onPageChange={pagination.setPage}
            />
          )}
        </CardBody>
      </Card>

      <UserFormModal isOpen={isModalOpen} onClose={closeModal} user={editingUser} />
    </div>
  );
}

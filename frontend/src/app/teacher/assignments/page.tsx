"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import {
  useDeleteAssignment,
  useMyAssignments,
  usePublishAssignment,
} from "@/features/assignments/hooks";
import { AssignmentFormWizard } from "@/features/assignments/components/AssignmentFormWizard";
import { AssignmentGrid } from "@/features/assignments/components/AssignmentGrid";
import type { AssignmentDto } from "@/features/assignments/types";
import { usePagination } from "@/shared/hooks/usePagination";
import { Button } from "@/shared/components/ui/Button";
import { Card, CardBody, CardHeader, CardTitle } from "@/shared/components/ui/Card";
import { Input } from "@/shared/components/ui/Input";
import { Modal } from "@/shared/components/ui/Modal";
import { Pagination } from "@/shared/components/ui/Pagination";
import type { ApiError } from "@/shared/types/api-error";

export default function TeacherAssignmentsPage() {
  const router = useRouter();
  const { setPage, search, updateSearch, query } = usePagination();

  const { data, isLoading } = useMyAssignments(query);
  const deleteAssignment = useDeleteAssignment();
  const publishAssignment = usePublishAssignment();

  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingAssignment, setEditingAssignment] = useState<AssignmentDto | undefined>(undefined);

  function openCreateForm() {
    setEditingAssignment(undefined);
    setIsFormOpen(true);
  }

  function openEditForm(assignment: AssignmentDto) {
    setEditingAssignment(assignment);
    setIsFormOpen(true);
  }

  function closeForm() {
    setIsFormOpen(false);
    setEditingAssignment(undefined);
  }

  function handleDelete(assignment: AssignmentDto) {
    if (!window.confirm(`Delete "${assignment.title}"? This cannot be undone.`)) return;
    deleteAssignment.mutate(assignment.id);
  }

  function handlePublish(assignment: AssignmentDto) {
    publishAssignment.mutate(assignment.id);
  }

  function handleViewSubmissions(assignment: AssignmentDto) {
    router.push(`/teacher/assignments/${assignment.id}/submissions`);
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <h2 className="text-xl font-semibold text-foreground">Assignments</h2>
        <Button onClick={openCreateForm}>New assignment</Button>
      </div>

      <Card>
        <CardHeader className="flex flex-wrap items-center justify-between gap-4">
          <CardTitle>My assignments</CardTitle>
          <Input
            placeholder="Search assignments…"
            value={search}
            onChange={(event) => updateSearch(event.target.value)}
            className="max-w-xs"
          />
        </CardHeader>
        <CardBody className="p-0">
          <AssignmentGrid
            assignments={data?.items ?? []}
            isLoading={isLoading}
            onEdit={openEditForm}
            onDelete={handleDelete}
            onPublish={handlePublish}
            onViewSubmissions={handleViewSubmissions}
          />
          {(deleteAssignment.isError || publishAssignment.isError) && (
            <p className="px-5 py-3 text-sm text-danger-500">
              {((deleteAssignment.error ?? publishAssignment.error) as ApiError).message}
            </p>
          )}
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
        isOpen={isFormOpen}
        onClose={closeForm}
        title={editingAssignment ? "Edit assignment" : "New assignment"}
      >
        <AssignmentFormWizard assignment={editingAssignment} onSuccess={closeForm} />
      </Modal>
    </div>
  );
}

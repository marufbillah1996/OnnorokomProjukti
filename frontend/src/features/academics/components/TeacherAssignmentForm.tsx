"use client";

import { type FormEvent, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { getUsers } from "@/features/users/api";
import type { UserDto } from "@/features/users/types";
import { Button } from "@/shared/components/ui/Button";
import { DataTable, type DataTableColumn } from "@/shared/components/ui/DataTable";
import { Select } from "@/shared/components/ui/Input";
import type { ApiError } from "@/shared/types/api-error";
import { useAssignTeacher, useClassSubjects, useSubjects } from "../hooks";
import type { ClassSubjectDto } from "../types";

export interface TeacherAssignmentFormProps {
  classId: string;
}

export function TeacherAssignmentForm({ classId }: TeacherAssignmentFormProps) {
  const [subjectId, setSubjectId] = useState("");
  const [teacherId, setTeacherId] = useState("");

  const { data: classSubjects, isLoading: isLoadingClassSubjects } = useClassSubjects(classId);
  const { data: subjectsPage } = useSubjects({ page: 1, pageSize: 100 });
  const { data: teachersPage, isLoading: isLoadingTeachers } = useQuery({
    queryKey: ["users", "list", { role: "Teacher", pageSize: 100 }],
    queryFn: () => getUsers({ role: "Teacher", pageSize: 100 }),
  });

  const assignTeacherMutation = useAssignTeacher();

  const columns: DataTableColumn<ClassSubjectDto>[] = [
    { key: "subject", header: "Subject", render: (row) => row.subjectName },
    { key: "teacher", header: "Teacher", render: (row) => row.teacherName },
  ];

  const subjectOptions = (subjectsPage?.items ?? []).map((subject) => ({
    value: subject.id,
    label: subject.name,
  }));
  const teacherOptions = (teachersPage?.items ?? []).map((teacher: UserDto) => ({
    value: teacher.id,
    label: teacher.name,
  }));

  function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (!subjectId || !teacherId) return;

    assignTeacherMutation.mutate(
      { classId, subjectId, teacherId },
      {
        onSuccess: () => {
          setSubjectId("");
          setTeacherId("");
        },
      },
    );
  }

  return (
    <div className="flex flex-col gap-4">
      <DataTable
        columns={columns}
        data={classSubjects ?? []}
        keyExtractor={(row) => row.id}
        isLoading={isLoadingClassSubjects}
        emptyMessage="No subjects assigned to this class yet."
      />

      <form onSubmit={handleSubmit} className="flex flex-col gap-3 sm:flex-row sm:items-end">
        <Select
          label="Subject"
          name="subjectId"
          value={subjectId}
          onChange={(event) => setSubjectId(event.target.value)}
          options={subjectOptions}
          placeholder="Select a subject"
        />
        <Select
          label="Teacher"
          name="teacherId"
          value={teacherId}
          onChange={(event) => setTeacherId(event.target.value)}
          options={teacherOptions}
          placeholder={isLoadingTeachers ? "Loading teachers…" : "Select a teacher"}
        />
        <Button type="submit" isLoading={assignTeacherMutation.isPending} disabled={!subjectId || !teacherId}>
          Assign
        </Button>
      </form>

      {assignTeacherMutation.isError && (
        <p className="text-sm text-danger-500">{(assignTeacherMutation.error as ApiError).message}</p>
      )}
    </div>
  );
}

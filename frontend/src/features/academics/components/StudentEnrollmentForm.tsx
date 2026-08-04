"use client";

import { type FormEvent, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { getUsers } from "@/features/users/api";
import type { UserDto } from "@/features/users/types";
import { Button } from "@/shared/components/ui/Button";
import { Select } from "@/shared/components/ui/Input";
import type { ApiError } from "@/shared/types/api-error";
import { useEnrollStudent } from "../hooks";

export interface StudentEnrollmentFormProps {
  classId: string;
}

export function StudentEnrollmentForm({ classId }: StudentEnrollmentFormProps) {
  const [studentId, setStudentId] = useState("");

  const { data: studentsPage, isLoading: isLoadingStudents } = useQuery({
    queryKey: ["users", "list", { role: "Student", pageSize: 100 }],
    queryFn: () => getUsers({ role: "Student", pageSize: 100 }),
  });

  const enrollStudentMutation = useEnrollStudent();

  const studentOptions = (studentsPage?.items ?? []).map((student: UserDto) => ({
    value: student.id,
    label: student.name,
  }));

  function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (!studentId) return;

    enrollStudentMutation.mutate(
      { classId, studentId },
      {
        onSuccess: () => setStudentId(""),
      },
    );
  }

  return (
    <div className="flex flex-col gap-3">
      <form onSubmit={handleSubmit} className="flex flex-col gap-3 sm:flex-row sm:items-end">
        <Select
          label="Student"
          name="studentId"
          value={studentId}
          onChange={(event) => setStudentId(event.target.value)}
          options={studentOptions}
          placeholder={isLoadingStudents ? "Loading students…" : "Select a student"}
        />
        <Button type="submit" isLoading={enrollStudentMutation.isPending} disabled={!studentId}>
          Enroll
        </Button>
      </form>

      {enrollStudentMutation.isSuccess && (
        <p className="text-sm text-success-500">Student enrolled successfully.</p>
      )}
      {enrollStudentMutation.isError && (
        <p className="text-sm text-danger-500">{(enrollStudentMutation.error as ApiError).message}</p>
      )}
    </div>
  );
}

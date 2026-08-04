"use client";

import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm, type Resolver } from "react-hook-form";
import { useClasses, useClassSubjects } from "@/features/academics/hooks";
import { Button } from "@/shared/components/ui/Button";
import { Card, CardBody, CardHeader, CardTitle } from "@/shared/components/ui/Card";
import { Input, Select, Textarea } from "@/shared/components/ui/Input";
import type { ApiError } from "@/shared/types/api-error";
import { useCreateAssignment, useUpdateAssignment } from "../hooks";
import {
  createAssignmentSchema,
  updateAssignmentSchema,
  type UpdateAssignmentFormValues,
} from "../schema";
import type { AssignmentDto } from "../types";

export interface AssignmentFormWizardProps {
  assignment?: AssignmentDto;
  onSuccess: () => void;
}

/**
 * Local form shape covering both modes. classSubjectId only applies (and is only rendered /
 * validated) on create — UpdateAssignmentRequest has no field for it, an assignment's class and
 * subject are fixed at creation time.
 */
type AssignmentFormValues = UpdateAssignmentFormValues & { classSubjectId?: string };

/**
 * <input type="datetime-local"> produces/consumes local time with no timezone info. We keep the
 * form field as that plain local string end-to-end and only convert to a UTC ISO string right
 * here, at the API boundary, via `new Date(value).toISOString()` — this is the one and only place
 * that conversion happens for assignments.
 */
function toUtcIso(localDateTime: string): string {
  return new Date(localDateTime).toISOString();
}

/** Inverse of toUtcIso, used to pre-fill the datetime-local input when editing. */
function toDatetimeLocalValue(utcIso: string): string {
  const date = new Date(utcIso);
  const pad = (value: number) => String(value).padStart(2, "0");
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

export function AssignmentFormWizard({ assignment, onSuccess }: AssignmentFormWizardProps) {
  const isEdit = Boolean(assignment);

  const createAssignmentMutation = useCreateAssignment();
  const updateAssignmentMutation = useUpdateAssignment();
  const activeMutation = isEdit ? updateAssignmentMutation : createAssignmentMutation;

  // Two-step Class -> Subject picker (create only). There is no single "class-subjects I teach"
  // endpoint, so we list classes, then list that class's subject rows (each already carrying the
  // teacher assigned to it) once one is picked — simplest UX for a secondary concern.
  const [selectedClassId, setSelectedClassId] = useState("");
  const { data: classesPage, isLoading: isLoadingClasses } = useClasses({ page: 1, pageSize: 100 });
  const { data: classSubjects, isLoading: isLoadingClassSubjects } = useClassSubjects(selectedClassId);

  const classOptions = (classesPage?.items ?? []).map((klass) => ({
    value: klass.id,
    label: klass.name,
  }));
  const classSubjectOptions = (classSubjects ?? []).map((classSubject) => ({
    value: classSubject.id,
    label: `${classSubject.subjectName} — ${classSubject.teacherName}`,
  }));

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm<AssignmentFormValues>({
    // Cast needed because the two branches resolve to different (subset) shapes — the resolver
    // actually used always matches the fields rendered below for the current mode.
    resolver: (isEdit
      ? zodResolver(updateAssignmentSchema)
      : zodResolver(createAssignmentSchema)) as unknown as Resolver<AssignmentFormValues>,
    defaultValues: assignment
      ? {
          title: assignment.title,
          description: assignment.description,
          deadlineUtc: toDatetimeLocalValue(assignment.deadlineUtc),
          maxMarks: assignment.maxMarks,
        }
      : {
          title: "",
          description: "",
          deadlineUtc: "",
          classSubjectId: "",
        },
  });

  function onSubmit(values: AssignmentFormValues) {
    const deadlineUtc = toUtcIso(values.deadlineUtc);

    if (isEdit && assignment) {
      updateAssignmentMutation.mutate(
        {
          id: assignment.id,
          req: {
            title: values.title,
            description: values.description,
            deadlineUtc,
            maxMarks: values.maxMarks,
          },
        },
        { onSuccess },
      );
      return;
    }

    if (!values.classSubjectId) return;

    createAssignmentMutation.mutate(
      {
        title: values.title,
        description: values.description,
        deadlineUtc,
        maxMarks: values.maxMarks,
        classSubjectId: values.classSubjectId,
      },
      { onSuccess },
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>{isEdit ? "Edit assignment" : "New assignment"}</CardTitle>
      </CardHeader>
      <CardBody>
        <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
          <Input label="Title" error={errors.title?.message} {...register("title")} />

          <Textarea
            label="Description"
            rows={5}
            error={errors.description?.message}
            {...register("description")}
          />

          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Input
              label="Deadline"
              type="datetime-local"
              error={errors.deadlineUtc?.message}
              {...register("deadlineUtc")}
            />
            <Input
              label="Max marks"
              type="number"
              min={1}
              error={errors.maxMarks?.message}
              {...register("maxMarks")}
            />
          </div>

          {!isEdit && (
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
              <Select
                label="Class"
                options={classOptions}
                placeholder={isLoadingClasses ? "Loading classes…" : "Select a class"}
                value={selectedClassId}
                onChange={(event) => {
                  setSelectedClassId(event.target.value);
                  setValue("classSubjectId", "");
                }}
              />
              <Select
                label="Subject"
                options={classSubjectOptions}
                placeholder={
                  !selectedClassId
                    ? "Select a class first"
                    : isLoadingClassSubjects
                      ? "Loading subjects…"
                      : "Select a subject"
                }
                disabled={!selectedClassId}
                error={errors.classSubjectId?.message}
                {...register("classSubjectId")}
              />
            </div>
          )}

          {activeMutation.isError && (
            <p className="text-sm text-danger-500">{(activeMutation.error as ApiError).message}</p>
          )}

          <div>
            <Button type="submit" isLoading={activeMutation.isPending}>
              {isEdit ? "Save changes" : "Create assignment"}
            </Button>
          </div>
        </form>
      </CardBody>
    </Card>
  );
}

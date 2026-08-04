"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { Button } from "@/shared/components/ui/Button";
import { StatusBadge } from "@/shared/components/ui/Badge";
import { Textarea } from "@/shared/components/ui/Input";
import { isPastDeadline } from "@/shared/lib/date";
import type { ApiError } from "@/shared/types/api-error";
import { useSubmitAssignment, useUpdateMySubmission } from "../hooks";
import { submitAnswerSchema, type SubmitAnswerFormValues } from "../schema";
import type { SubmissionDto } from "../types";

export interface SubmissionFormProps {
  assignmentId: string;
  deadlineUtc: string;
  existingSubmission?: SubmissionDto;
}

export function SubmissionForm({ assignmentId, deadlineUtc, existingSubmission }: SubmissionFormProps) {
  const submitAssignment = useSubmitAssignment();
  const updateMySubmission = useUpdateMySubmission();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<SubmitAnswerFormValues>({
    resolver: zodResolver(submitAnswerSchema),
    defaultValues: { content: existingSubmission?.content ?? "" },
  });

  // Already graded: show a read-only result summary instead of an editable form.
  if (existingSubmission && existingSubmission.status === "Graded") {
    return (
      <div className="flex flex-col gap-3 rounded-md border border-border bg-surface-muted p-4">
        <div className="flex items-center justify-between">
          <StatusBadge status={existingSubmission.status} />
          <span className="text-sm font-medium text-foreground">
            {existingSubmission.marksAwarded ?? "—"} / {existingSubmission.maxMarks}
          </span>
        </div>
        <div>
          <p className="text-sm font-medium text-foreground/70">Your answer</p>
          <p className="whitespace-pre-wrap text-sm text-foreground">{existingSubmission.content}</p>
        </div>
        {existingSubmission.feedback && (
          <div>
            <p className="text-sm font-medium text-foreground/70">Feedback</p>
            <p className="whitespace-pre-wrap text-sm text-foreground">{existingSubmission.feedback}</p>
          </div>
        )}
      </div>
    );
  }

  const canStillEdit =
    Boolean(existingSubmission) &&
    (existingSubmission?.status === "Pending" || existingSubmission?.status === "Returned");
  const deadlinePassed = isPastDeadline(deadlineUtc);
  const isLocked = deadlinePassed && !canStillEdit;

  const mutation = existingSubmission ? updateMySubmission : submitAssignment;

  function onSubmit(values: SubmitAnswerFormValues) {
    if (existingSubmission) {
      updateMySubmission.mutate({ id: existingSubmission.id, request: values });
    } else {
      submitAssignment.mutate({ assignmentId, request: values });
    }
  }

  if (isLocked) {
    return (
      <div className="rounded-md border border-border bg-surface-muted p-4">
        <p className="text-sm text-foreground/70">The deadline for this assignment has passed.</p>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
      {existingSubmission?.status === "Returned" && (
        <p className="text-sm text-warning-500">
          This submission was returned for correction. Update your answer and resubmit.
        </p>
      )}
      <Textarea
        label="Your answer"
        rows={10}
        error={errors.content?.message}
        {...register("content")}
      />
      {mutation.isError && (
        <p className="text-sm text-danger-500">{(mutation.error as ApiError).message}</p>
      )}
      <div>
        <Button type="submit" isLoading={mutation.isPending}>
          {existingSubmission ? "Update submission" : "Submit answer"}
        </Button>
      </div>
    </form>
  );
}

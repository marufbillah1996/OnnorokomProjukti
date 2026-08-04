"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { Button } from "@/shared/components/ui/Button";
import { StatusBadge } from "@/shared/components/ui/Badge";
import { Input, Textarea } from "@/shared/components/ui/Input";
import { formatDateTime } from "@/shared/lib/date";
import type { ApiError } from "@/shared/types/api-error";
import { useChangeSubmissionStatus, useGradeSubmission } from "../hooks";
import { gradeSubmissionSchema, type GradeSubmissionFormValues } from "../schema";
import type { SubmissionDto } from "../types";

export interface GradingPanelProps {
  submission: SubmissionDto;
}

export function GradingPanel({ submission }: GradingPanelProps) {
  const gradeSubmission = useGradeSubmission();
  const changeSubmissionStatus = useChangeSubmissionStatus();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<GradeSubmissionFormValues>({
    resolver: zodResolver(gradeSubmissionSchema),
    defaultValues: {
      marksAwarded: submission.marksAwarded ?? 0,
      feedback: submission.feedback ?? "",
    },
  });

  function onSubmit(values: GradeSubmissionFormValues) {
    gradeSubmission.mutate({
      id: submission.id,
      request: { marksAwarded: values.marksAwarded, feedback: values.feedback || null },
    });
  }

  function handleReturnForCorrection() {
    changeSubmissionStatus.mutate({ id: submission.id, request: { status: "Returned" } });
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-col gap-2 rounded-md border border-border bg-surface-muted p-4">
        <div className="flex items-center justify-between">
          <div>
            <p className="text-sm font-medium text-foreground">{submission.studentName}</p>
            <p className="text-xs text-foreground/60">
              Submitted {formatDateTime(submission.submittedAt)}
            </p>
          </div>
          <StatusBadge status={submission.status} />
        </div>
        <p className="whitespace-pre-wrap text-sm text-foreground">{submission.content}</p>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
        <Input
          label={`Marks (out of ${submission.maxMarks})`}
          type="number"
          min={0}
          max={submission.maxMarks}
          error={errors.marksAwarded?.message}
          {...register("marksAwarded", { valueAsNumber: true })}
        />
        <Textarea
          label="Feedback (optional)"
          rows={4}
          error={errors.feedback?.message}
          {...register("feedback")}
        />
        {gradeSubmission.isError && (
          <p className="text-sm text-danger-500">{(gradeSubmission.error as ApiError).message}</p>
        )}
        <div className="flex items-center gap-3">
          <Button type="submit" isLoading={gradeSubmission.isPending}>
            Save grade
          </Button>
          <Button
            type="button"
            variant="secondary"
            isLoading={changeSubmissionStatus.isPending}
            onClick={handleReturnForCorrection}
          >
            Return for correction
          </Button>
        </div>
        {changeSubmissionStatus.isError && (
          <p className="text-sm text-danger-500">
            {(changeSubmissionStatus.error as ApiError).message}
          </p>
        )}
      </form>
    </div>
  );
}

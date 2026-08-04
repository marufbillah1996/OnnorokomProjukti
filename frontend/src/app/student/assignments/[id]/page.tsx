"use client";

import { use } from "react";
import { Card, CardBody, CardHeader, CardTitle } from "@/shared/components/ui/Card";
import { SkeletonRows } from "@/shared/components/ui/Skeleton";
import { useStudentAssignment } from "@/features/assignments/hooks";
import { AssignmentDetailCard } from "@/features/assignments/components/AssignmentDetailCard";
import { useMySubmissions } from "@/features/submissions/hooks";
import { SubmissionForm } from "@/features/submissions/components/SubmissionForm";

interface StudentAssignmentDetailPageProps {
  params: Promise<{ id: string }>;
}

export default function StudentAssignmentDetailPage({ params }: StudentAssignmentDetailPageProps) {
  const { id } = use(params);

  const assignmentQuery = useStudentAssignment(id);
  // No single "get my submission for assignment X" endpoint exists — fetch this student's
  // submissions and find the one for this assignment client-side (fine at this scale).
  const submissionsQuery = useMySubmissions({ pageSize: 100 });

  if (assignmentQuery.isLoading) {
    return <SkeletonRows rows={6} />;
  }

  if (assignmentQuery.isError || !assignmentQuery.data) {
    return <p className="text-sm text-danger-500">This assignment could not be found.</p>;
  }

  const assignment = assignmentQuery.data;
  const existingSubmission = submissionsQuery.data?.items.find(
    (submission) => submission.assignmentId === id,
  );

  return (
    <div className="flex flex-col gap-6">
      <AssignmentDetailCard assignment={assignment} variant="student" />

      <Card>
        <CardHeader>
          <CardTitle>Your submission</CardTitle>
        </CardHeader>
        <CardBody>
          {submissionsQuery.isLoading ? (
            <SkeletonRows rows={3} />
          ) : (
            <SubmissionForm
              assignmentId={id}
              deadlineUtc={assignment.deadlineUtc}
              existingSubmission={existingSubmission}
            />
          )}
        </CardBody>
      </Card>
    </div>
  );
}

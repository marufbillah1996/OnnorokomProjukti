"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type { PaginationQuery } from "@/shared/types/paginated";
import {
  changeSubmissionStatus,
  getAdminSubmissions,
  getMySubmissions,
  getSubmissionsForAssignment,
  gradeSubmission,
  submitAssignment,
  updateMySubmission,
} from "./api";
import type {
  ChangeSubmissionStatusRequest,
  GradeSubmissionRequest,
  SubmitAnswerRequest,
} from "./types";

export function useSubmissionsForAssignment(assignmentId: string, query: PaginationQuery) {
  return useQuery({
    queryKey: ["submissions", "for-assignment", assignmentId, query],
    queryFn: () => getSubmissionsForAssignment(assignmentId, query),
    enabled: Boolean(assignmentId),
  });
}

export function useMySubmissions(query: PaginationQuery) {
  return useQuery({
    queryKey: ["submissions", "mine", query],
    queryFn: () => getMySubmissions(query),
  });
}

export function useAdminSubmissions(query: PaginationQuery) {
  return useQuery({
    queryKey: ["submissions", "admin", query],
    queryFn: () => getAdminSubmissions(query),
  });
}

export function useSubmitAssignment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ assignmentId, request }: { assignmentId: string; request: SubmitAnswerRequest }) =>
      submitAssignment(assignmentId, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["submissions"] });
    },
  });
}

export function useUpdateMySubmission() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, request }: { id: string; request: SubmitAnswerRequest }) =>
      updateMySubmission(id, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["submissions"] });
    },
  });
}

export function useGradeSubmission() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, request }: { id: string; request: GradeSubmissionRequest }) =>
      gradeSubmission(id, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["submissions"] });
    },
  });
}

export function useChangeSubmissionStatus() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, request }: { id: string; request: ChangeSubmissionStatusRequest }) =>
      changeSubmissionStatus(id, request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["submissions"] });
    },
  });
}

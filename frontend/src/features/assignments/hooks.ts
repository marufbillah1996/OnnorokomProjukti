"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type { PaginationQuery } from "@/shared/types/paginated";
import {
  createAssignment,
  deleteAssignment,
  getAdminAssignments,
  getAssignment,
  getMyAssignments,
  getPublishedAssignments,
  getStudentAssignment,
  publishAssignment,
  updateAssignment,
} from "./api";
import type { CreateAssignmentRequest, UpdateAssignmentRequest } from "./types";

export function useMyAssignments(query: PaginationQuery) {
  return useQuery({
    queryKey: ["assignments", "mine", query],
    queryFn: () => getMyAssignments(query),
  });
}

export function useAssignment(id: string) {
  return useQuery({
    queryKey: ["assignments", "detail", id],
    queryFn: () => getAssignment(id),
    enabled: Boolean(id),
  });
}

export function useCreateAssignment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (req: CreateAssignmentRequest) => createAssignment(req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["assignments"] });
    },
  });
}

export function useUpdateAssignment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, req }: { id: string; req: UpdateAssignmentRequest }) => updateAssignment(id, req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["assignments"] });
    },
  });
}

export function useDeleteAssignment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteAssignment(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["assignments"] });
    },
  });
}

export function usePublishAssignment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => publishAssignment(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["assignments"] });
    },
  });
}

export function useAdminAssignments(query: PaginationQuery) {
  return useQuery({
    queryKey: ["assignments", "admin", query],
    queryFn: () => getAdminAssignments(query),
  });
}

export function usePublishedAssignments(query: PaginationQuery) {
  return useQuery({
    queryKey: ["assignments", "published", query],
    queryFn: () => getPublishedAssignments(query),
  });
}

export function useStudentAssignment(id: string) {
  return useQuery({
    queryKey: ["assignments", "student-detail", id],
    queryFn: () => getStudentAssignment(id),
    enabled: Boolean(id),
  });
}

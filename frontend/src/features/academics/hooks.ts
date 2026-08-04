"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type { PaginationQuery } from "@/shared/types/paginated";
import {
  assignTeacher,
  createClass,
  createSubject,
  deleteClass,
  deleteSubject,
  enrollStudent,
  getClass,
  getClasses,
  getClassSubjects,
  getSubject,
  getSubjects,
  updateClass,
  updateSubject,
} from "./api";
import type { CreateClassRequest, CreateSubjectRequest, UpdateClassRequest, UpdateSubjectRequest } from "./types";

export function useClasses(query: PaginationQuery) {
  return useQuery({
    queryKey: ["classes", "list", query],
    queryFn: () => getClasses(query),
  });
}

export function useClass(id: string) {
  return useQuery({
    queryKey: ["classes", "detail", id],
    queryFn: () => getClass(id),
    enabled: Boolean(id),
  });
}

export function useCreateClass() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (req: CreateClassRequest) => createClass(req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["classes"] });
    },
  });
}

export function useUpdateClass() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, req }: { id: string; req: UpdateClassRequest }) => updateClass(id, req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["classes"] });
    },
  });
}

export function useDeleteClass() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteClass(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["classes"] });
    },
  });
}

export function useSubjects(query: PaginationQuery) {
  return useQuery({
    queryKey: ["subjects", "list", query],
    queryFn: () => getSubjects(query),
  });
}

export function useSubject(id: string) {
  return useQuery({
    queryKey: ["subjects", "detail", id],
    queryFn: () => getSubject(id),
    enabled: Boolean(id),
  });
}

export function useCreateSubject() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (req: CreateSubjectRequest) => createSubject(req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["subjects"] });
    },
  });
}

export function useUpdateSubject() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, req }: { id: string; req: UpdateSubjectRequest }) => updateSubject(id, req),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["subjects"] });
    },
  });
}

export function useDeleteSubject() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => deleteSubject(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["subjects"] });
    },
  });
}

export function useClassSubjects(classId: string) {
  return useQuery({
    queryKey: ["classSubjects", classId],
    queryFn: () => getClassSubjects(classId),
    enabled: Boolean(classId),
  });
}

export function useAssignTeacher() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ classId, subjectId, teacherId }: { classId: string; subjectId: string; teacherId: string }) =>
      assignTeacher(classId, subjectId, teacherId),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ["classSubjects", variables.classId] });
    },
  });
}

export function useEnrollStudent() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ classId, studentId }: { classId: string; studentId: string }) =>
      enrollStudent(classId, studentId),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ["classes", "detail", variables.classId] });
    },
  });
}

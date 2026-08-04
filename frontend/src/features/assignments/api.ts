import { apiClient } from "@/shared/lib/api-client";
import type { ClassSubjectDto } from "@/features/academics/types";
import type { Paginated, PaginationQuery } from "@/shared/types/paginated";
import type { AssignmentDto, CreateAssignmentRequest, UpdateAssignmentRequest } from "./types";

/** GET /assignments — the signed-in teacher's own assignments. */
export async function getMyAssignments(query: PaginationQuery): Promise<Paginated<AssignmentDto>> {
  const { data } = await apiClient.get<Paginated<AssignmentDto>>("/assignments", { params: query });
  return data;
}

/** GET /assignments/{id} — a teacher viewing one of their own assignments. */
export async function getAssignment(id: string): Promise<AssignmentDto> {
  const { data } = await apiClient.get<AssignmentDto>(`/assignments/${id}`);
  return data;
}

export async function createAssignment(req: CreateAssignmentRequest): Promise<AssignmentDto> {
  const { data } = await apiClient.post<AssignmentDto>("/assignments", req);
  return data;
}

export async function updateAssignment(id: string, req: UpdateAssignmentRequest): Promise<AssignmentDto> {
  const { data } = await apiClient.put<AssignmentDto>(`/assignments/${id}`, req);
  return data;
}

export async function deleteAssignment(id: string): Promise<void> {
  await apiClient.delete(`/assignments/${id}`);
}

/** PUT /assignments/{id}/publish — moves an assignment from Draft to Published. No request body. */
export async function publishAssignment(id: string): Promise<AssignmentDto> {
  const { data } = await apiClient.put<AssignmentDto>(`/assignments/${id}/publish`);
  return data;
}

/**
 * GET /assignments/class-subjects — the class/subject pairs the signed-in teacher is assigned
 * to teach. Backs the "Class" picker on the create-assignment form — a Teacher has no access to
 * the Admin-only GET /classes, so this teacher-scoped endpoint is the only valid source for it.
 */
export async function getMyClassSubjects(): Promise<ClassSubjectDto[]> {
  const { data } = await apiClient.get<ClassSubjectDto[]>("/assignments/class-subjects");
  return data;
}

/** GET /admin/assignments — admin oversight across every teacher's assignments. */
export async function getAdminAssignments(query: PaginationQuery): Promise<Paginated<AssignmentDto>> {
  const { data } = await apiClient.get<Paginated<AssignmentDto>>("/admin/assignments", { params: query });
  return data;
}

/** GET /student/assignments — published assignments for the signed-in student's own classes. */
export async function getPublishedAssignments(query: PaginationQuery): Promise<Paginated<AssignmentDto>> {
  const { data } = await apiClient.get<Paginated<AssignmentDto>>("/student/assignments", {
    params: query,
  });
  return data;
}

/** GET /student/assignments/{id} — a student viewing one published assignment's detail. */
export async function getStudentAssignment(id: string): Promise<AssignmentDto> {
  const { data } = await apiClient.get<AssignmentDto>(`/student/assignments/${id}`);
  return data;
}

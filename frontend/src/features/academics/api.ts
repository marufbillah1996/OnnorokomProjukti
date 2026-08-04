import { apiClient } from "@/shared/lib/api-client";
import type { Paginated, PaginationQuery } from "@/shared/types/paginated";
import type {
  ClassDto,
  ClassSubjectDto,
  CreateClassRequest,
  CreateSubjectRequest,
  SubjectDto,
  UpdateClassRequest,
  UpdateSubjectRequest,
} from "./types";

export async function getClasses(query: PaginationQuery): Promise<Paginated<ClassDto>> {
  const { data } = await apiClient.get<Paginated<ClassDto>>("/classes", { params: query });
  return data;
}

export async function getClass(id: string): Promise<ClassDto> {
  const { data } = await apiClient.get<ClassDto>(`/classes/${id}`);
  return data;
}

export async function createClass(req: CreateClassRequest): Promise<ClassDto> {
  const { data } = await apiClient.post<ClassDto>("/classes", req);
  return data;
}

export async function updateClass(id: string, req: UpdateClassRequest): Promise<ClassDto> {
  const { data } = await apiClient.put<ClassDto>(`/classes/${id}`, req);
  return data;
}

export async function deleteClass(id: string): Promise<void> {
  await apiClient.delete(`/classes/${id}`);
}

export async function getSubjects(query: PaginationQuery): Promise<Paginated<SubjectDto>> {
  const { data } = await apiClient.get<Paginated<SubjectDto>>("/subjects", { params: query });
  return data;
}

export async function getSubject(id: string): Promise<SubjectDto> {
  const { data } = await apiClient.get<SubjectDto>(`/subjects/${id}`);
  return data;
}

export async function createSubject(req: CreateSubjectRequest): Promise<SubjectDto> {
  const { data } = await apiClient.post<SubjectDto>("/subjects", req);
  return data;
}

export async function updateSubject(id: string, req: UpdateSubjectRequest): Promise<SubjectDto> {
  const { data } = await apiClient.put<SubjectDto>(`/subjects/${id}`, req);
  return data;
}

export async function deleteSubject(id: string): Promise<void> {
  await apiClient.delete(`/subjects/${id}`);
}

export async function getClassSubjects(classId: string): Promise<ClassSubjectDto[]> {
  const { data } = await apiClient.get<ClassSubjectDto[]>(`/classes/${classId}/subjects`);
  return data;
}

export async function assignTeacher(
  classId: string,
  subjectId: string,
  teacherId: string,
): Promise<ClassSubjectDto> {
  const { data } = await apiClient.post<ClassSubjectDto>(
    `/classes/${classId}/subjects/${subjectId}/teachers/${teacherId}`,
  );
  return data;
}

export async function enrollStudent(classId: string, studentId: string): Promise<void> {
  await apiClient.post(`/classes/${classId}/students/${studentId}`);
}

import { apiClient } from "@/shared/lib/api-client";
import type { Paginated, PaginationQuery } from "@/shared/types/paginated";
import type {
  ChangeSubmissionStatusRequest,
  GradeSubmissionRequest,
  SubmissionDto,
  SubmitAnswerRequest,
} from "./types";

/** GET /assignments/{assignmentId}/submissions — a teacher viewing all submissions for one assignment. */
export async function getSubmissionsForAssignment(
  assignmentId: string,
  query: PaginationQuery,
): Promise<Paginated<SubmissionDto>> {
  const { data } = await apiClient.get<Paginated<SubmissionDto>>(
    `/assignments/${assignmentId}/submissions`,
    { params: query },
  );
  return data;
}

/** GET /student/submissions — the signed-in student's own submissions. */
export async function getMySubmissions(query: PaginationQuery): Promise<Paginated<SubmissionDto>> {
  const { data } = await apiClient.get<Paginated<SubmissionDto>>("/student/submissions", {
    params: query,
  });
  return data;
}

/** GET /admin/submissions — admin oversight across all submissions. */
export async function getAdminSubmissions(query: PaginationQuery): Promise<Paginated<SubmissionDto>> {
  const { data } = await apiClient.get<Paginated<SubmissionDto>>("/admin/submissions", {
    params: query,
  });
  return data;
}

/** POST /student/assignments/{assignmentId}/submit — first-time submission for an assignment. */
export async function submitAssignment(
  assignmentId: string,
  request: SubmitAnswerRequest,
): Promise<SubmissionDto> {
  const { data } = await apiClient.post<SubmissionDto>(
    `/student/assignments/${assignmentId}/submit`,
    request,
  );
  return data;
}

/** PUT /student/submissions/{id} — student edits their own existing submission. */
export async function updateMySubmission(
  id: string,
  request: SubmitAnswerRequest,
): Promise<SubmissionDto> {
  const { data } = await apiClient.put<SubmissionDto>(`/student/submissions/${id}`, request);
  return data;
}

/** PUT /submissions/{id}/grade — teacher grades a submission. */
export async function gradeSubmission(
  id: string,
  request: GradeSubmissionRequest,
): Promise<SubmissionDto> {
  const { data } = await apiClient.put<SubmissionDto>(`/submissions/${id}/grade`, request);
  return data;
}

/** PUT /submissions/{id}/status — teacher changes a submission's status (e.g. return for correction). */
export async function changeSubmissionStatus(
  id: string,
  request: ChangeSubmissionStatusRequest,
): Promise<SubmissionDto> {
  const { data } = await apiClient.put<SubmissionDto>(`/submissions/${id}/status`, request);
  return data;
}

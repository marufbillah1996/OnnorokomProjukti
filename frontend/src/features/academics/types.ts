/** Mirrors the backend's ClassDto JSON shape (Admin-only Classes endpoints). */
export interface ClassDto {
  id: string;
  name: string;
  description: string | null;
}

export interface CreateClassRequest {
  name: string;
  description?: string | null;
}

export interface UpdateClassRequest {
  name: string;
  description?: string | null;
}

/** Mirrors the backend's SubjectDto JSON shape (Admin-only Subjects endpoints). */
export interface SubjectDto {
  id: string;
  name: string;
  code: string;
}

export interface CreateSubjectRequest {
  name: string;
  code: string;
}

export interface UpdateSubjectRequest {
  name: string;
  code: string;
}

/**
 * A subject taught within a class, with the teacher currently assigned to it.
 * Returned as a plain array (not paginated) from GET /classes/{classId}/subjects.
 */
export interface ClassSubjectDto {
  id: string;
  classId: string;
  className: string;
  subjectId: string;
  subjectName: string;
  teacherId: string;
  teacherName: string;
}

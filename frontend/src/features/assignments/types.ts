/** Mirrors the backend's AssignmentDto JSON shape exactly (camelCase). */
export interface AssignmentDto {
  id: string;
  title: string;
  description: string;
  deadlineUtc: string;
  maxMarks: number;
  status: "Draft" | "Published";
  classSubjectId: string;
  className: string;
  subjectName: string;
  createdByTeacherId: string;
  createdByTeacherName: string;
  createdAt: string;
}

/** Body for POST /assignments. */
export interface CreateAssignmentRequest {
  title: string;
  description: string;
  deadlineUtc: string;
  maxMarks: number;
  classSubjectId: string;
}

/** Body for PUT /assignments/{id} — classSubjectId is fixed at creation and cannot be changed. */
export interface UpdateAssignmentRequest {
  title: string;
  description: string;
  deadlineUtc: string;
  maxMarks: number;
}

/** Mirrors the backend's SubmissionDto JSON shape exactly (camelCase). */
export interface SubmissionDto {
  id: string;
  assignmentId: string;
  assignmentTitle: string;
  studentId: string;
  studentName: string;
  content: string;
  submittedAt: string;
  status: "Pending" | "Graded" | "Returned";
  marksAwarded: number | null;
  maxMarks: number;
  feedback: string | null;
}

export interface SubmitAnswerRequest {
  content: string;
}

export interface GradeSubmissionRequest {
  marksAwarded: number;
  feedback?: string | null;
}

export interface ChangeSubmissionStatusRequest {
  status: "Pending" | "Returned";
}

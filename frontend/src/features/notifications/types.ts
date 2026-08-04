/**
 * DTOs mirror the backend's JSON contracts exactly (camelCase — ASP.NET Core default).
 * See GET /notifications?unreadOnly=true|false -> NotificationDto[] (plain array, not paginated).
 */

export type NotificationType = "AssignmentPublished" | "SubmissionGraded";

export interface NotificationDto {
  id: string;
  type: NotificationType;
  /**
   * JSON-encoded string (System.Text.Json.JsonSerializer.Serialize output from the backend).
   * Deserializes to AssignmentPublishedPayload when type === "AssignmentPublished", or
   * SubmissionGradedPayload when type === "SubmissionGraded". Parse defensively — see api.ts /
   * components for the JSON.parse + try-catch guard.
   */
  payload: string;
  isRead: boolean;
  createdAt: string;
}

export interface AssignmentPublishedPayload {
  assignmentId: string;
  assignmentTitle: string;
}

/**
 * Note: the backend's serialized property here is literally "marks", not "marksAwarded" — it
 * does not match SubmissionDto's field naming. Do not assume consistency across payload shapes.
 */
export interface SubmissionGradedPayload {
  submissionId: string;
  marks: number | null;
}

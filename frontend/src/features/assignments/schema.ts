import { z } from "zod";

/**
 * Fields shared by create and update. deadlineUtc is bound to an <input type="datetime-local">
 * (see AssignmentFormWizard) — it stays a plain local-time string in the form itself and is only
 * converted to a UTC ISO string right before the API call. The future-date check here is a nice-
 * to-have UX guard; the backend re-validates authoritatively.
 */
const assignmentBaseFields = {
  title: z
    .string()
    .min(1, "Title is required.")
    .max(200, "Title must be at most 200 characters."),
  description: z.string().min(1, "Description is required."),
  deadlineUtc: z
    .string()
    .min(1, "Deadline is required.")
    .refine((value) => new Date(value).getTime() > Date.now(), {
      message: "Deadline must be in the future.",
    }),
  maxMarks: z.coerce
    .number()
    .int("Max marks must be a whole number.")
    .positive("Max marks must be greater than 0."),
};

export const createAssignmentSchema = z.object({
  ...assignmentBaseFields,
  classSubjectId: z.string().min(1, "Please select a class and subject."),
});

export type CreateAssignmentFormValues = z.infer<typeof createAssignmentSchema>;

export const updateAssignmentSchema = z.object(assignmentBaseFields);

export type UpdateAssignmentFormValues = z.infer<typeof updateAssignmentSchema>;

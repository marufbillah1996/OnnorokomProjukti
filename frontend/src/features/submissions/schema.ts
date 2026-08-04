import { z } from "zod";

export const submitAnswerSchema = z.object({
  content: z
    .string()
    .min(1, "Please write an answer before submitting.")
    .max(20000, "Answer must be 20000 characters or fewer."),
});

export type SubmitAnswerFormValues = z.infer<typeof submitAnswerSchema>;

export const gradeSubmissionSchema = z.object({
  marksAwarded: z.coerce
    .number()
    .int("Marks must be a whole number.")
    .min(0, "Marks cannot be negative."),
  feedback: z.string().max(2000, "Feedback must be 2000 characters or fewer.").optional(),
});

export type GradeSubmissionFormValues = z.infer<typeof gradeSubmissionSchema>;

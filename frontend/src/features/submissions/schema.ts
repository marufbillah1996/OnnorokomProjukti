import { z } from "zod";

export const submitAnswerSchema = z.object({
  content: z
    .string()
    .min(1, "Please write an answer before submitting.")
    .max(20000, "Answer must be 20000 characters or fewer."),
});

export type SubmitAnswerFormValues = z.infer<typeof submitAnswerSchema>;

export const gradeSubmissionSchema = z.object({
  // Plain z.number() (not z.coerce.number()) so the form's TS type is "number" both before and
  // after validation — the <input type="number"> -> number conversion happens via react-hook-form's
  // own register(..., { valueAsNumber: true }) instead, avoiding the well-known type mismatch
  // between zodResolver's inferred input/output types when z.coerce is used with useForm<T>.
  marksAwarded: z
    .number({ error: "Marks must be a number." })
    .int("Marks must be a whole number.")
    .min(0, "Marks cannot be negative."),
  feedback: z.string().max(2000, "Feedback must be 2000 characters or fewer.").optional(),
});

export type GradeSubmissionFormValues = z.infer<typeof gradeSubmissionSchema>;

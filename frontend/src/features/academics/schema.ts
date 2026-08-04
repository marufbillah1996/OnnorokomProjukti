import { z } from "zod";

export const classSchema = z.object({
  name: z.string().min(1, "Name is required").max(200, "Name must be at most 200 characters"),
  description: z
    .string()
    .max(1000, "Description must be at most 1000 characters")
    .optional()
    .or(z.literal("")),
});

export type ClassFormValues = z.infer<typeof classSchema>;

export const subjectSchema = z.object({
  name: z.string().min(1, "Name is required").max(200, "Name must be at most 200 characters"),
  code: z.string().min(1, "Code is required").max(20, "Code must be at most 20 characters"),
});

export type SubjectFormValues = z.infer<typeof subjectSchema>;

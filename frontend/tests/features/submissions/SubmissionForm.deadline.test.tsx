import { render, screen } from "@testing-library/react";
import { QueryClientProvider } from "@tanstack/react-query";
import type { ReactElement } from "react";
import { createQueryClient } from "@/shared/lib/query-client";
import { SubmissionForm } from "@/features/submissions/components/SubmissionForm";
import type { SubmissionDto } from "@/features/submissions/types";

// The component only needs to RENDER for these tests (deadline-gating logic), never actually
// submit, so the shared axios instance is mocked out entirely — no real network call can happen
// even if a future change accidentally fires a request during render.
jest.mock("@/shared/lib/api-client", () => ({
  apiClient: {
    get: jest.fn(),
    post: jest.fn(),
    put: jest.fn(),
    delete: jest.fn(),
  },
}));

const ASSIGNMENT_ID = "assignment-1";

function renderWithQueryClient(ui: ReactElement) {
  const queryClient = createQueryClient();
  return render(<QueryClientProvider client={queryClient}>{ui}</QueryClientProvider>);
}

function makeSubmission(overrides: Partial<SubmissionDto> = {}): SubmissionDto {
  return {
    id: "submission-1",
    assignmentId: ASSIGNMENT_ID,
    assignmentTitle: "Essay on Rivers",
    studentId: "student-1",
    studentName: "Jamie Doe",
    content: "My original answer.",
    submittedAt: "2026-01-01T00:00:00Z",
    status: "Pending",
    marksAwarded: null,
    maxMarks: 10,
    feedback: null,
    ...overrides,
  };
}

const PAST_DEADLINE = new Date(Date.now() - 24 * 60 * 60 * 1000).toISOString();
const FUTURE_DEADLINE = new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString();

describe("SubmissionForm deadline gating", () => {
  it("hides the editable form and shows a deadline-passed message when the deadline is past and there is no existing submission", () => {
    renderWithQueryClient(
      <SubmissionForm assignmentId={ASSIGNMENT_ID} deadlineUtc={PAST_DEADLINE} />,
    );

    // No submit control at all (absent, not merely disabled) once the deadline has passed and
    // there is nothing already on record to keep editing.
    expect(screen.queryByRole("button")).not.toBeInTheDocument();
    expect(screen.queryByRole("textbox")).not.toBeInTheDocument();

    // "deadline has passed" message, matched case-insensitively without assuming exact wording.
    expect(screen.getByText(/deadline/i)).toBeInTheDocument();
    expect(screen.getByText(/passed/i)).toBeInTheDocument();
  });

  it("shows an enabled content textarea and submit button when the deadline is in the future and there is no existing submission", () => {
    renderWithQueryClient(
      <SubmissionForm assignmentId={ASSIGNMENT_ID} deadlineUtc={FUTURE_DEADLINE} />,
    );

    const textarea = screen.getByRole("textbox", { name: /your answer/i });
    expect(textarea).toBeInTheDocument();
    expect(textarea).toBeEnabled();

    const submitButton = screen.getByRole("button", { name: /submit answer/i });
    expect(submitButton).toBeInTheDocument();
    expect(submitButton).toBeEnabled();
  });

  it("renders a read-only summary (no textarea) instead of the editable form when the existing submission is already Graded", () => {
    const graded = makeSubmission({
      status: "Graded",
      content: "My graded answer content.",
      marksAwarded: 8,
      maxMarks: 10,
      feedback: "Well done.",
    });

    renderWithQueryClient(
      <SubmissionForm
        assignmentId={ASSIGNMENT_ID}
        deadlineUtc={FUTURE_DEADLINE}
        existingSubmission={graded}
      />,
    );

    expect(screen.queryByRole("textbox")).not.toBeInTheDocument();
    expect(screen.getByText("My graded answer content.")).toBeInTheDocument();
    expect(screen.getByText(/8\s*\/\s*10/)).toBeInTheDocument();
  });
});

import { LoginForm } from "@/features/auth/components/LoginForm";

export default function LoginPage() {
  return (
    <div className="min-h-screen flex items-center justify-center bg-background px-4">
      <div className="flex w-full max-w-sm flex-col items-center gap-6">
        <div className="text-center">
          <h1 className="text-2xl font-bold">AssignmentHub</h1>
          <p className="text-sm text-foreground/60">
            Assignment &amp; Submission Management System
          </p>
        </div>
        <LoginForm />
      </div>
    </div>
  );
}

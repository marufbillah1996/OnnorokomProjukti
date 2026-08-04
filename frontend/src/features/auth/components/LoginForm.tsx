"use client";

import { useRouter } from "next/navigation";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { Card, CardBody, CardHeader, CardTitle } from "@/shared/components/ui/Card";
import { Button } from "@/shared/components/ui/Button";
import { Input } from "@/shared/components/ui/Input";
import { ROLE_HOME } from "@/shared/lib/role-routes";
import type { ApiError } from "@/shared/types/api-error";
import { useLogin } from "../hooks";
import { loginSchema, type LoginFormValues } from "../schema";

export function LoginForm() {
  const router = useRouter();
  const loginMutation = useLogin();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = (values: LoginFormValues) => {
    loginMutation.mutate(values, {
      onSuccess: (data) => {
        router.push(ROLE_HOME[data.role]);
      },
    });
  };

  return (
    <Card className="w-full max-w-sm">
      <CardHeader>
        <CardTitle>Sign in to AssignmentHub</CardTitle>
      </CardHeader>
      <CardBody>
        <form className="flex flex-col gap-4" onSubmit={handleSubmit(onSubmit)} noValidate>
          <Input
            label="Email"
            type="email"
            autoComplete="email"
            error={errors.email?.message}
            {...register("email")}
          />
          <Input
            label="Password"
            type="password"
            autoComplete="current-password"
            error={errors.password?.message}
            {...register("password")}
          />

          {loginMutation.isError && (
            <p className="text-sm text-danger-500">
              {(loginMutation.error as ApiError).message || "Invalid email or password."}
            </p>
          )}

          <Button type="submit" isLoading={loginMutation.isPending} className="w-full">
            Sign in
          </Button>
        </form>
      </CardBody>
    </Card>
  );
}

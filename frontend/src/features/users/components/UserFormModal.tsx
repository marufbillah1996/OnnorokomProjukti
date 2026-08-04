"use client";

import { Controller, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/shared/components/ui/Button";
import { Input } from "@/shared/components/ui/Input";
import { Modal } from "@/shared/components/ui/Modal";
import { Select, type SelectOption } from "@/shared/components/ui/Input";
import type { ApiError } from "@/shared/types/api-error";
import { ROLES } from "@/shared/types/role";
import { createUserSchema, updateUserSchema, type CreateUserFormValues, type UpdateUserFormValues } from "../schema";
import { useCreateUser, useUpdateUser } from "../hooks";
import type { UserDto } from "../types";

const ROLE_OPTIONS: SelectOption[] = ROLES.map((role) => ({ value: role, label: role }));

const STATUS_OPTIONS: SelectOption[] = [
  { value: "true", label: "Active" },
  { value: "false", label: "Inactive" },
];

export interface UserFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  user?: UserDto;
}

export function UserFormModal({ isOpen, onClose, user }: UserFormModalProps) {
  return (
    <Modal isOpen={isOpen} onClose={onClose} title={user ? "Edit user" : "Create user"}>
      {user ? (
        <EditUserForm key={user.id} user={user} onSuccess={onClose} onCancel={onClose} />
      ) : (
        <CreateUserForm key="create" onSuccess={onClose} onCancel={onClose} />
      )}
    </Modal>
  );
}

function CreateUserForm({ onSuccess, onCancel }: { onSuccess: () => void; onCancel: () => void }) {
  const createUser = useCreateUser();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<CreateUserFormValues>({ resolver: zodResolver(createUserSchema) });

  function onSubmit(values: CreateUserFormValues) {
    createUser.mutate(values, { onSuccess });
  }

  return (
    <form className="flex flex-col gap-4" onSubmit={handleSubmit(onSubmit)}>
      <Input label="Name" error={errors.name?.message} {...register("name")} />
      <Input label="Email" type="email" error={errors.email?.message} {...register("email")} />
      <Input label="Password" type="password" error={errors.password?.message} {...register("password")} />
      <Select label="Role" options={ROLE_OPTIONS} placeholder="Select a role" error={errors.role?.message} {...register("role")} />

      {createUser.isError && (
        <p className="text-sm text-danger-500">{(createUser.error as ApiError).message}</p>
      )}

      <div className="flex justify-end gap-2 pt-2">
        <Button type="button" variant="secondary" onClick={onCancel}>
          Cancel
        </Button>
        <Button type="submit" isLoading={createUser.isPending}>
          Create user
        </Button>
      </div>
    </form>
  );
}

function EditUserForm({
  user,
  onSuccess,
  onCancel,
}: {
  user: UserDto;
  onSuccess: () => void;
  onCancel: () => void;
}) {
  const updateUser = useUpdateUser();
  const {
    register,
    handleSubmit,
    control,
    formState: { errors },
  } = useForm<UpdateUserFormValues>({
    resolver: zodResolver(updateUserSchema),
    defaultValues: {
      name: user.name,
      email: user.email,
      role: user.role,
      isActive: user.isActive,
    },
  });

  function onSubmit(values: UpdateUserFormValues) {
    updateUser.mutate({ id: user.id, request: values }, { onSuccess });
  }

  return (
    <form className="flex flex-col gap-4" onSubmit={handleSubmit(onSubmit)}>
      <Input label="Name" error={errors.name?.message} {...register("name")} />
      <Input label="Email" type="email" error={errors.email?.message} {...register("email")} />
      <Select label="Role" options={ROLE_OPTIONS} error={errors.role?.message} {...register("role")} />

      <Controller
        control={control}
        name="isActive"
        render={({ field }) => (
          <Select
            label="Status"
            options={STATUS_OPTIONS}
            error={errors.isActive?.message}
            value={field.value ? "true" : "false"}
            onChange={(event) => field.onChange(event.target.value === "true")}
          />
        )}
      />

      {updateUser.isError && (
        <p className="text-sm text-danger-500">{(updateUser.error as ApiError).message}</p>
      )}

      <div className="flex justify-end gap-2 pt-2">
        <Button type="button" variant="secondary" onClick={onCancel}>
          Cancel
        </Button>
        <Button type="submit" isLoading={updateUser.isPending}>
          Save changes
        </Button>
      </div>
    </form>
  );
}

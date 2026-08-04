"use client";

import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { Button } from "@/shared/components/ui/Button";
import { Card, CardBody, CardHeader, CardTitle } from "@/shared/components/ui/Card";
import { Input, Select, Textarea } from "@/shared/components/ui/Input";
import { Modal } from "@/shared/components/ui/Modal";
import type { ApiError } from "@/shared/types/api-error";
import { ClassList } from "@/features/academics/components/ClassList";
import { StudentEnrollmentForm } from "@/features/academics/components/StudentEnrollmentForm";
import { SubjectList } from "@/features/academics/components/SubjectList";
import { TeacherAssignmentForm } from "@/features/academics/components/TeacherAssignmentForm";
import {
  useClasses,
  useCreateClass,
  useCreateSubject,
  useDeleteClass,
  useDeleteSubject,
  useSubjects,
  useUpdateClass,
  useUpdateSubject,
} from "@/features/academics/hooks";
import {
  classSchema,
  subjectSchema,
  type ClassFormValues,
  type SubjectFormValues,
} from "@/features/academics/schema";
import type { ClassDto, SubjectDto } from "@/features/academics/types";

/** Both lists are shown in full (no pagination UI) — a school's class/subject catalogue is
 * small enough that a large page size effectively acts as "show everything". */
const CATALOGUE_QUERY = { page: 1, pageSize: 100 };

function NewClassForm() {
  const createClass = useCreateClass();
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<ClassFormValues>({ resolver: zodResolver(classSchema), defaultValues: { name: "", description: "" } });

  function onSubmit(values: ClassFormValues) {
    createClass.mutate(
      { name: values.name, description: values.description || null },
      { onSuccess: () => reset({ name: "", description: "" }) },
    );
  }

  return (
    <form className="flex flex-col gap-3 border-b border-border pb-4" onSubmit={handleSubmit(onSubmit)}>
      <Input label="Name" placeholder="e.g. Class 8" error={errors.name?.message} {...register("name")} />
      <Textarea
        label="Description"
        placeholder="Optional"
        error={errors.description?.message}
        {...register("description")}
      />
      {createClass.isError && (
        <p className="text-sm text-danger-500">{(createClass.error as ApiError).message}</p>
      )}
      <div className="flex justify-end">
        <Button type="submit" size="sm" isLoading={createClass.isPending}>
          Add class
        </Button>
      </div>
    </form>
  );
}

function EditClassModal({ klass, onClose }: { klass: ClassDto; onClose: () => void }) {
  const updateClass = useUpdateClass();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ClassFormValues>({
    resolver: zodResolver(classSchema),
    defaultValues: { name: klass.name, description: klass.description ?? "" },
  });

  function onSubmit(values: ClassFormValues) {
    updateClass.mutate(
      { id: klass.id, req: { name: values.name, description: values.description || null } },
      { onSuccess: onClose },
    );
  }

  return (
    <Modal isOpen title="Edit class" onClose={onClose}>
      <form className="flex flex-col gap-4" onSubmit={handleSubmit(onSubmit)}>
        <Input label="Name" error={errors.name?.message} {...register("name")} />
        <Textarea label="Description" error={errors.description?.message} {...register("description")} />
        {updateClass.isError && (
          <p className="text-sm text-danger-500">{(updateClass.error as ApiError).message}</p>
        )}
        <div className="flex justify-end gap-2 pt-2">
          <Button type="button" variant="secondary" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" isLoading={updateClass.isPending}>
            Save changes
          </Button>
        </div>
      </form>
    </Modal>
  );
}

function NewSubjectForm() {
  const createSubject = useCreateSubject();
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<SubjectFormValues>({ resolver: zodResolver(subjectSchema), defaultValues: { name: "", code: "" } });

  function onSubmit(values: SubjectFormValues) {
    createSubject.mutate(values, { onSuccess: () => reset({ name: "", code: "" }) });
  }

  return (
    <form className="flex flex-col gap-3 border-b border-border pb-4" onSubmit={handleSubmit(onSubmit)}>
      <Input label="Name" placeholder="e.g. Mathematics" error={errors.name?.message} {...register("name")} />
      <Input label="Code" placeholder="e.g. MATH" error={errors.code?.message} {...register("code")} />
      {createSubject.isError && (
        <p className="text-sm text-danger-500">{(createSubject.error as ApiError).message}</p>
      )}
      <div className="flex justify-end">
        <Button type="submit" size="sm" isLoading={createSubject.isPending}>
          Add subject
        </Button>
      </div>
    </form>
  );
}

function EditSubjectModal({ subject, onClose }: { subject: SubjectDto; onClose: () => void }) {
  const updateSubject = useUpdateSubject();
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<SubjectFormValues>({
    resolver: zodResolver(subjectSchema),
    defaultValues: { name: subject.name, code: subject.code },
  });

  function onSubmit(values: SubjectFormValues) {
    updateSubject.mutate({ id: subject.id, req: values }, { onSuccess: onClose });
  }

  return (
    <Modal isOpen title="Edit subject" onClose={onClose}>
      <form className="flex flex-col gap-4" onSubmit={handleSubmit(onSubmit)}>
        <Input label="Name" error={errors.name?.message} {...register("name")} />
        <Input label="Code" error={errors.code?.message} {...register("code")} />
        {updateSubject.isError && (
          <p className="text-sm text-danger-500">{(updateSubject.error as ApiError).message}</p>
        )}
        <div className="flex justify-end gap-2 pt-2">
          <Button type="button" variant="secondary" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" isLoading={updateSubject.isPending}>
            Save changes
          </Button>
        </div>
      </form>
    </Modal>
  );
}

export default function AdminAcademicsPage() {
  const classesQuery = useClasses(CATALOGUE_QUERY);
  const subjectsQuery = useSubjects(CATALOGUE_QUERY);
  const deleteClass = useDeleteClass();
  const deleteSubject = useDeleteSubject();

  const [editingClass, setEditingClass] = useState<ClassDto | null>(null);
  const [editingSubject, setEditingSubject] = useState<SubjectDto | null>(null);
  const [selectedClassId, setSelectedClassId] = useState<string | null>(null);

  function handleDeleteClass(klass: ClassDto) {
    if (window.confirm(`Delete class "${klass.name}"? This cannot be undone.`)) {
      deleteClass.mutate(klass.id);
    }
  }

  function handleDeleteSubject(subject: SubjectDto) {
    if (window.confirm(`Delete subject "${subject.name}"? This cannot be undone.`)) {
      deleteSubject.mutate(subject.id);
    }
  }

  const classOptions = (classesQuery.data?.items ?? []).map((klass) => ({
    value: klass.id,
    label: klass.name,
  }));

  return (
    <div className="flex flex-col gap-6">
      <h2 className="text-xl font-semibold text-foreground">Academics</h2>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Classes</CardTitle>
          </CardHeader>
          <CardBody className="flex flex-col gap-4">
            <NewClassForm />
            <ClassList
              classes={classesQuery.data?.items ?? []}
              isLoading={classesQuery.isLoading}
              onEdit={setEditingClass}
              onDelete={handleDeleteClass}
            />
            {deleteClass.isError && (
              <p className="text-sm text-danger-500">{(deleteClass.error as ApiError).message}</p>
            )}
          </CardBody>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Subjects</CardTitle>
          </CardHeader>
          <CardBody className="flex flex-col gap-4">
            <NewSubjectForm />
            <SubjectList
              subjects={subjectsQuery.data?.items ?? []}
              isLoading={subjectsQuery.isLoading}
              onEdit={setEditingSubject}
              onDelete={handleDeleteSubject}
            />
            {deleteSubject.isError && (
              <p className="text-sm text-danger-500">{(deleteSubject.error as ApiError).message}</p>
            )}
          </CardBody>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Manage a class</CardTitle>
        </CardHeader>
        <CardBody className="flex flex-col gap-4">
          <Select
            label="Select a class to manage teachers and students"
            options={classOptions}
            placeholder={classesQuery.isLoading ? "Loading classes…" : "Choose a class…"}
            value={selectedClassId ?? ""}
            onChange={(event) => setSelectedClassId(event.target.value || null)}
            className="max-w-sm"
          />

          {selectedClassId && (
            <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
              <div>
                <h3 className="mb-2 text-sm font-semibold text-foreground">Assign teachers</h3>
                <TeacherAssignmentForm classId={selectedClassId} />
              </div>
              <div>
                <h3 className="mb-2 text-sm font-semibold text-foreground">Enroll students</h3>
                <StudentEnrollmentForm classId={selectedClassId} />
              </div>
            </div>
          )}
        </CardBody>
      </Card>

      {editingClass && <EditClassModal klass={editingClass} onClose={() => setEditingClass(null)} />}
      {editingSubject && <EditSubjectModal subject={editingSubject} onClose={() => setEditingSubject(null)} />}
    </div>
  );
}

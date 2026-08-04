"use client";

import { Card, CardBody, CardHeader, CardTitle } from "@/shared/components/ui/Card";
import { Skeleton } from "@/shared/components/ui/Skeleton";
import { useUsers } from "@/features/users/hooks";
import { useAdminAssignments } from "@/features/assignments/hooks";
import { useAdminSubmissions } from "@/features/submissions/hooks";

interface StatTileProps {
  title: string;
  value: number | undefined;
  isLoading: boolean;
}

function StatTile({ title, value, isLoading }: StatTileProps) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{title}</CardTitle>
      </CardHeader>
      <CardBody>
        {isLoading ? (
          <Skeleton className="h-9 w-20" />
        ) : (
          <p className="text-3xl font-semibold text-foreground">{value ?? 0}</p>
        )}
      </CardBody>
    </Card>
  );
}

export default function AdminDashboardPage() {
  const usersQuery = useUsers({ pageSize: 1 });
  const assignmentsQuery = useAdminAssignments({ pageSize: 1 });
  const submissionsQuery = useAdminSubmissions({ pageSize: 1 });

  return (
    <div className="flex flex-col gap-6">
      <h2 className="text-xl font-semibold text-foreground">Overview</h2>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <StatTile
          title="Total Users"
          value={usersQuery.data?.totalCount}
          isLoading={usersQuery.isLoading}
        />
        <StatTile
          title="Total Assignments"
          value={assignmentsQuery.data?.totalCount}
          isLoading={assignmentsQuery.isLoading}
        />
        <StatTile
          title="Total Submissions"
          value={submissionsQuery.data?.totalCount}
          isLoading={submissionsQuery.isLoading}
        />
      </div>
    </div>
  );
}

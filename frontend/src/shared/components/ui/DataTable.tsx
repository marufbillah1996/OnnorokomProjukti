import { SkeletonRows } from "@/shared/components/ui/Skeleton";

export interface DataTableColumn<T> {
  key: string;
  header: string;
  render: (row: T) => React.ReactNode;
  className?: string;
}

export interface DataTableProps<T> {
  columns: DataTableColumn<T>[];
  data: T[];
  keyExtractor: (row: T) => string;
  isLoading?: boolean;
  emptyMessage?: string;
}

export function DataTable<T>({
  columns,
  data,
  keyExtractor,
  isLoading,
  emptyMessage = "Nothing to show yet.",
}: DataTableProps<T>) {
  if (isLoading) {
    return (
      <div className="p-4">
        <SkeletonRows rows={5} />
      </div>
    );
  }

  if (data.length === 0) {
    return <p className="p-6 text-center text-sm text-foreground/60">{emptyMessage}</p>;
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-left text-sm">
        <thead className="border-b border-border bg-surface-muted text-xs uppercase tracking-wide text-foreground/60">
          <tr>
            {columns.map((column) => (
              <th key={column.key} className="px-4 py-3 font-medium">
                {column.header}
              </th>
            ))}
          </tr>
        </thead>
        <tbody className="divide-y divide-border">
          {data.map((row) => (
            <tr key={keyExtractor(row)} className="hover:bg-surface-muted/60">
              {columns.map((column) => (
                <td key={column.key} className={`px-4 py-3 text-foreground ${column.className ?? ""}`}>
                  {column.render(row)}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

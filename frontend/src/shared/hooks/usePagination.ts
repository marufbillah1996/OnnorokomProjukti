import { useMemo, useState } from "react";
import { useDebounce } from "@/shared/hooks/useDebounce";
import type { PaginationQuery } from "@/shared/types/paginated";

/**
 * Local UI state for a paginated/searchable/sortable list, matching the backend's
 * ?page=&pageSize=&search=&sortBy=&sortDir= convention (see IMPLEMENTATION_PLAN.md section 6).
 * Search is debounced before it flows into the returned query object, so callers can put that
 * object directly into a React Query key without re-fetching on every keystroke.
 */
export function usePagination(initialPageSize = 10) {
  const [page, setPage] = useState(1);
  const [pageSize] = useState(initialPageSize);
  const [search, setSearch] = useState("");
  const [sortBy, setSortBy] = useState<string | undefined>(undefined);
  const [sortDir, setSortDir] = useState<"asc" | "desc">("asc");

  const debouncedSearch = useDebounce(search, 300);

  const query: PaginationQuery = useMemo(
    () => ({
      page,
      pageSize,
      search: debouncedSearch || undefined,
      sortBy,
      sortDir,
    }),
    [page, pageSize, debouncedSearch, sortBy, sortDir],
  );

  function updateSearch(value: string) {
    setSearch(value);
    setPage(1);
  }

  function toggleSort(field: string) {
    if (sortBy !== field) {
      setSortBy(field);
      setSortDir("asc");
    } else {
      setSortDir((previous) => (previous === "asc" ? "desc" : "asc"));
    }
    setPage(1);
  }

  return { page, setPage, search, updateSearch, sortBy, sortDir, toggleSort, query };
}

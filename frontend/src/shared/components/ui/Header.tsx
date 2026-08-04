export interface HeaderProps {
  title: string;
  userName: string;
  onLogout: () => void;
  /** Composition slot for feature-level widgets (e.g. the notifications bell) — shared/
   * components never import from features/, so the caller passes them in here instead. */
  actions?: React.ReactNode;
}

export function Header({ title, userName, onLogout, actions }: HeaderProps) {
  return (
    <header className="flex items-center justify-between border-b border-border bg-surface px-6 py-4">
      <h1 className="text-lg font-semibold text-foreground">{title}</h1>
      <div className="flex items-center gap-4">
        {actions}
        <span className="text-sm text-foreground/70">{userName}</span>
        <button
          type="button"
          onClick={onLogout}
          className="text-sm font-medium text-brand-600 hover:text-brand-700"
        >
          Log out
        </button>
      </div>
    </header>
  );
}

import { AppShell } from "@/components/app-shell";
import { I18nProvider } from "@/lib/i18n";
import type { ReactNode } from "react";

export function Providers({ children }: { children: ReactNode }) {
  return (
    <I18nProvider>
      <AppShell>{children}</AppShell>
    </I18nProvider>
  );
}

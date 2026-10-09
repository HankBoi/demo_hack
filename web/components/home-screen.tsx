"use client";

import { SamplePreview } from "@/components/sample-preview";
import { SiteHeader } from "@/components/site-header";
import { StartForm } from "@/components/start-form";
import { Alert } from "@/components/ui/alert";
import { useI18n } from "@/lib/i18n";

export function HomeScreen() {
  const { t } = useI18n();

  return (
    <>
      <SiteHeader />
      <main className="mx-auto grid w-full max-w-6xl gap-8 px-4 py-8">
        <div className="max-w-3xl">
          <h1 className="text-3xl font-semibold tracking-tight sm:text-4xl">{t("productName")}</h1>
          <p className="mt-2 font-serif text-xl text-foreground">{t("tagline")}</p>
          <p className="mt-3 max-w-2xl text-base leading-7 text-muted">{t("description")}</p>
        </div>
        <Alert>
          <p className="font-medium">{t("boundaryTitle")}</p>
          <p className="mt-1 leading-6">{t("boundaryBody")}</p>
          <p className="mt-2 text-muted">{t("notOfficial")}</p>
        </Alert>
        <div className="grid items-start gap-6 lg:grid-cols-2">
          <StartForm />
          <SamplePreview />
        </div>
      </main>
    </>
  );
}

"use client";

import { ApiError, getConfig, getQuota, type AppConfig, type Quota } from "@/lib/api";
import { useI18n } from "@/lib/i18n";
import { useCallback, useEffect, useState } from "react";

/** Maps any thrown value to a localized, user-facing sentence. Unknown codes fall back to a safe generic text. */
export function useErrorText() {
  const { t, has } = useI18n();
  return useCallback(
    (caught: unknown) => {
      if (caught instanceof ApiError) {
        const key = `errors.${caught.code}`;
        return has(key) ? t(key) : t("errors.generic");
      }
      return t("errors.generic");
    },
    [t, has],
  );
}

const QUOTA_EVENT = "tec:quota-changed";

export function useQuota(refreshKey?: string) {
  const [quota, setQuota] = useState<Quota | null>(null);
  const [failed, setFailed] = useState(false);

  const [tick, setTick] = useState(0);

  useEffect(() => {
    let stop = false;
    getQuota()
      .then((value) => {
        if (stop) return;
        setQuota(value);
        setFailed(false);
      })
      .catch(() => {
        if (!stop) setFailed(true);
      });
    return () => {
      stop = true;
    };
  }, [tick, refreshKey]);

  // Every mounted useQuota instance (header chip, plan page) reloads when the quota changes.
  useEffect(() => {
    const onChange = () => setTick((value) => value + 1);
    window.addEventListener(QUOTA_EVENT, onChange);
    return () => window.removeEventListener(QUOTA_EVENT, onChange);
  }, []);

  const reload = useCallback(async () => {
    window.dispatchEvent(new Event(QUOTA_EVENT));
  }, []);

  return { quota, failed, reload };
}

export function useConfig() {
  const [config, setConfig] = useState<AppConfig | null>(null);
  const [failed, setFailed] = useState(false);

  useEffect(() => {
    let stop = false;
    getConfig()
      .then((value) => {
        if (!stop) setConfig(value);
      })
      .catch(() => {
        if (!stop) setFailed(true);
      });
    return () => {
      stop = true;
    };
  }, []);

  return { config, failed };
}

export function formatBytes(bytes: number) {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(0)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

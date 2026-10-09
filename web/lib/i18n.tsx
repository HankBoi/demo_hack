"use client";

import az from "@/messages/az.json";
import en from "@/messages/en.json";
import ru from "@/messages/ru.json";
import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useSyncExternalStore,
  type ReactNode,
} from "react";

export type Locale = "az" | "ru" | "en";

export const LOCALES: Locale[] = ["az", "ru", "en"];

// The type check below makes ru.json and en.json fail to compile when a key is missing.
const dictionaries = { az, ru, en } satisfies Record<Locale, typeof az>;

type Tree = { [key: string]: string | Tree };

const STORAGE_KEY = "tec-locale";
const listeners = new Set<() => void>();

function isLocale(value: string | null): value is Locale {
  return value === "az" || value === "ru" || value === "en";
}

function readLocale(): Locale {
  try {
    const stored = window.localStorage.getItem(STORAGE_KEY);
    return isLocale(stored) ? stored : "az";
  } catch {
    return "az";
  }
}

function subscribe(listener: () => void) {
  listeners.add(listener);
  window.addEventListener("storage", listener);
  return () => {
    listeners.delete(listener);
    window.removeEventListener("storage", listener);
  };
}

function writeLocale(locale: Locale) {
  try {
    window.localStorage.setItem(STORAGE_KEY, locale);
  } catch {
    // The choice still applies for this page view when storage is unavailable.
  }
  listeners.forEach((listener) => listener());
}

function lookup(tree: Tree, path: string): string {
  let current: string | Tree = tree;
  for (const part of path.split(".")) {
    if (typeof current === "string" || !(part in current)) {
      return path;
    }
    current = current[part];
  }
  return typeof current === "string" ? current : path;
}

type I18nValue = {
  locale: Locale;
  setLocale: (locale: Locale) => void;
  t: (path: string, vars?: Record<string, string | number>) => string;
  has: (path: string) => boolean;
  formatDate: (iso?: string | null) => string;
};

const I18nContext = createContext<I18nValue | null>(null);

export function I18nProvider({ children }: { children: ReactNode }) {
  const locale = useSyncExternalStore<Locale>(subscribe, readLocale, () => "az");
  const setLocale = useCallback((next: Locale) => writeLocale(next), []);

  const value = useMemo<I18nValue>(() => {
    const tree = dictionaries[locale] as Tree;
    return {
      locale,
      setLocale,
      t: (path, vars) => {
        let text = lookup(tree, path);
        if (vars) {
          for (const [key, replacement] of Object.entries(vars)) {
            text = text.replaceAll(`{${key}}`, String(replacement));
          }
        }
        return text;
      },
      has: (path) => lookup(tree, path) !== path,
      formatDate: (iso) => {
        if (!iso) return "—";
        const date = new Date(iso.endsWith("Z") || iso.includes("+") ? iso : `${iso}Z`);
        if (Number.isNaN(date.getTime())) return "—";
        const pad = (value: number) => String(value).padStart(2, "0");
        return `${pad(date.getDate())}.${pad(date.getMonth() + 1)}.${date.getFullYear()} ${pad(date.getHours())}:${pad(date.getMinutes())}`;
      },
    };
  }, [locale, setLocale]);

  useEffect(() => {
    document.documentElement.lang = locale;
    document.title = value.t("productName");
  }, [locale, value]);

  return <I18nContext.Provider value={value}>{children}</I18nContext.Provider>;
}

export function useI18n() {
  const value = useContext(I18nContext);
  if (!value) {
    throw new Error("useI18n must be used inside I18nProvider");
  }
  return value;
}

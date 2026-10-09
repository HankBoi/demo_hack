"use client";

import az from "@/messages/az.json";
import en from "@/messages/en.json";
import {
  createContext,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";

export type Locale = "az" | "en";

const dictionaries = { az, en };

type Tree = { [key: string]: string | Tree };

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
  t: (path: string) => string;
};

const I18nContext = createContext<I18nValue | null>(null);

export function I18nProvider({ children }: { children: ReactNode }) {
  const [locale, setLocale] = useState<Locale>("az");

  const value = useMemo<I18nValue>(() => {
    const tree = dictionaries[locale] as Tree;
    return {
      locale,
      setLocale,
      t: (path) => lookup(tree, path),
    };
  }, [locale]);

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

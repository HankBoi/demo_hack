import type { Metadata } from "next";
import localFont from "next/font/local";
import { Providers } from "./providers";
import "./globals.css";

const notoSans = localFont({
  src: [
    { path: "./fonts/NotoSans-Regular.ttf", weight: "400", style: "normal" },
    { path: "./fonts/NotoSans-SemiBold.ttf", weight: "600", style: "normal" },
  ],
  variable: "--font-noto-sans",
  display: "swap",
});

const notoSerif = localFont({
  src: "./fonts/NotoSerif-Regular.ttf",
  variable: "--font-noto-serif",
  display: "swap",
});

export const metadata: Metadata = {
  title: "Tender Evidence Checker",
  description: "Every requirement. Its source. Your evidence.",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html lang="az" className={`${notoSans.variable} ${notoSerif.variable} h-full`}>
      <body className="min-h-full bg-background font-sans text-foreground antialiased">
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}

import type { CSSProperties } from "react";

type Item = { emoji: string; className: string; delay: string };

const ITEMS: Item[] = [
  { emoji: "📄", className: "left-[2%] top-[4%] text-3xl sm:left-[4%] sm:text-4xl", delay: "0s" },
  { emoji: "🔍", className: "right-[3%] top-[0%] text-3xl sm:right-[6%] sm:text-5xl", delay: "-3s" },
  { emoji: "📎", className: "left-[1%] bottom-[8%] hidden text-3xl sm:block sm:text-4xl", delay: "-5s" },
  { emoji: "✅", className: "right-[2%] bottom-[2%] text-3xl sm:right-[8%] sm:text-4xl", delay: "-7s" },
];

/** Small slowly drifting emoji bubbles placed around the hero heading. Purely decorative. */
export function FloatingEmoji() {
  return (
    <div aria-hidden="true" className="pointer-events-none absolute inset-0 select-none">
      {ITEMS.map((item) => (
        <span
          key={item.emoji}
          className={`float-emoji glass absolute grid size-12 place-items-center rounded-2xl sm:size-16 ${item.className}`}
          style={{ "--float-delay": item.delay } as CSSProperties}
        >
          <span className="leading-none">{item.emoji}</span>
        </span>
      ))}
    </div>
  );
}

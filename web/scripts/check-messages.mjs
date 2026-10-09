// Verifies that every literal translation key used in the source exists in az, ru, and en,
// and that the three dictionaries have identical key sets.
import { readFileSync, readdirSync, statSync } from "node:fs";
import { join } from "node:path";

const root = new URL("..", import.meta.url).pathname.replace(/^\/([A-Za-z]:)/, "$1");
const dictionaries = Object.fromEntries(
  ["az", "ru", "en"].map((code) => [code, JSON.parse(readFileSync(join(root, "messages", `${code}.json`), "utf8"))]),
);

function flatten(tree, prefix = "") {
  return Object.entries(tree).flatMap(([key, value]) =>
    typeof value === "string" ? [prefix + key] : flatten(value, `${prefix}${key}.`),
  );
}

const keySets = Object.fromEntries(Object.entries(dictionaries).map(([code, tree]) => [code, new Set(flatten(tree))]));
let failures = 0;

for (const code of ["ru", "en"]) {
  for (const key of keySets.az) {
    if (!keySets[code].has(key)) {
      console.error(`MISSING in ${code}: ${key}`);
      failures++;
    }
  }
  for (const key of keySets[code]) {
    if (!keySets.az.has(key)) {
      console.error(`EXTRA in ${code}: ${key}`);
      failures++;
    }
  }
}

function walk(directory) {
  return readdirSync(directory).flatMap((name) => {
    if (["node_modules", ".next", "scripts", "messages"].includes(name)) return [];
    const path = join(directory, name);
    return statSync(path).isDirectory() ? walk(path) : /\.(tsx?|mjs)$/.test(name) ? [path] : [];
  });
}

const used = new Set();
const prefixes = new Set();
for (const file of walk(root)) {
  const source = readFileSync(file, "utf8");
  for (const match of source.matchAll(/\bt\(\s*"([A-Za-z0-9_.]+)"/g)) used.add(match[1]);
  for (const match of source.matchAll(/\bt\(\s*`([A-Za-z0-9_.]+)\.\$\{/g)) prefixes.add(match[1]);
  for (const match of source.matchAll(/\bt\(\s*`([A-Za-z0-9_.]+)`/g)) used.add(match[1]);
}

for (const key of used) {
  if (!keySets.az.has(key)) {
    console.error(`UNKNOWN key used in source: ${key}`);
    failures++;
  }
}
for (const prefix of prefixes) {
  if (![...keySets.az].some((key) => key.startsWith(`${prefix}.`))) {
    console.error(`UNKNOWN dynamic prefix used in source: ${prefix}.*`);
    failures++;
  }
}

console.log(`keys: az=${keySets.az.size} ru=${keySets.ru.size} en=${keySets.en.size}; literal keys used: ${used.size}; dynamic prefixes: ${prefixes.size}`);
if (failures > 0) {
  console.error(`${failures} problem(s)`);
  process.exit(1);
}
console.log("message check OK");

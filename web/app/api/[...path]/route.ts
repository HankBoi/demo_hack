import { NextResponse, type NextRequest } from "next/server";

// Accept a bare host as well as a full URL. Public Railway domains need https, private ones (.railway.internal) use http.
function normalizeTarget(value: string | undefined): string {
  const raw = (value ?? "").trim().replace(/\/+$/, "");
  if (!raw) return "http://127.0.0.1:43124";
  if (/^https?:\/\//i.test(raw)) return raw;
  return `${raw.endsWith(".railway.internal") || /\.railway\.internal:\d+$/.test(raw) ? "http" : "https"}://${raw}`;
}

const TARGET = normalizeTarget(process.env.API_PROXY_TARGET);

type RouteContext = { params: Promise<{ path: string[] }> };

async function proxy(request: NextRequest, context: RouteContext) {
  const { path } = await context.params;
  const target = new URL(`/api/${path.join("/")}`, TARGET);
  target.search = request.nextUrl.search;

  const headers = new Headers(request.headers);
  headers.delete("host");
  headers.delete("connection");

  try {
    const hasBody = request.method !== "GET" && request.method !== "HEAD";
    const response = await fetch(target, {
      method: request.method,
      headers,
      body: hasBody ? request.body : undefined,
      duplex: "half",
      redirect: "manual",
    } as RequestInit & { duplex: "half" });
    const out = new Headers(response.headers);
    out.delete("transfer-encoding");
    out.delete("content-encoding");
    return new NextResponse(response.body, { status: response.status, headers: out });
  } catch {
    return NextResponse.json(
      {
        code: "api_down",
        user_message: "The API is not responding. Start the API process, then try again.",
        retryable: true,
        request_id: "proxy",
      },
      { status: 503 },
    );
  }
}

export const GET = proxy;
export const POST = proxy;
export const PATCH = proxy;
export const DELETE = proxy;
export const PUT = proxy;
export const dynamic = "force-dynamic";

/* global Headers, Request, Response, URL, fetch */

function jsonResponse(body, status) {
  return new Response(JSON.stringify(body), {
    status,
    headers: { "content-type": "application/json; charset=utf-8" },
  });
}

async function proxyApi(request, env, url) {
  if (!env.API_BASE_URL) {
    return jsonResponse(
      {
        success: false,
        message: "Production API endpoint is not configured.",
        data: null,
        errors: ["Set the API_BASE_URL environment variable for this site."],
      },
      503,
    );
  }

  const apiBase = env.API_BASE_URL.replace(/\/+$/, "");
  const apiPath = url.pathname.replace(/^\/api(?=\/|$)/, "");
  const target = new URL(`${apiBase}${apiPath}${url.search}`);
  const proxyRequest = new Request(target, request);

  return fetch(proxyRequest);
}

async function serveSpa(request, env, url) {
  let response = await env.ASSETS.fetch(request);

  if (response.status === 404 && request.method === "GET") {
    const indexUrl = new URL("/index.html", url.origin);
    response = await env.ASSETS.fetch(new Request(indexUrl, request));
  }

  const contentType = response.headers.get("content-type") ?? "";
  if (contentType.includes("text/html")) {
    const html = (await response.text()).replaceAll(
      'content="/og.png"',
      `content="${url.origin}/og.png"`,
    );
    const headers = new Headers(response.headers);
    headers.delete("content-length");
    return new Response(html, {
      status: response.status,
      statusText: response.statusText,
      headers,
    });
  }

  return response;
}

export default {
  async fetch(request, env) {
    const url = new URL(request.url);

    if (url.pathname === "/api" || url.pathname.startsWith("/api/")) {
      return proxyApi(request, env, url);
    }

    return serveSpa(request, env, url);
  },
};

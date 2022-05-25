import { postgres } from "./deps.ts";
// const host = Deno.env.get("POSTGRES_HOST") ?? "";
// const port = (Deno.env.get("POSTGRES_PORT") ?? "5432") + ",";
// const database = Deno.env.get("POSTGRES_DBNAME") ?? "";
// const username = Deno.env.get("POSTGRES_USER") ?? "";
// const password = Deno.env.get("POSTGRES_PASSWORD") ?? "";

export const sql = postgres.default();

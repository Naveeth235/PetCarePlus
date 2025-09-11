// src/features/admin/usersApi.ts
const BASE = (import.meta as any).env?.VITE_API_BASE_URL as string | undefined;
const TOKEN_KEY = "APP_AT";

export type RoleFilter = "All" | "Admin" | "Vet" | "Owner";

export type UserListItem = {
  id: string;
  fullName: string | null;
  email: string | null;
  roles: string[];
  accountStatus?: string; // e.g. "Active"
  isActive?: boolean;
};

export type UserListOk = {
  ok: true;
  items: UserListItem[];
  total: number;
  page: number;
  pageSize: number;
};

export type UserListErr = {
  ok: false;
  code: "unauthorized" | "forbidden" | "network" | "failed";
  status?: number;
  message?: string;
};

export async function fetchAdminUsers(params: {
  page?: number;
  pageSize?: number;
  search?: string;
  role?: RoleFilter;
  signal?: AbortSignal;
}): Promise<UserListOk | UserListErr> {
  if (!BASE) return { ok: false, code: "failed", message: "No API base URL" };

  const token = localStorage.getItem(TOKEN_KEY);
  if (!token) return { ok: false, code: "unauthorized" };

  const q = new URLSearchParams();
  if (params.page) q.set("page", String(params.page));
  if (params.pageSize) q.set("pageSize", String(params.pageSize));
  if (params.search?.trim()) q.set("search", params.search.trim());
  if (params.role && params.role !== "All") q.set("role", params.role);

  const url = `${BASE}/api/admin/users?${q.toString()}`;
  try {
    const res = await fetch(url, {
      headers: { Authorization: `Bearer ${token}` },
      signal: params.signal,
    });

    if (res.status === 401)
      return { ok: false, code: "unauthorized", status: 401 };
    if (res.status === 403)
      return { ok: false, code: "forbidden", status: 403 };
    if (!res.ok) return { ok: false, code: "failed", status: res.status };

    const data = await res.json();
    return {
      ok: true,
      items: (data.items ?? []) as UserListItem[],
      total: data.total ?? 0,
      page: data.page ?? 1,
      pageSize: data.pageSize ?? 10,
    };
  } catch (e) {
    console.error("fetchAdminUsers error", e);
    return { ok: false, code: "network" };
  }
}

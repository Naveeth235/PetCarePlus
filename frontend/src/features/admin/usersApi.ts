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

export type UpdateUserBody = {
  fullName?: string;
  email?: string;
  accountStatus?: "Active" | "Inactive";
};

export type UpdateUserOk = {
  ok: true;
  user: {
    id: string;
    fullName: string | null;
    email: string | null;
    accountStatus: string;
    isActive: boolean;
    roles: string[];
  };
};

export type UpdateUserErr = {
  ok: false;
  code:
    | "unauthorized"
    | "forbidden"
    | "not_found"
    | "conflict"
    | "network"
    | "failed";
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

export async function updateUser(
  id: string,
  body: UpdateUserBody
): Promise<UpdateUserOk | UpdateUserErr> {
  const BASE = (import.meta as any).env?.VITE_API_BASE_URL as
    | string
    | undefined;
  const token = localStorage.getItem("APP_AT");
  if (!BASE) return { ok: false, code: "failed", message: "No API base URL" };
  if (!token) return { ok: false, code: "unauthorized" };

  try {
    const res = await fetch(`${BASE}/api/admin/users/${id}`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify(body),
    });

    if (res.status === 401)
      return { ok: false, code: "unauthorized", status: 401 };
    if (res.status === 403)
      return { ok: false, code: "forbidden", status: 403 };
    if (res.status === 404)
      return { ok: false, code: "not_found", status: 404 };
    if (res.status === 409) return { ok: false, code: "conflict", status: 409 };
    if (!res.ok) return { ok: false, code: "failed", status: res.status };

    const data = await res.json();
    return {
      ok: true,
      user: {
        id: data.id,
        fullName: data.fullName ?? null,
        email: data.email ?? null,
        accountStatus: data.accountStatus,
        isActive: !!data.isActive,
        roles: data.roles ?? [],
      },
    };
  } catch (e) {
    console.error("updateUser error", e);
    return { ok: false, code: "network" };
  }
}

// --- ME helpers (can live in usersApi.ts or a new meApi.ts) ---
type MeInfo = {
  userId: string;
  fullName?: string;
  email?: string;
  roles?: string[];
};

export async function getMe(): Promise<MeInfo> {
  if (!BASE) throw new Error("no-base");
  const token = localStorage.getItem(TOKEN_KEY);
  if (!token) throw new Error("no-token");

  const res = await fetch(`${BASE}/api/auth/me`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  if (!res.ok) throw new Error(`me-${res.status}`);
  return res.json();
}

export type UpdateMeOk = {
  ok: true;
  data: { id: string; fullName?: string; email?: string; roles: string[] };
};
export type UpdateMeErr = {
  ok: false;
  code: "unauthorized" | "conflict" | "failed" | "network";
  status?: number;
  message?: string;
};

export async function updateMe(body: {
  fullName?: string;
  email?: string;
}): Promise<UpdateMeOk | UpdateMeErr> {
  if (!BASE) return { ok: false, code: "failed", message: "No API base URL" };
  const token = localStorage.getItem(TOKEN_KEY);
  if (!token) return { ok: false, code: "unauthorized" };

  try {
    const res = await fetch(`${BASE}/api/users/me`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify(body),
    });

    if (res.status === 401)
      return { ok: false, code: "unauthorized", status: 401 };
    if (res.status === 409) return { ok: false, code: "conflict", status: 409 };
    if (!res.ok) return { ok: false, code: "failed", status: res.status };

    const data = await res.json();
    return { ok: true, data };
  } catch (e) {
    console.error("updateMe error", e);
    return { ok: false, code: "network" };
  }
}

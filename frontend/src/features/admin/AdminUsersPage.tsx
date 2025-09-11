import React, { useEffect, useState } from "react";
import { fetchAdminUsers, setUserActive } from "./usersApi";
import type { UserListItem } from "./usersApi";
// Optional: use Link instead of <a>
// import { Link } from "react-router-dom";

function AdminUsersPage() {
  const [users, setUsers] = useState<UserListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [busyId, setBusyId] = useState<string | null>(null);

  useEffect(() => {
    let active = true;
    const ctrl = new AbortController();

    async function load() {
      setLoading(true);
      setError(null);
      const res = await fetchAdminUsers({
        page: 1,
        pageSize: 10,
        signal: ctrl.signal,
      });
      if (!active) return;

      if (!res.ok) {
        setError(`Failed to load users: ${res.code}`);
        setUsers([]);
      } else {
        setUsers(res.items);
      }
      setLoading(false);
    }
    load();

    return () => {
      active = false;
      ctrl.abort();
    };
  }, []);

  async function onToggleActive(u: UserListItem) {
    const nextActive = !u.isActive;
    const verb = nextActive ? "activate" : "deactivate";
    if (
      !confirm(`Are you sure you want to ${verb} "${u.fullName ?? u.email}"?`)
    )
      return;

    try {
      setBusyId(u.id);

      // optimistic update
      setUsers((prev) =>
        prev.map((x) =>
          x.id === u.id
            ? {
                ...x,
                isActive: nextActive,
                accountStatus: nextActive ? "Active" : "Inactive",
              }
            : x
        )
      );

      const res = await setUserActive(u.id, nextActive);
      if (!res.ok) {
        // revert on failure
        setUsers((prev) =>
          prev.map((x) =>
            x.id === u.id
              ? {
                  ...x,
                  isActive: !nextActive,
                  accountStatus: !nextActive ? "Active" : "Inactive",
                }
              : x
          )
        );
        alert(
          res.code === "forbidden"
            ? "You are not allowed to do that."
            : res.code === "unauthorized"
            ? "Session expired. Please log in again."
            : res.code === "conflict"
            ? "Update conflict."
            : "Failed to update status."
        );
      }
    } finally {
      setBusyId(null);
    }
  }

  return (
    <section className="p-6">
      <h1 className="text-2xl font-bold mb-4">All Users</h1>

      {loading && <p>Loading...</p>}
      {error && <p className="text-red-600">{error}</p>}

      {!loading && !error && (
        <table className="w-full border-collapse border border-gray-300">
          <thead className="bg-gray-100">
            <tr>
              <th className="border border-gray-300 px-2 py-1 text-left">
                Full Name
              </th>
              <th className="border border-gray-300 px-2 py-1 text-left">
                Email
              </th>
              <th className="border border-gray-300 px-2 py-1 text-left">
                Roles
              </th>
              <th className="border border-gray-300 px-2 py-1 text-left">
                Status
              </th>
              <th className="border border-gray-300 px-2 py-1 text-left">
                Actions
              </th>
            </tr>
          </thead>
          <tbody>
            {users.map((u) => (
              <tr key={u.id}>
                <td className="border border-gray-300 px-2 py-1">
                  {u.fullName ?? "-"}
                </td>
                <td className="border border-gray-300 px-2 py-1">
                  {u.email ?? "-"}
                </td>
                <td className="border border-gray-300 px-2 py-1">
                  {u.roles.join(", ")}
                </td>
                <td className="border border-gray-300 px-2 py-1">
                  {u.isActive ? "Active" : "Inactive"}
                </td>
                <td className="border border-gray-300 px-2 py-1 space-x-3">
                  {/* Prefer Link to avoid full reload */}
                  {/* <Link className="text-blue-600 underline" to={`/admin/users/${u.id}`}>Edit</Link> */}
                  <a
                    className="text-blue-600 underline"
                    href={`/admin/users/${u.id}`}
                  >
                    Edit
                  </a>
                  <button
                    onClick={() => onToggleActive(u)}
                    disabled={busyId === u.id}
                    className={`px-2 py-1 rounded text-white ${
                      u.isActive
                        ? "bg-amber-600 hover:bg-amber-700"
                        : "bg-green-600 hover:bg-green-700"
                    } disabled:opacity-60`}
                    title={u.isActive ? "Deactivate user" : "Activate user"}
                  >
                    {busyId === u.id
                      ? "Saving..."
                      : u.isActive
                      ? "Deactivate"
                      : "Activate"}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  );
}

export default AdminUsersPage;

import React, { useEffect, useState } from "react";
import { fetchAdminUsers } from "./usersApi";
import type { UserListItem } from "./usersApi";

function AdminUsersPage() {
  const [users, setUsers] = useState<UserListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

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
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  );
}

export default AdminUsersPage;

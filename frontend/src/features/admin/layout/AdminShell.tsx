// src/features/admin/layout/AdminShell.tsx
import { useEffect, useState } from "react";
import { Outlet, Link } from "react-router-dom";
import AdminSidebar from "./AdminSidebar";

export default function AdminShell() {
  // persisted collapse state
  const [isCollapsed, setIsCollapsed] = useState<boolean>(() => {
    const saved = localStorage.getItem("admin_sidebar_collapsed");
    return saved === "true";
  });

  // mobile drawer state (independent from collapsed)
  const [drawerOpen, setDrawerOpen] = useState(false);

  useEffect(() => {
    localStorage.setItem("admin_sidebar_collapsed", String(isCollapsed));
  }, [isCollapsed]);

  return (
    <div className="min-h-screen flex bg-gradient-to-br from-blue-50 via-white to-teal-50">
      {/* Sidebar (desktop + mobile) */}
      <AdminSidebar
        isCollapsed={isCollapsed}
        onToggleCollapse={() => setIsCollapsed((v) => !v)}
        drawerOpen={drawerOpen}
        onCloseDrawer={() => setDrawerOpen(false)}
      />

      {/* Content */}
      <div className={`flex-1 ${isCollapsed ? "md:ml-20" : "md:ml-64"}`}>
        {/* Top bar */}
        <header className="bg-gradient-to-r from-blue-600 to-teal-600 text-white shadow px-4 md:px-6 py-3 flex items-center justify-between">
          <button
            className="md:hidden rounded bg-white/10 px-3 py-2"
            onClick={() => setDrawerOpen(true)}
            aria-label="Open menu"
          >
            ☰
          </button>

          <Link to="/admin" className="font-bold text-xl md:text-2xl">
            PetCare+
          </Link>

          <button
            className="px-3 py-1 bg-red-500 hover:bg-red-600 rounded-md text-sm"
            onClick={() => {
              localStorage.clear();
              window.location.assign("/");
            }}
          >
            Logout
          </button>
        </header>

        {/* Routed page content */}
        <main className="max-w-7xl mx-auto py-8 px-4">
          <Outlet />
        </main>
      </div>
    </div>
  );
}

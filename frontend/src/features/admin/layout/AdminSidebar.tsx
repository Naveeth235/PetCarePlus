// src/features/admin/layout/AdminSidebar.tsx
import { NavLink } from "react-router-dom";
import {
  FaChevronLeft,
  FaChevronRight,
  FaUserMd,
  FaUsers,
  FaHome,
  FaPaw,
  FaCalendarAlt,
  FaBoxes,
} from "react-icons/fa";

type Props = {
  isCollapsed: boolean;
  onToggleCollapse: () => void;
  drawerOpen: boolean;
  onCloseDrawer: () => void;
};

const menu = [
  { label: "Dashboard", to: "/admin", icon: <FaHome /> },
  { label: "Vet Management", to: "/admin/vets", icon: <FaUserMd /> },
  { label: "User Management", to: "/admin/users", icon: <FaUsers /> },
  { label: "Pets", to: "/admin/pets", icon: <FaPaw /> },
  { label: "Appointments", to: "/admin/appointments", icon: <FaCalendarAlt /> },
  { label: "Inventory", to: "/admin/inventory", icon: <FaBoxes /> },
];

export default function AdminSidebar({
  isCollapsed,
  onToggleCollapse,
  drawerOpen,
  onCloseDrawer,
}: Props) {
  // shared styles
  const base =
    "bg-gradient-to-b from-blue-600 to-teal-700 text-white shadow-lg z-40 p-3 md:p-4 h-full";
  const width = isCollapsed ? "w-20" : "w-64";

  const SidebarBody = (
    <aside className={`${base} ${width} flex flex-col`}>
      {/* Header + collapse toggle */}
      <div className="flex items-center justify-between mb-4">
        <div
          className={`font-bold ${
            isCollapsed ? "text-lg" : "text-2xl"
          } truncate`}
        >
          {isCollapsed ? "PC+" : "DashBoard"}
        </div>
        <button
          className="hidden md:flex items-center justify-center bg-white/10 rounded px-2 py-1"
          onClick={onToggleCollapse}
          aria-label={isCollapsed ? "Expand sidebar" : "Collapse sidebar"}
          aria-expanded={!isCollapsed}
          title={isCollapsed ? "Expand" : "Collapse"}
        >
          {isCollapsed ? <FaChevronRight /> : <FaChevronLeft />}
        </button>
      </div>

      {/* Nav */}
      <nav className="space-y-2 flex-1">
        {menu.map((m) => (
          <NavLink
            key={m.to}
            to={m.to}
            className={({ isActive }) =>
              [
                "flex items-center gap-3 px-3 py-2 rounded-lg bg-white/10 hover:bg-white/20 transition",
                isActive
                  ? "bg-yellow-400 text-blue-900 hover:bg-yellow-400"
                  : "",
                isCollapsed ? "justify-center" : "",
              ].join(" ")
            }
            onClick={onCloseDrawer}
          >
            <span className="text-lg">{m.icon}</span>
            {!isCollapsed && <span className="font-medium">{m.label}</span>}
          </NavLink>
        ))}
      </nav>
    </aside>
  );

  return (
    <>
      {/* Desktop fixed sidebar */}
      <div className="hidden md:block fixed inset-y-0 left-0">
        {SidebarBody}
      </div>

      {/* Mobile drawer */}
      {drawerOpen && (
        <div className="md:hidden fixed inset-0 z-50">
          <div
            className="absolute inset-0 bg-black/40"
            onClick={onCloseDrawer}
            aria-hidden="true"
          />
          <div className="absolute inset-y-0 left-0">{SidebarBody}</div>
        </div>
      )}
    </>
  );
}

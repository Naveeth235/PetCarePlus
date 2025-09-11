import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { fetchVets, type VetItem } from "./vetsApi";

const AdminDashboard: React.FC = () => {
  const [searchTerm, setSearchTerm] = useState("");
  const [vets, setVets] = useState<VetItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const stats = {
    totalVets: vets.length,
    totalPets: 0,
    pendingAppointments: 0,
    lowStockItems: 0,
    monthlyRevenue: 0,
    todayAppointments: 0,
  };

  // fetch vets
  useEffect(() => {
    let alive = true;
    (async () => {
      setLoading(true);
      setError(null);
      const res = await fetchVets({ page: 1, pageSize: 5, search: searchTerm });
      if (!alive) return;
      if (res.ok) {
        setVets(res.data.items);
      } else {
        setError(res.detail || "Failed to load vets.");
      }
      setLoading(false);
    })();
    return () => {
      alive = false;
    };
  }, [searchTerm]);

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case "active":
      case "approved":
      case "in stock":
        return "bg-green-100 text-green-700";
      case "pending":
      case "low stock":
        return "bg-yellow-100 text-yellow-700";
      case "inactive":
      case "cancelled":
      case "out of stock":
        return "bg-red-100 text-red-700";
      default:
        return "bg-gray-100 text-gray-700";
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 via-white to-teal-50">
      {/* Navbar */}
      <div className="bg-gradient-to-r from-blue-600 to-teal-600 text-white shadow px-6 py-4 flex justify-between items-center">
        <div className="font-bold text-2xl"> PetCare+</div>
        <div>
          <button
            className="px-3 py-1 bg-red-500 hover:bg-red-600 rounded-md text-sm"
            onClick={() => {
              localStorage.clear();
              window.location.assign("/");
            }}
          >
            Logout
          </button>
        </div>
      </div>

      {/* Dashboard Content */}
      <div className="max-w-7xl mx-auto py-8 px-4">
        <h2 className="text-3xl font-bold text-blue-800 mb-2">Admin Dashboard</h2>
        <p className="text-gray-600 mb-6">
          Manage your veterinary hospital operations with ease 🐶🐱
        </p>

        {/* Stats */}
        <div className="grid gap-6 md:grid-cols-3 lg:grid-cols-6 mb-8">
          {[
            { label: "Total Vets", value: stats.totalVets, border: "border-blue-500" },
            { label: "Total Pets", value: stats.totalPets, border: "border-teal-500" },
            { label: "Today's Appointments", value: stats.todayAppointments, border: "border-orange-500" },
            { label: "Pending Requests", value: stats.pendingAppointments, border: "border-yellow-500" },
            { label: "Low Stock Items", value: stats.lowStockItems, border: "border-purple-500" },
            { label: "Monthly Revenue", value: `$${stats.monthlyRevenue}`, border: "border-pink-500" },
          ].map((item, i) => (
            <div
              key={i}
              className={`bg-white rounded-xl shadow-md p-4 border-t-4 ${item.border}`}
            >
              <p className="text-sm text-gray-500">{item.label}</p>
              <p className="text-2xl font-bold text-blue-900">{item.value}</p>
            </div>
          ))}
        </div>

        {/* Vet Management */}
        <div className="bg-white rounded-xl shadow-md p-6 mb-8 border-l-4 border-blue-500">
          <div className="flex justify-between items-center mb-4">
            <div>
              <h3 className="font-semibold text-lg text-blue-700">Vet Management</h3>
              <p className="text-sm text-gray-500">Manage your registered veterinary doctors</p>
            </div>
            <div className="flex gap-3">
              <Link
                to="/admin/vets"
                className="px-4 py-2 bg-green-500 hover:opacity-90 text-white rounded-md text-sm"
              >
                View All Vets
              </Link>
              <Link
                to="/admin/vets/new"
                className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-md text-sm"
              >
                + Add Vet
              </Link>
            </div>
          </div>
          <input
            type="text"
            placeholder="Search vets..."
            className="w-full border rounded-md px-3 py-2 mb-4 focus:ring-2 focus:ring-blue-400"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />

          {loading && <p className="text-gray-500">Loading vets…</p>}
          {error && <p className="text-red-600">{error}</p>}
          {!loading && !error && vets.length === 0 && (
            <p className="text-gray-500">No vets found.</p>
          )}

          <div className="space-y-3">
            {vets.map((vet) => (
              <div
                key={vet.id}
                className="flex justify-between items-center border rounded-lg p-4 bg-gradient-to-r from-blue-50 to-teal-50"
              >
                <div>
                  <h4 className="font-semibold">{vet.fullName}</h4>
                  <p className="text-sm text-gray-500">{vet.email}</p>
                  <span
                    className={`inline-block text-xs px-2 py-1 rounded ${getStatusColor("active")}`}
                  >
                    Vet • Active
                  </span>
                </div>
                <div className="flex gap-2">
                  <button className="px-3 py-1 border rounded-md text-sm bg-yellow-300 hover:bg-yellow-600">
                    Edit
                  </button>
                  <button className="px-3 py-1 border rounded-md text-sm bg-red-500 hover:bg-red-600">
                    Remove
                  </button>
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Pets, Appointments, Inventory placeholders */}
      </div>
    </div>
  );
};

export default AdminDashboard;

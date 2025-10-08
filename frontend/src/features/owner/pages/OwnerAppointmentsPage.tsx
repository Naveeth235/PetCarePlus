// OwnerAppointmentsPage.tsx
// Purpose: Display all of owner's appointments with status filtering and visual indicators
// Features: Filter tabs (All/Pending/Approved/Cancelled), status badges, appointment cards with details
// Route: /owner/appointments - "As an owner, I want to track my appointment requests and their status"

import React, { useState, useEffect } from "react";
import { appointmentsApi } from "../../shared/api/appointmentsApi";
import type { Appointment } from "../../shared/types/appointment";

export const OwnerAppointmentsPage: React.FC = () => {
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filter, setFilter] = useState<
    "all" | "pending" | "approved" | "cancelled"
  >("all");

  useEffect(() => {
    loadMyAppointments();
  }, []);

  const loadMyAppointments = async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await appointmentsApi.getMy();
      setAppointments(data);
    } catch (error) {
      console.error("Failed to load appointments:", error);
      setError("Failed to load your appointments. Please try again.");
    } finally {
      setIsLoading(false);
    }
  };

  const filteredAppointments = appointments.filter((appointment) => {
    if (filter === "all") return true;
    return appointment.status.toLowerCase() === filter;
  });

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case "pending":
        return "bg-yellow-50 text-yellow-800 border-yellow-200";
      case "approved":
        return "bg-green-50 text-green-800 border-green-200";
      case "cancelled":
        return "bg-red-50 text-red-800 border-red-200";
      default:
        return "bg-gray-50 text-gray-800 border-gray-200";
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status.toLowerCase()) {
      case "pending":
        return "⏳";
      case "approved":
        return "✅";
      case "cancelled":
        return "❌";
      default:
        return "📅";
    }
  };

  if (isLoading) {
    return (
      <div className="flex h-64 items-center justify-center">
        <div className="text-lg text-slate-600 animate-pulse">
          Loading your appointments...
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-8">
      {/* Header */}
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-2">
        <div>
          <h1 className="text-3xl sm:text-4xl font-extrabold tracking-tight text-slate-800">
            My Appointments
          </h1>
          <p className="mt-1 text-slate-600 text-sm sm:text-base">
            Track your upcoming and past appointments
          </p>
        </div>
      </div>

      {/* Filter Tabs */}
      <div className="flex flex-wrap gap-2 bg-slate-50 rounded-xl p-2 shadow-sm">
        {[
          { key: "all", label: "All" },
          { key: "pending", label: "Pending" },
          { key: "approved", label: "Approved" },
          { key: "cancelled", label: "Cancelled" },
        ].map((tab) => (
          <button
            key={tab.key}
            onClick={() => setFilter(tab.key as any)}
            className={`px-4 py-2 rounded-2xl text-sm font-medium transition-colors ${
              filter === tab.key
                ? "bg-white text-slate-900 shadow"
                : "text-slate-600 hover:text-slate-900 hover:bg-white"
            }`}
          >
            {tab.label}
            {tab.key !== "all" && (
              <span className="ml-2 inline-flex items-center justify-center px-2 py-1 text-xs font-bold text-slate-700 bg-slate-200 rounded-full">
                {
                  appointments.filter((a) => a.status.toLowerCase() === tab.key)
                    .length
                }
              </span>
            )}
          </button>
        ))}
      </div>

      {/* Error State */}
      {error && (
        <div className="rounded-2xl bg-red-50 p-4 text-red-800 ring-1 ring-red-200">
          <p>{error}</p>
          <button
            onClick={loadMyAppointments}
            className="mt-2 inline-flex items-center text-sm font-medium text-red-700 underline hover:text-red-800"
          >
            Try again
          </button>
        </div>
      )}

      {/* Appointments List */}
      {filteredAppointments.length > 0 ? (
        <div className="space-y-6">
          {filteredAppointments.map((appointment) => (
            <div
              key={appointment.id}
              className="bg-white rounded-3xl shadow-sm p-6 border-l-4 border-indigo-600 hover:shadow-lg transition-shadow"
            >
              <div className="flex justify-between items-start">
                <div className="flex-1 space-y-3">
                  <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-2">
                    <div className="flex items-center gap-3">
                      <span className="text-2xl">
                        {getStatusIcon(appointment.status)}
                      </span>
                      <h3 className="text-lg font-semibold text-slate-800">
                        Appointment for {appointment.petName}
                      </h3>
                    </div>
                    <span
                      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border ${getStatusColor(
                        appointment.status
                      )}`}
                    >
                      {appointment.status}
                    </span>
                  </div>

                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm text-slate-600">
                    <div className="space-y-1">
                      <p>
                        <span className="font-medium">Date:</span>{" "}
                        {new Date(
                          appointment.requestedDateTime
                        ).toLocaleDateString()}
                      </p>
                      <p>
                        <span className="font-medium">Time:</span>{" "}
                        {new Date(
                          appointment.requestedDateTime
                        ).toLocaleTimeString()}
                      </p>
                      {appointment.vetName && (
                        <p>
                          <span className="font-medium">Veterinarian:</span>{" "}
                          {appointment.vetName}
                        </p>
                      )}
                    </div>
                    <div className="space-y-1">
                      {appointment.reasonForVisit && (
                        <p>
                          <span className="font-medium">Reason:</span>{" "}
                          {appointment.reasonForVisit}
                        </p>
                      )}
                      <p>
                        <span className="font-medium">Requested:</span>{" "}
                        {new Date(appointment.createdAt).toLocaleDateString()}
                      </p>
                    </div>
                  </div>

                  {appointment.adminNotes && (
                    <div className="mt-4 p-3 bg-slate-50 rounded-xl border border-slate-200">
                      <p className="text-sm text-slate-700">
                        <span className="font-medium">Admin Notes:</span>{" "}
                        {appointment.adminNotes}
                      </p>
                    </div>
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div className="rounded-2xl bg-white p-8 text-center ring-1 ring-slate-200">
          <div className="mb-3 text-6xl text-slate-400">📅</div>
          <p className="text-lg text-slate-600">
            {filter === "all"
              ? "You don't have any appointments yet."
              : `No ${filter} appointments found.`}
          </p>
          <p className="text-slate-500 mt-1">
            {filter === "all" &&
              "Request your first appointment to get started."}
          </p>
        </div>
      )}
    </div>
  );
};

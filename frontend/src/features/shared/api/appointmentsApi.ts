// appointmentsApi.ts
// Purpose: API interface for all appointment operations (CRUD) with proper error handling and authentication
// Features: Owner endpoints (create, getMy), Admin endpoints (getAll, getPending, updateStatus), Vet endpoints (getMyAssigned)
// Integration: Works with backend /api/appointments/* endpoints, includes TypeScript typing and token authentication

import { getToken } from '../../auth/token';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL as string;
const API_BASE = `${API_BASE_URL}/appointments`;

// Get auth token from localStorage
const getAuthHeaders = () => {
  const token = getToken();
  return {
    'Content-Type': 'application/json',
    ...(token && { 'Authorization': `Bearer ${token}` })
  };
};

// Generic API response handler
const handleResponse = async <T>(response: Response): Promise<T> => {
  if (!response.ok) {
    const errorData = await response.text();
    throw new Error(`HTTP ${response.status}: ${errorData}`);
  }
  
  if (response.status === 204) {
    return {} as T; // No content response
  }
  
  return response.json();
};

export interface CreateAppointmentRequest {
  petId: string;
  requestedDateTime: string;
  reasonForVisit: string;
  notes?: string;
}

export interface UpdateAppointmentStatusRequest {
  status: 'approved' | 'cancelled';
  adminNotes?: string;
  vetId?: string;
}

export const appointmentsApi = {
  // Sprint Feature: Owner endpoints for appointment management
  // Owner endpoints
  create: async (data: CreateAppointmentRequest) => {
    const response = await fetch(API_BASE, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify(data)
    });
    return handleResponse(response);
  },

  getMy: async () => {
    const response = await fetch(`${API_BASE}/my`, {
      headers: getAuthHeaders()
    });
    return handleResponse(response);
  },

  // Sprint Feature: Admin endpoints for appointment approval/rejection
  // Admin endpoints
  getAll: async () => {
    const response = await fetch(API_BASE, {
      headers: getAuthHeaders()
    });
    return handleResponse(response);
  },

  getPending: async () => {
    const response = await fetch(`${API_BASE}/pending`, {
      headers: getAuthHeaders()
    });
    return handleResponse(response);
  },

  // Sprint Key Feature: Update appointment status (approve/reject) with admin notes
  updateStatus: async (appointmentId: string, data: UpdateAppointmentStatusRequest) => {
    const response = await fetch(`${API_BASE}/${appointmentId}/status`, {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify(data)
    });
    return handleResponse(response);
  },

  // Future Enhancement: Vet endpoints for assigned appointments
  // Vet endpoints
  getMyAssigned: async () => {
    const response = await fetch(`${API_BASE}/assigned`, {
      headers: getAuthHeaders()
    });
    return handleResponse(response);
  }
};

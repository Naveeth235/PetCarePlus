// appointment.ts
// Purpose: TypeScript type definitions for appointment data structures
// Features: Complete appointment interface, summary views, request/update DTOs
// Benefits: Type safety, IntelliSense support, compile-time error checking for appointment operations

export interface Appointment {
  id: string;
  petId: string;
  petName: string;
  ownerId: string;
  ownerName: string;
  vetId?: string;
  vetName?: string;
  requestedDateTime: string;
  actualDateTime?: string;
  reasonForVisit: string;
  notes?: string;
  adminNotes?: string;
  status: 'pending' | 'approved' | 'cancelled' | 'completed';
  createdAt: string;
  updatedAt?: string;
}

export interface AppointmentSummary {
  id: string;
  petName: string;
  ownerName: string;
  requestedDateTime: string;
  status: string;
  reasonForVisit: string;
}

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

import React, { useState } from 'react';
import { Save, X } from 'lucide-react';
import { medicalRecordsApi, vaccinationsApi, treatmentsApi, prescriptionsApi } from '../shared/api/medicalRecordsApi';
import type { 
  CreateMedicalRecordRequest, 
  CreateVaccinationRequest, 
  CreateTreatmentRequest, 
  CreatePrescriptionRequest 
} from '../shared/types/medicalRecords';
import { MedicalRecordType, MEDICAL_RECORD_TYPE_LABELS } from '../shared/types/medicalRecords';

interface AddMedicalRecordFormProps {
  petId: string;
  petName: string;
  onSuccess: () => void;
  onCancel: () => void;
}

type RecordType = 'general' | 'vaccination' | 'treatment' | 'prescription';

const AddMedicalRecordForm: React.FC<AddMedicalRecordFormProps> = ({
  petId,
  petName,
  onSuccess,
  onCancel
}) => {
  const [recordType, setRecordType] = useState<RecordType>('general');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // General medical record form
  const [generalForm, setGeneralForm] = useState<CreateMedicalRecordRequest>({
    petId,
    recordType: MedicalRecordType.Other,
    recordDate: new Date().toISOString().split('T')[0],
    title: '',
    description: '',
    notes: ''
  });

  // Vaccination form
  const [vaccinationForm, setVaccinationForm] = useState<CreateVaccinationRequest>({
    petId,
    vaccineName: '',
    vaccinationDate: new Date().toISOString().split('T')[0],
    nextDueDate: '',
    batchNumber: '',
    manufacturer: '',
    notes: ''
  });

  // Treatment form
  const [treatmentForm, setTreatmentForm] = useState<CreateTreatmentRequest>({
    petId,
    treatmentType: '',
    diagnosis: '',
    treatmentDescription: '',
    treatmentDate: new Date().toISOString().split('T')[0],
    followUpDate: '',
    medications: '',
    instructions: '',
    notes: ''
  });

  // Prescription form
  const [prescriptionForm, setPrescriptionForm] = useState<CreatePrescriptionRequest>({
    petId,
    medicationName: '',
    dosage: '',
    frequency: '',
    prescribedDate: new Date().toISOString().split('T')[0],
    startDate: '',
    endDate: '',
    durationDays: undefined,
    instructions: '',
    notes: ''
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      let result;
      
      switch (recordType) {
        case 'general':
          result = await medicalRecordsApi.createMedicalRecord(generalForm);
          break;
        case 'vaccination':
          result = await vaccinationsApi.createVaccination(vaccinationForm);
          break;
        case 'treatment':
          result = await treatmentsApi.createTreatment(treatmentForm);
          break;
        case 'prescription':
          result = await prescriptionsApi.createPrescription(prescriptionForm);
          break;
        default:
          throw new Error('Invalid record type');
      }

      if (!result.ok) {
        throw new Error(result.message);
      }

      onSuccess();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create medical record');
    } finally {
      setLoading(false);
    }
  };

  const renderGeneralForm = () => (
    <div className="space-y-4">
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Record Type
        </label>
        <select
          value={generalForm.recordType}
          onChange={(e) => setGeneralForm(prev => ({ 
            ...prev, 
            recordType: Number(e.target.value) as MedicalRecordType 
          }))}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          required
        >
          {Object.entries(MEDICAL_RECORD_TYPE_LABELS).map(([value, label]) => (
            <option key={value} value={value}>{label}</option>
          ))}
        </select>
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Title *
        </label>
        <input
          type="text"
          value={generalForm.title}
          onChange={(e) => setGeneralForm(prev => ({ ...prev, title: e.target.value }))}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          required
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Record Date *
        </label>
        <input
          type="date"
          value={generalForm.recordDate}
          onChange={(e) => setGeneralForm(prev => ({ ...prev, recordDate: e.target.value }))}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          required
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Description
        </label>
        <textarea
          value={generalForm.description}
          onChange={(e) => setGeneralForm(prev => ({ ...prev, description: e.target.value }))}
          rows={3}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Notes
        </label>
        <textarea
          value={generalForm.notes}
          onChange={(e) => setGeneralForm(prev => ({ ...prev, notes: e.target.value }))}
          rows={2}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>
    </div>
  );

  const renderVaccinationForm = () => (
    <div className="space-y-4">
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Vaccine Name *
        </label>
        <input
          type="text"
          value={vaccinationForm.vaccineName}
          onChange={(e) => setVaccinationForm(prev => ({ ...prev, vaccineName: e.target.value }))}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          required
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Vaccination Date *
          </label>
          <input
            type="date"
            value={vaccinationForm.vaccinationDate}
            onChange={(e) => setVaccinationForm(prev => ({ ...prev, vaccinationDate: e.target.value }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            required
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Next Due Date
          </label>
          <input
            type="date"
            value={vaccinationForm.nextDueDate}
            onChange={(e) => setVaccinationForm(prev => ({ ...prev, nextDueDate: e.target.value }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Batch Number
          </label>
          <input
            type="text"
            value={vaccinationForm.batchNumber}
            onChange={(e) => setVaccinationForm(prev => ({ ...prev, batchNumber: e.target.value }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Manufacturer
          </label>
          <input
            type="text"
            value={vaccinationForm.manufacturer}
            onChange={(e) => setVaccinationForm(prev => ({ ...prev, manufacturer: e.target.value }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Notes
        </label>
        <textarea
          value={vaccinationForm.notes}
          onChange={(e) => setVaccinationForm(prev => ({ ...prev, notes: e.target.value }))}
          rows={2}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>
    </div>
  );

  const renderTreatmentForm = () => (
    <div className="space-y-4">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Treatment Type *
          </label>
          <input
            type="text"
            value={treatmentForm.treatmentType}
            onChange={(e) => setTreatmentForm(prev => ({ ...prev, treatmentType: e.target.value }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            required
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Treatment Date *
          </label>
          <input
            type="date"
            value={treatmentForm.treatmentDate}
            onChange={(e) => setTreatmentForm(prev => ({ ...prev, treatmentDate: e.target.value }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            required
          />
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Diagnosis *
        </label>
        <textarea
          value={treatmentForm.diagnosis}
          onChange={(e) => setTreatmentForm(prev => ({ ...prev, diagnosis: e.target.value }))}
          rows={2}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          required
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Treatment Description
        </label>
        <textarea
          value={treatmentForm.treatmentDescription}
          onChange={(e) => setTreatmentForm(prev => ({ ...prev, treatmentDescription: e.target.value }))}
          rows={3}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Medications
          </label>
          <textarea
            value={treatmentForm.medications}
            onChange={(e) => setTreatmentForm(prev => ({ ...prev, medications: e.target.value }))}
            rows={2}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Follow-up Date
          </label>
          <input
            type="date"
            value={treatmentForm.followUpDate}
            onChange={(e) => setTreatmentForm(prev => ({ ...prev, followUpDate: e.target.value }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Instructions
        </label>
        <textarea
          value={treatmentForm.instructions}
          onChange={(e) => setTreatmentForm(prev => ({ ...prev, instructions: e.target.value }))}
          rows={2}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Notes
        </label>
        <textarea
          value={treatmentForm.notes}
          onChange={(e) => setTreatmentForm(prev => ({ ...prev, notes: e.target.value }))}
          rows={2}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>
    </div>
  );

  const renderPrescriptionForm = () => (
    <div className="space-y-4">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Medication Name *
          </label>
          <input
            type="text"
            value={prescriptionForm.medicationName}
            onChange={(e) => setPrescriptionForm(prev => ({ ...prev, medicationName: e.target.value }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            required
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Dosage *
          </label>
          <input
            type="text"
            value={prescriptionForm.dosage}
            onChange={(e) => setPrescriptionForm(prev => ({ ...prev, dosage: e.target.value }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="e.g., 10mg"
            required
          />
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Frequency *
          </label>
          <input
            type="text"
            value={prescriptionForm.frequency}
            onChange={(e) => setPrescriptionForm(prev => ({ ...prev, frequency: e.target.value }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            placeholder="e.g., Twice daily"
            required
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Prescribed Date *
          </label>
          <input
            type="date"
            value={prescriptionForm.prescribedDate}
            onChange={(e) => setPrescriptionForm(prev => ({ ...prev, prescribedDate: e.target.value }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            required
          />
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Start Date
          </label>
          <input
            type="date"
            value={prescriptionForm.startDate}
            onChange={(e) => setPrescriptionForm(prev => ({ ...prev, startDate: e.target.value }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            End Date
          </label>
          <input
            type="date"
            value={prescriptionForm.endDate}
            onChange={(e) => setPrescriptionForm(prev => ({ ...prev, endDate: e.target.value }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Duration (Days)
          </label>
          <input
            type="number"
            value={prescriptionForm.durationDays || ''}
            onChange={(e) => setPrescriptionForm(prev => ({ 
              ...prev, 
              durationDays: e.target.value ? Number(e.target.value) : undefined 
            }))}
            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            min="1"
          />
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Instructions
        </label>
        <textarea
          value={prescriptionForm.instructions}
          onChange={(e) => setPrescriptionForm(prev => ({ ...prev, instructions: e.target.value }))}
          rows={2}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="e.g., Take with food"
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Notes
        </label>
        <textarea
          value={prescriptionForm.notes}
          onChange={(e) => setPrescriptionForm(prev => ({ ...prev, notes: e.target.value }))}
          rows={2}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>
    </div>
  );

  return (
    <div className="bg-white border rounded-lg p-6">
      <div className="mb-6">
        <h2 className="text-xl font-semibold text-gray-800">
          Add Medical Record - {petName}
        </h2>
        <p className="text-gray-600 mt-1">Create a new medical record for this pet</p>
      </div>

      {error && (
        <div className="mb-4 bg-red-50 border border-red-200 rounded p-3">
          <p className="text-red-700 text-sm">{error}</p>
        </div>
      )}

      <form onSubmit={handleSubmit}>
        {/* Record Type Selection */}
        <div className="mb-6">
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Record Type
          </label>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-2">
            <button
              type="button"
              onClick={() => setRecordType('general')}
              className={`p-3 text-sm rounded border ${
                recordType === 'general'
                  ? 'bg-blue-50 border-blue-200 text-blue-700'
                  : 'bg-gray-50 border-gray-200 text-gray-700 hover:bg-gray-100'
              }`}
            >
              General Record
            </button>
            <button
              type="button"
              onClick={() => setRecordType('vaccination')}
              className={`p-3 text-sm rounded border ${
                recordType === 'vaccination'
                  ? 'bg-blue-50 border-blue-200 text-blue-700'
                  : 'bg-gray-50 border-gray-200 text-gray-700 hover:bg-gray-100'
              }`}
            >
              Vaccination
            </button>
            <button
              type="button"
              onClick={() => setRecordType('treatment')}
              className={`p-3 text-sm rounded border ${
                recordType === 'treatment'
                  ? 'bg-blue-50 border-blue-200 text-blue-700'
                  : 'bg-gray-50 border-gray-200 text-gray-700 hover:bg-gray-100'
              }`}
            >
              Treatment
            </button>
            <button
              type="button"
              onClick={() => setRecordType('prescription')}
              className={`p-3 text-sm rounded border ${
                recordType === 'prescription'
                  ? 'bg-blue-50 border-blue-200 text-blue-700'
                  : 'bg-gray-50 border-gray-200 text-gray-700 hover:bg-gray-100'
              }`}
            >
              Prescription
            </button>
          </div>
        </div>

        {/* Form Fields */}
        {recordType === 'general' && renderGeneralForm()}
        {recordType === 'vaccination' && renderVaccinationForm()}
        {recordType === 'treatment' && renderTreatmentForm()}
        {recordType === 'prescription' && renderPrescriptionForm()}

        {/* Submit Buttons */}
        <div className="flex justify-end space-x-4 mt-6 pt-4 border-t">
          <button
            type="button"
            onClick={onCancel}
            className="px-4 py-2 text-gray-700 bg-gray-100 rounded hover:bg-gray-200 transition-colors flex items-center"
          >
            <X className="w-4 h-4 mr-2" />
            Cancel
          </button>
          <button
            type="submit"
            disabled={loading}
            className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 transition-colors flex items-center disabled:opacity-50"
          >
            <Save className="w-4 h-4 mr-2" />
            {loading ? 'Saving...' : 'Save Record'}
          </button>
        </div>
      </form>
    </div>
  );
};

export default AddMedicalRecordForm;
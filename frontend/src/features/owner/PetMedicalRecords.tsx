import React, { useState, useEffect } from 'react';
import { FileText, Pill, Stethoscope, AlertCircle, CheckCircle } from 'lucide-react';
import { medicalRecordsApi, vaccinationsApi, treatmentsApi } from '../shared/api/medicalRecordsApi';
import { petsApi } from '../shared/api/petsApi';
import MedicalRecordsPDFExport from '../shared/MedicalRecordsPDFExport';
import type { MedicalRecord, VaccinationReport, TreatmentHistoryReport } from '../shared/types/medicalRecords';
import type { Pet } from '../shared/types/pet';
import { MEDICAL_RECORD_TYPE_LABELS, VACCINATION_STATUS_LABELS } from '../shared/types/medicalRecords';

interface PetMedicalRecordsProps {
  petId: string;
  petName: string;
  isOwner?: boolean;
}

const PetMedicalRecords: React.FC<PetMedicalRecordsProps> = ({ 
  petId, 
  petName, 
  isOwner = true 
}) => {
  const [activeTab, setActiveTab] = useState('overview');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [petData, setPetData] = useState<Pet | null>(null);
  
  // State for medical records data
  const [medicalRecords, setMedicalRecords] = useState<MedicalRecord[]>([]);
  const [vaccinationReport, setVaccinationReport] = useState<VaccinationReport | null>(null);
  const [treatmentReport, setTreatmentReport] = useState<TreatmentHistoryReport | null>(null);

  useEffect(() => {
    fetchMedicalRecords();
    fetchPetData();
  }, [petId]);

  const fetchPetData = async () => {
    try {
      const pet = await petsApi.getById(petId);
      setPetData(pet);
    } catch (err) {
      console.error('Failed to fetch pet data:', err);
    }
  };

  const fetchMedicalRecords = async () => {
    setLoading(true);
    setError(null);

    try {
      // Fetch all medical records data
      const [recordsResult, vaccinationResult, treatmentResult] = await Promise.all([
        medicalRecordsApi.getMedicalRecordsByPet(petId),
        vaccinationsApi.getVaccinationReport(petId),
        treatmentsApi.getTreatmentHistoryReport(petId)
      ]);

      if (!recordsResult.ok) {
        throw new Error(recordsResult.message);
      }
      if (!vaccinationResult.ok) {
        throw new Error(vaccinationResult.message);
      }
      if (!treatmentResult.ok) {
        throw new Error(treatmentResult.message);
      }

      setMedicalRecords(recordsResult.data);
      setVaccinationReport(vaccinationResult.data);
      setTreatmentReport(treatmentResult.data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch medical records');
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center p-8">
        <div className="text-gray-600">Loading medical records...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-red-50 border border-red-200 rounded-lg p-4">
        <div className="flex items-center text-red-700">
          <AlertCircle className="w-5 h-5 mr-2" />
          {error}
        </div>
      </div>
    );
  }

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString();
  };

  const renderOverview = () => (
    <div className="space-y-6">
      {/* Vaccination Status */}
      <div className="bg-white border rounded-lg p-4">
        <div className="flex items-center justify-between mb-3">
          <h3 className="text-lg font-semibold flex items-center">
            <Pill className="w-5 h-5 mr-2 text-blue-600" />
            Vaccination Status
          </h3>
          {vaccinationReport?.isUpToDate ? (
            <CheckCircle className="w-5 h-5 text-green-600" />
          ) : (
            <AlertCircle className="w-5 h-5 text-red-600" />
          )}
        </div>
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div className="text-center p-3 bg-green-50 rounded">
            <div className="text-2xl font-bold text-green-600">
              {vaccinationReport?.vaccinations.length || 0}
            </div>
            <div className="text-sm text-green-700">Total Vaccinations</div>
          </div>
          <div className="text-center p-3 bg-red-50 rounded">
            <div className="text-2xl font-bold text-red-600">
              {vaccinationReport?.overdueVaccinations.length || 0}
            </div>
            <div className="text-sm text-red-700">Overdue</div>
          </div>
          <div className="text-center p-3 bg-yellow-50 rounded">
            <div className="text-2xl font-bold text-yellow-600">
              {vaccinationReport?.upcomingVaccinations.length || 0}
            </div>
            <div className="text-sm text-yellow-700">Upcoming</div>
          </div>
        </div>
      </div>

      {/* Treatment Summary */}
      <div className="bg-white border rounded-lg p-4">
        <h3 className="text-lg font-semibold flex items-center mb-3">
          <Stethoscope className="w-5 h-5 mr-2 text-purple-600" />
          Treatment History
        </h3>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="text-center p-3 bg-purple-50 rounded">
            <div className="text-2xl font-bold text-purple-600">
              {treatmentReport?.totalTreatments || 0}
            </div>
            <div className="text-sm text-purple-700">Total Treatments</div>
          </div>
          <div className="text-center p-3 bg-blue-50 rounded">
            <div className="text-lg font-semibold text-blue-600">
              {treatmentReport?.lastTreatmentDate ? 
                formatDate(treatmentReport.lastTreatmentDate) : 'None'}
            </div>
            <div className="text-sm text-blue-700">Last Treatment</div>
          </div>
        </div>
      </div>

      {/* Recent Records */}
      <div className="bg-white border rounded-lg p-4">
        <h3 className="text-lg font-semibold flex items-center mb-3">
          <FileText className="w-5 h-5 mr-2 text-gray-600" />
          Recent Medical Records
        </h3>
        {medicalRecords.length === 0 ? (
          <p className="text-gray-500 text-center py-4">No medical records found</p>
        ) : (
          <div className="space-y-3">
            {medicalRecords.slice(0, 5).map((record) => (
              <div key={record.id} className="border-l-4 border-blue-300 bg-gray-50 p-3 rounded-r">
                <div className="flex justify-between items-start">
                  <div>
                    <h4 className="font-medium">{record.title}</h4>
                    <p className="text-sm text-gray-600">
                      {MEDICAL_RECORD_TYPE_LABELS[record.recordType]} • {formatDate(record.recordDate)}
                    </p>
                    {record.description && (
                      <p className="text-sm text-gray-700 mt-1">{record.description}</p>
                    )}
                  </div>
                  <span className="text-xs text-gray-500">
                    Dr. {record.vetFullName || 'Unknown'}
                  </span>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );

  const renderVaccinations = () => (
    <div className="space-y-4">
      {vaccinationReport?.overdueVaccinations && vaccinationReport.overdueVaccinations.length > 0 && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4">
          <h3 className="text-lg font-semibold text-red-700 mb-3">Overdue Vaccinations</h3>
          <div className="space-y-2">
            {vaccinationReport.overdueVaccinations.map((vaccination) => (
              <div key={vaccination.id} className="bg-white p-3 rounded border-l-4 border-red-400">
                <div className="flex justify-between items-start">
                  <div>
                    <h4 className="font-medium text-red-700">{vaccination.vaccineName}</h4>
                    <p className="text-sm text-gray-600">
                      Given: {formatDate(vaccination.vaccinationDate)}
                      {vaccination.nextDueDate && ` • Due: ${formatDate(vaccination.nextDueDate)}`}
                    </p>
                  </div>
                  <span className="text-xs bg-red-100 text-red-700 px-2 py-1 rounded">
                    {VACCINATION_STATUS_LABELS[vaccination.status]}
                  </span>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      <div className="bg-white border rounded-lg p-4">
        <h3 className="text-lg font-semibold mb-3">All Vaccinations</h3>
        {vaccinationReport?.vaccinations && vaccinationReport.vaccinations.length > 0 ? (
          <div className="space-y-3">
            {vaccinationReport.vaccinations.map((vaccination) => (
              <div key={vaccination.id} className="border rounded p-3">
                <div className="flex justify-between items-start">
                  <div className="flex-1">
                    <h4 className="font-medium">{vaccination.vaccineName}</h4>
                    <p className="text-sm text-gray-600">
                      Given: {formatDate(vaccination.vaccinationDate)}
                      {vaccination.nextDueDate && ` • Next due: ${formatDate(vaccination.nextDueDate)}`}
                    </p>
                    {vaccination.manufacturer && (
                      <p className="text-sm text-gray-500">Manufacturer: {vaccination.manufacturer}</p>
                    )}
                    {vaccination.batchNumber && (
                      <p className="text-sm text-gray-500">Batch: {vaccination.batchNumber}</p>
                    )}
                    {vaccination.notes && (
                      <p className="text-sm text-gray-700 mt-1">{vaccination.notes}</p>
                    )}
                  </div>
                  <div className="text-right">
                    <span className={`text-xs px-2 py-1 rounded ${
                      vaccination.status === 1 ? 'bg-green-100 text-green-700' :
                      vaccination.status === 2 ? 'bg-red-100 text-red-700' :
                      vaccination.status === 3 ? 'bg-yellow-100 text-yellow-700' :
                      'bg-gray-100 text-gray-700'
                    }`}>
                      {VACCINATION_STATUS_LABELS[vaccination.status]}
                    </span>
                    <p className="text-xs text-gray-500 mt-1">
                      Dr. {vaccination.vetFullName || 'Unknown'}
                    </p>
                  </div>
                </div>
              </div>
            ))}
          </div>
        ) : (
          <p className="text-gray-500 text-center py-4">No vaccination records found</p>
        )}
      </div>
    </div>
  );

  const renderTreatments = () => (
    <div className="bg-white border rounded-lg p-4">
      <h3 className="text-lg font-semibold mb-3">Treatment History</h3>
      {treatmentReport?.treatments && treatmentReport.treatments.length > 0 ? (
        <div className="space-y-4">
          {treatmentReport.treatments.map((treatment) => (
            <div key={treatment.id} className="border rounded p-4">
              <div className="flex justify-between items-start mb-2">
                <div className="flex-1">
                  <h4 className="font-medium">{treatment.treatmentType}</h4>
                  <p className="text-sm text-gray-600">
                    {formatDate(treatment.treatmentDate)} • Dr. {treatment.vetFullName || 'Unknown'}
                  </p>
                </div>
                <span className={`text-xs px-2 py-1 rounded ${
                  treatment.status === 3 ? 'bg-green-100 text-green-700' :
                  treatment.status === 2 ? 'bg-blue-100 text-blue-700' :
                  treatment.status === 1 ? 'bg-yellow-100 text-yellow-700' :
                  'bg-gray-100 text-gray-700'
                }`}>
                  {treatment.status === 1 ? 'Planned' :
                   treatment.status === 2 ? 'In Progress' :
                   treatment.status === 3 ? 'Completed' : 'Cancelled'}
                </span>
              </div>
              
              <div className="space-y-2">
                <div>
                  <span className="text-sm font-medium text-gray-700">Diagnosis: </span>
                  <span className="text-sm text-gray-600">{treatment.diagnosis}</span>
                </div>
                
                {treatment.treatmentDescription && (
                  <div>
                    <span className="text-sm font-medium text-gray-700">Treatment: </span>
                    <span className="text-sm text-gray-600">{treatment.treatmentDescription}</span>
                  </div>
                )}
                
                {treatment.medications && (
                  <div>
                    <span className="text-sm font-medium text-gray-700">Medications: </span>
                    <span className="text-sm text-gray-600">{treatment.medications}</span>
                  </div>
                )}
                
                {treatment.instructions && (
                  <div>
                    <span className="text-sm font-medium text-gray-700">Instructions: </span>
                    <span className="text-sm text-gray-600">{treatment.instructions}</span>
                  </div>
                )}
                
                {treatment.followUpDate && (
                  <div>
                    <span className="text-sm font-medium text-gray-700">Follow-up: </span>
                    <span className="text-sm text-gray-600">{formatDate(treatment.followUpDate)}</span>
                  </div>
                )}
                
                {treatment.notes && (
                  <div>
                    <span className="text-sm font-medium text-gray-700">Notes: </span>
                    <span className="text-sm text-gray-600">{treatment.notes}</span>
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>
      ) : (
        <p className="text-gray-500 text-center py-4">No treatment records found</p>
      )}
    </div>
  );

  const renderReports = () => (
    <div className="space-y-6">
      <div className="bg-gray-50 p-4 rounded-lg">
        <h3 className="text-lg font-semibold text-gray-800 mb-2">Medical Reports</h3>
        <p className="text-gray-600 mb-4">
          Generate and download comprehensive medical reports for {petName}.
        </p>
      </div>
      
      {petData && (
        <MedicalRecordsPDFExport
          petId={petId}
          petName={petName}
          petSpecies={petData.species}
          ownerName={petData.ownerFullName}
        />
      )}
    </div>
  );

  return (
    <div className="max-w-4xl mx-auto">
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-800">
          Medical Records - {petName}
        </h1>
        <p className="text-gray-600 mt-1">
          {isOwner ? 'View your pet\'s medical history' : 'Medical history overview'}
        </p>
      </div>

      {/* Tab Navigation */}
      <div className="border-b border-gray-200 mb-6">
        <nav className="-mb-px flex space-x-8">
          <button
            onClick={() => setActiveTab('overview')}
            className={`py-2 px-1 border-b-2 font-medium text-sm ${
              activeTab === 'overview'
                ? 'border-blue-500 text-blue-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            Overview
          </button>
          <button
            onClick={() => setActiveTab('vaccinations')}
            className={`py-2 px-1 border-b-2 font-medium text-sm ${
              activeTab === 'vaccinations'
                ? 'border-blue-500 text-blue-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            Vaccinations
          </button>
          <button
            onClick={() => setActiveTab('treatments')}
            className={`py-2 px-1 border-b-2 font-medium text-sm ${
              activeTab === 'treatments'
                ? 'border-blue-500 text-blue-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            Treatments
          </button>
          <button
            onClick={() => setActiveTab('reports')}
            className={`py-2 px-1 border-b-2 font-medium text-sm ${
              activeTab === 'reports'
                ? 'border-blue-500 text-blue-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            Reports
          </button>
        </nav>
      </div>

      {/* Tab Content */}
      {activeTab === 'overview' && renderOverview()}
      {activeTab === 'vaccinations' && renderVaccinations()}
      {activeTab === 'treatments' && renderTreatments()}
      {activeTab === 'reports' && renderReports()}
    </div>
  );
};

export default PetMedicalRecords;
import React, { useState, useEffect } from 'react';
import { Plus, Search, Filter } from 'lucide-react';
import { adminPetsApi } from '../shared/api/petsApi';
import PetMedicalRecords from '../owner/PetMedicalRecords';
import AddMedicalRecordForm from './AddMedicalRecordForm';
import type { Pet } from '../shared/types/pet';
import { getSpeciesLabel } from '../shared/types/pet';

const VetMedicalRecords: React.FC = () => {
  const [pets, setPets] = useState<Pet[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedPet, setSelectedPet] = useState<Pet | null>(null);
  const [showAddForm, setShowAddForm] = useState(false);
  const [showRecords, setShowRecords] = useState(false);

  useEffect(() => {
    loadPets();
  }, []);

  const loadPets = async () => {
    try {
      setLoading(true);
      const petsData = await adminPetsApi.getAll();
      
      // Convert PetSummary to Pet objects by fetching detailed data
      const detailedPets = await Promise.all(
        petsData.map(async (petSummary) => {
          try {
            return await adminPetsApi.getById(petSummary.id);
          } catch (err) {
            console.error(`Failed to load details for pet ${petSummary.id}:`, err);
            // Return a basic Pet object with available data
            return {
              id: petSummary.id,
              name: petSummary.name,
              species: petSummary.species,
              breed: petSummary.breed,
              isActive: petSummary.isActive,
              ownerUserId: '',
              ownerFullName: petSummary.ownerFullName,
              createdAt: new Date().toISOString(),
              ageInYears: petSummary.ageInYears
            } as Pet;
          }
        })
      );
      
      setPets(detailedPets);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load pets');
    } finally {
      setLoading(false);
    }
  };

  const handleAddRecord = (pet: Pet) => {
    setSelectedPet(pet);
    setShowAddForm(true);
    setShowRecords(false);
  };

  const handleViewRecords = (pet: Pet) => {
    setSelectedPet(pet);
    setShowRecords(true);
    setShowAddForm(false);
  };

  const handleFormSuccess = () => {
    setShowAddForm(false);
    setSelectedPet(null);
    // Optionally refresh the records view
  };

  const handleFormCancel = () => {
    setShowAddForm(false);
    setSelectedPet(null);
  };

  const handleBackToList = () => {
    setShowRecords(false);
    setSelectedPet(null);
  };

  const filteredPets = pets.filter(pet =>
    pet.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    pet.ownerFullName?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    getSpeciesLabel(pet.species).toLowerCase().includes(searchTerm.toLowerCase())
  );

  if (loading) {
    return (
      <div className="flex justify-center items-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-4 border-blue-500 border-t-transparent"></div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-red-50 border border-red-200 rounded p-4">
        <p className="text-red-700">{error}</p>
        <button
          onClick={loadPets}
          className="mt-2 text-red-600 hover:text-red-800 underline"
        >
          Try again
        </button>
      </div>
    );
  }

  // Show add record form
  if (showAddForm && selectedPet) {
    return (
      <div className="space-y-6">
        <button
          onClick={handleFormCancel}
          className="text-blue-600 hover:text-blue-800 flex items-center"
        >
          ← Back to pets list
        </button>
        <AddMedicalRecordForm
          petId={selectedPet.id}
          petName={selectedPet.name}
          onSuccess={handleFormSuccess}
          onCancel={handleFormCancel}
        />
      </div>
    );
  }

  // Show medical records
  if (showRecords && selectedPet) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <button
            onClick={handleBackToList}
            className="text-blue-600 hover:text-blue-800 flex items-center"
          >
            ← Back to pets list
          </button>
          <button
            onClick={() => handleAddRecord(selectedPet)}
            className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 transition-colors flex items-center"
          >
            <Plus className="w-4 h-4 mr-2" />
            Add Record
          </button>
        </div>
        <PetMedicalRecords petId={selectedPet.id} petName={selectedPet.name} />
      </div>
    );
  }

  // Show pets list
  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-800">Medical Records Management</h1>
          <p className="text-gray-600">Manage medical records for all pets</p>
        </div>
      </div>

      {/* Search and Filters */}
      <div className="bg-white border rounded-lg p-4">
        <div className="flex flex-col sm:flex-row gap-4">
          <div className="flex-1 relative">
            <Search className="absolute left-3 top-3 h-4 w-4 text-gray-400" />
            <input
              type="text"
              placeholder="Search pets by name, owner, or species..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <button className="flex items-center px-4 py-2 text-gray-700 bg-gray-100 rounded hover:bg-gray-200 transition-colors">
            <Filter className="w-4 h-4 mr-2" />
            Filters
          </button>
        </div>
      </div>

      {/* Pets Grid */}
      {filteredPets.length === 0 ? (
        <div className="text-center py-12">
          <p className="text-gray-500">
            {searchTerm ? 'No pets found matching your search.' : 'No pets available.'}
          </p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {filteredPets.map((pet) => (
            <div key={pet.id} className="bg-white border rounded-lg p-6 hover:shadow-md transition-shadow">
              <div className="mb-4">
                <h3 className="text-lg font-semibold text-gray-800">{pet.name}</h3>
                <p className="text-gray-600">{getSpeciesLabel(pet.species)} • {pet.breed || 'Mixed breed'}</p>
                {pet.ownerFullName && (
                  <p className="text-sm text-gray-500">Owner: {pet.ownerFullName}</p>
                )}
              </div>

              <div className="space-y-2">
                <div className="grid grid-cols-2 gap-2 text-sm">
                  <div>
                    <span className="text-gray-500">Age:</span>
                    <span className="ml-1">{pet.ageInYears || 'Unknown'} years</span>
                  </div>
                  <div>
                    <span className="text-gray-500">Species:</span>
                    <span className="ml-1">{getSpeciesLabel(pet.species)}</span>
                  </div>
                  <div>
                    <span className="text-gray-500">Weight:</span>
                    <span className="ml-1">{pet.weight ? `${pet.weight} kg` : 'Not recorded'}</span>
                  </div>
                  <div>
                    <span className="text-gray-500">Color:</span>
                    <span className="ml-1">{pet.color || 'Not recorded'}</span>
                  </div>
                </div>
              </div>

              <div className="mt-4 pt-4 border-t flex gap-2">
                <button
                  onClick={() => handleViewRecords(pet)}
                  className="flex-1 px-3 py-2 text-blue-600 border border-blue-600 rounded hover:bg-blue-50 transition-colors text-sm"
                >
                  View Records
                </button>
                <button
                  onClick={() => handleAddRecord(pet)}
                  className="flex-1 px-3 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 transition-colors text-sm flex items-center justify-center"
                >
                  <Plus className="w-4 h-4 mr-1" />
                  Add Record
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default VetMedicalRecords;
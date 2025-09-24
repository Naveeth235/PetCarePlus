import React, { useState, useEffect } from 'react';
import { PetCard, PetForm } from '../../shared/components';
import { adminPetsApi } from '../../shared/api/petsApi';
import type { Pet, PetSummary, CreatePetRequest, UpdatePetRequest, UserSelection } from '../../shared/types/pet';

interface CreatePetModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

const CreatePetModal: React.FC<CreatePetModalProps> = ({ isOpen, onClose, onSuccess }) => {
  const [isLoading, setIsLoading] = useState(false);
  const [users, setUsers] = useState<UserSelection[]>([]);
  const [selectedUserId, setSelectedUserId] = useState<string>('');
  const [loadingUsers, setLoadingUsers] = useState(true);

  useEffect(() => {
    if (isOpen) {
      loadUsers();
    }
  }, [isOpen]);

  const loadUsers = async () => {
    try {
      setLoadingUsers(true);
      const usersData = await adminPetsApi.getUsersForSelection();
      setUsers(usersData);
      if (usersData.length > 0) {
        setSelectedUserId(usersData[0].id);
      }
    } catch (error) {
      console.error('Failed to load users:', error);
      alert('Failed to load users. Please try again.');
    } finally {
      setLoadingUsers(false);
    }
  };

  const handleSubmit = async (data: CreatePetRequest | UpdatePetRequest) => {
    if (!selectedUserId) {
      alert('Please select a user for the pet.');
      return;
    }

    setIsLoading(true);
    try {
      const createData = {
        ...(data as CreatePetRequest),
        ownerUserId: selectedUserId
      };
      await adminPetsApi.create(createData);
      onSuccess();
      onClose();
    } catch (error) {
      console.error('Failed to create pet:', error);
      alert('Failed to create pet. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg max-w-2xl w-full max-h-[90vh] overflow-y-auto">
        <div className="p-6">
          <h2 className="text-xl font-semibold mb-4">Create New Pet</h2>
          
          {loadingUsers ? (
            <div className="text-center py-4">Loading users...</div>
          ) : (
            <div className="space-y-4">
              {/* User Selection */}
              <div>
                <label htmlFor="owner" className="block text-sm font-medium text-gray-700 mb-1">
                  Select Owner *
                </label>
                <select
                  id="owner"
                  value={selectedUserId}
                  onChange={(e) => setSelectedUserId(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 mb-4"
                >
                  <option value="">Select a user</option>
                  {users.map((user) => (
                    <option key={user.id} value={user.id}>
                      {user.fullName} ({user.email})
                    </option>
                  ))}
                </select>
              </div>

              {/* Pet Form */}
              <PetForm
                mode="create"
                onSubmit={handleSubmit}
                onCancel={onClose}
                isLoading={isLoading}
                ownerUserId={selectedUserId} // Still needed for form validation
              />
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

interface EditPetModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  pet: Pet;
}

const EditPetModal: React.FC<EditPetModalProps> = ({ isOpen, onClose, onSuccess, pet }) => {
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (data: CreatePetRequest | UpdatePetRequest) => {
    setIsLoading(true);
    try {
      await adminPetsApi.update(pet.id, data as UpdatePetRequest);
      onSuccess();
      onClose();
    } catch (error) {
      console.error('Failed to update pet:', error);
      alert('Failed to update pet. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg max-w-2xl w-full max-h-[90vh] overflow-y-auto">
        <div className="p-6">
          <h2 className="text-xl font-semibold mb-4">Edit Pet</h2>
          <PetForm
            mode="edit"
            initialData={pet}
            onSubmit={handleSubmit}
            onCancel={onClose}
            isLoading={isLoading}
          />
        </div>
      </div>
    </div>
  );
};

interface AssignPetModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  pet: Pet;
}

const AssignPetModal: React.FC<AssignPetModalProps> = ({ isOpen, onClose, onSuccess, pet }) => {
  const [selectedUserId, setSelectedUserId] = useState('');
  const [users, setUsers] = useState<UserSelection[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [loadingUsers, setLoadingUsers] = useState(true);

  useEffect(() => {
    if (isOpen) {
      loadUsers();
    }
  }, [isOpen]);

  const loadUsers = async () => {
    try {
      setLoadingUsers(true);
      const usersData = await adminPetsApi.getUsersForSelection();
      setUsers(usersData);
      // Don't auto-select the current owner
      setSelectedUserId('');
    } catch (error) {
      console.error('Failed to load users:', error);
      alert('Failed to load users. Please try again.');
    } finally {
      setLoadingUsers(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedUserId.trim()) {
      alert('Please select a new owner for the pet');
      return;
    }

    // Check if trying to assign to the same owner
    if (selectedUserId === pet.ownerUserId) {
      alert('This pet is already owned by the selected user');
      return;
    }

    setIsLoading(true);
    try {
      await adminPetsApi.assign({
        petId: pet.id,
        newOwnerUserId: selectedUserId.trim()
      });
      onSuccess();
      onClose();
    } catch (error) {
      console.error('Failed to assign pet:', error);
      alert('Failed to assign pet. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg max-w-md w-full">
        <div className="p-6">
          <h2 className="text-xl font-semibold mb-4">Assign Pet to New Owner</h2>
          <p className="text-gray-600 mb-4">
            Assigning <strong>{pet.name}</strong> to a new owner.
            Current owner: <strong>{pet.ownerFullName}</strong>
          </p>
          
          <form onSubmit={handleSubmit}>
            {loadingUsers ? (
              <div className="text-center py-4">Loading owners...</div>
            ) : (
              <div className="mb-4">
                <label htmlFor="newOwner" className="block text-sm font-medium text-gray-700 mb-1">
                  Select New Owner *
                </label>
                <select
                  id="newOwner"
                  value={selectedUserId}
                  onChange={(e) => setSelectedUserId(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  required
                >
                  <option value="">Select a new owner</option>
                  {users.map((user) => (
                    <option 
                      key={user.id} 
                      value={user.id}
                      disabled={user.id === pet.ownerUserId}
                    >
                      {user.fullName} ({user.email})
                      {user.id === pet.ownerUserId ? ' (Current Owner)' : ''}
                    </option>
                  ))}
                </select>
              </div>
            )}

            <div className="flex justify-end space-x-3">
              <button
                type="button"
                onClick={onClose}
                className="px-4 py-2 text-gray-700 bg-gray-200 rounded-md hover:bg-gray-300 focus:outline-none focus:ring-2 focus:ring-gray-500"
                disabled={isLoading}
              >
                Cancel
              </button>
              <button
                type="submit"
                className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50"
                disabled={isLoading}
              >
                {isLoading ? 'Assigning...' : 'Assign Pet'}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};

export const AdminPetsPage: React.FC = () => {
  const [pets, setPets] = useState<PetSummary[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  
  // Modal states
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isAssignModalOpen, setIsAssignModalOpen] = useState(false);
  const [selectedPet, setSelectedPet] = useState<Pet | null>(null);

  useEffect(() => {
    loadPets();
  }, []);

  const loadPets = async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await adminPetsApi.getAll();
      setPets(data);
    } catch (error) {
      console.error('Failed to load pets:', error);
      setError('Failed to load pets. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  const handleDelete = async (pet: PetSummary) => {
    if (!confirm(`Are you sure you want to delete ${pet.name}? This action cannot be undone.`)) {
      return;
    }

    try {
      await adminPetsApi.delete(pet.id);
      await loadPets(); // Reload the list
    } catch (error) {
      console.error('Failed to delete pet:', error);
      alert('Failed to delete pet. Please try again.');
    }
  };

  const handleEdit = async (pet: PetSummary) => {
    try {
      // Fetch full pet details for editing
      const fullPet = await adminPetsApi.getById(pet.id);
      if (fullPet) {
        setSelectedPet(fullPet);
        setIsEditModalOpen(true);
      }
    } catch (error) {
      console.error('Failed to load pet details:', error);
      alert('Failed to load pet details. Please try again.');
    }
  };

  const handleAssign = async (pet: PetSummary) => {
    try {
      // Fetch full pet details for assignment
      const fullPet = await adminPetsApi.getById(pet.id);
      if (fullPet) {
        setSelectedPet(fullPet);
        setIsAssignModalOpen(true);
      }
    } catch (error) {
      console.error('Failed to load pet details:', error);
      alert('Failed to load pet details. Please try again.');
    }
  };

  const handleCreatePet = () => {
    setIsCreateModalOpen(true);
  };

  const filteredPets = pets.filter(pet =>
    pet.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    pet.breed?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    pet.ownerFullName?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-lg text-gray-600">Loading pets...</div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-bold text-gray-900">Pet Management</h1>
        <button
          onClick={handleCreatePet}
          className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500"
        >
          Create New Pet
        </button>
      </div>

      {/* Search */}
      <div className="max-w-md">
        <input
          type="text"
          placeholder="Search pets by name, breed, or owner..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>

      {/* Error State */}
      {error && (
        <div className="bg-red-50 border border-red-200 rounded-md p-4">
          <p className="text-red-800">{error}</p>
          <button
            onClick={loadPets}
            className="mt-2 text-red-600 hover:text-red-800 underline"
          >
            Try again
          </button>
        </div>
      )}

      {/* Pets Grid */}
      {filteredPets.length === 0 ? (
        <div className="text-center py-12">
          <p className="text-gray-500 text-lg">
            {searchTerm ? 'No pets found matching your search.' : 'No pets found.'}
          </p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {filteredPets.map((pet) => (
            <PetCard
              key={pet.id}
              pet={pet}
              showOwner={true}
              onEdit={() => handleEdit(pet)}
              onDelete={() => handleDelete(pet)}
              onAssign={() => handleAssign(pet)}
            />
          ))}
        </div>
      )}

      {/* Modals */}
      <CreatePetModal
        isOpen={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
        onSuccess={loadPets}
      />

      {selectedPet && (
        <>
          <EditPetModal
            isOpen={isEditModalOpen}
            onClose={() => {
              setIsEditModalOpen(false);
              setSelectedPet(null);
            }}
            onSuccess={loadPets}
            pet={selectedPet}
          />

          <AssignPetModal
            isOpen={isAssignModalOpen}
            onClose={() => {
              setIsAssignModalOpen(false);
              setSelectedPet(null);
            }}
            onSuccess={loadPets}
            pet={selectedPet}
          />
        </>
      )}
    </div>
  );
};
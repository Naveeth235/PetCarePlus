import { Link } from "react-router-dom";

const VetPage = () => {
  return (
    <main style={{ padding: 24 }}>
      <h1>Vet Dashboard</h1>
      <ul style={{ textAlign: 'left', marginTop: 12 }}>
        <li>Today's Appointments</li>
        <li>Search Pets</li>
        <li>
          <Link className="text-blue-600 underline" to="/vet/medical-records">
            Manage Medical Records
          </Link>
        </li>
      </ul>
    </main>
  );
};
export default VetPage;

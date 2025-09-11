const OwnerPage = () => {
  return (
    <main style={{ padding: 24 }}>
      <h1>Owner Dashboard</h1>
      <ul style={{ textAlign: 'left', marginTop: 12 }}>
        <li>My Pets</li>
        <li>Medical History (read-only)</li>
        <li>Request Appointment</li>
        <li>My Appointments</li>
      </ul>
    </main>
  );
};
export default OwnerPage;

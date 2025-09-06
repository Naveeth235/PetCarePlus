import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import LoginPage from "./features/auth/LoginPage.tsx";
import RegistrationPage from "./features/auth/RegistrationPage.tsx";

// NEW imports
import OwnerPage from "./features/owner/OwnerPage.tsx";
import VetPage from "./features/vet/VetPage.tsx";
import AdminPage from "./features/admin/AdminPage.tsx";
import Unauthorized from "./pages/Unauthorized.tsx";
import NotFound from "./pages/NotFound.tsx";

const App = () => {
  return (
    <Router>
      <Routes>
        {/* public */}
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegistrationPage />} />

        {/* placeholders for role areas (public for now) */}
        <Route path="/owner" element={<OwnerPage />} />
        <Route path="/vet" element={<VetPage />} />
        <Route path="/admin" element={<AdminPage />} />

        {/* misc */}
        <Route path="/unauthorized" element={<Unauthorized />} />

        {/* 404 */}
        <Route path="*" element={<NotFound />} />
      </Routes>
    </Router>
  );
};

export default App;

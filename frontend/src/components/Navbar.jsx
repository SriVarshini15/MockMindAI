import { BrainCircuit, LogOut, Shield } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext.jsx';
import Avatar from './Avatar.jsx';
import ThemeToggler from './ThemeToggler.jsx';

export default function Navbar() {
  const { student, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <nav className="navbar">
      <button className="brand" onClick={() => navigate('/dashboard')} title="Dashboard">
        <BrainCircuit size={28} />
        <span>MockMind AI</span>
      </button>
      <div className="nav-profile">
        <ThemeToggler />
        {student?.isAdmin && (
          <button className="icon-button" onClick={() => navigate('/admin')} title="Admin">
            <Shield size={18} />
          </button>
        )}
        <Avatar avatarKey={student?.avatarKey} size="sm" />
        <span>{student?.fullName || 'Student'}</span>
        <button className="icon-button" onClick={handleLogout} title="Logout">
          <LogOut size={18} />
        </button>
      </div>
    </nav>
  );
}

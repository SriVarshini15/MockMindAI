import { ArrowLeft, ShieldCheck } from 'lucide-react';
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../api/client.js';
import Navbar from '../components/Navbar.jsx';
import { useAuth } from '../context/AuthContext.jsx';

export default function Admin() {
  const { student } = useAuth();
  const navigate = useNavigate();
  const [users, setUsers] = useState([]);
  const [error, setError] = useState('');

  const loadUsers = async () => {
    try {
      const { data } = await api.get('/admin/users');
      setUsers(data);
    } catch {
      setError('Admin access is required.');
    }
  };

  useEffect(() => {
    loadUsers();
  }, []);

  const toggleUser = async (user) => {
    await api.patch(`/admin/users/${user.id}/disable?disabled=${!user.isDisabled}`);
    await loadUsers();
  };

  return (
    <>
      <Navbar />
      <main className="app-shell">
        <section className="section">
          <div className="section-title">
            <h1>User Management</h1>
            <div className="section-actions">
              <button className="secondary-button" onClick={() => navigate('/dashboard')} type="button">
                <ArrowLeft size={18} /> Dashboard
              </button>
              <ShieldCheck size={24} />
            </div>
          </div>
          {!student?.isAdmin && <div className="error">Only admins can manage users.</div>}
          {error && <div className="error">{error}</div>}
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Email</th>
                  <th>Department</th>
                  <th>Interviews</th>
                  <th>Average</th>
                  <th>Status</th>
                  <th>Action</th>
                </tr>
              </thead>
              <tbody>
                {users.map((user) => (
                  <tr key={user.id}>
                    <td>{user.fullName}</td>
                    <td>{user.email}</td>
                    <td>{user.department}</td>
                    <td>{user.interviewCount}</td>
                    <td>{user.averageScore}/10</td>
                    <td>{user.isDisabled ? 'Disabled' : 'Active'}</td>
                    <td>
                      <button className="secondary-button" onClick={() => toggleUser(user)}>
                        {user.isDisabled ? 'Enable' : 'Disable'}
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </section>
      </main>
    </>
  );
}

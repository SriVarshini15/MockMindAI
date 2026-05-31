import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import api from '../api/client.js';
import { avatarOptions } from '../components/avatarOptions.js';
import PasswordInput from '../components/PasswordInput.jsx';
import { useAuth } from '../context/AuthContext.jsx';

export default function Register() {
  const [form, setForm] = useState({ fullName: '', email: '', password: '', department: '', collegeName: '', avatarKey: 'mentor' });
  const [error, setError] = useState('');
  const { login } = useAuth();
  const navigate = useNavigate();

  const submit = async (event) => {
    event.preventDefault();
    setError('');
    try {
      const { data } = await api.post('/auth/register', form);
      login(data);
      navigate('/dashboard');
    } catch (requestError) {
      setError(requestError.response?.data || 'Registration failed. Check that the backend is running and try again.');
    }
  };

  return (
    <main className="auth-page">
      <form className="auth-panel wide" onSubmit={submit}>
        <h1>Create Account</h1>
        {error && <div className="error">{error}</div>}
        <input placeholder="Full Name" value={form.fullName} onChange={(e) => setForm({ ...form, fullName: e.target.value })} required />
        <input placeholder="Email" type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} required />
        <PasswordInput value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} />
        <input placeholder="Department" value={form.department} onChange={(e) => setForm({ ...form, department: e.target.value })} required />
        <input placeholder="College Name" value={form.collegeName} onChange={(e) => setForm({ ...form, collegeName: e.target.value })} required />
        <div className="avatar-picker">
          {avatarOptions.map((avatar) => (
            <button
              className={`avatar-choice ${form.avatarKey === avatar.key ? 'selected' : ''}`}
              key={avatar.key}
              onClick={() => setForm({ ...form, avatarKey: avatar.key })}
              title={avatar.label}
              type="button"
            >
              <img alt="" src={avatar.image} style={{ objectPosition: avatar.position }} />
            </button>
          ))}
        </div>
        <button className="primary-button">Register</button>
        <span>Already registered? <Link to="/login">Login</Link></span>
      </form>
    </main>
  );
}

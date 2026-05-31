import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import api from '../api/client.js';
import PasswordInput from '../components/PasswordInput.jsx';
import { useAuth } from '../context/AuthContext.jsx';

export default function Login() {
  const [form, setForm] = useState({ email: '', password: '' });
  const [error, setError] = useState('');
  const { login } = useAuth();
  const navigate = useNavigate();

  const submit = async (event) => {
    event.preventDefault();
    setError('');
    try {
      const { data } = await api.post('/auth/login', form);
      login(data);
      navigate('/dashboard');
    } catch (requestError) {
      setError(requestError.response?.data || 'Login failed. Check that the backend is running and try again.');
    }
  };

  return (
    <main className="auth-page">
      <form className="auth-panel" onSubmit={submit}>
        <h1>MockMind AI</h1>
        <p>Sign in to continue your interview practice.</p>
        {error && <div className="error">{error}</div>}
        <input placeholder="Email" type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} required />
        <PasswordInput value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} />
        <button className="primary-button">Login</button>
        <span>New here? <Link to="/register">Create account</Link></span>
      </form>
    </main>
  );
}

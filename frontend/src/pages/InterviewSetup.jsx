import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../api/client.js';
import Navbar from '../components/Navbar.jsx';

const options = {
  role: ['C# Developer', 'ASP.NET Core Developer', 'Full Stack Developer', 'React Developer', 'SQL Developer', 'Other'],
  difficulty: ['Easy', 'Medium', 'Hard'],
  experience: ['Fresher', '1-2 Years', '3-5 Years'],
  interviewType: ['Technical', 'HR', 'Behavioral']
};

export default function InterviewSetup() {
  const [setup, setSetup] = useState({
    role: options.role[0],
    difficulty: 'Easy',
    experience: 'Fresher',
    interviewType: 'Technical',
    isTimedMode: false,
    durationMinutes: 20,
    customRole: ''
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const generateQuestions = async (event) => {
    event.preventDefault();
    setLoading(true);
    setError('');
    const selectedRole = setup.role === 'Other' ? setup.customRole.trim() : setup.role;
    if (!selectedRole) {
      setError('Enter your preferred role.');
      setLoading(false);
      return;
    }

    const requestSetup = { ...setup, role: selectedRole };
    delete requestSetup.customRole;

    try {
      const { data } = await api.post('/interview/generate-questions', requestSetup);
      navigate('/session', { state: { setup: requestSetup, questions: data.questions } });
    } catch (err) {
      setError(err.response?.data?.message || 'Unable to generate questions. Check Gemini API key and backend logs.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <Navbar />
      <main className="app-shell narrow">
        <section className="section">
          <h1>Interview Setup</h1>
          {error && <div className="error">{error}</div>}
          <form className="setup-form" onSubmit={generateQuestions}>
            {Object.entries(options).map(([key, values]) => (
              <label key={key}>
                <span>{key === 'interviewType' ? 'Interview Type' : key[0].toUpperCase() + key.slice(1)}</span>
                <select value={setup[key]} onChange={(e) => setSetup({ ...setup, [key]: e.target.value })}>
                  {values.map((value) => <option key={value}>{value}</option>)}
                </select>
              </label>
            ))}
            {setup.role === 'Other' && (
              <label>
                <span>Preferred Role</span>
                <input
                  onChange={(e) => setSetup({ ...setup, customRole: e.target.value })}
                  placeholder="Enter your preferred role"
                  required
                  value={setup.customRole}
                />
              </label>
            )}
            <label className="toggle-row">
              <input
                checked={setup.isTimedMode}
                onChange={(e) => setSetup({ ...setup, isTimedMode: e.target.checked, durationMinutes: e.target.checked ? 20 : 0 })}
                type="checkbox"
              />
              <span>20-minute timed interview mode</span>
            </label>
            <button className="primary-button" disabled={loading}>{loading ? 'Generating...' : 'Generate Questions'}</button>
          </form>
        </section>
      </main>
    </>
  );
}

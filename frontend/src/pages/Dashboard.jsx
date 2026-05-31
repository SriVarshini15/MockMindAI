import { Award, Flame, Rocket, Trophy } from 'lucide-react';
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../api/client.js';
import Avatar from '../components/Avatar.jsx';
import { avatarOptions } from '../components/avatarOptions.js';
import CircularProgressRing from '../components/CircularProgressRing.jsx';
import HistoryModal from '../components/HistoryModal.jsx';
import Navbar from '../components/Navbar.jsx';
import SkillTrendChart from '../components/SkillTrendChart.jsx';
import { useAuth } from '../context/AuthContext.jsx';

export default function Dashboard() {
  const [dashboard, setDashboard] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showHistory, setShowHistory] = useState(false);
  const [savingAvatar, setSavingAvatar] = useState(false);
  const [selectedHeatmapRole, setSelectedHeatmapRole] = useState('');
  const navigate = useNavigate();
  const { logout, updateStudent } = useAuth();

  const loadDashboard = async () => {
    setLoading(true);
    setError('');
    try {
      const { data } = await api.get('/dashboard');
      setDashboard(data);
      if (data.skillHeatmapGroups?.length) {
        setSelectedHeatmapRole(data.skillHeatmapGroups[0].role);
      }
    } catch (requestError) {
      if (requestError.response?.status === 401) {
        logout();
        navigate('/login', { replace: true });
        return;
      }

      setError(
        requestError.response?.data?.message
          || 'Unable to load the dashboard. Check that the backend is running and the API URL is correct.'
      );
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadDashboard();
  }, []);

  if (loading) {
    return <div className="loading">Loading dashboard...</div>;
  }

  if (error) {
    return (
      <>
        <Navbar />
        <main className="app-shell narrow">
          <section className="section">
            <h1>Dashboard unavailable</h1>
            <div className="error">{error}</div>
            <button className="primary-button" onClick={loadDashboard} type="button">Retry</button>
          </section>
        </main>
      </>
    );
  }

  const { profile, recentAssessments, allAssessments } = dashboard;
  const heatmapGroups = dashboard.skillHeatmapGroups?.length
    ? dashboard.skillHeatmapGroups
    : [{ role: 'Interview Skills', skills: dashboard.skillHeatmap || {} }];
  const selectedGroup = heatmapGroups.find((group) => group.role === selectedHeatmapRole) || heatmapGroups[0];
  const averagePercent = Math.round(profile.averageScore * 10);

  const updateAvatar = async (avatarKey) => {
    setSavingAvatar(true);
    try {
      await api.put('/profile', { avatarKey });
      setDashboard({ ...dashboard, profile: { ...profile, avatarKey } });
      updateStudent({ avatarKey });
    } finally {
      setSavingAvatar(false);
    }
  };

  return (
    <>
      <Navbar />
      <main className="app-shell">
        <section className="profile-grid">
          <div className="profile-card">
            <div className="profile-heading">
              <Avatar avatarKey={profile.avatarKey} size="lg" />
              <div>
                <h2>{profile.fullName}</h2>
                <p>{profile.email}</p>
              </div>
            </div>
            <p>{profile.department}</p>
            <div className="avatar-picker compact">
              {avatarOptions.map((avatar) => (
                <button
                  className={`avatar-choice ${profile.avatarKey === avatar.key ? 'selected' : ''}`}
                  disabled={savingAvatar}
                  key={avatar.key}
                  onClick={() => updateAvatar(avatar.key)}
                  title={avatar.label}
                  type="button"
                >
                  <img alt="" src={avatar.image} style={{ objectPosition: avatar.position }} />
                </button>
              ))}
            </div>
          </div>
          <CircularProgressRing
            helper="Target 10"
            label="Total Interviews"
            max={10}
            value={profile.totalInterviewsAttended}
          />
          <CircularProgressRing
            helper={`${averagePercent}% performance`}
            label="Average Score"
            max={10}
            suffix="/10"
            value={profile.averageScore}
          />
        </section>

        <button className="prepare-card" onClick={() => navigate('/setup')}>
          <Rocket size={42} />
          <span>Prepare For Interview</span>
          <small>AI questions, timed mode, instant evaluation</small>
        </button>

        <section className="section">
          <div className="section-title">
            <h2>Skill Trend Chart</h2>
            <span className="eyebrow">Last 5 Interviews</span>
          </div>
          <SkillTrendChart attempts={allAssessments} />
        </section>

        <section className="insight-grid">
          <div className="section">
            <div className="section-title">
              <h2>Skill Heatmap</h2>
              <Flame size={20} />
            </div>
            {heatmapGroups.length > 1 && (
              <div className="role-filter">
                {heatmapGroups.map((group) => (
                  <button
                    className={selectedGroup.role === group.role ? 'selected' : ''}
                    key={group.role}
                    onClick={() => setSelectedHeatmapRole(group.role)}
                    type="button"
                  >
                    {group.role}
                  </button>
                ))}
              </div>
            )}
            <div className="heatmap">
              {selectedGroup && (
                <div className="heatmap-group" key={selectedGroup.role}>
                  <h3>{selectedGroup.role}</h3>
                  {Object.entries(selectedGroup.skills).map(([skill, score]) => (
                    <div className="skill-row" key={`${selectedGroup.role}-${skill}`}>
                      <span>{skill}</span>
                      <div className="skill-bar"><i style={{ width: `${score}%` }} /></div>
                      <strong>{score}%</strong>
                    </div>
                  ))}
                </div>
              )}
              {heatmapGroups.every((group) => Object.keys(group.skills).length === 0) && (
                <p className="muted">Complete an interview to build a personalized skill heatmap.</p>
              )}
            </div>
          </div>
          <div className="section">
            <div className="section-title">
              <h2>Achievement Badges</h2>
              <Award size={20} />
            </div>
            <div className="badge-list">
              {dashboard.achievementBadges.length > 0
                ? dashboard.achievementBadges.map((badge) => <span className="badge" key={badge}>{badge}</span>)
                : <p className="muted">Complete your first interview to unlock badges.</p>}
            </div>
          </div>
        </section>

        <section className="section">
          <div className="section-title">
            <h2>Leaderboard</h2>
            <Trophy size={20} />
          </div>
          <div className="leaderboard-list">
            {dashboard.leaderboard.map((entry) => (
              <div className="leaderboard-row" key={`${entry.rank}-${entry.fullName}`}>
                <strong>#{entry.rank}</strong>
                <Avatar avatarKey={entry.avatarKey} size="sm" />
                <span>{entry.fullName}</span>
                <small>{entry.department}</small>
                <b>{entry.averageScore}/10</b>
                <small>{entry.interviewStreak} day streak</small>
                <small>{entry.interviewCount} interviews</small>
              </div>
            ))}
            {dashboard.leaderboard.length === 0 && <p className="muted">Leaderboard appears after interviews are completed.</p>}
          </div>
        </section>

        <section className="section">
          <div className="section-title">
            <h2>Recent Interview Assessments</h2>
            <button className="secondary-button" onClick={() => setShowHistory(true)}>See More</button>
          </div>
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Role</th>
                  <th>Difficulty</th>
                  <th>Interview Type</th>
                  <th>Score</th>
                </tr>
              </thead>
              <tbody>
                {recentAssessments.map((attempt) => (
                  <tr key={attempt.id}>
                    <td>{new Date(attempt.date).toLocaleDateString()}</td>
                    <td>{attempt.role}</td>
                    <td>{attempt.difficulty}</td>
                    <td>{attempt.interviewType}</td>
                    <td>{attempt.score}/10</td>
                  </tr>
                ))}
                {recentAssessments.length === 0 && (
                  <tr><td colSpan="5">No interviews yet.</td></tr>
                )}
              </tbody>
            </table>
          </div>
        </section>
      </main>
      {showHistory && <HistoryModal attempts={allAssessments} onClose={() => setShowHistory(false)} />}
    </>
  );
}

import { Search, X } from 'lucide-react';
import { useMemo, useState } from 'react';

export default function HistoryModal({ attempts, onClose }) {
  const [query, setQuery] = useState('');
  const [difficulty, setDifficulty] = useState('All');
  const [type, setType] = useState('All');
  const [sort, setSort] = useState('date-desc');

  const filtered = useMemo(() => {
    return [...attempts].filter((attempt) => {
      const matchesQuery = `${attempt.role} ${attempt.interviewType} ${attempt.difficulty}`.toLowerCase().includes(query.toLowerCase());
      const matchesDifficulty = difficulty === 'All' || attempt.difficulty === difficulty;
      const matchesType = type === 'All' || attempt.interviewType === type;
      return matchesQuery && matchesDifficulty && matchesType;
    }).sort((a, b) => {
      if (sort === 'date-asc') return new Date(a.date) - new Date(b.date);
      if (sort === 'score-desc') return b.score - a.score;
      if (sort === 'score-asc') return a.score - b.score;
      if (sort === 'role') return a.role.localeCompare(b.role);
      return new Date(b.date) - new Date(a.date);
    });
  }, [attempts, query, difficulty, type, sort]);

  return (
    <div className="modal-backdrop">
      <section className="modal">
        <div className="modal-header">
          <h2>Interview History</h2>
          <button className="icon-button" onClick={onClose} title="Close">
            <X size={20} />
          </button>
        </div>
        <div className="filters">
          <label className="search-box">
            <Search size={18} />
            <input value={query} onChange={(event) => setQuery(event.target.value)} placeholder="Search role or type" />
          </label>
          <select value={difficulty} onChange={(event) => setDifficulty(event.target.value)}>
            <option>All</option>
            <option>Easy</option>
            <option>Medium</option>
            <option>Hard</option>
          </select>
          <select value={type} onChange={(event) => setType(event.target.value)}>
            <option>All</option>
            <option>Technical</option>
            <option>HR</option>
            <option>Behavioral</option>
          </select>
          <select value={sort} onChange={(event) => setSort(event.target.value)}>
            <option value="date-desc">Newest first</option>
            <option value="date-asc">Oldest first</option>
            <option value="score-desc">Highest score</option>
            <option value="score-asc">Lowest score</option>
            <option value="role">Role A-Z</option>
          </select>
        </div>
        <div className="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Date</th>
                <th>Role</th>
                <th>Difficulty</th>
                <th>Type</th>
                <th>Mode</th>
                <th>Score</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((attempt) => (
                <tr key={attempt.id}>
                  <td>{new Date(attempt.date).toLocaleDateString()}</td>
                  <td>{attempt.role}</td>
                  <td>{attempt.difficulty}</td>
                  <td>{attempt.interviewType}</td>
                  <td>{attempt.isTimedMode ? 'Timed' : 'Practice'}</td>
                  <td>{attempt.score}/10</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>
    </div>
  );
}

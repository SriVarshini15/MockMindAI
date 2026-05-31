import { ArrowLeft, RotateCcw } from 'lucide-react';
import {
  BarElement,
  CategoryScale,
  Chart as ChartJS,
  LinearScale,
  Tooltip
} from 'chart.js';
import { Bar } from 'react-chartjs-2';
import { Navigate, useLocation, useNavigate } from 'react-router-dom';
import Navbar from '../components/Navbar.jsx';

ChartJS.register(CategoryScale, LinearScale, BarElement, Tooltip);

export default function Result() {
  const { state } = useLocation();
  const navigate = useNavigate();

  if (!state?.result) {
    return <Navigate to="/dashboard" replace />;
  }

  const result = state.result;
  const graphData = {
    labels: ['Score', 'Remaining'],
    datasets: [{
      label: 'Performance',
      data: [result.score, 10 - result.score],
      backgroundColor: ['#1d8f70', '#dbe4ef']
    }]
  };

  return (
    <>
      <Navbar />
      <main className="app-shell">
        <section className="result-grid">
          <div className="section">
            <span className="eyebrow">Overall Score</span>
            <div className="score">{result.score}/10</div>
            <Bar data={graphData} options={{ responsive: true, plugins: { legend: { display: false } }, scales: { y: { max: 10, beginAtZero: true } } }} />
          </div>
          <div className="section">
            <h2>Strengths</h2>
            <ul>{result.strengths.map((item) => <li key={item}>{item}</li>)}</ul>
            <h2>Weaknesses</h2>
            <ul>{result.weaknesses.map((item) => <li key={item}>{item}</li>)}</ul>
          </div>
        </section>
        <section className="section">
          <h2>Improved Answer</h2>
          <p>{result.improvedAnswer}</p>
          <h2>Feedback</h2>
          <p>{result.feedback}</p>
          <div className="session-actions">
            <button className="secondary-button" onClick={() => navigate('/setup')}><RotateCcw size={18} /> Retake Interview</button>
            <button className="primary-button" onClick={() => navigate('/dashboard')}><ArrowLeft size={18} /> Back To Dashboard</button>
          </div>
        </section>
      </main>
    </>
  );
}

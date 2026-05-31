import { useMemo, useState } from 'react';

function buildSmoothPath(points) {
  if (points.length === 0) {
    return '';
  }

  if (points.length === 1) {
    return `M ${points[0].x} ${points[0].y}`;
  }

  return points.reduce((path, point, index) => {
    if (index === 0) {
      return `M ${point.x} ${point.y}`;
    }

    const previous = points[index - 1];
    const controlX = previous.x + (point.x - previous.x) / 2;
    return `${path} C ${controlX} ${previous.y}, ${controlX} ${point.y}, ${point.x} ${point.y}`;
  }, '');
}

export default function SkillTrendChart({ attempts }) {
  const [activeIndex, setActiveIndex] = useState(null);
  const trend = useMemo(() => {
    return [...attempts]
      .sort((a, b) => new Date(a.date) - new Date(b.date))
      .slice(-5)
      .map((attempt, index) => ({
        label: attempt.role || `Interview ${index + 1}`,
        score: attempt.score,
        date: attempt.date
      }));
  }, [attempts]);

  const points = trend.map((point, index) => {
    const x = trend.length === 1 ? 150 : 24 + (index * (252 / (trend.length - 1)));
    const y = 136 - ((Math.min(Math.max(point.score, 0), 10) / 10) * 104);
    return { ...point, x, y };
  });
  const path = buildSmoothPath(points);
  const active = activeIndex === null ? null : points[activeIndex];

  if (trend.length === 0) {
    return <p className="muted">Complete interviews to see score progression.</p>;
  }

  return (
    <div className="trend-chart">
      <svg viewBox="0 0 300 168" role="img" aria-label="Score progression over last five interviews">
        {[0, 5, 10].map((tick) => {
          const y = 136 - ((tick / 10) * 104);
          return (
            <g key={tick}>
              <line className="trend-grid" x1="20" x2="284" y1={y} y2={y} />
              <text className="trend-tick" x="4" y={y + 4}>{tick}</text>
            </g>
          );
        })}
        <path className="trend-area" d={`${path} L ${points.at(-1).x} 148 L ${points[0].x} 148 Z`} />
        <path className="trend-line" d={path} />
        {points.map((point, index) => (
          <g key={`${point.date}-${index}`}>
            <circle
              className="trend-hit"
              cx={point.x}
              cy={point.y}
              onMouseEnter={() => setActiveIndex(index)}
              onMouseLeave={() => setActiveIndex(null)}
              r="15"
            />
            <circle className="trend-dot" cx={point.x} cy={point.y} r="5" />
          </g>
        ))}
      </svg>
      {active && (
        <div className="trend-tooltip" style={{ left: `${(active.x / 300) * 100}%`, top: `${active.y}px` }}>
          <strong>{active.score}/10</strong>
          <span>{active.label}</span>
        </div>
      )}
    </div>
  );
}

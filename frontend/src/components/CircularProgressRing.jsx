import { useEffect, useId, useState } from 'react';

export default function CircularProgressRing({ label, value, max = 100, suffix = '', helper }) {
  const [animatedValue, setAnimatedValue] = useState(0);
  const gradientId = useId();
  const radius = 42;
  const stroke = 9;
  const circumference = 2 * Math.PI * radius;
  const percent = max > 0 ? Math.min(Math.max(animatedValue / max, 0), 1) : 0;
  const dashOffset = circumference * (1 - percent);

  useEffect(() => {
    const frame = requestAnimationFrame(() => setAnimatedValue(value));
    return () => cancelAnimationFrame(frame);
  }, [value]);

  return (
    <div className="progress-ring-card">
      <svg className="progress-ring" viewBox="0 0 112 112" role="img" aria-label={`${label}: ${value}${suffix}`}>
        <defs>
          <linearGradient id={gradientId} x1="0%" x2="100%" y1="0%" y2="0%">
            <stop offset="0%" stopColor="#6366F1" />
            <stop offset="100%" stopColor="#10B981" />
          </linearGradient>
        </defs>
        <circle
          className="progress-ring-track"
          cx="56"
          cy="56"
          fill="none"
          r={radius}
          strokeWidth={stroke}
        />
        <circle
          className="progress-ring-value"
          cx="56"
          cy="56"
          fill="none"
          r={radius}
          stroke={`url(#${gradientId})`}
          strokeDasharray={circumference}
          strokeDashoffset={dashOffset}
          strokeLinecap="round"
          strokeWidth={stroke}
        />
      </svg>
      <div className="progress-ring-content">
        <span>{label}</span>
        <strong>{value}{suffix}</strong>
        {helper && <small>{helper}</small>}
      </div>
    </div>
  );
}

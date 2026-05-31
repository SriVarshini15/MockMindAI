import { ChevronLeft, ChevronRight, Clock, Minus, Plus, Send } from 'lucide-react';
import { useCallback, useEffect, useRef, useState } from 'react';
import { Navigate, useLocation, useNavigate } from 'react-router-dom';
import api from '../api/client.js';
import Navbar from '../components/Navbar.jsx';

export default function InterviewSession() {
  const { state } = useLocation();
  const navigate = useNavigate();
  const [index, setIndex] = useState(0);
  const [answers, setAnswers] = useState(Array(state?.questions?.length || 0).fill(''));
  const [answerFontSize, setAnswerFontSize] = useState(16);
  const [submitting, setSubmitting] = useState(false);
  const [secondsLeft, setSecondsLeft] = useState((state?.setup?.durationMinutes || 0) * 60);
  const submittedRef = useRef(false);
  const hasSession = Boolean(state?.questions?.length && state?.setup);
  const question = hasSession ? state.questions[index] : '';
  const updateAnswer = (value) => {
    const copy = [...answers];
    copy[index] = value;
    setAnswers(copy);
  };

  const submit = useCallback(async (wasAutoSubmitted = false) => {
    if (!hasSession || submittedRef.current) {
      return;
    }

    submittedRef.current = true;
    setSubmitting(true);
    const { data } = await api.post('/interview/submit', {
      ...state.setup,
      wasAutoSubmitted,
      questions: state.questions,
      answers
    });
    navigate('/result', { state: { result: data } });
  }, [answers, hasSession, navigate, state]);

  useEffect(() => {
    if (!hasSession || !state.setup.isTimedMode || submitting) {
      return undefined;
    }

    const timer = setInterval(() => {
      setSecondsLeft((value) => {
        if (value <= 1) {
          clearInterval(timer);
          submit(true);
          return 0;
        }

        return value - 1;
      });
    }, 1000);

    return () => clearInterval(timer);
  }, [hasSession, state?.setup?.isTimedMode, submit, submitting]);

  const minutes = Math.floor(secondsLeft / 60).toString().padStart(2, '0');
  const seconds = (secondsLeft % 60).toString().padStart(2, '0');
  const adjustAnswerFont = (amount) => {
    setAnswerFontSize((value) => Math.min(24, Math.max(14, value + amount)));
  };

  if (!hasSession) {
    return <Navigate to="/setup" replace />;
  }

  return (
    <>
      <Navbar />
      <main className="app-shell narrow">
        <section className="section session-panel">
          <div className="session-topline">
            <span className="eyebrow">Question {index + 1} of {state.questions.length}</span>
            {state.setup.isTimedMode && (
              <span className={`timer ${secondsLeft < 300 ? 'danger' : ''}`}>
                <Clock size={17} /> {minutes}:{seconds}
              </span>
            )}
          </div>
          <h1>{question}</h1>
          <div className="answer-toolbar">
            <span>Answer Text Size</span>
            <div className="font-size-controls">
              <button
                className="icon-button"
                disabled={answerFontSize <= 14}
                onClick={() => adjustAnswerFont(-1)}
                title="Decrease answer text size"
                type="button"
              >
                <Minus size={16} />
              </button>
              <strong>{answerFontSize}px</strong>
              <button
                className="icon-button"
                disabled={answerFontSize >= 24}
                onClick={() => adjustAnswerFont(1)}
                title="Increase answer text size"
                type="button"
              >
                <Plus size={16} />
              </button>
            </div>
          </div>
          <textarea
            className="answer-textarea"
            onChange={(e) => updateAnswer(e.target.value)}
            placeholder="Type your answer here"
            style={{ fontSize: `${answerFontSize}px` }}
            value={answers[index]}
          />
          <div className="session-actions">
            <button className="secondary-button" onClick={() => setIndex(index - 1)} disabled={index === 0}>
              <ChevronLeft size={18} /> Previous
            </button>
            {index < state.questions.length - 1 ? (
              <button className="secondary-button" onClick={() => setIndex(index + 1)}>
                Next <ChevronRight size={18} />
              </button>
            ) : (
              <button className="primary-button" onClick={() => submit(false)} disabled={submitting}>
                <Send size={18} /> {submitting ? 'Submitting...' : 'Submit Answer'}
              </button>
            )}
          </div>
        </section>
      </main>
    </>
  );
}

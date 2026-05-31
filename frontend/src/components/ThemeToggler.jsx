import { Moon, Sun } from 'lucide-react';
import { useEffect, useState } from 'react';

export default function ThemeToggler() {
  const [dark, setDark] = useState(() => localStorage.getItem('mockmind_theme') === 'dark');

  useEffect(() => {
    document.documentElement.classList.toggle('dark', dark);
    localStorage.setItem('mockmind_theme', dark ? 'dark' : 'light');
  }, [dark]);

  return (
    <button
      className="icon-button theme-toggle"
      onClick={() => setDark(!dark)}
      title={dark ? 'Switch to light mode' : 'Switch to dark mode'}
      type="button"
    >
      {dark ? <Sun size={18} /> : <Moon size={18} />}
    </button>
  );
}

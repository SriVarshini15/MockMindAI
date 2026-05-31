import { Eye, EyeOff } from 'lucide-react';
import { useState } from 'react';

export default function PasswordInput({ onChange, value }) {
  const [visible, setVisible] = useState(false);

  return (
    <div className="password-field">
      <input
        onChange={onChange}
        placeholder="Password"
        required
        type={visible ? 'text' : 'password'}
        value={value}
      />
      <button
        aria-label={visible ? 'Hide password' : 'Show password'}
        className="password-toggle"
        onClick={() => setVisible(!visible)}
        title={visible ? 'Hide password' : 'Show password'}
        type="button"
      >
        {visible ? <EyeOff size={18} /> : <Eye size={18} />}
      </button>
    </div>
  );
}

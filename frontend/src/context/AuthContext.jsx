import { createContext, useContext, useMemo, useState } from 'react';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [token, setToken] = useState(localStorage.getItem('mockmind_token'));
  const [student, setStudent] = useState(() => {
    const value = localStorage.getItem('mockmind_student');
    return value ? JSON.parse(value) : null;
  });

  const login = (authResponse) => {
    localStorage.setItem('mockmind_token', authResponse.token);
    localStorage.setItem('mockmind_student', JSON.stringify(authResponse.student));
    setToken(authResponse.token);
    setStudent(authResponse.student);
  };

  const logout = () => {
    localStorage.removeItem('mockmind_token');
    localStorage.removeItem('mockmind_student');
    setToken(null);
    setStudent(null);
  };

  const updateStudent = (updates) => {
    const next = { ...student, ...updates };
    localStorage.setItem('mockmind_student', JSON.stringify(next));
    setStudent(next);
  };

  const value = useMemo(() => ({ token, student, login, logout, updateStudent }), [token, student]);
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  return useContext(AuthContext);
}

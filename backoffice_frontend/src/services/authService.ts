import api from './api';
import { LoginRequest, User } from '../types';

export const authService = {
  async login(credentials: LoginRequest): Promise<User> {
    const response = await api.post('/auth/login', credentials);
    const user = response.data;
    localStorage.setItem('token', user.token);
    localStorage.setItem('user', JSON.stringify(user));
    return user;
  },

  async forgotPassword(email: string): Promise<void> {
    await api.post('/auth/forgot-password', { email });
  },

  async resetPassword(email: string, token: string, password: string): Promise<void> {
    await api.post('/auth/reset-password', { email, token, password });
  },

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  },

  getCurrentUser(): User | null {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  },

  isAuthenticated(): boolean {
    return !!localStorage.getItem('token');
  }
};
import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { Eye, EyeOff, AlertTriangle } from 'lucide-react';
import { authService } from '../services/authService';
import { LoginRequest } from '../types';
import toast from 'react-hot-toast';

const schema = yup.object({
  email: yup.string().email('Email inválido').required('Email é obrigatório'),
  password: yup.string().required('Senha é obrigatória'),
});

const Login: React.FC = () => {
  const navigate = useNavigate();
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginRequest>({
    resolver: yupResolver(schema),
  });

  const onSubmit = async (data: LoginRequest) => {
    setLoading(true);
    try {
      await authService.login(data);
      toast.success('Login realizado com sucesso!');
      navigate('/dashboard');
    } catch (error) {
      toast.error('Credenciais inválidas');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        {/* Logo/Title */}
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-16 h-16 glass rounded-full mb-4">
            <AlertTriangle className="w-8 h-8 text-purple-400" />
          </div>
          <h1 className="text-3xl font-bold text-white mb-2">Web Grande Irmão</h1>
          <p className="text-gray-300">Sistema de Monitoramento de Anomalias</p>
        </div>

        {/* Login Form */}
        <div className="glass rounded-2xl p-8">
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
            <div>
              <label htmlFor="email" className="block text-sm font-medium text-gray-300 mb-2">
                Email
              </label>
              <input
                {...register('email')}
                type="email"
                id="email"
                className="input-field w-full"
                placeholder="seu@email.com"
              />
              {errors.email && (
                <p className="mt-1 text-sm text-red-400">{errors.email.message}</p>
              )}
            </div>

            <div>
              <label htmlFor="password" className="block text-sm font-medium text-gray-300 mb-2">
                Senha
              </label>
              <div className="relative">
                <input
                  {...register('password')}
                  type={showPassword ? 'text' : 'password'}
                  id="password"
                  className="input-field w-full pr-12"
                  placeholder="Sua senha"
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 hover:text-white"
                >
                  {showPassword ? <EyeOff size={20} /> : <Eye size={20} />}
                </button>
              </div>
              {errors.password && (
                <p className="mt-1 text-sm text-red-400">{errors.password.message}</p>
              )}
            </div>

            <button
              type="submit"
              disabled={loading}
              className="w-full btn-primary disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {loading ? 'Entrando...' : 'Entrar'}
            </button>
          </form>

          <div className="mt-6 text-center">
            <Link
              to="/forgot-password"
              className="text-sm text-purple-400 hover:text-purple-300 transition-colors"
            >
              Esqueci minha senha
            </Link>
          </div>
        </div>

        {/* Test credentials info */}
        <div className="mt-4 p-4 glass rounded-lg">
          <p className="text-xs text-gray-400 text-center">
            <strong>Credenciais de teste:</strong><br />
            Email: teste@teste.com<br />
            Senha: Teste123
          </p>
        </div>
      </div>
    </div>
  );
};

export default Login;
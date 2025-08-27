import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { AlertTriangle, ArrowLeft, Mail } from 'lucide-react';
import { authService } from '../services/authService';
import toast from 'react-hot-toast';

const schema = yup.object({
  email: yup.string().email('Email inválido').required('Email é obrigatório'),
});

interface ForgotPasswordForm {
  email: string;
}

const ForgotPassword: React.FC = () => {
  const [loading, setLoading] = useState(false);
  const [emailSent, setEmailSent] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
    getValues
  } = useForm<ForgotPasswordForm>({
    resolver: yupResolver(schema),
  });

  const onSubmit = async (data: ForgotPasswordForm) => {
    setLoading(true);
    try {
      await authService.forgotPassword(data.email);
      setEmailSent(true);
      toast.success('Email de recuperação enviado com sucesso!');
    } catch (error) {
      toast.error('Erro ao enviar email de recuperação');
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
          <h1 className="text-3xl font-bold text-white mb-2">Recuperar Senha</h1>
          <p className="text-gray-300">Digite seu email para receber o link de recuperação</p>
        </div>

        {/* Form */}
        <div className="glass rounded-2xl p-8">
          {!emailSent ? (
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
              <div>
                <label htmlFor="email" className="block text-sm font-medium text-gray-300 mb-2">
                  Email
                </label>
                <div className="relative">
                  <input
                    {...register('email')}
                    type="email"
                    id="email"
                    className="input-field w-full pl-12"
                    placeholder="seu@email.com"
                  />
                  <Mail className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" size={20} />
                </div>
                {errors.email && (
                  <p className="mt-1 text-sm text-red-400">{errors.email.message}</p>
                )}
              </div>

              <button
                type="submit"
                disabled={loading}
                className="w-full btn-primary disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {loading ? 'Enviando...' : 'Enviar Link de Recuperação'}
              </button>
            </form>
          ) : (
            <div className="text-center space-y-4">
              <div className="w-16 h-16 bg-green-500/20 rounded-full flex items-center justify-center mx-auto mb-4">
                <Mail className="w-8 h-8 text-green-400" />
              </div>
              <h3 className="text-lg font-semibold text-white">Email Enviado!</h3>
              <p className="text-gray-300">
                Um link de recuperação foi enviado para <strong>{getValues('email')}</strong>
              </p>
              <p className="text-sm text-gray-400">
                Verifique sua caixa de entrada e spam. O link expira em 24 horas.
              </p>
            </div>
          )}

          <div className="mt-6">
            <Link
              to="/login"
              className="flex items-center justify-center text-sm text-purple-400 hover:text-purple-300 transition-colors"
            >
              <ArrowLeft size={16} className="mr-1" />
              Voltar para o Login
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ForgotPassword;
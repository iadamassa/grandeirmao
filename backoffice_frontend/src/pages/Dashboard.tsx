import React, { useState, useEffect } from 'react';
import { 
  Search, 
  Tags, 
  Globe, 
  AlertTriangle,
  TrendingUp,
  Calendar
} from 'lucide-react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, PieChart, Pie, Cell, LineChart, Line, Area, AreaChart } from 'recharts';
import api from '../services/api';
import { DashboardData } from '../types';
import toast from 'react-hot-toast';

const Dashboard: React.FC = () => {
  const [data, setData] = useState<DashboardData | null>(null);
  const [loading, setLoading] = useState(true);
  const [selectedPeriod, setSelectedPeriod] = useState<string>('30');
  const [dateRange, setDateRange] = useState({
    startDate: '',
    endDate: ''
  });

  const periodOptions = [
    { value: '7', label: 'Últimos 7 dias' },
    { value: '15', label: 'Últimos 15 dias' },
    { value: '30', label: 'Últimos 30 dias' },
    { value: '60', label: 'Últimos 60 dias' },
    { value: '90', label: 'Últimos 90 dias' },
    { value: '180', label: 'Últimos 180 dias' },
    { value: '365', label: 'Último ano' },
    { value: '730', label: 'Últimos 2 anos' },
    { value: '1095', label: 'Últimos 3 anos' },
    { value: 'custom', label: 'Período customizado' }
  ];

  const handlePeriodChange = (period: string) => {
    setSelectedPeriod(period);
    
    if (period === 'custom') {
      // Período customizado - não altera as datas, deixa o usuário escolher
      return;
    }
    
    const endDate = new Date();
    const startDate = new Date();
    startDate.setDate(endDate.getDate() - parseInt(period));
    
    setDateRange({
      startDate: startDate.toISOString().split('T')[0],
      endDate: endDate.toISOString().split('T')[0]
    });
  };

  const loadDashboardData = async () => {
    try {
      setLoading(true);
      const params = new URLSearchParams();
      if (dateRange.startDate) params.append('startDate', dateRange.startDate);
      if (dateRange.endDate) params.append('endDate', dateRange.endDate);
      
      const response = await api.get(`/dashboard?${params.toString()}`);
      setData(response.data);
    } catch (error) {
      toast.error('Erro ao carregar dados do dashboard');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadDashboardData();
  }, [dateRange]);

  useEffect(() => {
    // Carregar dados iniciais com período padrão
    handlePeriodChange('30');
  }, []);

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-purple-500"></div>
      </div>
    );
  }

  if (!data) {
    return (
      <div className="text-center text-white">
        <p>Erro ao carregar dados do dashboard</p>
      </div>
    );
  }

  const cards = [
    {
      title: 'Assuntos',
      value: data.totalSubjects,
      icon: Search,
      color: 'from-blue-500 to-blue-600'
    },
    {
      title: 'Categorias',
      value: data.totalCategories,
      icon: Tags,
      color: 'from-green-500 to-green-600'
    },
    {
      title: 'Sites',
      value: data.totalSites,
      icon: Globe,
      color: 'from-purple-500 to-purple-600'
    },
    {
      title: 'Anomalias',
      value: data.totalAnomalies,
      icon: AlertTriangle,
      color: 'from-red-500 to-red-600'
    }
  ];

  const COLORS = ['#3B82F6', '#10B981', '#8B5CF6', '#F59E0B', '#EF4444'];

  return (
    <div className="space-y-6">
      {/* Date Filter */}
      <div className="glass rounded-lg p-4">
        <div className="space-y-4">
          <div className="flex items-center text-white">
            <Calendar className="mr-2" size={20} />
            <span className="font-medium">Filtro por Período:</span>
          </div>
          
          {/* Period Selector */}
          <div className="flex flex-wrap items-center gap-4">
            <div className="min-w-[200px]">
              <label className="block text-sm text-gray-300 mb-1">Período</label>
              <select
                value={selectedPeriod}
                onChange={(e) => handlePeriodChange(e.target.value)}
                className="w-full px-3 py-2 bg-white/10 border border-white/20 rounded-lg text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                {periodOptions.map(option => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
            </div>
            
            {/* Custom Date Range - Only visible when custom period is selected */}
            {selectedPeriod === 'custom' && (
              <div className="flex gap-4">
                <div>
                  <label className="block text-sm text-gray-300 mb-1">Data Inicial</label>
                  <input
                    type="date"
                    value={dateRange.startDate}
                    onChange={(e) => setDateRange(prev => ({ ...prev, startDate: e.target.value }))}
                    className="px-3 py-2 bg-white/10 border border-white/20 rounded-lg text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-sm text-gray-300 mb-1">Data Final</label>
                  <input
                    type="date"
                    value={dateRange.endDate}
                    onChange={(e) => setDateRange(prev => ({ ...prev, endDate: e.target.value }))}
                    className="px-3 py-2 bg-white/10 border border-white/20 rounded-lg text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
                <div className="flex items-end">
                  <button
                    onClick={() => {
                      setDateRange({ startDate: '', endDate: '' });
                    }}
                    className="px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700 transition-colors"
                  >
                    Limpar Datas
                  </button>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {cards.map((card, index) => {
          const Icon = card.icon;
          return (
            <div key={index} className="glass rounded-lg p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-gray-300 text-sm font-medium">{card.title}</p>
                  <p className="text-3xl font-bold text-white mt-2">{card.value}</p>
                </div>
                <div className={`w-12 h-12 rounded-lg bg-gradient-to-r ${card.color} flex items-center justify-center`}>
                  <Icon className="text-white" size={24} />
                </div>
              </div>
            </div>
          );
        })}
      </div>

      {/* Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Anomalies Over Time - Line Chart */}
        <div className="glass rounded-lg p-6">
          <div className="flex items-center mb-4">
            <TrendingUp className="mr-2 text-purple-400" size={20} />
            <h3 className="text-lg font-semibold text-white">Tendência de Anomalias</h3>
          </div>
          <div className="h-64">
            <ResponsiveContainer width="100%" height="100%">
              <AreaChart data={data.anomaliesChart}>
                <defs>
                  <linearGradient id="colorAnomaly" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#8B5CF6" stopOpacity={0.8}/>
                    <stop offset="95%" stopColor="#8B5CF6" stopOpacity={0.1}/>
                  </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
                <XAxis 
                  dataKey="date" 
                  stroke="#9CA3AF"
                  fontSize={12}
                  tickFormatter={(value) => new Date(value).toLocaleDateString('pt-BR')}
                />
                <YAxis stroke="#9CA3AF" fontSize={12} />
                <Tooltip 
                  contentStyle={{ 
                    backgroundColor: '#1F2937', 
                    border: '1px solid #374151',
                    borderRadius: '8px',
                    color: '#F3F4F6'
                  }}
                  labelFormatter={(value) => `Data: ${new Date(value).toLocaleDateString('pt-BR')}`}
                />
                <Area 
                  type="monotone" 
                  dataKey="count" 
                  stroke="#8B5CF6" 
                  fillOpacity={1} 
                  fill="url(#colorAnomaly)" 
                  strokeWidth={2}
                />
              </AreaChart>
            </ResponsiveContainer>
          </div>
        </div>

        {/* Top Sites - Bar Chart */}
        <div className="glass rounded-lg p-6">
          <h3 className="text-lg font-semibold text-white mb-4">Top Sites com Mais Anomalias</h3>
          <div className="h-64">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={data.topSites.slice(0, 6)} layout="horizontal">
                <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
                <XAxis type="number" stroke="#9CA3AF" fontSize={12} />
                <YAxis 
                  type="category" 
                  dataKey="siteName" 
                  stroke="#9CA3AF" 
                  fontSize={12}
                  width={120}
                />
                <Tooltip 
                  contentStyle={{ 
                    backgroundColor: '#1F2937', 
                    border: '1px solid #374151',
                    borderRadius: '8px',
                    color: '#F3F4F6'
                  }}
                />
                <Bar dataKey="anomaliesCount" fill="#10B981" radius={[0, 4, 4, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>
      </div>

      {/* Top Subjects */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Subjects Pie Chart */}
        <div className="glass rounded-lg p-6">
          <h3 className="text-lg font-semibold text-white mb-4">Distribuição dos Assuntos</h3>
          <div className="h-64">
            {data.topSubjects.length > 0 ? (
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie
                    data={data.topSubjects.slice(0, 5)}
                    cx="50%"
                    cy="50%"
                    outerRadius={80}
                    fill="#8884d8"
                    dataKey="anomaliesCount"
                    label={({ subjectName, anomaliesCount }) => `${subjectName}: ${anomaliesCount}`}
                  >
                    {data.topSubjects.slice(0, 5).map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                    ))}
                  </Pie>
                  <Tooltip 
                    contentStyle={{ 
                      backgroundColor: '#1F2937', 
                      border: '1px solid #374151',
                      borderRadius: '8px',
                      color: '#F3F4F6'
                    }}
                  />
                </PieChart>
              </ResponsiveContainer>
            ) : (
              <div className="flex items-center justify-center h-full text-gray-400">
                Nenhum dado disponível
              </div>
            )}
          </div>
        </div>

        {/* Subjects List */}
        <div className="glass rounded-lg p-6">
          <h3 className="text-lg font-semibold text-white mb-4">Ranking de Assuntos</h3>
          <div className="space-y-3 max-h-64 overflow-y-auto">
            {data.topSubjects.slice(0, 10).map((subject, index) => (
              <div key={index} className="flex items-center justify-between p-3 bg-white/5 rounded-lg">
                <div className="flex items-center">
                  <div 
                    className="w-4 h-4 rounded-full mr-3"
                    style={{ backgroundColor: COLORS[index % COLORS.length] }}
                  ></div>
                  <span className="text-gray-300 text-sm">{subject.subjectName}</span>
                </div>
                <span className="text-white font-semibold">{subject.anomaliesCount}</span>
              </div>
            ))}
            {data.topSubjects.length === 0 && (
              <div className="text-gray-400 text-center py-8">
                Nenhum dado disponível
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;
import React from 'react';
import { Outlet, useNavigate, useLocation } from 'react-router-dom';
import { 
  LayoutDashboard, 
  Search, 
  Tags, 
  Globe, 
  AlertTriangle, 
  LogOut,
  Menu,
  X,
  Play
} from 'lucide-react';
import { authService } from '../services/authService';
import { crawlerService } from '../services/crawlerService';
import { useState } from 'react';
import toast from 'react-hot-toast';

const Layout: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [isStartingCrawl, setIsStartingCrawl] = useState(false);
  const user = authService.getCurrentUser();

  const handleLogout = () => {
    authService.logout();
    navigate('/login');
  };

  const handleStartCrawling = async () => {
    if (isStartingCrawl) return;
    
    setIsStartingCrawl(true);
    try {
      const result = await crawlerService.triggerCrawlForAllSites();
      
      if (result.success) {
        toast.success(result.message);
      } else {
        toast.error(result.message);
      }
    } catch (error: any) {
      console.error('Erro ao iniciar crawling:', error);
      toast.error('Erro ao iniciar o crawling. Tente novamente.');
    } finally {
      setIsStartingCrawl(false);
    }
  };

  const menuItems = [
    { path: '/dashboard', icon: LayoutDashboard, label: 'Dashboard' },
    { path: '/categories', icon: Tags, label: 'Categorias de Sites' },
    { path: '/subjects', icon: Search, label: 'Assuntos a Pesquisar' },
    { path: '/sites', icon: Globe, label: 'Sites' },
    { path: '/anomalies', icon: AlertTriangle, label: 'Anomalias' },
  ];

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-900 via-purple-900 to-slate-900 flex">
      {/* Mobile sidebar backdrop */}
      {sidebarOpen && (
        <div
          className="fixed inset-0 z-40 bg-black bg-opacity-50 lg:hidden"
          onClick={() => setSidebarOpen(false)}
        />
      )}

      {/* Sidebar */}
      <div className={`fixed inset-y-0 left-0 z-50 w-64 glass lg:translate-x-0 transition-transform duration-200 ease-in-out ${
        sidebarOpen ? 'translate-x-0' : '-translate-x-full'
      } lg:relative lg:flex lg:flex-col`}>
        <div className="flex flex-col h-full">
          {/* Logo */}
          <div className="flex items-center justify-between h-16 px-6">
            <h1 className="text-xl font-bold text-white">Web Grande Irmão</h1>
            <button
              onClick={() => setSidebarOpen(false)}
              className="lg:hidden text-white hover:text-gray-300"
            >
              <X size={24} />
            </button>
          </div>

          {/* Navigation */}
          <nav className="flex-1 px-4 space-y-2">
            {menuItems.map((item) => {
              const Icon = item.icon;
              const isActive = location.pathname === item.path;
              
              return (
                <button
                  key={item.path}
                  onClick={() => {
                    navigate(item.path);
                    setSidebarOpen(false);
                  }}
                  className={`w-full flex items-center px-4 py-3 text-left rounded-lg transition-all duration-200 ${
                    isActive
                      ? 'bg-white/20 text-white border border-white/30'
                      : 'text-gray-300 hover:bg-white/10 hover:text-white'
                  }`}
                >
                  <Icon size={20} className="mr-3" />
                  {item.label}
                </button>
              );
            })}
            
            {/* Divider */}
            <div className="border-t border-white/20 my-4"></div>
            
            {/* Crawler Action Button */}
            <button
              onClick={handleStartCrawling}
              disabled={isStartingCrawl}
              className={`w-full flex items-center px-4 py-3 text-left rounded-lg transition-all duration-200 ${
                isStartingCrawl
                  ? 'bg-gray-600 text-gray-400 cursor-not-allowed'
                  : 'bg-green-600 hover:bg-green-700 text-white border border-green-500'
              }`}
            >
              <Play size={20} className={`mr-3 ${isStartingCrawl ? 'animate-pulse' : ''}`} />
              {isStartingCrawl ? 'Iniciando Crawling...' : 'Iniciar Crawling'}
            </button>
          </nav>

          {/* User info and logout */}
          <div className="p-4 border-t border-white/20">
            <div className="mb-3">
              <p className="text-sm text-gray-300">Conectado como:</p>
              <p className="text-white font-medium">{user?.name}</p>
            </div>
            <button
              onClick={handleLogout}
              className="w-full flex items-center px-4 py-2 text-gray-300 hover:text-white hover:bg-white/10 rounded-lg transition-all duration-200"
            >
              <LogOut size={18} className="mr-3" />
              Sair
            </button>
          </div>
        </div>
      </div>

      {/* Main content */}
      <div className="flex-1 lg:ml-0">
        {/* Top bar */}
        <div className="glass border-b border-white/20">
          <div className="flex items-center justify-between h-16 px-6">
            <button
              onClick={() => setSidebarOpen(true)}
              className="lg:hidden text-white hover:text-gray-300"
            >
              <Menu size={24} />
            </button>
            <div className="text-white">
              <h2 className="text-lg font-semibold">
                {menuItems.find(item => item.path === location.pathname)?.label || 'Sistema de Monitoramento'}
              </h2>
            </div>
          </div>
        </div>

        {/* Page content */}
        <main className="p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
};

export default Layout;
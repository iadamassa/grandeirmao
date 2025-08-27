import React, { useState, useEffect } from 'react';
import { 
  Plus, 
  Search, 
  Eye,
  Filter,
  Calendar,
  X,
  Download,
  ExternalLink
} from 'lucide-react';
import { anomaliesService, Anomaly, AnomalyFilters, PagedResult } from '../services/anomaliesService';
import { subjectsService } from '../services/subjectsService';
import { sitesService } from '../services/sitesService';
import { siteLinksService } from '../services/siteLinksService';
import { SubjectToResearch, Site, SiteLink } from '../types';
import Pagination from '../components/Pagination';
import toast from 'react-hot-toast';

const Anomalies: React.FC = () => {
  const [anomaliesData, setAnomaliesData] = useState<PagedResult<Anomaly> | null>(null);
  const [subjects, setSubjects] = useState<SubjectToResearch[]>([]);
  const [sites, setSites] = useState<Site[]>([]);
  const [siteLinks, setSiteLinks] = useState<SiteLink[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [showModal, setShowModal] = useState(false);
  const [selectedAnomaly, setSelectedAnomaly] = useState<Anomaly | null>(null);
  const [showFilters, setShowFilters] = useState(false);
  
  const [filters, setFilters] = useState<AnomalyFilters>({});
  
  const [formData, setFormData] = useState({
    siteLinkId: 0,
    subjectToResearchId: 0,
    estimatedDateTime: '',
    identifiedSubject: '',
    exampleOrReason: ''
  });
  const [selectedSiteId, setSelectedSiteId] = useState<number>(0);

  const loadAnomalies = async () => {
    try {
      setLoading(true);
      const data = await anomaliesService.getAll(filters, currentPage, pageSize, searchTerm);
      console.log('Anomalies loaded:', data); // Debug log
      setAnomaliesData(data);
    } catch (error) {
      console.error('Error loading anomalies:', error); // Debug log
      toast.error('Erro ao carregar anomalias');
    } finally {
      setLoading(false);
    }
  };

  const loadSubjects = async () => {
    try {
      const data = await subjectsService.getAll(true, 1, 1000);
      setSubjects(data.data);
    } catch (error) {
      toast.error('Erro ao carregar assuntos');
    }
  };

  const loadSites = async () => {
    try {
      const data = await sitesService.getAll(true, undefined, 1, 1000);
      setSites(data.data);
    } catch (error) {
      toast.error('Erro ao carregar sites');
    }
  };

  const loadSiteLinks = async (siteId?: number) => {
    if (!siteId) {
      setSiteLinks([]);
      return;
    }
    
    try {
      const data = await siteLinksService.getBySiteId(siteId, true);
      setSiteLinks(data);
    } catch (error) {
      toast.error('Erro ao carregar links do site');
    }
  };

  useEffect(() => {
    loadAnomalies();
  }, [filters, currentPage, pageSize, searchTerm]);

  useEffect(() => {
    loadSubjects();
    loadSites();
  }, []);

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  const handlePageSizeChange = (size: number) => {
    setPageSize(size);
    setCurrentPage(1);
  };

  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchTerm(e.target.value);
    setCurrentPage(1);
  };

  const handleCreate = () => {
    setSelectedAnomaly(null);
    setFormData({
      siteLinkId: 0,
      subjectToResearchId: 0,
      estimatedDateTime: new Date().toISOString().slice(0, 16),
      identifiedSubject: '',
      exampleOrReason: ''
    });
    setSelectedSiteId(0);
    setSiteLinks([]);
    setShowModal(true);
  };


  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await anomaliesService.create(formData);
      toast.success('Anomalia criada com sucesso!');
      setShowModal(false);
      loadAnomalies();
    } catch (error) {
      toast.error('Erro ao salvar anomalia');
    }
  };


  const handleSiteChange = (siteId: number) => {
    setSelectedSiteId(siteId);
    setFormData({ ...formData, siteLinkId: 0 });
    loadSiteLinks(siteId);
  };

  const applyFilters = () => {
    loadAnomalies();
    setShowFilters(false);
  };

  const clearFilters = () => {
    setFilters({});
    setShowFilters(false);
  };

  const exportToExcel = async () => {
    try {
      const blob = await anomaliesService.exportToExcel(filters);
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `anomalias_${new Date().toISOString().slice(0, 10)}.xlsx`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      toast.success('Excel exportado com sucesso!');
    } catch (error) {
      toast.error('Erro ao exportar Excel');
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div className="flex gap-2">
          <button
            onClick={exportToExcel}
            className="flex items-center px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-all duration-200"
          >
            <Download size={20} className="mr-2" />
            Exportar Excel
          </button>
          <button
            onClick={handleCreate}
            className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg flex items-center gap-2"
          >
            <Plus size={20} />
            Nova Anomalia
          </button>
        </div>
      </div>

      <div className="glass rounded-xl border border-white/20 overflow-hidden">
        <div className="p-4 border-b border-white/20">
          <div className="flex flex-col sm:flex-row gap-4">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-3 h-4 w-4 text-gray-400" />
              <input
                type="text"
                placeholder="Buscar anomalias..."
                value={searchTerm}
                onChange={handleSearchChange}
                className="pl-9 pr-4 py-2 w-full bg-white/10 border border-white/20 rounded-lg text-white placeholder-gray-400 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              />
            </div>
            <button
              onClick={() => setShowFilters(!showFilters)}
              className="px-4 py-2 border border-white/20 rounded-lg hover:bg-white/10 text-white flex items-center gap-2"
            >
              <Filter size={16} />
              Filtros
            </button>
          </div>

          {showFilters && (
            <div className="mt-4 p-4 bg-white/10 rounded-lg">
              <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-1">
                    Data Inicial
                  </label>
                  <input
                    type="datetime-local"
                    value={filters.startDate || ''}
                    onChange={(e) => setFilters({ ...filters, startDate: e.target.value })}
                    className="w-full px-3 py-2 bg-white/10 border border-white/20 rounded-lg text-white focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-1">
                    Data Final
                  </label>
                  <input
                    type="datetime-local"
                    value={filters.endDate || ''}
                    onChange={(e) => setFilters({ ...filters, endDate: e.target.value })}
                    className="w-full px-3 py-2 bg-white/10 border border-white/20 rounded-lg text-white focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-1">
                    Assunto
                  </label>
                  <select
                    value={filters.subjectToResearchId || ''}
                    onChange={(e) => setFilters({ ...filters, subjectToResearchId: e.target.value ? parseInt(e.target.value) : undefined })}
                    className="w-full px-3 py-2 bg-white/10 border border-white/20 rounded-lg text-white focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  >
                    <option value="">Todos os assuntos</option>
                    {subjects.map(subject => (
                      <option key={subject.id} value={subject.id}>
                        {subject.name}
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-1">
                    Site
                  </label>
                  <select
                    value={filters.siteId || ''}
                    onChange={(e) => setFilters({ ...filters, siteId: e.target.value ? parseInt(e.target.value) : undefined })}
                    className="w-full px-3 py-2 bg-white/10 border border-white/20 rounded-lg text-white focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  >
                    <option value="">Todos os sites</option>
                    {sites.map(site => (
                      <option key={site.id} value={site.id}>
                        {site.name}
                      </option>
                    ))}
                  </select>
                </div>
              </div>
              <div className="flex gap-2 mt-4">
                <button
                  onClick={applyFilters}
                  className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                >
                  Aplicar Filtros
                </button>
                <button
                  onClick={clearFilters}
                  className="px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700"
                >
                  Limpar Filtros
                </button>
              </div>
            </div>
          )}
        </div>

        <div className="overflow-x-auto">
          <table className="w-full">
            <thead className="bg-white/10">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-300 uppercase tracking-wider">
                  Data/Hora Estimada
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-300 uppercase tracking-wider">
                  Assunto
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-300 uppercase tracking-wider">
                  Site
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-300 uppercase tracking-wider">
                  Link
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-300 uppercase tracking-wider">
                  Assunto Identificado
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-300 uppercase tracking-wider">
                  Exemplo/Motivo
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-300 uppercase tracking-wider">
                  Ações
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-white/10">
              {anomaliesData?.data?.map((anomaly) => (
                <tr key={anomaly.id} className="hover:bg-white/5">
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-white">
                    {new Date(anomaly.estimatedDateTime).toLocaleString('pt-BR')}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-white">
                    {anomaly.subjectName}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-white">
                    {anomaly.siteName}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-white">
                    <a 
                      href={anomaly.siteLinkUrl} 
                      target="_blank" 
                      rel="noopener noreferrer"
                      className="text-blue-400 hover:text-blue-300 underline flex items-center"
                    >
                      {anomaly.siteLinkName}
                      <ExternalLink size={12} className="ml-1" />
                    </a>
                  </td>
                  <td className="px-6 py-4 text-sm text-white">
                    {anomaly.identifiedSubject}
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-300">
                    {anomaly.exampleOrReason}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                    <div className="flex justify-end space-x-2">
                      {/* Anomalias não podem ser editadas nem excluídas */}
                    </div>
                  </td>
                </tr>
              ))}
              {(!anomaliesData?.data || anomaliesData.data.length === 0) && (
                <tr>
                  <td colSpan={7} className="px-6 py-4 text-center text-gray-300">
                    Nenhuma anomalia encontrada
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      {showModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="glass rounded-xl border border-white/20 max-w-2xl w-full max-h-[90vh] overflow-y-auto">
            <div className="flex justify-between items-center p-6 border-b border-white/20">
              <h2 className="text-xl font-semibold text-white">
                Nova Anomalia
              </h2>
              <button
                onClick={() => setShowModal(false)}
                className="text-gray-400 hover:text-gray-300"
              >
                <X size={24} />
              </button>
            </div>

            <form onSubmit={handleSubmit} className="p-6">
              <div className="grid grid-cols-1 gap-6">
                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Assunto para Pesquisa *
                  </label>
                  <select
                    value={formData.subjectToResearchId}
                    onChange={(e) => setFormData({ ...formData, subjectToResearchId: parseInt(e.target.value) })}
                    className="w-full px-3 py-2 bg-white/10 border border-white/20 rounded-lg text-white focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    required
                  >
                    <option value={0}>Selecione um assunto</option>
                    {subjects.map(subject => (
                      <option key={subject.id} value={subject.id}>
                        {subject.name}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Site *
                  </label>
                  <select
                    value={selectedSiteId}
                    onChange={(e) => handleSiteChange(parseInt(e.target.value))}
                    className="w-full px-3 py-2 bg-white/10 border border-white/20 rounded-lg text-white focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    required
                  >
                    <option value={0}>Selecione um site</option>
                    {sites.map(site => (
                      <option key={site.id} value={site.id}>
                        {site.name}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Link do Site *
                  </label>
                  <select
                    value={formData.siteLinkId}
                    onChange={(e) => setFormData({ ...formData, siteLinkId: parseInt(e.target.value) })}
                    className="w-full px-3 py-2 bg-white/10 border border-white/20 rounded-lg text-white focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    required
                  >
                    <option value={0}>Selecione um link</option>
                    {siteLinks.map(link => (
                      <option key={link.id} value={link.id}>
                        {link.name}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Data/Hora Estimada *
                  </label>
                  <input
                    type="datetime-local"
                    value={formData.estimatedDateTime}
                    onChange={(e) => setFormData({ ...formData, estimatedDateTime: e.target.value })}
                    className="w-full px-3 py-2 bg-white/10 border border-white/20 rounded-lg text-white focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    required
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Assunto Identificado *
                  </label>
                  <input
                    type="text"
                    value={formData.identifiedSubject}
                    onChange={(e) => setFormData({ ...formData, identifiedSubject: e.target.value })}
                    className="w-full px-3 py-2 bg-white/10 border border-white/20 rounded-lg text-white placeholder-gray-400 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    required
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Exemplo ou Motivo *
                  </label>
                  <textarea
                    value={formData.exampleOrReason}
                    onChange={(e) => setFormData({ ...formData, exampleOrReason: e.target.value })}
                    rows={4}
                    className="w-full px-3 py-2 bg-white/10 border border-white/20 rounded-lg text-white placeholder-gray-400 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                    required
                  />
                </div>
              </div>

              <div className="flex justify-end gap-3 mt-6">
                <button
                  type="button"
                  onClick={() => setShowModal(false)}
                  className="px-4 py-2 border border-white/20 rounded-lg text-gray-300 hover:bg-white/10"
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
                >
                  Criar
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Pagination */}
      {anomaliesData && anomaliesData.totalCount > 0 && (
        <Pagination
          currentPage={anomaliesData.pageNumber}
          totalPages={anomaliesData.totalPages}
          pageSize={anomaliesData.pageSize}
          totalCount={anomaliesData.totalCount}
          onPageChange={handlePageChange}
          onPageSizeChange={handlePageSizeChange}
        />
      )}
    </div>
  );
};

export default Anomalies;
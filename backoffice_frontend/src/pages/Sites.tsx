import React, { useState, useEffect } from 'react';
import { Plus, Edit, Search, Filter, ExternalLink, X, Download, Trash2 } from 'lucide-react';
import { toast } from 'react-hot-toast';
import { Site, SiteCategory, SiteLink } from '../types';
import { sitesService, CreateSiteRequest, UpdateSiteRequest, CreateSiteLinkRequest, PagedResult } from '../services/sitesService';
import { categoriesService } from '../services/categoriesService';
import { siteLinksService } from '../services/siteLinksService';
import Pagination from '../components/Pagination';

const Sites: React.FC = () => {
  const [sitesData, setSitesData] = useState<PagedResult<Site> | null>(null);
  const [categories, setCategories] = useState<SiteCategory[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<'all' | 'active' | 'inactive'>('all');
  const [categoryFilter, setCategoryFilter] = useState<number | ''>('');
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingSite, setEditingSite] = useState<Site | null>(null);
  const [formData, setFormData] = useState({
    name: '',
    url: '',
    description: '',
    isActive: true,
    categoryIds: [] as number[]
  });

  // Estados para links
  const [siteLinks, setSiteLinks] = useState<SiteLink[]>([]);
  const [newLink, setNewLink] = useState({
    name: '',
    url: '',
    description: ''
  });

  useEffect(() => {
    loadSites();
  }, [statusFilter, categoryFilter, currentPage, pageSize, searchTerm]);

  useEffect(() => {
    loadCategories();
  }, []);

  const loadSites = async () => {
    try {
      setLoading(true);
      const isActive = statusFilter === 'all' ? undefined : statusFilter === 'active';
      const categoryId = categoryFilter === '' ? undefined : Number(categoryFilter);
      const data = await sitesService.getAll(isActive, categoryId, currentPage, pageSize, searchTerm);
      setSitesData(data);
    } catch (error) {
      toast.error('Erro ao carregar sites');
    } finally {
      setLoading(false);
    }
  };

  const loadCategories = async () => {
    try {
      const data = await categoriesService.getAll(true, 1, 1000); // Only active categories
      setCategories(data.data);
    } catch (error) {
      toast.error('Erro ao carregar categorias');
    }
  };

  const loadSiteLinks = async (siteId: number) => {
    try {
      const data = await siteLinksService.getBySiteId(siteId);
      setSiteLinks(data);
    } catch (error) {
      toast.error('Erro ao carregar links do site');
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (editingSite) {
        const request: UpdateSiteRequest = {
          id: editingSite.id,
          ...formData
        };
        await sitesService.update(request);
        toast.success('Site atualizado com sucesso!');
      } else {
        const request: CreateSiteRequest = formData;
        await sitesService.create(request);
        toast.success('Site criado com sucesso!');
      }
      
      closeModal();
      loadSites();
    } catch (error) {
      toast.error('Erro ao salvar site');
    }
  };

  const handleEdit = async (site: Site) => {
    setEditingSite(site);
    setFormData({
      name: site.name,
      url: site.url,
      description: site.description || '',
      isActive: site.isActive,
      categoryIds: site.categories.map(c => c.id)
    });
    await loadSiteLinks(site.id);
    setNewLink({ name: '', url: '', description: '' });
    setIsModalOpen(true);
  };

  const handleToggleStatus = async (site: Site) => {
    const newStatus = !site.isActive;
    const action = newStatus ? 'ativar' : 'desativar';
    
    if (window.confirm(`Tem certeza que deseja ${action} este site?`)) {
      try {
        const request: UpdateSiteRequest = {
          id: site.id,
          name: site.name,
          url: site.url,
          description: site.description || '',
          isActive: newStatus,
          categoryIds: site.categories.map(c => c.id)
        };
        await sitesService.update(request);
        toast.success(`Site ${newStatus ? 'ativado' : 'desativado'} com sucesso!`);
        loadSites();
      } catch (error) {
        toast.error(`Erro ao ${action} site`);
      }
    }
  };

  const openCreateModal = () => {
    setEditingSite(null);
    setFormData({
      name: '',
      url: '',
      description: '',
      isActive: true,
      categoryIds: []
    });
    setSiteLinks([]);
    setNewLink({ name: '', url: '', description: '' });
    setIsModalOpen(true);
  };

  const closeModal = () => {
    setIsModalOpen(false);
    setEditingSite(null);
  };

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

  const exportToExcel = async () => {
    try {
      const isActive = statusFilter === 'all' ? undefined : statusFilter === 'active';
      const categoryId = categoryFilter === '' ? undefined : Number(categoryFilter);
      const blob = await sitesService.exportToExcel(isActive, categoryId);
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `sites_${new Date().toISOString().slice(0, 10)}.xlsx`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      toast.success('Excel exportado com sucesso!');
    } catch (error) {
      toast.error('Erro ao exportar Excel');
    }
  };

  const handleCategoryChange = (categoryId: number, checked: boolean) => {
    if (checked) {
      setFormData({
        ...formData,
        categoryIds: [...formData.categoryIds, categoryId]
      });
    } else {
      setFormData({
        ...formData,
        categoryIds: formData.categoryIds.filter(id => id !== categoryId)
      });
    }
  };

  const handleAddLink = async () => {
    if (!newLink.name.trim()) {
      toast.error('Nome do link é obrigatório');
      return;
    }
    if (!newLink.url.trim()) {
      toast.error('URL do link é obrigatória');
      return;
    }

    if (!editingSite) {
      // Se está criando um novo site, adiciona o link temporariamente
      const tempLink: SiteLink = {
        id: Date.now(), // ID temporário
        name: newLink.name,
        url: newLink.url,
        description: newLink.description,
        siteId: 0,
        isActive: true,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        siteName: ''
      };
      setSiteLinks([...siteLinks, tempLink]);
      setNewLink({ name: '', url: '', description: '' });
      return;
    }

    try {
      const request: CreateSiteLinkRequest = {
        name: newLink.name,
        url: newLink.url,
        description: newLink.description,
        isActive: true
      };
      
      await sitesService.createSiteLink(editingSite.id, request);
      toast.success('Link adicionado com sucesso!');
      await loadSiteLinks(editingSite.id);
      setNewLink({ name: '', url: '', description: '' });
    } catch (error) {
      toast.error('Erro ao adicionar link');
    }
  };

  const handleDeleteLink = async (linkId: number) => {
    if (!editingSite) {
      // Remove link temporário
      setSiteLinks(siteLinks.filter(l => l.id !== linkId));
      return;
    }

    try {
      await siteLinksService.delete(linkId);
      toast.success('Link removido com sucesso!');
      await loadSiteLinks(editingSite.id);
    } catch (error) {
      toast.error('Erro ao remover link');
    }
  };


  return (
    <div className="space-y-6">
      {/* Header */}
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
            onClick={openCreateModal}
            className="flex items-center px-4 py-2 bg-gradient-to-r from-blue-500 to-purple-600 text-white rounded-lg hover:from-blue-600 hover:to-purple-700 transition-all duration-200"
          >
            <Plus size={20} className="mr-2" />
            Novo Site
          </button>
        </div>
      </div>

      {/* Filters */}
      <div className="glass p-6 rounded-xl border border-white/20">
        <div className="flex flex-col sm:flex-row gap-4">
          <div className="relative flex-1">
            <Search size={20} className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" />
            <input
              type="text"
              placeholder="Buscar sites..."
              value={searchTerm}
              onChange={handleSearchChange}
              className="w-full pl-10 pr-4 py-2 bg-white/10 border border-white/20 rounded-lg text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <div className="relative">
            <Filter size={20} className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" />
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value as 'all' | 'active' | 'inactive')}
              className="pl-10 pr-8 py-2 bg-white/10 border border-white/20 rounded-lg text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="all">Todos</option>
              <option value="active">Ativos</option>
              <option value="inactive">Inativos</option>
            </select>
          </div>
          <div className="relative">
            <select
              value={categoryFilter}
              onChange={(e) => setCategoryFilter(e.target.value === '' ? '' : Number(e.target.value))}
              className="pl-3 pr-8 py-2 bg-white/10 border border-white/20 rounded-lg text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">Todas as categorias</option>
              {categories.map(category => (
                <option key={category.id} value={category.id}>
                  {category.name}
                </option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {/* Sites Table */}
      <div className="glass rounded-xl border border-white/20 overflow-hidden">
        {loading ? (
          <div className="p-6 text-center text-white">Carregando...</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-white/10">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-300 uppercase tracking-wider">
                    Nome
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-300 uppercase tracking-wider">
                    URL
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-300 uppercase tracking-wider">
                    Categorias
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-300 uppercase tracking-wider">
                    Criado em
                  </th>
                  <th className="px-6 py-3 text-center text-xs font-medium text-gray-300 uppercase tracking-wider">
                    Ativo
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-medium text-gray-300 uppercase tracking-wider">
                    Ações
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-white/10">
                {sitesData?.data?.map((site) => (
                  <tr key={site.id} className="hover:bg-white/5">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm font-medium text-white">{site.name}</div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex items-center space-x-2">
                        <a 
                          href={site.url} 
                          target="_blank" 
                          rel="noopener noreferrer"
                          className="text-sm text-blue-400 hover:text-blue-300 max-w-xs truncate flex items-center"
                        >
                          {site.url}
                          <ExternalLink size={12} className="ml-1" />
                        </a>
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex flex-wrap gap-1">
                        {site.categories.map(category => (
                          <span 
                            key={category.id}
                            className="inline-flex px-2 py-1 text-xs font-semibold rounded-full bg-blue-100 text-blue-800"
                          >
                            {category.name}
                          </span>
                        ))}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-300">
                      {new Date(site.createdAt).toLocaleDateString('pt-BR')}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-center">
                      <label className="toggle-switch">
                        <input
                          type="checkbox"
                          checked={site.isActive}
                          onChange={() => handleToggleStatus(site)}
                        />
                        <span className="toggle-slider"></span>
                      </label>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <div className="flex justify-end space-x-2">
                        <button
                          onClick={() => handleEdit(site)}
                          className="text-blue-400 hover:text-blue-300"
                          title="Editar site"
                        >
                          <Edit size={16} />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            {(!sitesData?.data || sitesData.data.length === 0) && (
              <div className="p-6 text-center text-gray-400">
                Nenhum site encontrado
              </div>
            )}
          </div>
        )}
      </div>

      {/* Modal */}
      {isModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black bg-opacity-50">
          <div className="glass rounded-xl border border-white/20 w-full max-w-6xl max-h-[90vh] overflow-y-auto">
            <div className="p-6">
              <div className="flex justify-between items-center mb-6">
                <h2 className="text-xl font-bold text-white">
                  {editingSite ? 'Editar Site' : 'Novo Site'}
                </h2>
                <button
                  onClick={closeModal}
                  className="text-gray-400 hover:text-white"
                >
                  <X size={24} />
                </button>
              </div>
              
              <form onSubmit={handleSubmit} className="space-y-6">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      Nome *
                    </label>
                    <input
                      type="text"
                      required
                      value={formData.name}
                      onChange={(e) => setFormData({...formData, name: e.target.value})}
                      className="w-full px-3 py-2 bg-white/10 border border-white/20 rounded-lg text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-300 mb-2">
                      URL Principal *
                    </label>
                    <input
                      type="url"
                      required
                      value={formData.url}
                      onChange={(e) => setFormData({...formData, url: e.target.value})}
                      className="w-full px-3 py-2 bg-white/10 border border-white/20 rounded-lg text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
                      placeholder="https://example.com"
                    />
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Descrição
                  </label>
                  <textarea
                    value={formData.description}
                    onChange={(e) => setFormData({...formData, description: e.target.value})}
                    rows={3}
                    className="w-full px-3 py-2 bg-white/10 border border-white/20 rounded-lg text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Categorias
                  </label>
                  <div className="grid grid-cols-2 gap-2 max-h-32 overflow-y-auto">
                    {categories.map(category => (
                      <label key={category.id} className="flex items-center space-x-2">
                        <input
                          type="checkbox"
                          checked={formData.categoryIds.includes(category.id)}
                          onChange={(e) => handleCategoryChange(category.id, e.target.checked)}
                          className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                        />
                        <span className="text-sm text-gray-300">{category.name}</span>
                      </label>
                    ))}
                  </div>
                </div>


                {/* Seção de Links */}
                <div className="border-t border-white/20 pt-6">
                  <h3 className="text-lg font-semibold text-white mb-4">Links do Site</h3>
                  
                  {/* Form para adicionar link */}
                  <div className="glass p-4 rounded-lg border border-white/10 mb-4">
                    <div className="grid grid-cols-1 md:grid-cols-4 gap-3">
                      <div>
                        <input
                          type="text"
                          placeholder="Nome do link"
                          value={newLink.name}
                          onChange={(e) => setNewLink({...newLink, name: e.target.value})}
                          className="w-full px-3 py-2 bg-white/10 border border-white/20 rounded-lg text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
                        />
                      </div>
                      <div>
                        <input
                          type="url"
                          placeholder="URL do link"
                          value={newLink.url}
                          onChange={(e) => setNewLink({...newLink, url: e.target.value})}
                          className="w-full px-3 py-2 bg-white/10 border border-white/20 rounded-lg text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
                        />
                      </div>
                      <div>
                        <input
                          type="text"
                          placeholder="Descrição (opcional)"
                          value={newLink.description}
                          onChange={(e) => setNewLink({...newLink, description: e.target.value})}
                          className="w-full px-3 py-2 bg-white/10 border border-white/20 rounded-lg text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
                        />
                      </div>
                      <div>
                        <button
                          type="button"
                          onClick={handleAddLink}
                          className="w-full px-3 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors"
                        >
                          Adicionar
                        </button>
                      </div>
                    </div>
                  </div>

                  {/* Lista de links */}
                  <div className="space-y-2 max-h-60 overflow-y-auto">
                    {siteLinks.map((link) => (
                      <div key={link.id} className="flex items-center justify-between p-3 bg-white/5 rounded-lg">
                        <div className="flex-1">
                          <div className="text-white font-medium">{link.name}</div>
                          <a 
                            href={link.url} 
                            target="_blank" 
                            rel="noopener noreferrer"
                            className="text-blue-400 hover:text-blue-300 text-sm flex items-center"
                          >
                            {link.url}
                            <ExternalLink size={12} className="ml-1" />
                          </a>
                          {link.description && (
                            <div className="text-gray-400 text-sm">{link.description}</div>
                          )}
                        </div>
                        <button
                          type="button"
                          onClick={() => handleDeleteLink(link.id)}
                          className="text-red-400 hover:text-red-300 ml-4"
                        >
                          <Trash2 size={16} />
                        </button>
                      </div>
                    ))}
                    {siteLinks.length === 0 && (
                      <div className="text-center text-gray-400 py-4">
                        Nenhum link adicionado
                      </div>
                    )}
                  </div>
                </div>

                <div className="flex justify-end space-x-3 pt-4 border-t border-white/20">
                  <button
                    type="button"
                    onClick={closeModal}
                    className="px-4 py-2 text-gray-300 hover:text-white transition-colors"
                  >
                    Cancelar
                  </button>
                  <button
                    type="submit"
                    className="px-4 py-2 bg-gradient-to-r from-blue-500 to-purple-600 text-white rounded-lg hover:from-blue-600 hover:to-purple-700 transition-all duration-200"
                  >
                    {editingSite ? 'Atualizar' : 'Criar'}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}

      {/* Pagination */}
      {sitesData && sitesData.totalCount > 0 && (
        <Pagination
          currentPage={sitesData.pageNumber}
          totalPages={sitesData.totalPages}
          pageSize={sitesData.pageSize}
          totalCount={sitesData.totalCount}
          onPageChange={handlePageChange}
          onPageSizeChange={handlePageSizeChange}
        />
      )}
    </div>
  );
};

export default Sites;
import React, { useState, useEffect } from 'react';
import { Plus, Edit, Search, Filter, Download } from 'lucide-react';
import { toast } from 'react-hot-toast';
import { SiteCategory } from '../types';
import { categoriesService, CreateCategoryRequest, UpdateCategoryRequest, PagedResult } from '../services/categoriesService';
import Pagination from '../components/Pagination';

const Categories: React.FC = () => {
  const [categoriesData, setCategoriesData] = useState<PagedResult<SiteCategory> | null>(null);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<'all' | 'active' | 'inactive'>('all');
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingCategory, setEditingCategory] = useState<SiteCategory | null>(null);
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    isActive: true
  });

  useEffect(() => {
    loadCategories();
  }, [statusFilter, currentPage, pageSize, searchTerm]);

  const loadCategories = async () => {
    try {
      setLoading(true);
      const isActive = statusFilter === 'all' ? undefined : statusFilter === 'active';
      const data = await categoriesService.getAll(isActive, currentPage, pageSize, searchTerm);
      setCategoriesData(data);
    } catch (error) {
      toast.error('Erro ao carregar categorias');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (editingCategory) {
        const request: UpdateCategoryRequest = {
          id: editingCategory.id,
          ...formData
        };
        await categoriesService.update(request);
        toast.success('Categoria atualizada com sucesso!');
      } else {
        const request: CreateCategoryRequest = formData;
        await categoriesService.create(request);
        toast.success('Categoria criada com sucesso!');
      }
      
      closeModal();
      loadCategories();
    } catch (error) {
      toast.error('Erro ao salvar categoria');
    }
  };

  const handleEdit = (category: SiteCategory) => {
    setEditingCategory(category);
    setFormData({
      name: category.name,
      description: category.description || '',
      isActive: category.isActive
    });
    setIsModalOpen(true);
  };

  const handleToggleStatus = async (category: SiteCategory) => {
    const newStatus = !category.isActive;
    const action = newStatus ? 'ativar' : 'desativar';
    
    if (window.confirm(`Tem certeza que deseja ${action} esta categoria?`)) {
      try {
        const request: UpdateCategoryRequest = {
          id: category.id,
          name: category.name,
          description: category.description || '',
          isActive: newStatus
        };
        await categoriesService.update(request);
        toast.success(`Categoria ${newStatus ? 'ativada' : 'desativada'} com sucesso!`);
        loadCategories();
      } catch (error) {
        toast.error(`Erro ao ${action} categoria`);
      }
    }
  };

  const openCreateModal = () => {
    setEditingCategory(null);
    setFormData({
      name: '',
      description: '',
      isActive: true
    });
    setIsModalOpen(true);
  };

  const closeModal = () => {
    setIsModalOpen(false);
    setEditingCategory(null);
  };

  const exportToExcel = async () => {
    try {
      const isActive = statusFilter === 'all' ? undefined : statusFilter === 'active';
      const blob = await categoriesService.exportToExcel(isActive);
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `categorias_${new Date().toISOString().slice(0, 10)}.xlsx`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      toast.success('Excel exportado com sucesso!');
    } catch (error) {
      toast.error('Erro ao exportar Excel');
    }
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
            Nova Categoria
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
              placeholder="Buscar categorias..."
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
        </div>
      </div>

      {/* Categories Table */}
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
                    Descrição
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
                {categoriesData?.data?.map((category) => (
                  <tr key={category.id} className="hover:bg-white/5">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm font-medium text-white">{category.name}</div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="text-sm text-gray-300 max-w-xs truncate">
                        {category.description || '-'}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-300">
                      {new Date(category.createdAt).toLocaleDateString('pt-BR')}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-center">
                      <label className="toggle-switch">
                        <input
                          type="checkbox"
                          checked={category.isActive}
                          onChange={() => handleToggleStatus(category)}
                        />
                        <span className="toggle-slider"></span>
                      </label>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <div className="flex justify-end space-x-2">
                        <button
                          onClick={() => handleEdit(category)}
                          className="text-blue-400 hover:text-blue-300"
                          title="Editar categoria"
                        >
                          <Edit size={16} />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            {(!categoriesData?.data || categoriesData.data.length === 0) && (
              <div className="p-6 text-center text-gray-400">
                Nenhuma categoria encontrada
              </div>
            )}
          </div>
        )}
      </div>

      {/* Pagination */}
      {categoriesData && categoriesData.totalCount > 0 && (
        <Pagination
          currentPage={categoriesData.pageNumber}
          totalPages={categoriesData.totalPages}
          pageSize={categoriesData.pageSize}
          totalCount={categoriesData.totalCount}
          onPageChange={handlePageChange}
          onPageSizeChange={handlePageSizeChange}
        />
      )}

      {/* Modal */}
      {isModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black bg-opacity-50">
          <div className="glass rounded-xl border border-white/20 w-full max-w-md">
            <div className="p-6">
              <h2 className="text-xl font-bold text-white mb-4">
                {editingCategory ? 'Editar Categoria' : 'Nova Categoria'}
              </h2>
              
              <form onSubmit={handleSubmit} className="space-y-4">
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
                    Descrição
                  </label>
                  <textarea
                    value={formData.description}
                    onChange={(e) => setFormData({...formData, description: e.target.value})}
                    rows={3}
                    className="w-full px-3 py-2 bg-white/10 border border-white/20 rounded-lg text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>


                <div className="flex justify-end space-x-3 pt-4">
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
                    {editingCategory ? 'Atualizar' : 'Criar'}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default Categories;
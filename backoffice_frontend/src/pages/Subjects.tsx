import React, { useState, useEffect } from 'react';
import { 
  Plus, 
  Search, 
  Edit, 
  Trash2, 
  Eye,
  Filter,
  Download,
  X
} from 'lucide-react';
import { subjectsService, PagedResult } from '../services/subjectsService';
import { subjectExamplesService, CreateSubjectExampleRequest } from '../services/subjectExamplesService';
import { categoriesService } from '../services/categoriesService';
import { SubjectToResearch, SubjectExample, SiteCategory } from '../types';
import Pagination from '../components/Pagination';
import toast from 'react-hot-toast';

const Subjects: React.FC = () => {
  const [pagedResult, setPagedResult] = useState<PagedResult<SubjectToResearch> | null>(null);
  const [categories, setCategories] = useState<SiteCategory[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [selectedSubject, setSelectedSubject] = useState<SubjectToResearch | null>(null);
  const [filterActive, setFilterActive] = useState<boolean | undefined>(undefined);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    isActive: true,
    categoryIds: [] as number[]
  });

  // Estados para exemplos
  const [examples, setExamples] = useState<SubjectExample[]>([]);
  const [newExample, setNewExample] = useState({
    name: '',
    description: ''
  });

  const loadSubjects = async () => {
    try {
      setLoading(true);
      console.log('Loading subjects with params:', { filterActive, currentPage, pageSize, searchTerm });
      const data = await subjectsService.getAll(filterActive, currentPage, pageSize, searchTerm || undefined);
      console.log('Subjects loaded:', data);
      setPagedResult(data);
    } catch (error) {
      console.error('Erro ao carregar assuntos:', error);
      toast.error('Erro ao carregar assuntos');
    } finally {
      setLoading(false);
    }
  };

  const handlePageChange = (page: number) => {
    console.log('Page changed to:', page);
    setCurrentPage(page);
  };

  const handlePageSizeChange = (newPageSize: number) => {
    console.log('Page size changed to:', newPageSize);
    setPageSize(newPageSize);
    setCurrentPage(1); // Reset to first page when changing page size
  };

  const handleSearchChange = (value: string) => {
    setSearchTerm(value);
    setCurrentPage(1); // Reset to first page when searching
  };

  const loadCategories = async () => {
    try {
      const data = await categoriesService.getAll(true, 1, 1000); // Only active categories
      setCategories(data.data);
    } catch (error) {
      toast.error('Erro ao carregar categorias');
    }
  };

  const loadExamples = async (subjectId: number) => {
    try {
      const data = await subjectExamplesService.getBySubjectId(subjectId);
      setExamples(data);
    } catch (error) {
      toast.error('Erro ao carregar exemplos');
    }
  };

  useEffect(() => {
    loadSubjects();
  }, [filterActive, currentPage, pageSize, searchTerm]);

  useEffect(() => {
    loadCategories();
  }, []);

  const subjects = pagedResult?.data || [];

  const handleCreate = () => {
    setSelectedSubject(null);
    setFormData({
      name: '',
      description: '',
      isActive: true,
      categoryIds: []
    });
    setExamples([]);
    setNewExample({ name: '', description: '' });
    setShowModal(true);
  };

  const handleEdit = async (subject: SubjectToResearch) => {
    setSelectedSubject(subject);
    
    // Extract category IDs from the subject's categories
    const categoryIds = subject.categories ? subject.categories.map(c => c.id) : [];
    
    const newFormData = {
      name: subject.name,
      description: subject.description,
      isActive: subject.isActive,
      categoryIds: categoryIds
    };
    
    setFormData(newFormData);
    
    await loadExamples(subject.id);
    setNewExample({ name: '', description: '' });
    setShowModal(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (selectedSubject) {
        const updateData = {
          id: selectedSubject.id,
          name: formData.name,
          description: formData.description,
          isActive: formData.isActive,
          categoryIds: Array.isArray(formData.categoryIds) ? formData.categoryIds : []
        };
        
        await subjectsService.update(updateData);
        toast.success('Assunto atualizado com sucesso!');
      } else {
        const createData = {
          name: formData.name,
          description: formData.description,
          isActive: formData.isActive,
          categoryIds: Array.isArray(formData.categoryIds) ? formData.categoryIds : []
        };
        
        await subjectsService.create(createData);
        toast.success('Assunto criado com sucesso!');
      }
      
      setShowModal(false);
      loadSubjects();
    } catch (error) {
      console.error('Erro ao salvar assunto:', error);
      toast.error('Erro ao salvar assunto');
    }
  };

  const handleToggleStatus = async (subject: SubjectToResearch) => {
    const newStatus = !subject.isActive;
    const action = newStatus ? 'ativar' : 'desativar';
    
    if (window.confirm(`Tem certeza que deseja ${action} este assunto?`)) {
      try {
        await subjectsService.update({
          id: subject.id,
          name: subject.name,
          description: subject.description,
          isActive: newStatus,
          categoryIds: subject.categories && Array.isArray(subject.categories) 
            ? subject.categories.map(c => c.id) 
            : []
        });
        toast.success(`Assunto ${newStatus ? 'ativado' : 'desativado'} com sucesso!`);
        loadSubjects();
      } catch (error) {
        toast.error(`Erro ao ${action} assunto`);
      }
    }
  };

  const handleCategoryChange = (categoryId: number, checked: boolean) => {
    const newFormData = { ...formData };
    
    if (!newFormData.categoryIds) {
      newFormData.categoryIds = [];
    }
    
    if (checked) {
      if (!newFormData.categoryIds.includes(categoryId)) {
        newFormData.categoryIds.push(categoryId);
      }
    } else {
      newFormData.categoryIds = newFormData.categoryIds.filter(id => id !== categoryId);
    }
    
    setFormData(newFormData);
  };

  const handleAddExample = async () => {
    if (!newExample.name.trim()) {
      toast.error('Nome do exemplo é obrigatório');
      return;
    }

    if (!selectedSubject) {
      // Se está criando um novo assunto, adiciona o exemplo temporariamente
      const tempExample: SubjectExample = {
        id: Date.now(), // ID temporário
        title: newExample.name,
        description: newExample.description,
        example: newExample.description || '',
        subjectToResearchId: 0,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      };
      setExamples([...examples, tempExample]);
      setNewExample({ name: '', description: '' });
      return;
    }

    try {
      const request: CreateSubjectExampleRequest = {
        name: newExample.name,
        description: newExample.description,
        subjectToResearchId: selectedSubject.id
      };
      
      await subjectExamplesService.create(request);
      toast.success('Exemplo adicionado com sucesso!');
      await loadExamples(selectedSubject.id);
      setNewExample({ name: '', description: '' });
    } catch (error) {
      toast.error('Erro ao adicionar exemplo');
    }
  };

  const handleDeleteExample = async (exampleId: number) => {
    if (!selectedSubject) {
      // Remove exemplo temporário
      setExamples(examples.filter(e => e.id !== exampleId));
      return;
    }

    try {
      await subjectExamplesService.delete(exampleId);
      toast.success('Exemplo removido com sucesso!');
      await loadExamples(selectedSubject.id);
    } catch (error) {
      toast.error('Erro ao remover exemplo');
    }
  };

  const exportToExcel = async () => {
    try {
      const blob = await subjectsService.exportToExcel(filterActive);
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `assuntos_${new Date().toISOString().slice(0, 10)}.xlsx`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      toast.success('Excel exportado com sucesso!');
    } catch (error) {
      toast.error('Erro ao exportar Excel');
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
            onClick={handleCreate}
            className="flex items-center px-4 py-2 bg-gradient-to-r from-blue-500 to-purple-600 text-white rounded-lg hover:from-blue-600 hover:to-purple-700 transition-all duration-200"
          >
            <Plus size={20} className="mr-2" />
            Novo Assunto
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
              placeholder="Buscar assuntos..."
              value={searchTerm}
              onChange={(e) => handleSearchChange(e.target.value)}
              className="w-full pl-10 pr-4 py-2 bg-white/10 border border-white/20 rounded-lg text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <div className="relative">
            <Filter size={20} className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" />
            <select
              value={filterActive === undefined ? 'all' : filterActive ? 'active' : 'inactive'}
              onChange={(e) => {
                const value = e.target.value;
                setFilterActive(value === 'all' ? undefined : value === 'active');
              }}
              className="pl-10 pr-8 py-2 bg-white/10 border border-white/20 rounded-lg text-white focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="all">Todos</option>
              <option value="active">Ativos</option>
              <option value="inactive">Inativos</option>
            </select>
          </div>
        </div>
      </div>

      {/* Subjects Table */}
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
                {subjects.map((subject) => (
                  <tr key={subject.id} className="hover:bg-white/5">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="text-sm font-medium text-white">{subject.name}</div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="text-sm text-gray-300 max-w-xs truncate">
                        {subject.description}
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex flex-wrap gap-1">
                        {subject.categories && subject.categories.length > 0 ? (
                          subject.categories.map(category => (
                            <span
                              key={category.id}
                              className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                              style={{ backgroundColor: category.color + '20', color: category.color }}
                            >
                              {category.name}
                            </span>
                          ))
                        ) : (
                          <span className="text-gray-400 text-sm">Sem categorias</span>
                        )}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-300">
                      {new Date(subject.createdAt).toLocaleDateString('pt-BR')}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-center">
                      <label className="toggle-switch">
                        <input
                          type="checkbox"
                          checked={subject.isActive}
                          onChange={() => handleToggleStatus(subject)}
                        />
                        <span className="toggle-slider"></span>
                      </label>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                      <div className="flex justify-end space-x-2">
                        <button
                          onClick={() => handleEdit(subject)}
                          className="text-blue-400 hover:text-blue-300"
                          title="Editar assunto"
                        >
                          <Edit size={16} />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            {subjects.length === 0 && !loading && (
              <div className="p-6 text-center text-gray-400">
                Nenhum assunto encontrado
              </div>
            )}
          </div>
        )}
        
        {/* Pagination */}
        {pagedResult && pagedResult.totalCount > 0 && (
          <Pagination
            currentPage={pagedResult.pageNumber}
            totalPages={pagedResult.totalPages}
            pageSize={pagedResult.pageSize}
            totalCount={pagedResult.totalCount}
            onPageChange={handlePageChange}
            onPageSizeChange={handlePageSizeChange}
          />
        )}
      </div>

      {/* Modal */}
      {showModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black bg-opacity-50">
          <div className="glass rounded-xl border border-white/20 w-full max-w-4xl max-h-[90vh] overflow-y-auto">
            <div className="p-6">
              <div className="flex justify-between items-center mb-6">
                <h2 className="text-xl font-bold text-white">
                  {selectedSubject ? 'Editar Assunto' : 'Novo Assunto'}
                </h2>
                <button
                  onClick={() => setShowModal(false)}
                  className="text-gray-400 hover:text-white"
                >
                  <X size={24} />
                </button>
              </div>
              
              <form onSubmit={handleSubmit} className="space-y-6">
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

                <div>
                  <label className="block text-sm font-medium text-gray-300 mb-2">
                    Categorias
                  </label>
                  <div className="space-y-2">
                    {categories.map(category => {
                      const isChecked = formData.categoryIds && formData.categoryIds.includes(category.id);
                      
                      return (
                        <div key={category.id} className="flex items-center space-x-2 p-2 bg-white/5 rounded">
                          <input
                            type="checkbox"
                            id={`category-${category.id}`}
                            checked={isChecked || false}
                            onChange={(e) => handleCategoryChange(category.id, e.target.checked)}
                            className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                          />
                          <label htmlFor={`category-${category.id}`} className="text-sm text-gray-300">
                            {category.name}
                          </label>
                        </div>
                      );
                    })}
                  </div>
                </div>

                {/* Seção de Exemplos */}
                <div className="border-t border-white/20 pt-6">
                  <h3 className="text-lg font-semibold text-white mb-4">Exemplos do Assunto</h3>
                  
                  {/* Form para adicionar exemplo */}
                  <div className="glass p-4 rounded-lg border border-white/10 mb-4">
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
                      <div>
                        <input
                          type="text"
                          placeholder="Nome do exemplo"
                          value={newExample.name}
                          onChange={(e) => setNewExample({...newExample, name: e.target.value})}
                          className="w-full px-3 py-2 bg-white/10 border border-white/20 rounded-lg text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
                        />
                      </div>
                      <div>
                        <input
                          type="text"
                          placeholder="Descrição (opcional)"
                          value={newExample.description}
                          onChange={(e) => setNewExample({...newExample, description: e.target.value})}
                          className="w-full px-3 py-2 bg-white/10 border border-white/20 rounded-lg text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
                        />
                      </div>
                      <div>
                        <button
                          type="button"
                          onClick={handleAddExample}
                          className="w-full px-3 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors"
                        >
                          Adicionar
                        </button>
                      </div>
                    </div>
                  </div>

                  {/* Lista de exemplos */}
                  <div className="space-y-2 max-h-60 overflow-y-auto">
                    {examples.map((example) => (
                      <div key={example.id} className="flex items-center justify-between p-3 bg-white/5 rounded-lg">
                        <div>
                          <div className="text-white font-medium">{example.title}</div>
                          {example.description && (
                            <div className="text-gray-400 text-sm">{example.description}</div>
                          )}
                        </div>
                        <button
                          type="button"
                          onClick={() => handleDeleteExample(example.id)}
                          className="text-red-400 hover:text-red-300"
                        >
                          <Trash2 size={16} />
                        </button>
                      </div>
                    ))}
                    {examples.length === 0 && (
                      <div className="text-center text-gray-400 py-4">
                        Nenhum exemplo adicionado
                      </div>
                    )}
                  </div>
                </div>

                <div className="flex justify-end space-x-3 pt-4 border-t border-white/20">
                  <button
                    type="button"
                    onClick={() => setShowModal(false)}
                    className="px-4 py-2 text-gray-300 hover:text-white transition-colors"
                  >
                    Cancelar
                  </button>
                  <button
                    type="submit"
                    className="px-4 py-2 bg-gradient-to-r from-blue-500 to-purple-600 text-white rounded-lg hover:from-blue-600 hover:to-purple-700 transition-all duration-200"
                  >
                    {selectedSubject ? 'Atualizar' : 'Criar'}
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

export default Subjects;
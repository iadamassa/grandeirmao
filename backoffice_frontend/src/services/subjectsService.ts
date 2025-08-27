import api from './api';
import { SubjectToResearch } from '../types';

export interface PagedResult<T> {
  data: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export const subjectsService = {
  async getAll(isActive?: boolean, page: number = 1, pageSize: number = 10, searchTerm?: string): Promise<PagedResult<SubjectToResearch>> {
    const params = new URLSearchParams();
    if (isActive !== undefined) {
      params.append('isActive', isActive.toString());
    }
    params.append('page', page.toString());
    params.append('pageSize', pageSize.toString());
    if (searchTerm) {
      params.append('searchTerm', searchTerm);
    }
    const response = await api.get(`/subjects-to-research?${params.toString()}`);
    return response.data;
  },

  async getById(id: number): Promise<SubjectToResearch> {
    const response = await api.get(`/subjects-to-research/${id}`);
    return response.data;
  },

  async create(subject: Omit<SubjectToResearch, 'id' | 'createdAt' | 'updatedAt' | 'examples' | 'categories'> & { categoryIds?: number[] }): Promise<SubjectToResearch> {
    const response = await api.post('/subjects-to-research', subject);
    return response.data;
  },

  async update(subject: Omit<SubjectToResearch, 'createdAt' | 'updatedAt' | 'examples' | 'categories'> & { categoryIds?: number[] }): Promise<SubjectToResearch> {
    const response = await api.put(`/subjects-to-research/${subject.id}`, subject);
    return response.data;
  },

  async delete(id: number): Promise<void> {
    await api.delete(`/subjects-to-research/${id}`);
  },

  async exportToExcel(isActive?: boolean): Promise<Blob> {
    const params = new URLSearchParams();
    if (isActive !== undefined) {
      params.append('isActive', isActive.toString());
    }
    const response = await api.get(`/subjects-to-research/export/excel?${params.toString()}`, {
      responseType: 'blob'
    });
    return response.data;
  }
};
import api from './api';
import { SiteCategory } from '../types';

export interface PagedResult<T> {
  data: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface CreateCategoryRequest {
  name: string;
  description?: string;
  isActive: boolean;
}

export interface UpdateCategoryRequest {
  id: number;
  name: string;
  description?: string;
  isActive: boolean;
}

export const categoriesService = {
  async getAll(isActive?: boolean, page: number = 1, pageSize: number = 10, searchTerm?: string): Promise<PagedResult<SiteCategory>> {
    const params = new URLSearchParams();
    if (isActive !== undefined) {
      params.append('isActive', isActive.toString());
    }
    params.append('page', page.toString());
    params.append('pageSize', pageSize.toString());
    if (searchTerm) {
      params.append('searchTerm', searchTerm);
    }
    
    const response = await api.get(`/site-categories?${params.toString()}`);
    return response.data;
  },

  async getById(id: number): Promise<SiteCategory> {
    const response = await api.get(`/site-categories/${id}`);
    return response.data;
  },

  async create(request: CreateCategoryRequest): Promise<SiteCategory> {
    const response = await api.post('/site-categories', request);
    return response.data;
  },

  async update(request: UpdateCategoryRequest): Promise<SiteCategory> {
    const response = await api.put(`/site-categories/${request.id}`, request);
    return response.data;
  },

  async delete(id: number): Promise<void> {
    await api.delete(`/site-categories/${id}`);
  },

  async exportToExcel(isActive?: boolean): Promise<Blob> {
    const params = new URLSearchParams();
    if (isActive !== undefined) {
      params.append('isActive', isActive.toString());
    }
    const response = await api.get(`/site-categories/export/excel?${params.toString()}`, {
      responseType: 'blob'
    });
    return response.data;
  }
};
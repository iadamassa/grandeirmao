import api from './api';
import { Site } from '../types';

export interface PagedResult<T> {
  data: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface CreateSiteRequest {
  name: string;
  url: string;
  description?: string;
  isActive: boolean;
  categoryIds: number[];
}

export interface UpdateSiteRequest {
  id: number;
  name: string;
  url: string;
  description?: string;
  isActive: boolean;
  categoryIds: number[];
}

export interface CreateSiteLinkRequest {
  name: string;
  url: string;
  description?: string;
  isActive: boolean;
}

export const sitesService = {
  async getAll(isActive?: boolean, categoryId?: number, page: number = 1, pageSize: number = 10, searchTerm?: string): Promise<PagedResult<Site>> {
    const params = new URLSearchParams();
    if (isActive !== undefined) {
      params.append('isActive', isActive.toString());
    }
    if (categoryId !== undefined) {
      params.append('categoryId', categoryId.toString());
    }
    params.append('page', page.toString());
    params.append('pageSize', pageSize.toString());
    if (searchTerm) {
      params.append('searchTerm', searchTerm);
    }
    
    const response = await api.get(`/sites?${params.toString()}`);
    return response.data;
  },

  async getById(id: number): Promise<Site> {
    const response = await api.get(`/sites/${id}`);
    return response.data;
  },

  async create(request: CreateSiteRequest): Promise<Site> {
    const response = await api.post('/sites', request);
    return response.data;
  },

  async update(request: UpdateSiteRequest): Promise<Site> {
    const response = await api.put(`/sites/${request.id}`, request);
    return response.data;
  },

  async delete(id: number): Promise<void> {
    await api.delete(`/sites/${id}`);
  },

  async createSiteLink(siteId: number, request: CreateSiteLinkRequest): Promise<any> {
    const response = await api.post(`/sites/${siteId}/links`, request);
    return response.data;
  },

  async exportToExcel(isActive?: boolean, categoryId?: number): Promise<Blob> {
    const params = new URLSearchParams();
    if (isActive !== undefined) {
      params.append('isActive', isActive.toString());
    }
    if (categoryId !== undefined) {
      params.append('categoryId', categoryId.toString());
    }
    const response = await api.get(`/sites/export/excel?${params.toString()}`, {
      responseType: 'blob'
    });
    return response.data;
  }
};
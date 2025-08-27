import api from './api';
import { SiteLink } from '../types';

export interface CreateSiteLinkRequest {
  name: string;
  url: string;
  description?: string;
  siteId: number;
  isActive: boolean;
}

export const siteLinksService = {
  async getBySiteId(siteId: number, isActive?: boolean): Promise<SiteLink[]> {
    const params = new URLSearchParams();
    if (isActive !== undefined) {
      params.append('isActive', isActive.toString());
    }
    
    const response = await api.get(`/site-links/by-site/${siteId}?${params.toString()}`);
    return response.data;
  },

  async create(request: CreateSiteLinkRequest): Promise<SiteLink> {
    const response = await api.post('/site-links', request);
    return response.data;
  },

  async delete(id: number): Promise<void> {
    await api.delete(`/site-links/${id}`);
  }
};
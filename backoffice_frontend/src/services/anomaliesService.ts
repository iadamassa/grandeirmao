import api from './api';

export interface PagedResult<T> {
  data: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface Anomaly {
  id: number;
  siteLinkId: number;
  subjectToResearchId: number;
  estimatedDateTime: string;
  identifiedSubject: string;
  exampleOrReason: string;
  createdAt: string;
  updatedAt?: string;
  siteLinkName: string;
  siteLinkUrl: string;
  siteName: string;
  subjectName: string;
}

export interface AnomalyFilters {
  startDate?: string;
  endDate?: string;
  subjectToResearchId?: number;
  siteId?: number;
}

export const anomaliesService = {
  async getAll(filters?: AnomalyFilters, page: number = 1, pageSize: number = 10, searchTerm?: string): Promise<PagedResult<Anomaly>> {
    const params = new URLSearchParams();
    if (filters?.startDate) {
      params.append('startDate', filters.startDate);
    }
    if (filters?.endDate) {
      params.append('endDate', filters.endDate);
    }
    if (filters?.subjectToResearchId) {
      params.append('subjectToResearchId', filters.subjectToResearchId.toString());
    }
    if (filters?.siteId) {
      params.append('siteId', filters.siteId.toString());
    }
    params.append('page', page.toString());
    params.append('pageSize', pageSize.toString());
    if (searchTerm) {
      params.append('searchTerm', searchTerm);
    }
    
    const response = await api.get(`/anomalies?${params.toString()}`);
    return response.data;
  },

  async getById(id: number): Promise<Anomaly> {
    const response = await api.get(`/anomalies/${id}`);
    return response.data;
  },

  async create(anomaly: Omit<Anomaly, 'id' | 'createdAt' | 'updatedAt' | 'siteLinkName' | 'siteLinkUrl' | 'siteName' | 'subjectName'>): Promise<Anomaly> {
    const response = await api.post('/anomalies', {
      siteLinkId: anomaly.siteLinkId,
      subjectToResearchId: anomaly.subjectToResearchId,
      estimatedDateTime: anomaly.estimatedDateTime,
      identifiedSubject: anomaly.identifiedSubject,
      exampleOrReason: anomaly.exampleOrReason
    });
    return response.data;
  },

  async update(anomaly: Omit<Anomaly, 'createdAt' | 'updatedAt' | 'siteLinkName' | 'siteLinkUrl' | 'siteName' | 'subjectName'>): Promise<Anomaly> {
    const response = await api.put(`/anomalies/${anomaly.id}`, {
      id: anomaly.id,
      siteLinkId: anomaly.siteLinkId,
      subjectToResearchId: anomaly.subjectToResearchId,
      estimatedDateTime: anomaly.estimatedDateTime,
      identifiedSubject: anomaly.identifiedSubject,
      exampleOrReason: anomaly.exampleOrReason
    });
    return response.data;
  },

  async delete(id: number): Promise<void> {
    await api.delete(`/anomalies/${id}`);
  },

  async exportToExcel(filters?: AnomalyFilters): Promise<Blob> {
    const params = new URLSearchParams();
    if (filters?.startDate) {
      params.append('startDate', filters.startDate);
    }
    if (filters?.endDate) {
      params.append('endDate', filters.endDate);
    }
    if (filters?.subjectToResearchId) {
      params.append('subjectToResearchId', filters.subjectToResearchId.toString());
    }
    if (filters?.siteId) {
      params.append('siteId', filters.siteId.toString());
    }
    
    const response = await api.get(`/anomalies/export/excel?${params.toString()}`, {
      responseType: 'blob'
    });
    return response.data;
  }
};
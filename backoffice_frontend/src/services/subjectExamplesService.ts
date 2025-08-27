import api from './api';
import { SubjectExample } from '../types';

export interface CreateSubjectExampleRequest {
  name: string;
  description?: string;
  subjectToResearchId: number;
}

export const subjectExamplesService = {
  async getBySubjectId(subjectId: number): Promise<SubjectExample[]> {
    const response = await api.get(`/subject-examples/by-subject/${subjectId}`);
    return response.data;
  },

  async create(request: CreateSubjectExampleRequest): Promise<SubjectExample> {
    const response = await api.post('/subject-examples', request);
    return response.data;
  },

  async delete(id: number): Promise<void> {
    await api.delete(`/subject-examples/${id}`);
  }
};
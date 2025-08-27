import api from './api';

export interface CrawlerResponse {
  message: string;
  publishedCount: number;
  success: boolean;
}

export interface CrawlerStatus {
  message: string;
  timestamp: string;
  success: boolean;
}

class CrawlerService {
  async triggerCrawlForAllSites(): Promise<CrawlerResponse> {
    const response = await api.post<CrawlerResponse>('/crawler/trigger-all-sites');
    return response.data;
  }

  async getCrawlerStatus(): Promise<CrawlerStatus> {
    const response = await api.get<CrawlerStatus>('/crawler/status');
    return response.data;
  }
}

export const crawlerService = new CrawlerService();
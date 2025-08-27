export interface User {
  id: string;
  email: string;
  name: string;
  token: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface SubjectToResearch {
  id: number;
  name: string;
  description: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
  examples: SubjectExample[];
  categories: SiteCategory[];
}

export interface SubjectExample {
  id: number;
  subjectToResearchId: number;
  title: string;
  description: string;
  example: string;
  createdAt: string;
  updatedAt?: string;
}

export interface SiteCategory {
  id: number;
  name: string;
  description: string;
  color: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface Site {
  id: number;
  name: string;
  url: string;
  description: string;
  isActive: boolean;
  lastVerification?: string;
  createdAt: string;
  updatedAt?: string;
  links: SiteLink[];
  categories: SiteCategory[];
}

export interface SiteLink {
  id: number;
  siteId: number;
  name: string;
  url: string;
  description: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
  siteName: string;
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

export interface DashboardData {
  totalSubjects: number;
  totalCategories: number;
  totalSites: number;
  totalAnomalies: number;
  anomaliesChart: Array<{
    date: string;
    count: number;
  }>;
  topSites: Array<{
    siteName: string;
    anomaliesCount: number;
  }>;
  topSubjects: Array<{
    subjectName: string;
    anomaliesCount: number;
  }>;
}
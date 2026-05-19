export interface Translation {
  id?: number | string;
  translationKey: string;
  originalText: string;
  translation: string;
  tags?: string;
  client?: string;
  project?: string;
  projectId?: number;
}

export interface DashboardStats {
  totalKeys: number;
  translated: number;
  completion: number;
}

export interface User {
  id: string;
  name: string;
  language: string;
  role: string;
}

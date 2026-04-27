export interface Translation {
  id?: string;
  translationKey: string;
  originalText: string;
  translation: string;
  tags: string;
  client?: string;
  project?: string;
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

export interface Translation {
  id?: number | string;
  translationKey: string;
  originalText: string;
  translation: string;
  isModified?: boolean;
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

export interface Language {
  code: string;
  name: string;
}

export interface AddTranslationRequest {
  keyId: number;
  value: string;
  languageCode: string;
}

export interface User {
  id: string;
  name: string;
  language: string;
  role: string;
}

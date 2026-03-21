import { api } from './client';

export interface Book {
  id: number;
  title: string;
  author: string;
  totalPages: number;
}

export const getBooks = (): Promise<Book[]> => api.get('/api/books');

export const createBook = (book: Omit<Book, 'id'>) =>
  api.post('/api/books', book);

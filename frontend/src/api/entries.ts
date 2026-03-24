import { api } from './client';
import type { Book } from './books';

export type ReadingStatus = 'WantToRead' | 'Reading' | 'Finished';

export interface ReadingEntry {
  id: number;
  userId: number;
  bookId: number;
  status: ReadingStatus;
  pagesRead: number;
  rating?: number;
  startDate?: string;
  finishDate?: string;
  book: Book;
}

export const getUserEntries = (userId: number): Promise<ReadingEntry[]> =>
  api.get(`/api/users/${userId}/entries`);

export const addEntry = (userId: number, bookId: number) =>
  api.post('/api/readingentries', { userId, bookId });

export const changeStatus = (entryId: number, status: ReadingStatus) =>
  api.patch(`/api/readingentries/${entryId}/status`, { status });

export const setRating = (entryId: number, rating: number) =>
  api.patch(`/api/readingentries/${entryId}/rating`, { rating });

export const updateProgress = (entryId: number, pagesRead: number) =>
  api.patch(`/api/readingentries/${entryId}/progress`, { pagesRead });

export const setStartDate = (entryId: number, startDate: string) =>
  api.patch(`/api/readingentries/${entryId}/start-date`, { startDate });

export const setFinishDate = (entryId: number, finishDate: string) =>
  api.patch(`/api/readingentries/${entryId}/finish-date`, { finishDate });

export const deleteEntry = (entryId: number) =>
  api.delete(`/api/readingentries/${entryId}`);

import { useEffect, useState } from 'react';
import type { Book } from '../api/books';
import { getBooks } from '../api/books';
import type { ReadingEntry, ReadingStatus } from '../api/entries';
import { addEntry, changeStatus, getUserEntries, setRating, updateProgress } from '../api/entries';

const USER_ID = 1;

const STATUS_LABELS: Record<ReadingStatus, string> = {
  WantToRead: 'Want to Read',
  Reading: 'Reading',
  Finished: 'Finished',
};

const NEXT_STATUS: Partial<Record<ReadingStatus, ReadingStatus>> = {
  WantToRead: 'Reading',
  Reading: 'Finished',
};

export default function ReadingListPage() {
  const [entries, setEntries] = useState<ReadingEntry[]>([]);
  const [books, setBooks] = useState<Book[]>([]);
  const [selectedBookId, setSelectedBookId] = useState('');
  const [error, setError] = useState('');

  const load = () => getUserEntries(USER_ID).then(setEntries);

  useEffect(() => {
    load();
    getBooks().then(setBooks);
  }, []);

  const handleAdd = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    const res = await addEntry(USER_ID, Number(selectedBookId));
    if (res.ok) { setSelectedBookId(''); load(); }
    else { setError('Already in your list or invalid book.'); }
  };

  const handleStatusChange = async (entry: ReadingEntry) => {
    const next = NEXT_STATUS[entry.status];
    if (!next) return;
    await changeStatus(entry.id, next);
    load();
  };

  const handleRating = async (entry: ReadingEntry, rating: number) => {
    const res = await setRating(entry.id, rating);
    if (res.ok) load();
  };

  const handleProgress = async (entry: ReadingEntry, pages: number) => {
    const res = await updateProgress(entry.id, pages);
    if (res.ok) load();
    else alert('Pages exceed total pages of the book.');
  };

  return (
    <div className="page">
      <h1>My Reading List</h1>

      <form onSubmit={handleAdd} className="form">
        <select value={selectedBookId} onChange={(e) => setSelectedBookId(e.target.value)} required>
          <option value="">Select a book...</option>
          {books.map((b) => (
            <option key={b.id} value={b.id}>{b.title} — {b.author}</option>
          ))}
        </select>
        <button type="submit">Add to List</button>
        {error && <p className="error">{error}</p>}
      </form>

      <table>
        <thead>
          <tr><th>Book</th><th>Status</th><th>Progress</th><th>Rating</th><th>Actions</th></tr>
        </thead>
        <tbody>
          {entries.map((e) => (
            <tr key={e.id}>
              <td>{e.book.title}</td>
              <td>{STATUS_LABELS[e.status]}</td>
              <td>
                {e.status === 'Reading' && (
                  <input
                    type="number"
                    defaultValue={e.pagesRead}
                    min={0}
                    max={e.book.totalPages}
                    onBlur={(ev) => handleProgress(e, Number(ev.target.value))}
                    style={{ width: 60 }}
                  />
                )}
                {e.status !== 'Reading' && `${e.pagesRead} / ${e.book.totalPages}`}
              </td>
              <td>
                {e.status === 'Finished' && (
                  <select
                    value={e.rating ?? ''}
                    onChange={(ev) => handleRating(e, Number(ev.target.value))}
                  >
                    <option value="">—</option>
                    {[1, 2, 3, 4, 5].map((r) => <option key={r} value={r}>{'★'.repeat(r)}</option>)}
                  </select>
                )}
              </td>
              <td>
                {NEXT_STATUS[e.status] && (
                  <button onClick={() => handleStatusChange(e)}>
                    → {STATUS_LABELS[NEXT_STATUS[e.status]!]}
                  </button>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

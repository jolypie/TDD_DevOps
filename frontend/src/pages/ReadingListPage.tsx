import { useEffect, useState } from 'react';
import type { Book } from '../api/books';
import { getBooks } from '../api/books';
import type { ReadingEntry, ReadingStatus } from '../api/entries';
import {
  addEntry,
  changeStatus,
  deleteEntry,
  getUserEntries,
  setFinishDate,
  setRating,
  setStartDate,
  updateProgress,
} from '../api/entries';

const USER_ID = 1;

const STATUS_LABELS: Record<ReadingStatus, string> = {
  WantToRead: 'Want to Read',
  Reading: 'Reading',
  Finished: 'Finished',
};

const STATUS_DOT: Record<ReadingStatus, string> = {
  WantToRead: '○',
  Reading: '◉',
  Finished: '●',
};

const ALL_STATUSES: ReadingStatus[] = ['WantToRead', 'Reading', 'Finished'];

const toInputDate = (iso?: string) => iso ? iso.split('T')[0] : '';
const today = () => new Date().toISOString().split('T')[0];

export default function ReadingListPage() {
  const [entries, setEntries] = useState<ReadingEntry[]>([]);
  const [books, setBooks] = useState<Book[]>([]);
  const [selectedBookId, setSelectedBookId] = useState('');
  const [error, setError] = useState('');
  const [progressValues, setProgressValues] = useState<Record<number, string>>({});

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
    else { setError('Already in your list or invalid selection.'); }
  };

  const handleStatusChange = async (entry: ReadingEntry, newStatus: ReadingStatus) => {
    if (newStatus === entry.status) return;
    if (newStatus === 'Finished' && !entry.startDate) {
      alert('Set a start date first before marking as Finished! 📅');
      return;
    }
    const res = await changeStatus(entry.id, newStatus);
    if (!res.ok) {
      alert('Cannot skip from "Want to Read" directly to "Finished" — read it first! 📖');
      return;
    }
    load();
  };

  const handleRating = async (entry: ReadingEntry, rating: number) => {
    await setRating(entry.id, rating);
    load();
  };

  const handleProgress = async (entry: ReadingEntry) => {
    const val = Number(progressValues[entry.id] ?? entry.pagesRead);
    const res = await updateProgress(entry.id, val);
    if (!res.ok) alert('Pages cannot exceed total pages of the book.');
    else load();
  };

  const handleStartDate = async (entry: ReadingEntry, date: string) => {
    if (!date) return;
    const res = await setStartDate(entry.id, new Date(date).toISOString());
    if (!res.ok) alert('Start date cannot be in the future.');
    else load();
  };

  const handleFinishDate = async (entry: ReadingEntry, date: string) => {
    if (!date) return;
    const res = await setFinishDate(entry.id, new Date(date).toISOString());
    if (!res.ok) alert('Finish date cannot be in the future.');
    else load();
  };

  const handleDelete = async (entry: ReadingEntry) => {
    if (!confirm(`Remove "${entry.book.title}" from your list?`)) return;
    await deleteEntry(entry.id);
    load();
  };

  return (
    <div className="page">
      <div className="page-header">
        <h1>My Reading List</h1>
        <p>{entries.length} book{entries.length !== 1 ? 's' : ''} tracked</p>
      </div>

      <form onSubmit={handleAdd} className="add-form">
        <select
          value={selectedBookId}
          onChange={(e) => setSelectedBookId(e.target.value)}
          required
        >
          <option value="">Select a book to add...</option>
          {books.map((b) => (
            <option key={b.id} value={b.id}>{b.title} — {b.author}</option>
          ))}
        </select>
        <button type="submit" className="btn btn-primary">Add to List</button>
      </form>
      {error && <p className="error-msg">{error}</p>}

      {entries.length === 0 ? (
        <div className="empty">
          <div className="empty-icon">🎯</div>
          <p>Your reading list is empty. Add a book above to get started.</p>
        </div>
      ) : (
        <div className="cards-grid">
          {entries.map((entry) => {
            const pct = entry.book.totalPages > 0
              ? Math.round((entry.pagesRead / entry.book.totalPages) * 100)
              : 0;

            return (
              <div className="card" key={entry.id}>
                <div className="card-top">
                  <div>
                    <div className="card-title">{entry.book.title}</div>
                    <div className="card-meta">{entry.book.author} · {entry.book.totalPages} pages</div>
                  </div>
                  <div className="card-actions">
                    <div className="status-switcher">
                      {ALL_STATUSES.map((s) => (
                        <button
                          key={s}
                          className={`status-pill ${s === entry.status ? 'status-pill-active status-pill-' + s : ''}`}
                          onClick={() => handleStatusChange(entry, s)}
                          title={s === entry.status ? 'Current status' : `Switch to ${STATUS_LABELS[s]}`}
                        >
                          {STATUS_DOT[s]} {STATUS_LABELS[s]}
                        </button>
                      ))}
                    </div>
                    <button
                      className="btn btn-sm btn-danger"
                      onClick={() => handleDelete(entry)}
                    >
                      ✕
                    </button>
                  </div>
                </div>

                {entry.status !== 'WantToRead' && (
                  <div className="progress-section">
                    <div className="progress-label">
                      <span>Progress</span>
                      <span>
                        {progressValues[entry.id] ?? entry.pagesRead} / {entry.book.totalPages} pages
                        {' '}({entry.book.totalPages > 0
                          ? Math.round(((Number(progressValues[entry.id] ?? entry.pagesRead)) / entry.book.totalPages) * 100)
                          : 0}%)
                      </span>
                    </div>
                    {entry.status === 'Reading' ? (
                      <input
                        type="range"
                        className="progress-slider"
                        min={0}
                        max={entry.book.totalPages}
                        value={progressValues[entry.id] ?? entry.pagesRead}
                        onChange={(e) =>
                          setProgressValues((prev) => ({ ...prev, [entry.id]: e.target.value }))
                        }
                        onMouseUp={() => handleProgress(entry)}
                        onTouchEnd={() => handleProgress(entry)}
                      />
                    ) : (
                      <div className="progress-bar-wrap">
                        <div className="progress-bar-fill" style={{ width: `${pct}%` }} />
                      </div>
                    )}
                  </div>
                )}

                {entry.status === 'Reading' && (
                  <div className="date-row">
                    <label className="date-label">
                      📅 Started reading
                      <input
                        type="date"
                        className="date-input"
                        max={today()}
                        value={toInputDate(entry.startDate)}
                        onChange={(e) => handleStartDate(entry, e.target.value)}
                      />
                    </label>
                  </div>
                )}

                {entry.status === 'Finished' && (
                  <div className="date-row">
                    <label className="date-label">
                      📅 Started
                      <input
                        type="date"
                        className="date-input"
                        max={today()}
                        value={toInputDate(entry.startDate)}
                        onChange={(e) => handleStartDate(entry, e.target.value)}
                      />
                    </label>
                    <label className="date-label">
                      🏁 Finished
                      <input
                        type="date"
                        className="date-input"
                        max={today()}
                        value={toInputDate(entry.finishDate)}
                        onChange={(e) => handleFinishDate(entry, e.target.value)}
                      />
                    </label>
                  </div>
                )}

                {entry.status === 'Finished' && (
                  <div className="stars">
                    {[1, 2, 3, 4, 5].map((star) => (
                      <span
                        key={star}
                        className={`star ${(entry.rating ?? 0) >= star ? 'active' : ''}`}
                        onClick={() => handleRating(entry, star)}
                        title={`Rate ${star} star${star > 1 ? 's' : ''}`}
                      >
                        ⭐
                      </span>
                    ))}
                    {entry.rating && (
                      <span style={{ fontSize: '0.8rem', color: 'var(--text-muted)', marginLeft: '0.5rem', alignSelf: 'center' }}>
                        {entry.rating}/5
                      </span>
                    )}
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}

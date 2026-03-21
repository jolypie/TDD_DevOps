import { useEffect, useState } from 'react';
import type { Book } from '../api/books';
import { createBook, getBooks } from '../api/books';

export default function BooksPage() {
  const [books, setBooks] = useState<Book[]>([]);
  const [title, setTitle] = useState('');
  const [author, setAuthor] = useState('');
  const [totalPages, setTotalPages] = useState('');
  const [error, setError] = useState('');

  const load = () => getBooks().then(setBooks);
  useEffect(() => { load(); }, []);

  const handleAdd = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    const res = await createBook({ title, author, totalPages: Number(totalPages) });
    if (res.ok) {
      setTitle(''); setAuthor(''); setTotalPages('');
      load();
    } else {
      setError('Check your input — all fields are required.');
    }
  };

  return (
    <div className="page">
      <div className="page-header">
        <h1>Books</h1>
        <p>Add books to the library, then track them in My List.</p>
      </div>

      <form onSubmit={handleAdd} className="add-form">
        <input
          placeholder="Title"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          required
        />
        <input
          placeholder="Author"
          value={author}
          onChange={(e) => setAuthor(e.target.value)}
          required
        />
        <input
          placeholder="Pages"
          type="number"
          min={1}
          value={totalPages}
          onChange={(e) => setTotalPages(e.target.value)}
          required
          style={{ maxWidth: 100 }}
        />
        <button type="submit" className="btn btn-primary">Add Book</button>
      </form>
      {error && <p className="error-msg">{error}</p>}

      {books.length === 0 ? (
        <div className="empty">
          <div className="empty-icon">📚</div>
          <p>No books yet. Add the first one above.</p>
        </div>
      ) : (
        <table className="books-table">
          <thead>
            <tr>
              <th>Title</th>
              <th>Author</th>
              <th>Pages</th>
            </tr>
          </thead>
          <tbody>
            {books.map((b) => (
              <tr key={b.id}>
                <td style={{ fontWeight: 500 }}>{b.title}</td>
                <td style={{ color: 'var(--text-muted)' }}>{b.author}</td>
                <td style={{ color: 'var(--text-muted)' }}>{b.totalPages}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}

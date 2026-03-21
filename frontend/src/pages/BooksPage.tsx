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
      setError('Failed to add book. Check your input.');
    }
  };

  return (
    <div className="page">
      <h1>Books</h1>

      <form onSubmit={handleAdd} className="form">
        <input placeholder="Title" value={title} onChange={(e) => setTitle(e.target.value)} required />
        <input placeholder="Author" value={author} onChange={(e) => setAuthor(e.target.value)} required />
        <input placeholder="Total pages" type="number" value={totalPages} onChange={(e) => setTotalPages(e.target.value)} required />
        <button type="submit">Add Book</button>
        {error && <p className="error">{error}</p>}
      </form>

      <table>
        <thead>
          <tr><th>Title</th><th>Author</th><th>Pages</th></tr>
        </thead>
        <tbody>
          {books.map((b) => (
            <tr key={b.id}>
              <td>{b.title}</td>
              <td>{b.author}</td>
              <td>{b.totalPages}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

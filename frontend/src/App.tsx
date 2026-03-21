import { BrowserRouter, Link, Route, Routes } from 'react-router-dom';
import BooksPage from './pages/BooksPage';
import ReadingListPage from './pages/ReadingListPage';
import './App.css';

export default function App() {
  return (
    <BrowserRouter>
      <nav>
        <span className="nav-brand">📚 Book Library</span>
        <Link to="/">Books</Link>
        <Link to="/my-list">My List</Link>
      </nav>
      <Routes>
        <Route path="/" element={<BooksPage />} />
        <Route path="/my-list" element={<ReadingListPage />} />
      </Routes>
    </BrowserRouter>
  );
}

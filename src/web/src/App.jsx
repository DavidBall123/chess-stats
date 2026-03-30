export default function App() {
  return (
    <main className="container">
      <h1>ChessMonitor</h1>
      <p>Web stub is running.</p>
      <ul>
        <li>API base URL: {import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:8080'}</li>
        <li>Status: Phase 3 data layer and contracts complete</li>
      </ul>
    </main>
  );
}

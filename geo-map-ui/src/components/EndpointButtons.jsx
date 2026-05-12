export default function EndpointButtons({ endpoints, active, loading, onSelect }) {
  return (
    <nav className="endpoint-buttons">
      {endpoints.map((ep) => (
        <button
          key={ep.id}
          className={`ep-btn ep-btn--${ep.id} ${active === ep.id ? 'ep-btn--active' : ''}`}
          onClick={() => onSelect(ep)}
          disabled={loading}
        >
          <span className="ep-method">{ep.method}</span>
          <span className="ep-path">{ep.url}</span>
        </button>
      ))}
    </nav>
  )
}

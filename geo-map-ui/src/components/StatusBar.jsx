export default function StatusBar({ status, loading }) {
  return (
    <div className={`status status--${status.type}`}>
      {loading && <span className="spinner" />}
      {status.text}
    </div>
  )
}

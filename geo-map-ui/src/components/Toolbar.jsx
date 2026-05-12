export default function Toolbar({ brand, children }) {
  return (
    <header className="toolbar">
      <div className="brand">{brand}</div>
      {children}
    </header>
  )
}

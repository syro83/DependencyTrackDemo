export function CitySelector({ cities, selectedCity, onCityChange, onRandom, loading }) {
  if (cities.length === 0) return null

  return (
    <section className="city-selector-panel">
      <h2 className="city-selector-heading">Explore a City</h2>
      <div className="city-selector">
        <select
          value={selectedCity}
          onChange={(e) => onCityChange(e.target.value)}
        >
          <option value="">— Pick a city —</option>
          {cities.map((c) => (
            <option key={c.name} value={c.name}>{c.name}, {c.country}</option>
          ))}
        </select>
        <button onClick={onRandom} disabled={loading}>
          {loading ? 'Loading…' : 'Random City'}
        </button>
      </div>
    </section>
  )
}

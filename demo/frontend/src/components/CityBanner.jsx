import { CityMap } from './CityMap'

export function CityBanner({ data, loading, noData, cities, selectedCity, onCityChange, onRandom }) {
  return (
    <section
      className={`city-banner${noData ? ' city-banner--no-data' : ''}`}
      style={data?.cityImageUrl ? { backgroundImage: `url(${data.cityImageUrl})` } : undefined}
    >
      <div className="city-banner-overlay" />
      {data && <CityMap lat={data.latitude} lon={data.longitude} />}
      <div className="city-banner-content">
        {data ? (
          <>
            <p className="eyebrow">{data.region} · {data.country}</p>
            <h1 className="city-name">{data.city}</h1>
          </>
        ) : (
          <p className="eyebrow">{loading ? 'Loading…' : 'City Lens'}</p>
        )}
        {cities.length > 0 && (
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
        )}
      </div>
    </section>
  )
}

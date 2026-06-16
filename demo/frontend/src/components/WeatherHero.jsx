const SUMMARY_ICON = {
  'Clear Sky': '☀️',
  'Mainly Clear': '🌤️',
  'Partly Cloudy': '⛅',
  'Overcast': '☁️',
  'Foggy': '🌫️',
  'Drizzle': '🌦️',
  'Freezing Drizzle': '🌧️',
  'Rain': '🌧️',
  'Freezing Rain': '🌨️',
  'Snow': '❄️',
  'Snow Grains': '🌨️',
  'Rain Showers': '🌦️',
  'Snow Showers': '🌨️',
  'Thunderstorm': '⛈️',
  'Thunderstorm with Hail': '⛈️',
}

export function WeatherHero({ today, copy, noData, error }) {
  return (
    <section className={`hero${noData ? ' hero--no-data' : ''}`}>
      <div className="hero-overlay" />
      {today?.weatherImageUrl && (
        <img src={today.weatherImageUrl} alt={today.summary} className="hero-image" />
      )}
      <div className="hero-content">
        <p className="eyebrow">Weather Lens</p>
        {today?.summary && <h1 className="weather-type">{today.summary} {SUMMARY_ICON[today.summary] ?? '🌡️'}</h1>}
        <p className="hero-tagline"><span className="hero-copy-title">{copy.title}</span><span className="hero-copy-sep"> · </span><span className="hero-copy-subtitle">{copy.subtitle}</span></p>
        {today && (
          <>
            <div className="today-weather">
              <span className="today-temp-value">{today.temperatureMaxC}°C</span>
              <span className="today-temp-details">
                Low {today.temperatureMinC}°C &nbsp;·&nbsp; High {today.temperatureMaxF}°F / Low {today.temperatureMinF}°F
              </span>
            </div>
            <div className="today-details">
              <span className="today-detail-item">
                <span className="today-detail-icon">🌧</span>
                {today.precipitationMm} mm
              </span>
              {today.precipitationProbability != null && (
                <span className="today-detail-item">
                  <span className="today-detail-icon">🌂</span>
                  {today.precipitationProbability}% rain
                </span>
              )}
              <span className="today-detail-item">
                <span className="today-detail-icon">🌤</span>
                {today.sunshineHours} h sun
              </span>
              <span className="today-detail-item">
                <span className="today-detail-icon">💨</span>
                {today.windSpeedKmh} km/h
              </span>
              <span className="today-detail-item">
                <span className="today-detail-icon">☀️</span>
                UV {today.uvIndex}
              </span>
              <span className="today-detail-item">
                <span className="today-detail-icon">🌅</span>
                {today.sunrise}
              </span>
              <span className="today-detail-item">
                <span className="today-detail-icon">🌇</span>
                {today.sunset}
              </span>
            </div>
          </>
        )}
        {error && <p className="hero-error">{error}</p>}
      </div>
    </section>
  )
}

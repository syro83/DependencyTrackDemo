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

function summaryIcon(summary) {
  return SUMMARY_ICON[summary] ?? '🌡️'
}

export function ForecastCards({ forecast}) {
  if (forecast.length === 0) return null

  return (
    <section className="panel">
      <p className="meta">5-day outlook</p>
      <div className="cards">
        {forecast.map((day) => (
          <div key={day.date} className="card">
            <p className="day">
              {new Date(day.date).toLocaleDateString('en-US', { weekday: 'short', month: 'short', day: 'numeric' })}
            </p>
            <h2>{day.summary}</h2>
            <p className="temp"><span className="temp-icon">{summaryIcon(day.summary)}</span> {day.temperatureMaxC}° / {day.temperatureMinC}°C</p>
            <div className="card-details">
              <span className="card-detail-item" title="Rain">🌧 {day.precipitationMm} mm</span>
              <span className="card-detail-item" title="Sunshine">🧁 {day.sunshineHours} h</span>
              <span className="card-detail-item" title="Wind">💨 {day.windSpeedKmh} km/h</span>
            </div>
          </div>
        ))}
      </div>
    </section>
  )
}

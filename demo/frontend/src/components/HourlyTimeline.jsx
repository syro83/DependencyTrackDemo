import { useState } from 'react'

const WINDOW = 12
const STEP = 3

const SUMMARY_ICON = {
  'Clear Sky': '☀️',
  'Mainly Clear': '🌤️',
  'Partly Cloudy': '⛅',
  'Overcast': '☁️',
  'Foggy': '🌫️',
  'Light Drizzle': '🌦️',
  'Dense Drizzle': '🌧️',
  'Drizzle': '🌦️',
  'Freezing Drizzle': '🌧️',
  'Rain': '🌧️',
  'Heavy Rain': '🌧️',
  'Freezing Rain': '🌨️',
  'Snow': '❄️',
  'Heavy Snow': '❄️',
  'Snow Grains': '🌨️',
  'Rain Showers': '🌦️',
  'Violent Rain Showers': '⛈️',
  'Snow Showers': '🌨️',
  'Thunderstorm': '⛈️',
  'Thunderstorm with Hail': '⛈️',
}

function summaryIcon(summary) {
  return SUMMARY_ICON[summary] ?? '🌡️'
}

export function HourlyTimeline({ hours }) {
  const [start, setStart] = useState(() => {
    if (!hours || hours.length === 0) return 0
    const currentHour = new Date().getHours()
    const idx = hours.findIndex(h => parseInt(h.time, 10) >= currentHour)
    return idx < 0 ? 0 : Math.min(idx, hours.length - WINDOW)
  })

  if (!hours || hours.length === 0) return null

  const canBack = start > 0
  const canForward = start + WINDOW < hours.length
  const visible = hours.slice(start, start + WINDOW)

  return (
    <section className="hourly-panel">
      <p className="meta">Today · hour by hour · {hours[start].time}–{hours[Math.min(start + WINDOW - 1, hours.length - 1)].time}</p>
      <div className="hourly-track">
        <button
          className="hourly-nav"
          onClick={() => setStart(s => Math.max(0, s - STEP))}
          disabled={!canBack}
          aria-label="Earlier hours"
        >&#8249;</button>
        <div className="hourly-scroll">
          {visible.map((h) => (
            <div key={h.time} className="hourly-slot">
              <span className="hourly-time">{h.time}</span>
              <span className="hourly-icon">{summaryIcon(h.summary)}</span>
              <span className="hourly-temp">{h.temperatureC}°</span>
              {h.precipitationProbability != null && (
                <span className="hourly-rain">🌂 {h.precipitationProbability}%</span>
              )}
              <span className="hourly-wind">💨 {h.windSpeedKmh}</span>
            </div>
          ))}
        </div>
        <button
          className="hourly-nav"
          onClick={() => setStart(s => Math.min(hours.length - WINDOW, s + STEP))}
          disabled={!canForward}
          aria-label="Later hours"
        >&#8250;</button>
      </div>
    </section>
  )
}

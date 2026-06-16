export async function fetchWeather(baseUrl, city = null) {
  const base = baseUrl.replace(/\/$/, '')
  const url = city
    ? `${base}/weatherforecast/${encodeURIComponent(city)}`
    : `${base}/weatherforecast`
  const response = await fetch(url)
  if (!response.ok) {
    throw new Error(`Weather API returned ${response.status}`)
  }
  return response.json()
}

export async function fetchHourlyWeather(baseUrl, city) {
  const base = baseUrl.replace(/\/$/, '')
  const response = await fetch(`${base}/weatherforecast/${encodeURIComponent(city)}/hourly`)
  if (!response.ok) {
    throw new Error(`Hourly weather API returned ${response.status}`)
  }
  return response.json()
}

export async function fetchCities(baseUrl) {
  const base = baseUrl.replace(/\/$/, '')
  const response = await fetch(`${base}/cities`)
  if (!response.ok) return []
  return response.json()
}

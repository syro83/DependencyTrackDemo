import { useState } from 'react'
import './App.css'
import { getAppInsights } from './telemetry'
import { useAppConfig } from './hooks/useAppConfig'
import { useWeather } from './hooks/useWeather'
import { useCities } from './hooks/useCities'
import { useHourlyWeather } from './hooks/useHourlyWeather'
import { defaultCopy, pickRandom } from './weatherCopy'
import { CityBanner } from './components/CityBanner'
import { WeatherHero } from './components/WeatherHero'
import { ForecastCards } from './components/ForecastCards'
import { HourlyTimeline } from './components/HourlyTimeline'
import { CitySelector } from './components/CitySelector'

function App() {
  const { apiBaseUrl, configLoaded } = useAppConfig()
  const { data, loading, error, todayCopy, loadWeather } = useWeather(apiBaseUrl, configLoaded)
  const { cities } = useCities(apiBaseUrl, configLoaded)
  const [selectedCity, setSelectedCity] = useState('')

  const forecast = data?.forecast ?? []
  const today = forecast[0]
  const noData = !loading && (!!error || !data)
  const copy = todayCopy ?? pickRandom(defaultCopy)

  const { hours } = useHourlyWeather(apiBaseUrl, data?.city)

  const handleRefresh = () => {
    getAppInsights()?.trackEvent({ name: 'WeatherForecastRefreshed' })
    loadWeather()
  }

  const handleCityChange = (city) => {
    setSelectedCity(city)
    if (city) loadWeather(city)
  }

  return (
    <main className="page">
      <CityBanner
        data={data}
        loading={loading}
        noData={noData}
        cities={cities}
        selectedCity={selectedCity}
        onCityChange={handleCityChange}
        onRandom={handleRefresh}
      />
      <WeatherHero today={today} copy={copy} noData={noData} error={error} />
      <HourlyTimeline hours={hours} />
      <ForecastCards forecast={forecast} />
    </main>
  )
}

export default App
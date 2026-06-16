import { useCallback, useEffect, useState } from 'react'
import { fetchHourlyWeather } from '../api'

export function useHourlyWeather(apiBaseUrl, city) {
  const [hours, setHours] = useState([])
  const [loading, setLoading] = useState(false)

  const load = useCallback(async (cityName) => {
    if (!apiBaseUrl || !cityName) return
    setLoading(true)
    try {
      const json = await fetchHourlyWeather(apiBaseUrl, cityName)
      setHours(json?.hours ?? [])
    } catch {
      setHours([])
    } finally {
      setLoading(false)
    }
  }, [apiBaseUrl])

  useEffect(() => {
    load(city)
  }, [city, load])

  return { hours, loading, reload: load }
}

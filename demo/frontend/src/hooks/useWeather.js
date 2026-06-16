import { useCallback, useEffect, useState } from 'react'
import { fetchWeather } from '../api'
import { defaultCopy, pickRandom, weatherCopy } from '../weatherCopy'

export function useWeather(apiBaseUrl, configLoaded) {
  const [data, setData] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [todayCopy, setTodayCopy] = useState(null)

  const loadWeather = useCallback(async (city = null) => {
    setLoading(true)
    setError('')

    try {
      const json = await fetchWeather(apiBaseUrl, city)
      setData(json)
      const summary = json?.forecast?.[0]?.summary
      const copies = (summary && weatherCopy[summary]) ?? defaultCopy
      setTodayCopy(pickRandom(copies))
    } catch (fetchError) {
      setError(fetchError.message)
    } finally {
      setLoading(false)
    }
  }, [apiBaseUrl])

  useEffect(() => {
    if (!configLoaded) return
    loadWeather()
  }, [configLoaded, loadWeather])

  return { data, loading, error, todayCopy, loadWeather }
}

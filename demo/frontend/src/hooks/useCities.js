import { useEffect, useState } from 'react'
import { fetchCities } from '../api'

export function useCities(apiBaseUrl, configLoaded) {
  const [cities, setCities] = useState([])

  useEffect(() => {
    if (!configLoaded) return
    fetchCities(apiBaseUrl)
      .then((list) => setCities(list.sort((a, b) => a.name.localeCompare(b.name))))
      .catch(() => {
        // cities list is optional — ignore errors
      })
  }, [configLoaded, apiBaseUrl])

  return { cities }
}

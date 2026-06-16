import { useEffect, useState } from 'react'
import { initializeTelemetry } from '../telemetry'

const defaultApiBaseUrl =
  import.meta.env.VITE_WEATHER_API_BASE_URL || __REACT_APP_API_URL__ || 'http://localhost:5063'

export function useAppConfig() {
  const [apiBaseUrl, setApiBaseUrl] = useState(defaultApiBaseUrl)
  const [configLoaded, setConfigLoaded] = useState(false)

  useEffect(() => {
    let isActive = true

    const loadRuntimeConfig = async () => {
      try {
        const response = await fetch('/app-config.json', { cache: 'no-store' })
        if (!response.ok) return

        const config = await response.json()
        const runtimeApiBaseUrl = config.apiBaseUrl?.trim()
        if (isActive && runtimeApiBaseUrl) {
          setApiBaseUrl(runtimeApiBaseUrl)
        }

        initializeTelemetry(config.appInsightsConnectionString)
      } catch {
        // Fall back to build-time configuration when no runtime config file is present.
      } finally {
        if (isActive) setConfigLoaded(true)
      }
    }

    loadRuntimeConfig()
    return () => { isActive = false }
  }, [])

  return { apiBaseUrl, configLoaded }
}

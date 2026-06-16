import { ApplicationInsights } from '@microsoft/applicationinsights-web'

let appInsights = null

/**
 * Initializes Application Insights.
 * Safe to call with a null/empty connection string — telemetry is silently disabled.
 * @param {string|null|undefined} connectionString
 */
export function initializeTelemetry(connectionString) {
  if (!connectionString) return

  appInsights = new ApplicationInsights({
    config: {
      connectionString,
      // Automatically track SPA route changes as page views.
      enableAutoRouteTracking: true,
      // Inject W3C traceparent / Request-Id headers so backend traces are
      // linked to the originating frontend operation in Application Insights.
      enableCorsCorrelation: true,
      enableRequestHeaderTracking: true,
      enableResponseHeaderTracking: true,
      // Exclude external image CDN domains to avoid CORS preflight failures
      // from the correlation header injection.
      correlationHeaderExcludedDomains: ['images.unsplash.com'],
      samplingPercentage: 50
    },
  })

  appInsights.loadAppInsights()
  appInsights.trackPageView()
}

/** Returns the initialized ApplicationInsights instance, or null if not configured. */
export function getAppInsights() {
  return appInsights
}

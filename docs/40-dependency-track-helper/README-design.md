
# Dependency-Track Helper API

Dependency-Track supports project and version management through its REST API, but recurring release lifecycle tasks are operational work. This API wraps those tasks into one endpoint so pipelines can call a single operation.

1. The CI/CD pipeline creates a BOM when the application is built.
2. Before the app will be deployed to prod, the CI/CD pipeline uploads the BOM directly to Dependency-Track, e.g for WeatherApiService version 1.2.3.
   - The project is usable in Dependency-Track, vulnerability and license analysis will be done, the results are visible in Dependency-Track. Notifications are triggered if configured.
3. Then the WeatherApiService is actually deployed to the PROD environment.
4. After step 3, the CI/CD pipeline calls this Helper API for WeatherApiService /1.2.3. The helper service will do 4 phases.
   - The new version becomes active and latest is set.
   - A Parent project is created if needed, and the relation is set.
   - Older versions are deactivated (with the same application name).
   - Older versions are optionally pruned (with the same application name)

![CI/CD pipeline use case infographic](assets/image-9.png)

> **Note**: This demo does not contain the pre-deployment-gate with the validation check code, you can do this by your self.

---

## Tech stack

Minimal ASP.NET Core API that automates Dependency-Track project lifecycle operations for versioned applications.

- .NET net10.0 minimal API
- Typed repository layer for Dependency-Track HTTP calls
- Explicit process context propagation for API key
- Structured logging with process-phase messages

---

## API contract

### Health endpoint

- Method: GET
- Path: /health
- No authentication required

Returns `200 OK` with `{ "status": "ok" }`.

### Project process endpoint

- Method: POST
- Path: /api/v1/projectprocess
- Required header: X-Api-Key

The API key is forwarded to Dependency-Track for upstream calls.

### Request body

The endpoint accepts `ProjectProcessRequestDto` with these JSON fields:

- projectName (string, required)
- version (string, required)
- activateVersion (bool, optional)
- cleanInactiveProjects (int, optional)
- parentName (string, optional)

### Behavior details

When the endpoint is called:

1. Parent resolution
     - If parentName is provided, parent lookup is executed in Dependency-Track.
     - If not found, a parent project is created.

2. Target activation
     - Target project is looked up by projectName + version.
     - Target is updated to active = true and isLatest = true.
     - If parent is resolved, parent relation is applied.

3. Sibling deactivation
     - Other projects with the same name are updated to active = false and isLatest = false.

4. Cleanup
     - Inactive versions are sorted by lastBomImport desc, then version desc.
     - If cleanInactiveProjects is missing or <= 0, deletion is skipped.
     - Otherwise, only the newest cleanInactiveProjects inactive versions are retained.
     - Older inactive versions are deleted.

### Success response

Returns 200 OK with:

```json
{
    "projectUuid": "3c570e4a-8d5b-4a3a-9f8c-a4ed7a4f2d9a",
    "projectName": "my-app",
    "version": "1.2.3",
    "active": true,
    "isLatest": true,
    "parentUuid": "f7cb4a8b-33d2-49e8-a622-f90a2fae612d",
    "deactivatedCount": 4,
    "deletedInactiveCount": 2
}
```

### Error responses

- 400 Bad Request: missing or invalid request body values
- 401 Unauthorized: missing X-Api-Key
- 404 Not Found: target project version does not exist in Dependency-Track
- 500 Internal Server Error: local configuration or processing failure
- 502 Bad Gateway: Dependency-Track request failed upstream

---

## Configuration

Configuration key:

```json
{
    "DependencyTrack": {
        "BaseUrl": "https://your-dependency-track-host"
    }
}
```

Files:

- appsettings.json

The application loads configuration from the configuration file and environment variables.

Set nested values with double underscores, for example:

- DependencyTrack__BaseUrl=<https://your-dependency-track-host>

---

## Example requests

cURL:

```bash
curl -X POST `
    "https://dtdevsubql3-api.purplepond-38abd33f.northeurope.azurecontainerapps.io/api/v1/projectprocess" `
    -H 'X-Api-Key: xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx' `
    -H "Content-Type: application/json" `
    -d '{ "projectName": "WeatherApiService-backend", "version": "0.51.0-pbi.27", "activateVersion": true, "cleanInactiveProjects": 3, "parentName": "WeatherApiService" }'    `
    --verbose --show-headers --ssl-no-revoke
```

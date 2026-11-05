$api_url = $env:API_BASEURL + 'profiler/public/ManualDeployment/RestartWarewolfLogger.json'
try { $response = Invoke-WebRequest -URI $api_url } catch {
      $_.Exception.Response.StatusCode.Value__}
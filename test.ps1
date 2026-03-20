

Add-Type -AssemblyName System.Net.Http

$client = New-Object System.Net.Http.HttpClient
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

for ($i = 1; $i -le 10; $i++) {

    $content = New-Object System.Net.Http.MultipartFormDataContent

    $filePath = "C:\"
    $fileStream = [System.IO.File]::OpenRead($filePath)

    $fileContent = New-Object System.Net.Http.StreamContent($fileStream)
    $fileContent.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse("application/pdf")

    $content.Add($fileContent, "files", [System.IO.Path]::GetFileName($filePath))

    $response = $client.PostAsync("http://localhost:5119/api/files/upload", $content).Result

    Write-Host "Request $i Status:" $response.StatusCode

    $fileStream.Close()
}

$stopwatch.Stop()

Write-Host "Tiempo total:" $stopwatch.Elapsed.TotalSeconds "segundos"
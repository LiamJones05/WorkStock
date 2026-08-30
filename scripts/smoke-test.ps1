param(
    [string] $BaseUrl = "http://localhost:8080"
)

$ErrorActionPreference = "Stop"
$suffix = [guid]::NewGuid().ToString("N").Substring(0, 8)

$registerBody = @{
    organisationName = "Smoke Test $suffix"
    displayName = "Owner User"
    email = "owner-$suffix@example.com"
    password = "Password12345"
} | ConvertTo-Json

$session = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/auth/register" -Body $registerBody -ContentType "application/json"
$headers = @{ Authorization = "Bearer $($session.token)" }

$employeeBody = @{
    displayName = "Field Employee"
    email = "employee-$suffix@example.com"
    password = "Password12345"
    role = "Employee"
} | ConvertTo-Json
Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/users" -Headers $headers -Body $employeeBody -ContentType "application/json" | Out-Null
$users = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/users" -Headers $headers

$customerBody = @{
    name = "Image Test Customer"
    email = "customer-$suffix@example.com"
} | ConvertTo-Json
$customer = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/customers" -Headers $headers -Body $customerBody -ContentType "application/json"

$statuses = Invoke-RestMethod -Method Get -Uri "$BaseUrl/api/jobs/statuses" -Headers $headers
$jobBody = @{
    customerId = $customer.id
    jobStatusId = $statuses[0].id
    title = "Image upload smoke test"
    priority = "Normal"
} | ConvertTo-Json
$job = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/jobs" -Headers $headers -Body $jobBody -ContentType "application/json"

$pngPath = Join-Path $env:TEMP "workstock-smoke-$suffix.png"
[byte[]] $png = 137,80,78,71,13,10,26,10,0,0,0,13,73,72,68,82,0,0,0,1,0,0,0,1,8,6,0,0,0,31,21,196,137,0,0,0,13,73,68,65,84,120,218,99,248,207,192,240,31,0,5,0,1,255,137,153,61,29,0,0,0,0,73,69,78,68,174,66,96,130
[IO.File]::WriteAllBytes($pngPath, $png)

$uploadJson = curl.exe -s -X POST "$BaseUrl/api/jobs/$($job.id)/documents" -H "Authorization: Bearer $($session.token)" -F "file=@$pngPath;type=image/png"
$document = $uploadJson | ConvertFrom-Json
$downloadPath = Join-Path $env:TEMP "workstock-smoke-download-$suffix.png"
$headersPath = Join-Path $env:TEMP "workstock-smoke-headers-$suffix.txt"
curl.exe -s -D $headersPath -o $downloadPath "$BaseUrl/api/documents/$($document.id)" -H "Authorization: Bearer $($session.token)" | Out-Null
$contentType = (Select-String -Path $headersPath -Pattern "^Content-Type:" | Select-Object -First 1).Line
Remove-Item -LiteralPath $pngPath -Force
Remove-Item -LiteralPath $downloadPath -Force
Remove-Item -LiteralPath $headersPath -Force

@{
    health = (Invoke-RestMethod "$BaseUrl/api/health").status
    activeEmployeeCount = $users.activeEmployeeCount
    userCount = $users.users.Count
    jobNumber = $job.jobNumber
    uploadedContentType = $contentType
} | ConvertTo-Json -Compress

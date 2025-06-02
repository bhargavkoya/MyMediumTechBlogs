# HTTP Server Testing Examples

## Testing Your .NET HTTP Server from Command Line

This document provides practical examples for testing your .NET HTTP server using various command line tools.

## 1. cURL Examples

### Basic GET Request
```bash
curl http://localhost:8080/
curl -v http://localhost:8080/api/users
```

### GET with Query Parameters
```bash
curl "http://localhost:8080/api/users?id=123&status=active"
curl -G -d "name=john" -d "age=25" http://localhost:8080/api/search
```

### POST with JSON Data
```bash
curl -X POST -H "Content-Type: application/json" \
     -d '{"name":"John","email":"john@example.com"}' \
     http://localhost:8080/api/users

# From file
curl -X POST -H "Content-Type: application/json" \
     -d @user.json http://localhost:8080/api/users
```

### POST with Form Data
```bash
curl -X POST -d "name=John&email=john@example.com" \
     http://localhost:8080/api/users

curl -X POST -F "name=John" -F "email=john@example.com" \
     http://localhost:8080/api/users
```

### File Upload
```bash
curl -X POST -F "file=@document.pdf" \
     -F "description=Test document" \
     http://localhost:8080/api/upload
```

### Custom Headers
```bash
curl -H "Authorization: Bearer token123" \
     -H "X-API-Key: mykey" \
     http://localhost:8080/api/protected
```

### PUT Request
```bash
curl -X PUT -H "Content-Type: application/json" \
     -d '{"id":1,"name":"Updated Name"}' \
     http://localhost:8080/api/users/1
```

### DELETE Request
```bash
curl -X DELETE http://localhost:8080/api/users/1
```

## 2. PowerShell Examples

### Basic GET Request
```powershell
Invoke-RestMethod -Uri "http://localhost:8080/api/users"
Invoke-WebRequest -Uri "http://localhost:8080/" -Method GET
```

### GET with Headers
```powershell
$headers = @{
    "Authorization" = "Bearer token123"
    "X-API-Key" = "mykey"
}
Invoke-RestMethod -Uri "http://localhost:8080/api/protected" -Headers $headers
```

### POST with JSON
```powershell
$body = @{
    name = "John"
    email = "john@example.com"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:8080/api/users" `
                  -Method POST `
                  -Body $body `
                  -ContentType "application/json"
```

### POST with Form Data
```powershell
$body = @{
    name = "John"
    email = "john@example.com"
}

Invoke-RestMethod -Uri "http://localhost:8080/api/users" `
                  -Method POST `
                  -Body $body
```

### File Upload
```powershell
$filePath = "C:\path\to\file.txt"
$fileBytes = [System.IO.File]::ReadAllBytes($filePath)
$fileEnc = [System.Text.Encoding]::GetEncoding('ISO-8859-1').GetString($fileBytes)

$boundary = [System.Guid]::NewGuid().ToString()
$body = @"
--$boundary
Content-Disposition: form-data; name="file"; filename="file.txt"
Content-Type: text/plain

$fileEnc
--$boundary--
"@

Invoke-RestMethod -Uri "http://localhost:8080/api/upload" `
                  -Method POST `
                  -Body $body `
                  -ContentType "multipart/form-data; boundary=$boundary"
```

## 3. HTTPie Examples (if installed)

### Basic GET Request
```bash
http GET localhost:8080/api/users
```

### POST with JSON
```bash
http POST localhost:8080/api/users name=John email=john@example.com
```

### Custom Headers
```bash
http GET localhost:8080/api/protected Authorization:"Bearer token123"
```

### File Upload
```bash
http --form POST localhost:8080/api/upload file@document.pdf description="Test document"
```

## 4. Testing Response Status Codes

### Using cURL
```bash
# Check status code
curl -o /dev/null -s -w "%{http_code}\n" http://localhost:8080/api/users

# Fail on non-2xx status codes
curl --fail http://localhost:8080/api/nonexistent
```

### Using PowerShell
```powershell
try {
    $response = Invoke-WebRequest -Uri "http://localhost:8080/api/users"
    Write-Host "Status Code: $($response.StatusCode)"
} catch {
    Write-Host "Error: $($_.Exception.Message)"
}
```

## 5. Authentication Examples

### Basic Authentication
```bash
# cURL
curl -u username:password http://localhost:8080/api/protected

# PowerShell
$cred = Get-Credential
Invoke-RestMethod -Uri "http://localhost:8080/api/protected" -Credential $cred
```

### Bearer Token
```bash
# cURL
curl -H "Authorization: Bearer YOUR_TOKEN" http://localhost:8080/api/protected

# PowerShell
$headers = @{ "Authorization" = "Bearer YOUR_TOKEN" }
Invoke-RestMethod -Uri "http://localhost:8080/api/protected" -Headers $headers
```

## 6. Testing Tips

### Save Response to File
```bash
# cURL
curl http://localhost:8080/api/users > response.json

# PowerShell
Invoke-RestMethod -Uri "http://localhost:8080/api/users" | ConvertTo-Json | Out-File response.json
```

### Verbose Output
```bash
# cURL
curl -v http://localhost:8080/api/users

# PowerShell
Invoke-WebRequest -Uri "http://localhost:8080/api/users" -Verbose
```

### Time the Request
```bash
# cURL
curl -w "Time: %{time_total}s\n" http://localhost:8080/api/users

# PowerShell
Measure-Command { Invoke-RestMethod -Uri "http://localhost:8080/api/users" }
```
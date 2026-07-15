@{
    "productCode": "HRM",
    "name": "ArsuHrm Solutions",
    "email": "info@arsuhrm.com",
    "address": "Kathmandu, Nepal",
    "phoneNo": "9829967841",
    "pan": "123456789",
    "regNo": "REG-001",
    "url": "https://arsuhrm.com",
    "mainUsername": "admin.arsuhrm",
    "mainUserFirstName": "Arman",
    "mainUserLastName": "Shrestha",
    "mainUserEmail": "admin@arsuhrm.com",
    "mainUserContactNo": "9800000001"
} | ConvertTo-Json | Out-File -FilePath "D:\ARMAN\SaasProject\test-company-request.json" -Encoding UTF8

Write-Host "Test request payload saved to: D:\ARMAN\SaasProject\test-company-request.json"
Write-Host ""
Write-Host "To test the API, make a POST request to:"
Write-Host "http://localhost:5000/UserManagement/company (or your API port)"
Write-Host ""
Write-Host "Example using curl:"
Write-Host 'curl -X POST "http://localhost:5000/UserManagement/company" -H "Content-Type: application/json" -d @D:\ARMAN\SaasProject\test-company-request.json'

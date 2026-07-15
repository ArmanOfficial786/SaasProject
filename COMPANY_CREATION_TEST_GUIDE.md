# Company Creation Feature - Testing & Verification Guide

## Implementation Summary

The company creation endpoint (`POST /UserManagement/company`) has been enhanced to automatically provision the following when a company is created:

### What Gets Created Automatically

1. **Company Entity** - With the provided details
2. **Default Agent/Branch** - Named "Main Agent" linked to the company
3. **Default User** - The main user (admin) for the company
4. **Default "Owner" Role** - A company-scoped role with description "Default owner role with full permissions"
5. **Role Assignment** - The main user is automatically assigned the "Owner" role

## Changes Made

### Modified File
- **Src/UserManagement/UserManagement.Application/Commands/CompanyCommands/CreateCompany/CreateCompanyCommandHandler.cs**

### Key Changes
1. Added `RoleManager<Role>` dependency injection
2. After user creation, automatically creates an "Owner" role scoped to the company
3. Assigns the "Owner" role to the main user
4. Includes comprehensive error handling for each step

## Testing Instructions

### Prerequisites
1. Ensure the API is running (typically on http://localhost:5000 or your configured port)
2. Database is initialized with migrations applied
3. API is not behind authentication (or test with appropriate credentials)

### Test Payload
```json
{
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
}
```

### Make the Request

#### Using PowerShell
```powershell
$payload = @{
    productCode = "HRM"
    name = "ArsuHrm Solutions"
    email = "info@arsuhrm.com"
    address = "Kathmandu, Nepal"
    phoneNo = "9829967841"
    pan = "123456789"
    regNo = "REG-001"
    url = "https://arsuhrm.com"
    mainUsername = "admin.arsuhrm"
    mainUserFirstName = "Arman"
    mainUserLastName = "Shrestha"
    mainUserEmail = "admin@arsuhrm.com"
    mainUserContactNo = "9800000001"
} | ConvertTo-Json

Invoke-WebRequest -Uri "http://localhost:5000/UserManagement/company" `
    -Method POST `
    -ContentType "application/json" `
    -Body $payload
```

#### Using CURL
```bash
curl -X POST "http://localhost:5000/UserManagement/company" \
  -H "Content-Type: application/json" \
  -d '{
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
  }'
```

#### Using Postman
1. Create a new POST request
2. URL: `http://localhost:5000/UserManagement/company`
3. Headers: `Content-Type: application/json`
4. Body (raw): Paste the JSON payload above
5. Click Send

### Expected Response (Success)
```json
{
  "isSuccessful": true,
  "message": "Company created successfully",
  "data": {
    "id": 1,
    "productCode": "HRM",
    "name": "ArsuHrm Solutions",
    "email": "info@arsuhrm.com",
    "address": "Kathmandu, Nepal",
    "phoneNo": "9829967841",
    "pan": "123456789",
    "regNo": "REG-001",
    "url": "https://arsuhrm.com"
  }
}
```

## Database Verification

After successful creation, verify the following in the database:

### 1. Check Company Created
```sql
SELECT * FROM Companies 
WHERE Name = 'ArsuHrm Solutions';
```

### 2. Check Agent/Branch Created
```sql
SELECT * FROM Agents 
WHERE CompanyId = (SELECT Id FROM Companies WHERE Name = 'ArsuHrm Solutions')
AND IsParent = 1;
```

### 3. Check Main User Created
```sql
SELECT * FROM AspNetUsers 
WHERE UserName = 'admin.arsuhrm';
```

### 4. Check "Owner" Role Created
```sql
SELECT * FROM AspNetRoles 
WHERE Name = 'Owner' 
AND CompanyId = (SELECT Id FROM Companies WHERE Name = 'ArsuHrm Solutions');
```

### 5. Check Role Assignment
```sql
SELECT ru.*, r.Name 
FROM AspNetUserRoles ru
INNER JOIN AspNetRoles r ON ru.RoleId = r.Id
INNER JOIN AspNetUsers u ON ru.UserId = u.Id
WHERE u.UserName = 'admin.arsuhrm'
AND r.Name = 'Owner';
```

## Error Handling

The implementation includes error handling for:

1. **Duplicate Company** - If company with same name, email, PAN, or RegNo exists
2. **User Creation Failure** - If Identity user creation fails
3. **Role Creation Failure** - If role creation fails
4. **Role Assignment Failure** - If assigning the role to user fails

All errors are logged with detailed messages for debugging.

## Logging

Check application logs for the following entries:
- `Starting company creation...`
- `Failed to create main user for company {CompanyId}: {Errors}` (if user creation fails)
- `Failed to create default Owner role for company {CompanyId}: {Errors}` (if role creation fails)
- `Failed to assign Owner role to user {UserId} for company {CompanyId}: {Errors}` (if assignment fails)
- `Company created successfully` (on success)

## Next Steps

After verifying the company creation is working:

1. Test logging in with the main user credentials:
   - Username: `admin.arsuhrm`
   - Email: `admin@arsuhrm.com`
   - Password: (set during creation or reset as needed)

2. Verify the user has the "Owner" role permissions

3. Test accessing company-specific features

4. Create additional users and assign them different roles

## Troubleshooting

### Issue: Role Manager not found
- Ensure `using Microsoft.AspNetCore.Identity;` is included
- RoleManager should be registered in Startup.cs/Program.cs

### Issue: User created but role not assigned
- Check that user.Id is valid before role assignment
- Verify SaveChangesAsync is called after role creation

### Issue: Role creation fails with "already exists"
- The implementation checks for role duplicates
- Verify role name is correctly set to "Owner"

### Issue: Database transaction errors
- Multiple SaveChangesAsync calls might need transaction management
- Current implementation uses separate commits; consider wrapping in ExplicitTransaction if needed

## Performance Considerations

- Multiple SaveChangesAsync calls: Company → Agent → User → Role → Assignment
- Each separate commit reduces transaction duration but increases I/O
- For production, consider implementing explicit transaction handling if needed

---

**Last Updated**: 2024
**Feature**: Automatic Company Setup with Default Role
**Status**: Implemented and Ready for Testing

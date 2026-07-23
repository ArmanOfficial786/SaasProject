-- ============================================
-- COMPANY CREATION VERIFICATION SCRIPT
-- ============================================
-- Run these queries after creating a company to verify:
-- 1. Company was created
-- 2. Default Agent/Branch was created  
-- 3. Main user was created
-- 4. Owner role was created
-- 5. User was assigned to Owner role
-- ============================================

PRINT '1. CHECK COMPANY CREATED'
PRINT '========================'
SELECT 
    Id,
    ProductCode,
    Name,
    Email,
    Address,
    PhoneNo,
    Pan,
    RegNo,
    Url
FROM Companies 
WHERE Name = 'ArsuHrm Solutions'
ORDER BY Id DESC;

PRINT ''
PRINT '2. CHECK DEFAULT AGENT/BRANCH CREATED'
PRINT '======================================'
SELECT 
    a.Id,
    a.Name,
    a.Address,
    a.Pan,
    a.RegNo,
    a.IsParent,
    a.ReferralCode,
    a.CompanyId,
    c.Name AS CompanyName
FROM Agents a
INNER JOIN Companies c ON a.CompanyId = c.Id
WHERE c.Name = 'ArsuHrm Solutions'
ORDER BY a.Id DESC;

PRINT ''
PRINT '3. CHECK MAIN USER CREATED'
PRINT '============================'
SELECT 
    u.Id,
    u.UserName,
    u.FirstName,
    u.LastName,
    u.Email,
    u.Contact,
    u.CompanyId,
    c.Name AS CompanyName
FROM AspNetUsers u
INNER JOIN Companies c ON u.CompanyId = c.Id
WHERE u.UserName = 'admin.arsuhrm'
ORDER BY u.Id DESC;

PRINT ''
PRINT '4. CHECK OWNER ROLE CREATED'
PRINT '============================='
SELECT 
    r.Id,
    r.Name,
    r.Desc,
    r.CompanyId,
    c.Name AS CompanyName
FROM Roles r
INNER JOIN Companies c ON r.CompanyId = c.Id
WHERE r.Name = 'Owner' 
  AND c.Name = 'ArsuHrm Solutions'
ORDER BY r.Id DESC;

PRINT ''
PRINT '5. CHECK USER-ROLE ASSIGNMENT'
PRINT '=============================='
SELECT 
    ur.UserId,
    u.UserName,
    ur.RoleId,
    r.Name AS RoleName,
    r.Desc,
    c.Name AS CompanyName
FROM AspNetUserRoles ur
INNER JOIN AspNetUsers u ON ur.UserId = u.Id
INNER JOIN Roles r ON ur.RoleId = r.Id
INNER JOIN Companies c ON r.CompanyId = c.Id
WHERE u.UserName = 'admin.arsuhrm'
  AND r.Name = 'Owner'
ORDER BY ur.UserId;

PRINT ''
PRINT '6. SUMMARY - ALL CREATIONS'
PRINT '============================'
DECLARE @CompanyId INT = (SELECT Id FROM Companies WHERE Name = 'ArsuHrm Solutions' ORDER BY Id DESC OFFSET 0 ROWS FETCH NEXT 1 ROW ONLY)

IF @CompanyId IS NOT NULL
BEGIN
    DECLARE @CompanyCount INT = 1 -- Already known
    DECLARE @AgentCount INT = (SELECT COUNT(*) FROM Agents WHERE CompanyId = @CompanyId AND IsParent = 1)
    DECLARE @UserCount INT = (SELECT COUNT(*) FROM AspNetUsers WHERE CompanyId = @CompanyId AND UserName = 'admin.arsuhrm')
    DECLARE @RoleCount INT = (SELECT COUNT(*) FROM Roles WHERE CompanyId = @CompanyId AND Name = 'Owner')
    DECLARE @UserRoleCount INT = (
        SELECT COUNT(*) FROM AspNetUserRoles ur
        INNER JOIN AspNetUsers u ON ur.UserId = u.Id
        INNER JOIN Roles r ON ur.RoleId = r.Id
        WHERE u.CompanyId = @CompanyId AND u.UserName = 'admin.arsuhrm' AND r.Name = 'Owner'
    )

    PRINT 'Company ID: ' + CAST(@CompanyId AS VARCHAR(10))
    PRINT '✓ Company Created: ' + CAST(@CompanyCount AS VARCHAR(10))
    PRINT '✓ Agent Created: ' + CAST(@AgentCount AS VARCHAR(10))
    PRINT '✓ User Created: ' + CAST(@UserCount AS VARCHAR(10))
    PRINT '✓ Owner Role Created: ' + CAST(@RoleCount AS VARCHAR(10))
    PRINT '✓ User-Role Assignment: ' + CAST(@UserRoleCount AS VARCHAR(10))
    PRINT ''

    IF @CompanyCount = 1 AND @AgentCount = 1 AND @UserCount = 1 AND @RoleCount = 1 AND @UserRoleCount = 1
    BEGIN
        PRINT '✅ ALL CHECKS PASSED - Company setup complete!'
    END
    ELSE
    BEGIN
        PRINT '❌ SOME ITEMS MISSING - Check the queries above'
    END
END
ELSE
BEGIN
    PRINT '❌ No company found named "ArsuHrm Solutions"'
END

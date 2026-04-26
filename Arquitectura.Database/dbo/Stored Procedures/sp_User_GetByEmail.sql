-- sp_User_GetByEmail: Get a user by email
CREATE PROCEDURE sp_User_GetByEmail
    @Email NVARCHAR(255)
AS
BEGIN
    SELECT
        Id,
        Email,
        PasswordHash,
        FirstName,
        LastName,
        PhoneNumber,
        EmailConfirmed,
        IsActive,
        LastLoginAt,
        RefreshToken,
        RefreshTokenExpiresAt,
        CreatedAt,
        UpdatedAt,
        CreatedBy,
        UpdatedBy
    FROM Users
    WHERE Email = @Email;
END
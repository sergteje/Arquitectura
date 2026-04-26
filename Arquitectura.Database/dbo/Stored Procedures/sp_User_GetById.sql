-- sp_User_GetById: Get a user by ID
CREATE PROCEDURE sp_User_GetById
    @Id INT
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
    WHERE Id = @Id;
END
-- sp_User_Update: Update an existing user
CREATE PROCEDURE sp_User_Update
    @Id INT,
    @Email NVARCHAR(255),
    @PasswordHash NVARCHAR(500),
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @PhoneNumber NVARCHAR(50),
    @IsActive BIT,
    @RefreshToken NVARCHAR(500),
    @RefreshTokenExpiresAt DATETIME,
    @LastLoginAt DATETIME,
    @UpdatedBy NVARCHAR(100)
AS
BEGIN
    UPDATE Users
    SET
        Email = @Email,
        PasswordHash = @PasswordHash,
        FirstName = @FirstName,
        LastName = @LastName,
        PhoneNumber = @PhoneNumber,
        IsActive = @IsActive,
        RefreshToken = @RefreshToken,
        RefreshTokenExpiresAt = @RefreshTokenExpiresAt,
        LastLoginAt = @LastLoginAt,
        UpdatedBy = @UpdatedBy,
        UpdatedAt = GETDATE()
    WHERE Id = @Id;
END
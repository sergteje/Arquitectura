-- sp_User_Create: Create a new user
CREATE PROCEDURE sp_User_Create
    @Email NVARCHAR(255),
    @PasswordHash NVARCHAR(500),
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @PhoneNumber NVARCHAR(50),
    @CreatedBy NVARCHAR(100)
AS
BEGIN
    INSERT INTO Users (Email, PasswordHash, FirstName, LastName, PhoneNumber, CreatedBy)
    VALUES (@Email, @PasswordHash, @FirstName, @LastName, @PhoneNumber, @CreatedBy);

    SELECT SCOPE_IDENTITY();
END
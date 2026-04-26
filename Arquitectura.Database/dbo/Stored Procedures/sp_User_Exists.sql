-- sp_User_Exists: Check if a user exists by email
CREATE PROCEDURE sp_User_Exists
    @Email NVARCHAR(255)
AS
BEGIN
    SELECT COUNT(1)
    FROM Users
    WHERE Email = @Email;
END

namespace App.Constants;

public static class BasicConstants
{
    public const string SUCCCESS = "success";
    public const string BAD_REQUEST = "bad_request";
    public const string NOT_FOUND = "not_found";
    public const string INTERNAL_SERVER_ERROR = "internal_server_error";
    public const string SIGNED_USER = "signedUser";
}

public class SignedUser
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Username { get; set; }
    public SignedUserRoleAssignee?[] RoleAssignee { get; set; } = [];
}

public class SignedUserRoleAssignee
{
    public required string Name { get; set; }
}
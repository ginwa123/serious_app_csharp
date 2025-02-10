
namespace App.Constants;

public static class ErrorConstants
{
    public const string InternalServerError = "InternalServerError";
    public const string GET_USER_BY_ID_ERROR = "U1";

    public const string BAD_REQUEST = "BadRequest";


    // SIGN IN
    public const string SIGN_IN_FAILED_USER_NOT_FOUND = "SIGNIN_U1";
    public const string SIGN_IN_FAILED_INVALID_PASSWORD = "SIGNIN_U2";
    public const string SIGN_IN_FAILED_USER_NOT_ADMIN = "SIGNIN_U3";

    // Api Url
    public const string API_URL_GET_API_URL_BY_ID_ERROR = "API_URLS_U1";
    public const string API_URL_CREATE_API_URL_FAILED = "API_URLS_U2";
    public const string API_URL_CREATE_API_URL_FAILED_CONFLICT = "API_URLS_U3";
    public const string API_URL_DELETE_API_URL_ERROR = "API_URLS_U4";
    public const string API_URL_UPDATE_API_URL_ERROR_NOT_FOUND = "API_URLS_U5";
    public const string API_URL_UPDATE_API_URL_FAILED = "API_URLS_U6";

    // Api User permissions
    public const string API_USER_PERMISSION_DELETE_ERROR_NOT_FOUND = "API_USER_PERMISSIONS_U1";
    public const string API_USER_PERMISSION_CREATE_ERROR_DUPLICATE = "API_USER_PERMISSIONS_U2";


    // api url permission
    public const string ID_NANO_CONFLICT = "API_URL_PERMISSIONS_U1";

    // users
    public const string API_USER_CREATE_ERROR_DUPLICATE = "API_USERS_U1";
    public const string API_USER_CREATE_ERROR_NO_MEMBER_ROLE = "API_USERS_U2";
    public const string API_USER_UPDATE_ERROR_USER_NOT_FOUND = "API_USERS_U3";
    public const string API_USER_UPDATE_ERROR_INVALID_PASSWORD = "API_USERS_U4";
    public const string API_USER_UPDATE_ERROR_DUPLICATE = "API_USERS_U5";
    public const string API_USER_DELETE_USER_ERROR = "API_USERS_U6";
    public const string API_USER_UPDATE_ERROR_INVALID_USER_SIGNED = "API_USERS_U7";


    // o=workspace
    public const string API_GET_WORKSPACE_BY_ID_ERROR_NOT_FOUND = "AUTH_WORKSPACES_U1";

    public const string FAILED_GET_LOCK = "FAILED_GET_LOCK";
    public const string UNAUTHORIZED = "UNAUTHORIZED";

    public const string NO_TOKEN_AUTHORIZATION = "NO_TOKEN_AUTHORIZATION";
}


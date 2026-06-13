namespace ManagerUser.Application.Common.Constants;
public static class ApplicationCodes
{
    public static class User
    {
        public const string NotFound = "USER_NOT_FOUND";
        public const string AlreadyExists = "USER_ALREADY_EXISTS";
        public const string InvalidUserId = "USER_INVALID_ID";
        public const string UsernameAlreadyExists = "USER_USERNAME_ALREADY_EXISTS";
        public const string EmailAlreadyExists = "USER_EMAIL_ALREADY_EXISTS";
        public const string InvalidStatus = "USER_INVALID_STATUS";
    }
}

using SharedKernel;

namespace Modules.Users.Application.Core.Errors;

internal static class FollowersValidationErrors
{
    public static class StartFollowing
    {
        public static readonly Error UserIdIsRequired = Error.Problem(
            "StartFollowing.UserIdIsRequired",
            "The user identifier is required.");

        public static readonly Error FollowedIdIsRequired = Error.Problem(
            "StartFollowing.FollowedIdIsRequired",
            "The followed identifier is required.");
    }

    public static class StopFollowing
    {
        public static readonly Error UserIdIsRequired = Error.Problem(
            "StopFollowing.UserIdIsRequired",
            "The user identifier is required.");

        public static readonly Error FollowedIdIsRequired = Error.Problem(
            "StopFollowing.FollowedIdIsRequired",
            "The followed identifier is required.");
    }
}

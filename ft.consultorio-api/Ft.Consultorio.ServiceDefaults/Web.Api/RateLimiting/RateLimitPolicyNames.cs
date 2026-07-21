namespace Ft.Consultorio.ServiceDefaults.Web.Api.RateLimiting
{
    public static class RateLimitPolicyNames
    {
        public const string AuthStrict = "auth-strict";
        public const string AuthModerate = "auth-moderate";
        public const string SearchPublic = "search-public";
        public const string RecommendationsPublic = "recommendations-public";
        public const string RecommendationsAuth = "recommendations-auth";
    }
}

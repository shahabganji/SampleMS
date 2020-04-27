using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Sample.Api.Authorization
{
    public sealed class MustSayHelloHeaderRequirement : IAuthorizationRequirement
    {
    }

    public sealed class MustSayHelloHeaderAuthorizationHandler : AuthorizationHandler<MustSayHelloHeaderRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MustSayHelloHeaderAuthorizationHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            MustSayHelloHeaderRequirement requirement)
        {

            if (!_httpContextAccessor.HttpContext.Request.Headers.TryGetValue("Greet", out var greetValues))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var greet = greetValues.First();
            if (greet != "hi")
            {
                context.Fail();
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}

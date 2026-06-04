using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Translation.API.Tests
{
    /// <summary>
    /// Shared helpers for controller unit tests.
    /// Since we're testing controllers in isolation (no HTTP pipeline),
    /// we manually wire up HttpContext, ClaimsPrincipal, and RemoteIpAddress.
    /// </summary>
    public static class ControllerTestHelper
    {
        /// <summary>
        /// Sets up a controller with a fake authenticated user carrying the given claims.
        /// </summary>
        public static void SetUser(ControllerBase controller, string empId = "emp001", string role = "Admin")
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "1"),
                new(ClaimTypes.Name,           empId),
                new(ClaimTypes.Role,           role),
                new("empId",                   empId)
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };
        }

        /// <summary>
        /// Sets up a controller with a fake authenticated user AND a remote IP address.
        /// Used for endpoints that read HttpContext.Connection.RemoteIpAddress (e.g. Refresh).
        /// </summary>
        public static void SetUserWithIp(ControllerBase controller,
            string empId = "emp001",
            string role = "Admin",
            string ip = "127.0.0.1")
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "1"),
                new(ClaimTypes.Name,           empId),
                new(ClaimTypes.Role,           role),
                new("empId",                   empId)
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext { User = principal };
            httpContext.Connection.RemoteIpAddress =
                System.Net.IPAddress.Parse(ip);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        /// <summary>
        /// Extracts the value from an OkObjectResult. Throws if the result is not Ok.
        /// </summary>
        public static T GetOkValue<T>(IActionResult result)
        {
            var ok = result as OkObjectResult
                ?? throw new InvalidCastException($"Expected OkObjectResult but got {result.GetType().Name}");
            return (T)ok.Value!;
        }

        /// <summary>
        /// Asserts the result is a BadRequestObjectResult and returns its value.
        /// </summary>
        public static object? GetBadRequestValue(IActionResult result)
        {
            var bad = result as BadRequestObjectResult
                ?? throw new InvalidCastException($"Expected BadRequestObjectResult but got {result.GetType().Name}");
            return bad.Value;
        }
    }
}
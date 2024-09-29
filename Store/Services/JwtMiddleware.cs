using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Store.Services
{
    public class JwtAuthenticationAttribute : ActionFilterAttribute
    {
        private readonly string[] _allowedRoles;

        public JwtAuthenticationAttribute(params string[] roles)
        {
            _allowedRoles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request = filterContext.HttpContext.Request;

            // Check if the JWT cookie exists
            if (request.Cookies["jwt"] == null)
            {
                filterContext.Result = new HttpStatusCodeResult(401, "Token not found!");
                return; // Exit early if no token
            }

            var token = request.Cookies["jwt"].Value;
            if (token != null)
            {
                var jwtToken = Authentication.ValidateToken(token);
                if (jwtToken != null)
                {
                    var userName = jwtToken.Claims.First(claim => claim.Type == "kid").Value;
                    if (userName == null)
                    {
                        filterContext.Result = new HttpStatusCodeResult(401, "Token is expired!");
                        return; // Exit early if token is expired
                    }

                    string[] roleList = Authentication.GetRolesFromToken(token);

                    // Get current roles from the claims
                    var currentRoles = roleList;

                    // Check if user has any of the allowed roles
                    if (!_allowedRoles.Intersect(currentRoles).Any())
                    {
                        filterContext.Result = new HttpStatusCodeResult(403, "You do not have permission to access this resource!");
                        return; // Exit if the user does not have the required role
                    }
                }
            }
            else
            {
                filterContext.Result = new HttpStatusCodeResult(401, "Token not found!");
                return; // Exit early if no token
            }

            base.OnActionExecuting(filterContext);
        }
    }


}
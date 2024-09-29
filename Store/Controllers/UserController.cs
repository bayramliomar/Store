using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Store.Helpers;
using Store.Models;
using Store.Services;

namespace Store.Controllers
{
    public class UserController : Controller
    {
        private readonly Model _db;
        private readonly GoogleAuthenticatorService _googleAuthenticatorService;

        public UserController()
        {
            _db = new Model();
            _googleAuthenticatorService = new GoogleAuthenticatorService();
        }

        [JwtAuthentication("User")]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login(string token)
        {
            if (token is null) return new HttpStatusCodeResult(403);
            string result = SymmetricEncryption.Decrypt(token);
            string[] arr = result.Split('+');
            Guid userId = new Guid(arr[0]);
            var user = _db.Users.FirstOrDefault(u => u.Id == userId && u.UserName == u.UserName);
            if (user != null)
            {
                var userDetail = _db.UserDetails.FirstOrDefault(ud => ud.UserId == userId);
                if (userDetail != null && userDetail.TokenExpiration != null)
                {
                    DateTime date1 = Convert.ToDateTime(userDetail.TokenExpiration);
                    DateTime date2 = DateTime.Now;
                    TimeSpan difference = date2 - date1;
                    if (difference.TotalSeconds > 30)
                    {
                        return new HttpStatusCodeResult(403);
                    }
                    else
                    {
                        Session["UserId"] = null;
                        Session["UserName"] = null;
                        Session["RoleId"] = null;
                        Session["IsValidTwoFactorAuthentication"] = null;
                        return View();
                    }
                }
            }
            return new HttpStatusCodeResult(403);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(UserModel user)
        {
            if (ModelState.IsValid)
            {
                var userResult = _db.Users.FirstOrDefault(u => u.Email == user.Email);
                var usr = _db.Users.FirstOrDefault(u => u.Email == user.Email && u.PasswordHash == user.Password);


                if (userResult.AccessFailedCount >= 3)
                {
                    ModelState.AddModelError("", "Your account is blocked");
                    return View();
                }

                if (userResult != null && usr == null)
                {
                    if (userResult.AccessFailedCount >= 3)
                    {
                        ModelState.AddModelError("", "Your account is blocked");
                    }
                    userResult.AccessFailedCount += 1;
                    _db.SaveChanges();
                }

                if (usr is null)
                {
                    ModelState.AddModelError("", "Please enter valid username and password");
                    return View();
                }
                // Proceed with normal login (if Google Authenticator is not enabled)

                // Check if Google Authenticator is enabled for this user
                if (usr.TwoFactorEnabled) // Assuming IsGoogleAuthenticatorEnabled is a flag in your User model
                {
                    // Store the UserId in Session or TempData for later use in Google Auth verification
                    Session["UserIdFor2FA"] = usr.Id;

                    if (!string.IsNullOrEmpty(usr.SecurityStamp))
                    {
                        // Redirect to Google Authenticator Verification page
                        return RedirectToAction("VerifyGoogleAuthenticator");
                    }
                    else
                    {
                        return RedirectToAction("EnableGoogleAuthenticator");
                    }
                }

                return RedirectToAction("LoggedIn");
            }

            ModelState.AddModelError("", "Please enter valid username and password");
            return View();
        }

        [JwtAuthentication("User")]
        public ActionResult LoggedIn()
        {
            if (Session["UserId"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }


        public ActionResult EnableGoogleAuthenticator()
        {
            if(Session["UserIdFor2FA"]==null) return RedirectToAction("Login");

            System.Guid userId = new System.Guid(Session["UserIdFor2FA"].ToString());
            var user = _db.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            if(user.SecurityStamp!=null) return new HttpStatusCodeResult(403);

            // Generate a secret key and store it in the user's account
            string secretKey = _googleAuthenticatorService.GenerateSecretKey();
            user.SecurityStamp = secretKey;
            _db.SaveChanges();

            // Generate the QR code URI and image
            string qrCodeUri = _googleAuthenticatorService.GenerateQrCodeUri(user.UserName, secretKey, "YourAppName");
            string qrCodeImage = _googleAuthenticatorService.GenerateQrCodeImage(qrCodeUri);

            ViewBag.QrCodeImage = qrCodeImage;
            ViewBag.SecretKey = secretKey;

            return View();
        }

        [HttpGet]
        public ActionResult VerifyGoogleAuthenticator()
        {
            if (Session["UserIdFor2FA"] == null) return RedirectToAction("Login");
            return View();
        }

        [HttpPost]
        public ActionResult VerifyGoogleAuthenticator(string authCode)
        {
            System.Guid userId = new System.Guid(Session["UserIdFor2FA"].ToString());
            var user = _db.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return RedirectToAction("Login");
            }
            if (user.AccessFailedCount >= 3)
            {
                ModelState.AddModelError("", "Your account is blocked");
                return View();
            }

            // Use the service to verify the code
            bool isValid = _googleAuthenticatorService.VerifyCode(user.SecurityStamp, authCode);
            if (isValid)
            {
                Session["IsValidTwoFactorAuthentication"] = true; // Mark 2FA as passed

                var userRoles = _db.UserRoles.Where(ur => ur.UserId == user.Id).Select(ur => ur.RoleId).ToList();
                List<string> roles = _db.Roles.Where(r => userRoles.Contains(r.Id)).Select(r => r.Name).ToList();

                string jwtToken = Authentication.GenerateJWTAuthetication(user.UserName, roles);
                var cookie = new HttpCookie("jwt", jwtToken)
                {
                    HttpOnly = true,
                };
                Response.Cookies.Add(cookie);

                Session["UserId"] = user.Id.ToString();
                Session["UserName"] = user.UserName;
                Session["Roles"] = roles;

                return RedirectToAction("LoggedIn");
            }
            user.AccessFailedCount += 1;
            _db.SaveChanges();
            ModelState.AddModelError("", "Invalid Google Authenticator code");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            // Clear the session
            Session.Clear();
            Session.Abandon();

            // Sign out the user
            System.Web.Security.FormsAuthentication.SignOut();

            // Clear all cookies
            foreach (var cookie in Request.Cookies.AllKeys)
            {
                // Expire each cookie
                var expiredCookie = new HttpCookie(cookie)
                {
                    Expires = DateTime.Now.AddDays(-1) // Set the expiration date to the past
                };
                Response.Cookies.Add(expiredCookie);
            }

            // Optionally: If you want to clear the authentication cookie specifically
            if (Request.Cookies[System.Web.Security.FormsAuthentication.FormsCookieName] != null)
            {
                var authCookie = new HttpCookie(System.Web.Security.FormsAuthentication.FormsCookieName)
                {
                    Expires = DateTime.Now.AddDays(-1)
                };
                Response.Cookies.Add(authCookie);
            }

            // Redirect to login or another page after logging out
            return RedirectToAction("Login", "User");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

}

using Store.Helpers;
using Store.Models;
using Store.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Telegram.Bot.Types;

namespace Store.Controllers
{
    public class TelegramController : Controller
    {
        private readonly TelegramService _telegramService;
        private readonly Model _db;

        public TelegramController()
        {
            _db = new Model();
            _telegramService = new TelegramService();
        }

        [HttpPost]
        public async Task<ActionResult> Post([System.Web.Http.FromBody] Update update)
        {
            if (update?.Message?.Text == "/login")
            {
                UserDetail userDetail = null;
                var from = update.Message.From;
                var user = _db.Users.FirstOrDefault(u => u.UserName == from.Username);
                if (user != null)
                {
                    userDetail = _db.UserDetails.FirstOrDefault(ud => ud.UserId == user.Id);
                }
                if (user != null && userDetail != null)
                {
                    userDetail.TelegramId = from.Id;
                    userDetail.TelegramUserName = from.Username;
                    var loginLink = GenerateLoginLink(userDetail, user);
                    await _telegramService.SendMessageAsync(update.Message.Chat.Id, $"Your login link: {loginLink}");
                }
                else if (user != null && userDetail == null)
                {
                    UserDetail ud = new UserDetail()
                    {
                        UserId = user.Id,
                        TelegramId = from.Id,
                        TelegramUserName = from.Username
                    };
                    _db.UserDetails.Add(ud);
                    var loginLink = GenerateLoginLink(ud, user);
                    await _telegramService.SendMessageAsync(update.Message.Chat.Id, $"Your login link:\n {loginLink}");
                }
                else
                {
                    await _telegramService.SendMessageAsync(update.Message.Chat.Id, "Username not found.");
                }
            }

            return new HttpStatusCodeResult(200);
        }

        private string GenerateLoginLink(UserDetail userDetail, Models.User user)
        {
            string dataToEncrypt = $"{userDetail.UserId}+{user.UserName}+{DateTime.Now.TimeOfDay.ToString()}";
            string token = SymmetricEncryption.Encrypt(dataToEncrypt);
            var loginLink = Url.Action("Login", "User", new { token }, Request.Url.Scheme);

            //Save token to database with expiration
            userDetail.TokenExpiration = DateTime.Now.AddSeconds(30);
            _db.SaveChanges();

            return loginLink;
        }
    }
}
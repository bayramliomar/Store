using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Telegram.Bot;

namespace Store
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            string botToken = ConfigurationManager.AppSettings["config:TelegramBotToken"];
            var botClient = new TelegramBotClient(botToken);

            string webhookUrl = "https://b9e4-37-32-65-118.ngrok-free.app/Telegram/Post";
            botClient.SetWebhookAsync(webhookUrl).Wait();
        }
    }
}

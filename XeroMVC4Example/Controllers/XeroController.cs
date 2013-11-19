using System;
using System.Web.Mvc;
using XeroMVC4Example.Filters;
using XeroMVC4Example.Services;

namespace XeroMVC4Example.Controllers
{
	[InitXeroProvider]
    public class XeroController : Controller
    {

        public ActionResult Connect()
        {
			var oauthSession = XeroProvider.GetCurrentSession();
			var repository = XeroProvider.GetCurrentRepository();

			if (oauthSession.HasValidAccessToken && repository != null && repository.Organisation != null)
			{
				return new RedirectResult("~/");
			}

			var callbackUri = new UriBuilder(Request.Url.Scheme, Request.Url.Host, Request.Url.Port, Url.Action("Callback"));

			var requestToken = oauthSession.GetRequestToken(callbackUri.Uri);
			var authorisationUrl = oauthSession.GetUserAuthorizationUrl();
			
			return new RedirectResult(authorisationUrl);
        }


		public ActionResult Refresh()
		{
			var session = XeroProvider.GetCurrentSession();
			var newAccessToken = session.RenewAccessToken();

			return Redirect("~/");
		}

		public ActionResult Callback()
		{
			var oauthSession = XeroProvider.GetCurrentSession();
			var verificationCode = Request.Params["oauth_verifier"];

			var accessToken = oauthSession.ExchangeRequestTokenForAccessToken(verificationCode);
			var xeroRepo = XeroProvider.GetCurrentRepository();
			var xeroOrganisation = xeroRepo.Organisation;

			return Redirect("~/");
		}
    }
}

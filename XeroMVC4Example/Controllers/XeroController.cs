using System;
using System.Collections.Specialized;
using System.Web.Mvc;
using XeroApi.Model.Reporting;
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
				Playroom();
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
			Playroom();
			
			return Redirect("~/");
		}

		private static void Playroom()
		{
			var xeroRepo = XeroProvider.GetCurrentRepository();
			if (xeroRepo.Organisation != null)
			{
				var panlReport = xeroRepo.Reports.RunDynamicReport(new ProfitAndLossReport());
				var balanceReport = xeroRepo.Reports.RunDynamicReport(new BalanceSheetReport());
				//var arReport = xeroRepo.Reports.RunDynamicReport(new AgedPayablesByContactReport());
				//var apReport = xeroRepo.Reports.RunDynamicReport(new AgedPayablesByContactReport());

				if (panlReport != null)
				{
					foreach (var reportTitle in panlReport.ReportTitles)
					{
						Console.WriteLine("\t" + reportTitle);
					}

					foreach (var reportRow in panlReport.Rows)
					{
						Console.WriteLine(" " + reportRow.Title);

						if (reportRow.Rows != null)
						{
							foreach (var subRow in reportRow.Rows)
							{
								Console.Write(" Row: " + subRow.RowType);

								foreach (var cell in subRow.Cells)
								{
									Console.Write(cell.Value + ", ");
								}

								Console.WriteLine();
							}
						}
					}
				}
			}
		}
    }
}

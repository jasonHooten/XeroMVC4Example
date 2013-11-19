using System;
using System.Web.Mvc;
using XeroMVC4Example.Services;

namespace XeroMVC4Example.Filters
{
	[AttributeUsage(AttributeTargets.Class)]
	public class InitXeroProviderAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			XeroProvider.CurrentTokenRepository = new SessionStateTokenRepository(filterContext.HttpContext.Session);
		}
	}
}
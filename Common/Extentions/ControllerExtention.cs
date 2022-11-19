using Microsoft.AspNetCore.Mvc;

namespace Common.Extentions
{
    public static class ControllerExtention
    {
        public static String? ControllerAction<T>(this Microsoft.AspNetCore.Mvc.IUrlHelper urlHelper, string name, object? arg)
            where T:ControllerBase
        {
            var ct = typeof(T);
            var mi = ct.GetMethod(name);
            if (mi == null)
                return null;
            var controller = ct.Name.Replace("Controller", "");
            var action = urlHelper.Action(name,controller,arg);
            return action;
        }
    }
}

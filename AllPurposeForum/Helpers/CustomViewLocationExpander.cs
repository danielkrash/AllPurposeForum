using Microsoft.AspNetCore.Mvc.Razor;

namespace AllPurposeForum.Helpers
{
    public class CustomViewLocationExpander : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        { }

        public virtual IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            //Replace folder view with CustomViews
            return viewLocations.Select(f => f.Replace("/Views/", "/Web/Views/"));
        }
    }
}

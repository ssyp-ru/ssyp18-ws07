using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LenteApp.UI.Web.Pages
{
    public class IndexModel : PageModel
    {
        public static LenteApp.Impl.Sergey.SergeySearch dosearch = null;

        public void OnGet()
        {
        }
    }
}
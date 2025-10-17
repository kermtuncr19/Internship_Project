using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing; // <-- eklendi
using StoreApp.Models;

namespace StoreApp.Infrastructure.TagHelpers
{
    [HtmlTargetElement("div", Attributes = "page-model")]
    public class PageLinkTagHelper : TagHelper
    {
        private readonly IUrlHelperFactory _urlHelperFactory;

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext? ViewContext { get; set; }

        public Pagination PageModel { get; set; } = default!;
        public string? PageAction { get; set; }

        public bool PageClassesEnabled { get; set; } = false;
        public string PageClass { get; set; } = string.Empty;
        public string PageClassNormal { get; set; } = string.Empty;
        public string PageClassSelected { get; set; } = string.Empty;

        // 🔑 View'dan "page-url-*" olarak gelen her şey burada toplanır
        [HtmlAttributeName(DictionaryAttributePrefix = "page-url-")]
        public Dictionary<string, string?> PageUrlValues { get; set; } = new();

        public PageLinkTagHelper(IUrlHelperFactory urlHelperFactory)
        {
            _urlHelperFactory = urlHelperFactory;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (ViewContext is null || PageModel is null) return;

            IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);
            TagBuilder container = new TagBuilder("div");

            for (int i = 1; i <= PageModel.TotalPage; i++)
            {
                // route değerlerini oluştur
                var routeValues = new RouteValueDictionary();
                foreach (var kv in PageUrlValues)
                    routeValues[kv.Key] = kv.Value;

                routeValues["PageNumber"] = i; // her link için sayfa numarası

                var a = new TagBuilder("a");
                a.Attributes["href"] = urlHelper.Action(PageAction, routeValues);
                a.InnerHtml.Append(i.ToString());

                if (PageClassesEnabled)
                {
                    a.AddCssClass(PageClass);
                    a.AddCssClass(i == PageModel.CurrentPage ? PageClassSelected : PageClassNormal);
                }

                // Erişilebilirlik (opsiyonel ama faydalı)
                if (i == PageModel.CurrentPage)
                    a.Attributes["aria-current"] = "page";

                container.InnerHtml.AppendHtml(a);
            }

            output.Content.SetHtmlContent(container.InnerHtml);
        }
    }
}

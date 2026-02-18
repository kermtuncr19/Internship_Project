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

            int total = PageModel.TotalPage;
            int current = PageModel.CurrentPage;

            // Kaç komşu sayfa görünsün? (6'nın solu/sağı 2 => 4 5 6 7 8)
            int window = 1; // istersen 2 yap

            // Helper: Link üret
            TagBuilder BuildLink(int page, string text, bool isActive = false, bool isDisabled = false, string? ariaLabel = null)
            {
                var routeValues = new RouteValueDictionary();
                foreach (var kv in PageUrlValues)
                    routeValues[kv.Key] = kv.Value;

                routeValues["PageNumber"] = page;

                var a = new TagBuilder("a");

                if (isDisabled)
                {
                    a.Attributes["href"] = "#";
                    a.Attributes["tabindex"] = "-1";
                    a.Attributes["aria-disabled"] = "true";
                }
                else
                {
                    a.Attributes["href"] = urlHelper.Action(PageAction, routeValues);
                }

                a.InnerHtml.Append(text);

                if (PageClassesEnabled)
                {
                    a.AddCssClass(PageClass);
                    a.AddCssClass(isActive ? PageClassSelected : PageClassNormal);
                    if (isDisabled) a.AddCssClass("disabled");
                }

                if (isActive) a.Attributes["aria-current"] = "page";
                if (!string.IsNullOrWhiteSpace(ariaLabel)) a.Attributes["aria-label"] = ariaLabel;

                return a;
            }

            // 1) Prev
            container.InnerHtml.AppendHtml(
                BuildLink(
                    page: Math.Max(1, current - 1),
                    text: "«",
                    isActive: false,
                    isDisabled: current == 1,
                    ariaLabel: "Önceki sayfa"
                )
            );

            // 2) Sayfa aralığı hesapla
            int start = Math.Max(1, current - window);
            int end = Math.Min(total, current + window);

            // Her zaman 1 ve total görünsün istiyoruz
            // Start 2'nin üstündeyse başa 1 + "..."
            if (start > 1)
            {
                container.InnerHtml.AppendHtml(BuildLink(1, "1", isActive: current == 1));

                if (start > 2)
                {
                    var dots = new TagBuilder("span");
                    dots.InnerHtml.Append("…");
                    dots.AddCssClass("px-2");
                    container.InnerHtml.AppendHtml(dots);
                }
            }

            // 3) Orta sayfalar
            for (int i = start; i <= end; i++)
            {
                // 1 ve total zaten yukarı/aşağıda basılabilir, çakışmayı önle
                if (i == 1 || i == total) continue;
                container.InnerHtml.AppendHtml(BuildLink(i, i.ToString(), isActive: i == current));
            }

            // End sondan küçükse sona "..." + total
            if (end < total)
            {
                if (end < total - 1)
                {
                    var dots = new TagBuilder("span");
                    dots.InnerHtml.Append("…");
                    dots.AddCssClass("px-2");
                    container.InnerHtml.AppendHtml(dots);
                }

                container.InnerHtml.AppendHtml(BuildLink(total, total.ToString(), isActive: current == total));
            }

            // Eğer toplam sayfa 1 ise 1'i bas (yukarıdaki koşullar atlayabilir)
            if (total == 1)
            {
                container.InnerHtml.AppendHtml(BuildLink(1, "1", isActive: true));
            }

            // 4) Next
            container.InnerHtml.AppendHtml(
                BuildLink(
                    page: Math.Min(total, current + 1),
                    text: "»",
                    isActive: false,
                    isDisabled: current == total,
                    ariaLabel: "Sonraki sayfa"
                )
            );

            output.Content.SetHtmlContent(container.InnerHtml);
        }

    }
}

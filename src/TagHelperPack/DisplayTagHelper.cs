using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TagHelperPack;

/// <summary>
/// Renders the HTML markup from a display template for the specified model expression.
/// </summary>
[HtmlTargetElement("display", Attributes = "for", TagStructure = TagStructure.WithoutEndTag)]
public class DisplayTagHelper : TagHelper
{
    private const string ViewDataDictionaryName = "view-data";
    private const string ViewDataPrefix = "view-data-";

    private readonly IHtmlHelper _htmlHelper;
    private IDictionary<string, object> _viewData;

    /// <summary>
    /// Creates a new instance of the <see cref="DisplayNameTagHelper" /> class.
    /// </summary>
    /// <param name="htmlHelper">The <see cref="IHtmlHelper"/>.</param>
    public DisplayTagHelper(IHtmlHelper htmlHelper)
    {
        _htmlHelper = htmlHelper;
    }

    /// <summary>
    /// An expression to be evaluated against the current model.
    /// </summary>
    [HtmlAttributeName("for")]
    public ModelExpression For { get; set; }

    /// <summary>
    /// The name of the HTML field to use instead of the default one.
    /// </summary>
    [HtmlAttributeName("html-field-name")]
    public string HtmlFieldName { get; set; }

    /// <summary>
    /// The name of the template to use instead of the default one.
    /// </summary>
    [HtmlAttributeName("template-name")]
    public string TemplateName { get; set; }

    /// <summary>
    /// Additional view data.
    /// </summary>
    [HtmlAttributeName(ViewDataDictionaryName, DictionaryAttributePrefix = ViewDataPrefix)]
    public IDictionary<string, object> ViewData
    {
        get => _viewData ??= new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        set => _viewData = value;
    }

    /// <summary>
    /// Gets or sets the <see cref="ViewContext"/>.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; }

    /// <summary>
    /// CSS class name.
    /// </summary>
    [HtmlAttributeName("class")]
    public string Class { get; set; }

    /// <summary>
    /// CSS inline style.
    /// </summary>
    [HtmlAttributeName("style")]
    public string Style { get; set; }

    /// <summary>
    /// HTML id.
    /// </summary>
    [HtmlAttributeName("id")]
    public string Id { get; set; }

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (output == null)
        {
            throw new ArgumentNullException(nameof(output));
        }

        if (context.SuppressedByAspIf() || context.SuppressedByAspAuthz())
        {
            return;
        }

        ((IViewContextAware)_htmlHelper).Contextualize(ViewContext);

        //create a local htmlAttributes dictionary for html attributes exposed by the tag helper
        IDictionary<string, object> htmlAttributes = new Dictionary<string, object>();
        if (!string.IsNullOrWhiteSpace(Id))
        {
            htmlAttributes["id"] = Id;
        }
        if (!string.IsNullOrWhiteSpace(Class))
        {
            htmlAttributes["class"] = Class;
        }
        if (!string.IsNullOrWhiteSpace(Style))
        {
            htmlAttributes["style"] = Style;
        }

        //get the htmlAttributes property from tag ViewData
        ViewData.TryGetValue("htmlAttributes", out var viewDataHtmlAttributes);

        //merging local and ViewData htmlAttributes properties
        htmlAttributes = _htmlHelper.MergeHtmlAttributesObjects(htmlAttributes, viewDataHtmlAttributes);

        //setting the merged htmlAttributes on the ViewData of the tag.
        ViewData["htmlAttributes"] = htmlAttributes;

        output.Content.SetHtmlContent(_htmlHelper.Display(For, HtmlFieldName, TemplateName, ViewData));

        output.TagName = null;
    }
}

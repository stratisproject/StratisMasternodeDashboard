#pragma checksum "F:\BlockchainPractice\StratisProjects\StratisMasterNodeDashboardSatyaBranch\StratisMasternodeDashboard-master\Views\Shared\Partials\IDGBMemberKick.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "1ca238adba23832ffd95ef83e0994f7449c3b742"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Shared_Partials_IDGBMemberKick), @"mvc.1.0.view", @"/Views/Shared/Partials/IDGBMemberKick.cshtml")]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#nullable restore
#line 1 "F:\BlockchainPractice\StratisProjects\StratisMasterNodeDashboardSatyaBranch\StratisMasternodeDashboard-master\Views\_ViewImports.cshtml"
using Stratis.FederatedSidechains.AdminDashboard;

#line default
#line hidden
#nullable disable
#nullable restore
#line 2 "F:\BlockchainPractice\StratisProjects\StratisMasterNodeDashboardSatyaBranch\StratisMasternodeDashboard-master\Views\_ViewImports.cshtml"
using Stratis.FederatedSidechains.AdminDashboard.Models;

#line default
#line hidden
#nullable disable
#nullable restore
#line 3 "F:\BlockchainPractice\StratisProjects\StratisMasterNodeDashboardSatyaBranch\StratisMasternodeDashboard-master\Views\_ViewImports.cshtml"
using Stratis.FederatedSidechains.AdminDashboard.Entities;

#line default
#line hidden
#nullable disable
#nullable restore
#line 4 "F:\BlockchainPractice\StratisProjects\StratisMasterNodeDashboardSatyaBranch\StratisMasternodeDashboard-master\Views\_ViewImports.cshtml"
using Newtonsoft.Json;

#line default
#line hidden
#nullable disable
#nullable restore
#line 5 "F:\BlockchainPractice\StratisProjects\StratisMasterNodeDashboardSatyaBranch\StratisMasternodeDashboard-master\Views\_ViewImports.cshtml"
using Newtonsoft.Json.Linq;

#line default
#line hidden
#nullable disable
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"1ca238adba23832ffd95ef83e0994f7449c3b742", @"/Views/Shared/Partials/IDGBMemberKick.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"e1a24f040cc4175f37c8ac496f676714e6b28ec0", @"/Views/_ViewImports.cshtml")]
    public class Views_Shared_Partials_IDGBMemberKick : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<Vote>
    {
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_0 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("type", "text", global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_1 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("class", new global::Microsoft.AspNetCore.Html.HtmlString("form-control"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_2 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("id", new global::Microsoft.AspNetCore.Html.HtmlString("kick"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_3 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("aria-describedby", new global::Microsoft.AspNetCore.Html.HtmlString("hashHelp"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_4 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("placeholder", new global::Microsoft.AspNetCore.Html.HtmlString("Enter IDGB Member key"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_5 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("id", new global::Microsoft.AspNetCore.Html.HtmlString("kickForm"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_6 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("class", new global::Microsoft.AspNetCore.Html.HtmlString("needs-validation"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        #line hidden
        #pragma warning disable 0649
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperExecutionContext __tagHelperExecutionContext;
        #pragma warning restore 0649
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner __tagHelperRunner = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner();
        #pragma warning disable 0169
        private string __tagHelperStringValueBuffer;
        #pragma warning restore 0169
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __backed__tagHelperScopeManager = null;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __tagHelperScopeManager
        {
            get
            {
                if (__backed__tagHelperScopeManager == null)
                {
                    __backed__tagHelperScopeManager = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager(StartTagHelperWritingScope, EndTagHelperWritingScope);
                }
                return __backed__tagHelperScopeManager;
            }
        }
        private global::Microsoft.AspNetCore.Mvc.TagHelpers.FormTagHelper __Microsoft_AspNetCore_Mvc_TagHelpers_FormTagHelper;
        private global::Microsoft.AspNetCore.Mvc.TagHelpers.RenderAtEndOfFormTagHelper __Microsoft_AspNetCore_Mvc_TagHelpers_RenderAtEndOfFormTagHelper;
        private global::Microsoft.AspNetCore.Mvc.TagHelpers.InputTagHelper __Microsoft_AspNetCore_Mvc_TagHelpers_InputTagHelper;
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            WriteLiteral(@"
<script type=""text/javascript"">
    function submitKick() {         
        var pubKey = $(""#kick"").val();
        $.ajax({
            type: 'POST',
            url: '/sidechain-node/schedulekick',
            dataType: 'json',
            contentType: 'application/json',
            data: JSON.stringify(pubKey),
        })
            .done(function (data) {
                $(""#kickError"").hide();
                var pubKey = $(""#kick"").val();
                $(""#kick"").val("""");
                $(""#kickLabel"").text(pubKey);
                $(""#federationVotesIDGBKick"").modal('hide');
                $(""#confirmationModalIDGBKick"").modal('show');
            })
            .fail(function (result) {
                if (!!result.responseText) {
                    $(""#kickError"").text(result.responseText);
                    $(""#kickError"").show();
                    return;
                }
                $(""#kickError"").text(""Failed to kick. Unknown error. Please try again and ");
            WriteLiteral(@"make sure nodes are running."");
                $(""#kickError"").show();
            });
    }
</script>
<div class=""modal fade right"" id=""federationVotesIDGBKick"" tabindex=""-1"" role=""dialog"">
    <div class=""modal-dialog modal-dialog-centered"" role=""document"" style=""width: 950px"">
        <div class=""modal-content"">
            <div class=""modal-header"">
                <h5 class=""modal-title"">IDGB Kick Federation Member</h5>
                <button type=""button"" class=""close"" data-dismiss=""modal"" aria-label=""Close"">
                    <span aria-hidden=""true"">&times;</span>
                </button>
            </div>
            <div class=""modal-body"">
                <div class=""alert alert-danger"" role=""alert"" style=""display: none;"" id=""kickError"">
                </div>
                ");
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("form", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "1ca238adba23832ffd95ef83e0994f7449c3b7428786", async() => {
                WriteLiteral("\r\n                    <div class=\"form-group\">\r\n                        <label for=\"exampleInputEmail1\">IDGB Member Key</label>\r\n                        ");
                __tagHelperExecutionContext = __tagHelperScopeManager.Begin("input", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagOnly, "1ca238adba23832ffd95ef83e0994f7449c3b7429203", async() => {
                }
                );
                __Microsoft_AspNetCore_Mvc_TagHelpers_InputTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.InputTagHelper>();
                __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_InputTagHelper);
#nullable restore
#line 47 "F:\BlockchainPractice\StratisProjects\StratisMasterNodeDashboardSatyaBranch\StratisMasternodeDashboard-master\Views\Shared\Partials\IDGBMemberKick.cshtml"
__Microsoft_AspNetCore_Mvc_TagHelpers_InputTagHelper.For = ModelExpressionProvider.CreateModelExpression(ViewData, __model => __model.PubKey);

#line default
#line hidden
#nullable disable
                __tagHelperExecutionContext.AddTagHelperAttribute("asp-for", __Microsoft_AspNetCore_Mvc_TagHelpers_InputTagHelper.For, global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
                __Microsoft_AspNetCore_Mvc_TagHelpers_InputTagHelper.InputTypeName = (string)__tagHelperAttribute_0.Value;
                __tagHelperExecutionContext.AddTagHelperAttribute(__tagHelperAttribute_0);
                __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_1);
                __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_2);
                __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_3);
                __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_4);
                BeginWriteTagHelperAttribute();
                __tagHelperStringValueBuffer = EndWriteTagHelperAttribute();
                __tagHelperExecutionContext.AddHtmlAttribute("required", Html.Raw(__tagHelperStringValueBuffer), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.Minimized);
                await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
                if (!__tagHelperExecutionContext.Output.IsContentModified)
                {
                    await __tagHelperExecutionContext.SetOutputContentAsync();
                }
                Write(__tagHelperExecutionContext.Output);
                __tagHelperExecutionContext = __tagHelperScopeManager.End();
                WriteLiteral(@"
                        <small id=""hashHelp"" class=""form-text text-muted"">Paste IDGB Member Key.</small>
                        <div class=""invalid-feedback"">
                            Please provide a member Key.
                        </div>
                    </div>
                    <button type=""button"" class=""btn btn-primary"" onclick=""submitKick()"">Submit</button>
                ");
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_FormTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.FormTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_FormTagHelper);
            __Microsoft_AspNetCore_Mvc_TagHelpers_RenderAtEndOfFormTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.RenderAtEndOfFormTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_RenderAtEndOfFormTagHelper);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_5);
            BeginWriteTagHelperAttribute();
            __tagHelperStringValueBuffer = EndWriteTagHelperAttribute();
            __tagHelperExecutionContext.AddHtmlAttribute("novalidate", Html.Raw(__tagHelperStringValueBuffer), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.Minimized);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_6);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            WriteLiteral(@"

                <div>
                    <label class=""mt-3""><strong>IDGB Member Kicking </strong></label>
                    <table class=""table table-sm table-striped table-bordered"" id=""kickfedmem-polls-table"">
                        <thead>
                            <tr>
                                <th class=""text-left"">ID</th>
                                <th class=""text-center"">Public Key</th>
                                <th class=""text-center"">Collateral Address</th>
                                <th class=""text-center"">Votes In Favour</th>

                            </tr>
                        </thead>
                        <tbody>
");
#nullable restore
#line 69 "F:\BlockchainPractice\StratisProjects\StratisMasterNodeDashboardSatyaBranch\StratisMasternodeDashboard-master\Views\Shared\Partials\IDGBMemberKick.cshtml"
                               int ordinalKick = 0; 

#line default
#line hidden
#nullable disable
#nullable restore
#line 70 "F:\BlockchainPractice\StratisProjects\StratisMasterNodeDashboardSatyaBranch\StratisMasternodeDashboard-master\Views\Shared\Partials\IDGBMemberKick.cshtml"
                             foreach (var poll in Model?.KickFederationMemberPolls ?? new List<PendingPoll>())
                            {

#line default
#line hidden
#nullable disable
            WriteLiteral("                                <tr>\r\n                                    <td class=\"text-left\">");
#nullable restore
#line 73 "F:\BlockchainPractice\StratisProjects\StratisMasterNodeDashboardSatyaBranch\StratisMasternodeDashboard-master\Views\Shared\Partials\IDGBMemberKick.cshtml"
                                                      Write(ordinalKick++);

#line default
#line hidden
#nullable disable
            WriteLiteral("</td>\r\n                                    <td class=\"text-left ellipsis\" style=\"min-width: 350px;\"");
            BeginWriteAttribute("title", " title=\"", 3716, "\"", 3736, 1);
#nullable restore
#line 74 "F:\BlockchainPractice\StratisProjects\StratisMasterNodeDashboardSatyaBranch\StratisMasternodeDashboard-master\Views\Shared\Partials\IDGBMemberKick.cshtml"
WriteAttributeValue("", 3724, poll.PubKey, 3724, 12, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(">");
#nullable restore
#line 74 "F:\BlockchainPractice\StratisProjects\StratisMasterNodeDashboardSatyaBranch\StratisMasternodeDashboard-master\Views\Shared\Partials\IDGBMemberKick.cshtml"
                                                                                                             Write(poll.PubKey);

#line default
#line hidden
#nullable disable
            WriteLiteral("</td>\r\n                                    <td class=\"text-center ellipsis\" style=\"min-width: 250px;\"");
            BeginWriteAttribute("title", " title=\"", 3851, "\"", 3882, 1);
#nullable restore
#line 75 "F:\BlockchainPractice\StratisProjects\StratisMasterNodeDashboardSatyaBranch\StratisMasternodeDashboard-master\Views\Shared\Partials\IDGBMemberKick.cshtml"
WriteAttributeValue("", 3859, poll.CollateralAddress, 3859, 23, false);

#line default
#line hidden
#nullable disable
            EndWriteAttribute();
            WriteLiteral(">");
#nullable restore
#line 75 "F:\BlockchainPractice\StratisProjects\StratisMasterNodeDashboardSatyaBranch\StratisMasternodeDashboard-master\Views\Shared\Partials\IDGBMemberKick.cshtml"
                                                                                                                          Write(poll.CollateralAddress);

#line default
#line hidden
#nullable disable
            WriteLiteral("</td>                                   \r\n                                    <td class=\"text-center ellipsis\">");
#nullable restore
#line 76 "F:\BlockchainPractice\StratisProjects\StratisMasterNodeDashboardSatyaBranch\StratisMasternodeDashboard-master\Views\Shared\Partials\IDGBMemberKick.cshtml"
                                                                 Write(poll.IsExecuted ? "51%" : poll.PubKeysHexVotedInFavor.Count.ToString());

#line default
#line hidden
#nullable disable
            WriteLiteral("</td>\r\n                                </tr>\r\n");
#nullable restore
#line 78 "F:\BlockchainPractice\StratisProjects\StratisMasterNodeDashboardSatyaBranch\StratisMasternodeDashboard-master\Views\Shared\Partials\IDGBMemberKick.cshtml"
                            }

#line default
#line hidden
#nullable disable
            WriteLiteral(@"                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>
</div>

<div class=""modal fade"" id=""confirmationModalIDGBKick"" tabindex=""-1"" role=""dialog"" aria-labelledby=""exampleModalLabel1"" aria-hidden=""true"">
    <div class=""modal-dialog"" role=""document"">
        <div class=""modal-content"">
            <div class=""modal-header"">
                <h5 class=""modal-title"" id=""exampleModalLabel1"">Pub Key submitted</h5>
                <button type=""button"" class=""close"" data-dismiss=""modal"" aria-label=""Close"">
                    <span aria-hidden=""true"">&times;</span>
                </button>
            </div>
            <div class=""modal-body"">
                You have successfully submitted <b id=""kickLabel""></b> Pub Key.
            </div>
            <div class=""modal-footer"">
                <button type=""button"" class=""btn btn-secondary"" data-dismiss=""modal"">Close</button>
            </div>
        </div>
    </");
            WriteLiteral("div>\r\n</div>\r\n");
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<Vote> Html { get; private set; }
    }
}
#pragma warning restore 1591

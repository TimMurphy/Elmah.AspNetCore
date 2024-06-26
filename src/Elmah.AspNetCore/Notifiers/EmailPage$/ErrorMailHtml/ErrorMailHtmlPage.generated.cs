using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34003
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Elmah.AspNetCore.Notifiers.ErrorMailHtml
{

//#line 2 "..\..\ErrorMailHtmlPage.cshtml"
    
    
    //#line default
    //#line hidden
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    internal partial class ErrorMailHtmlPage : RazorTemplateBase
    {

//#line hidden

        public override void Execute()
        {


WriteLiteral("\r\n");




            
            //#line 4 "..\..\ErrorMailHtmlPage.cshtml"
  
    // NB cast is not really required, but aids with intellisense!
    var error = (Error) this.Error;


            
            //#line default
            //#line hidden
WriteLiteral("<html>\r\n    <head>\r\n        <title>Error: ");


            
            //#line 10 "..\..\ErrorMailHtmlPage.cshtml"
                 Write(error.Message);

            
            //#line default
            //#line hidden
WriteLiteral(@"</title>
        <style type=""text/css"">
            body { font-family: verdana, arial, helvetic; font-size: x-small; }
            table.collection { border-collapse: collapse; border-spacing: 0; border: 1px solid black; width: 100% }
            table.collection tr { vertical-align: top; }
            table.collection th { text-align: left; border: 1px solid black; padding: 4px; font-weight: bold; }
            table.collection td { border: 1px solid black; padding: 4px; }
            td, th, pre { font-size: x-small; } 
            #errorDetail { padding: 1em; background-color: #FFFFCC; } 
            #errorMessage { font-size: medium; font-style: italic; color: maroon; }
            h1 { font-size: small; }
        </style>
    </head>
    <body>
        <p id=""errorMessage"">");


            
            //#line 24 "..\..\ErrorMailHtmlPage.cshtml"
                        Write(error.Type);

            
            //#line default
            //#line hidden
WriteLiteral(": ");


            
            //#line 24 "..\..\ErrorMailHtmlPage.cshtml"
                                     Write(error.Message);

            
            //#line default
            //#line hidden
WriteLiteral("</p>\r\n");


            
            //#line 25 "..\..\ErrorMailHtmlPage.cshtml"
         if (error.Time != DateTime.MinValue)
        {

            
            //#line default
            //#line hidden
WriteLiteral("            <p>Generated: ");


            
            //#line 27 "..\..\ErrorMailHtmlPage.cshtml"
                     Write(error.Time.ToString("r"));

            
            //#line default
            //#line hidden
WriteLiteral("</p>\r\n");


            
            //#line 28 "..\..\ErrorMailHtmlPage.cshtml"
        }

            
            //#line default
            //#line hidden

            
            //#line 29 "..\..\ErrorMailHtmlPage.cshtml"
         if (error.Detail.Length != 0)
        {

            
            //#line default
            //#line hidden


WriteLiteral("<pre id=\"errorDetail\">");


            
            //#line 34 "..\..\ErrorMailHtmlPage.cshtml"
                               Write(error.Detail);

            
            //#line default
            //#line hidden
WriteLiteral("</pre>\r\n");


            
            //#line 35 "..\..\ErrorMailHtmlPage.cshtml"
        }

            
            //#line default
            //#line hidden

            
            //#line 36 "..\..\ErrorMailHtmlPage.cshtml"
         foreach (var collection in 
            from collection in new[] 
            {
                new
                {
                    Id    = "ServerVariables",
                    Title = "Server Variables",
                    Items = error.ServerVariables,
                }
            }
            let data = collection.Items
            where data != null && data.Count > 0
            let items = from i in Enumerable.Range(0, data.Count)
                        select new KeyValuePair<string,string>(data.GetKey(i), data[i])
            select new
            {
                collection.Id, 
                collection.Title,
                Items = items.OrderBy(e => e.Key, StringComparer.OrdinalIgnoreCase)
            }
        )
        {

            
            //#line default
            //#line hidden
WriteLiteral("            <div id=\"");


            
            //#line 58 "..\..\ErrorMailHtmlPage.cshtml"
                Write(collection.Id);

            
            //#line default
            //#line hidden
WriteLiteral("\">\r\n                <h1>");


            
            //#line 59 "..\..\ErrorMailHtmlPage.cshtml"
               Write(collection.Title);

            
            //#line default
            //#line hidden
WriteLiteral("</h1>\r\n                <table class=\"collection\">\r\n                    <tr><th>Na" +
"me</th>            \r\n                        <th>Value</th></tr>\r\n");


            
            //#line 63 "..\..\ErrorMailHtmlPage.cshtml"
                     foreach (var item in collection.Items)
                    {

            
            //#line default
            //#line hidden
WriteLiteral("                        <tr><td>");


            
            //#line 65 "..\..\ErrorMailHtmlPage.cshtml"
                           Write(item.Key);

            
            //#line default
            //#line hidden
WriteLiteral("</td>\r\n                            <td>");


            
            //#line 66 "..\..\ErrorMailHtmlPage.cshtml"
                           Write(item.Value);

            
            //#line default
            //#line hidden
WriteLiteral("</td></tr>\r\n");


            
            //#line 67 "..\..\ErrorMailHtmlPage.cshtml"
                    }

            
            //#line default
            //#line hidden
WriteLiteral("                </table>\r\n            </div>\r\n");


            
            //#line 70 "..\..\ErrorMailHtmlPage.cshtml"
        }

            
            //#line default
            //#line hidden
WriteLiteral("        <p>");


            
            
            //#line default
            //#line hidden
WriteLiteral("</p>\r\n    </body>\r\n</html>\r\n");


        }
    }
}
#pragma warning restore 1591

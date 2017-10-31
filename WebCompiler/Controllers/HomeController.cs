using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using WebCompiler.Models;
using System.Threading.Tasks;
using System.Threading;
using WebCompiler.Code;

namespace WebCompiler.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var method = @"public string Test() { return ""Hello World!""; }";
            var result = new CSharpCompiler<MethodTemplate>().ExecuteCode(method, "Test", null);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Build(string code, string functionName, string parameters, int timeLimit)
        {
            var compiledParams = new CSharpCompiler<ParamsTemplates>().ExecuteCode(parameters, "GetParams", null) as object[];
            var result = new CodeResult();

            try
            {
                result.Result = new CSharpCompiler<MethodTemplate>().ExecuteCode(code, functionName, timeLimit, compiledParams).ToString();
            }
            catch(StackOverflowException e)
            {
                result.Result = e.Message;
            }
            catch (Exception e)
            {
                result.Errors = e.Message;

            }
            return Json(result);
        }

        
    }
}
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

namespace WebCompiler.Controllers
{
    public class HomeController : Controller
    {
        
        public ActionResult Index()
        {

            var code = @"
                using System;
                namespace HelloWorld
                {
                    class Hello 
                    {
                       
                        public int Get(int[] x, int n) 
                        {
                            int s = 0;
                            for(int i = 0 ; i< n ; i++)
                                s += i;
                            return s;
                        }

                        public string Other() 
                        {
                            return ""Hello"";
                        }
                    }
                }  
            ";

            var result = ExecuteCode(code, "HelloWorld", "Hello", "Get", false, new int[] { 1, 2, 3, 4, 5 }, 5).ToString();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public ActionResult Build(string code, string functionName, string parameters)
        {
            code = Templates.GenerateTemplateForCode(code);

            var paramsCode = String.Format(Templates.Params, parameters);
            var compiledParams = ExecuteCode(paramsCode, "Templates", "Params", "GetParams", false) as object[];

            var result = new CodeResult();
            try
            {
                result.Result = ExecuteCode(code, "HelloWorld", "Hello", functionName, false, compiledParams).ToString();
            }
            catch (Exception e)
            {
                result.Errors = e.Message;
            }
            return Json(result);
        }

        private Assembly BuildAssembly(string code)
        {
            CSharpCodeProvider provider = new CSharpCodeProvider();
            ICodeCompiler compiler = provider.CreateCompiler();
            CompilerParameters compilerparams = new CompilerParameters();
            compilerparams.GenerateExecutable = false;
            compilerparams.GenerateInMemory = true;
            CompilerResults results = compiler.CompileAssemblyFromSource(compilerparams, code);
            if (results.Errors.HasErrors)
            {
                StringBuilder errors = new StringBuilder("Compiler Errors :\r\n");
                foreach (CompilerError error in results.Errors)
                {
                    errors.AppendFormat("Line {0},{1}\t: {2}\n", error.Line-5, error.Column, error.ErrorText);
                }
                throw new Exception(errors.ToString());
            }
            else
            {
                return results.CompiledAssembly;
            }
        }

        public object ExecuteCode(string code, string namespacename, string classname, string functionname, bool isstatic, params object[] args)
        {
            object returnval = null;
            Assembly asm = BuildAssembly(code);
            object instance = null;
            Type type = null;
            if (isstatic)
            {
                type = asm.GetType(namespacename + "." + classname);
            }
            else
            {
                instance = asm.CreateInstance(namespacename + "." + classname);
                type = instance.GetType();
            }
            MethodInfo method = type.GetMethod(functionname);
            returnval = method.Invoke(instance, args);
            return returnval;
        }
    }
}
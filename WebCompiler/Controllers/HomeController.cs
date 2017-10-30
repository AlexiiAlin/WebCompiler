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

            var result = ExecuteCode(code, "HelloWorld", "Hello", "Get", false, 10000, new int[] { 1, 2, 3, 4, 5 }, 5).ToString();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Build(string code, string functionName, string parameters, int timeLimit)
        {
            code = Templates.GenerateTemplateForCode(code);

            var paramsCode = String.Format(Templates.Params, parameters);
            var compiledParams = ExecuteCode(paramsCode, "Templates", "Params", "GetParams", false, 10000) as object[];

            var result = new CodeResult();
            try
            {
                result.Result = ExecuteCode(code, "HelloWorld", "Hello", functionName, false, timeLimit, compiledParams).ToString();
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

        //public string FormatErrorMessage ( string error)
        //{
        //    var index = 0;
        //    var copyError = error;
        //    while (index != -1)
        //    {
        //        index = copyError.IndexOf("Line");
        //        error.Insert(index, "<br/>");
        //        index+=
        //    }
        //}

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
                    errors.AppendFormat("<br/>Line {0},{1}\t: {2}\n", error.Line-5, error.Column, error.ErrorText);
                }
                throw new Exception(errors.ToString());
            }
            else
            {
                return results.CompiledAssembly;
            }
        }

        public object ExecuteCode(string code, string namespacename, string classname, string functionname, bool isstatic, int timeLimit, params object[] args)
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
            returnval = ExecuteMethod(method, instance, timeLimit, args);

            return returnval;
        }

        private object ExecuteMethod(MethodInfo method, object instance, int timeLimit, params object[] args) 
        {
            var tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;
            var task = Task.Factory.StartNew(() => method.Invoke(instance, args));

            if(!task.Wait(timeLimit, token))
            {
                tokenSource.Cancel();
                if (task.IsCompleted)
                {
                    task.Dispose();
                }
                throw new StackOverflowException("Time exceeded");
            }

            return task.Result;
        }
    }
}
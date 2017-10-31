using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace WebCompiler.Code
{
    public class CSharpCompiler<T>
        where T : CodeTemplate, new()
    {
        private CodeTemplate template;
        private int numberOfLinesDelay = 13;

    public CSharpCompiler()
        {
            template = new T();
        }

        

        public object ExecuteCode(string code, string functionName, int? timeLimit, params object[] args)
        {
            var isStatic = false;
            var namespaceName = template.Namespace;
            var className = template.ClassName;

            object returnval = null;
            Assembly asm = BuildAssembly(template.GetTemplate(code));
            object instance = null;
            Type type = null;
            if (isStatic)
            {
                type = asm.GetType(namespaceName + "." + className);
            }
            else
            {
                instance = asm.CreateInstance(namespaceName + "." + className);
                type = instance.GetType();
            }
            MethodInfo method = type.GetMethod(functionName);
            if(timeLimit.HasValue)
            {
                returnval = ExecuteMethod(method, instance, timeLimit.Value, args);
            }
            else
            {
                returnval = ExecuteMethod(method, instance, args);
            }
            

            return returnval;
        }

        private Assembly BuildAssembly(string code)
        {
            CSharpCodeProvider provider = new CSharpCodeProvider();
            ICodeCompiler compiler = provider.CreateCompiler();
            CompilerParameters compilerparams = new CompilerParameters();
            compilerparams.ReferencedAssemblies.Add("mscorlib.dll");
            compilerparams.ReferencedAssemblies.Add("System.dll");
            compilerparams.ReferencedAssemblies.Add("System.Core.dll");
            compilerparams.ReferencedAssemblies.Add("System.Data.Linq.dll");
            compilerparams.ReferencedAssemblies.Add("System.Data.Entity.dll");
            compilerparams.GenerateExecutable = false;
            compilerparams.GenerateInMemory = true;
            CompilerResults results = compiler.CompileAssemblyFromSource(compilerparams, code);

            if (results.Errors.HasErrors)
            {
                StringBuilder errors = new StringBuilder("Compiler Errors :\r\n");
                foreach (CompilerError error in results.Errors)
                {
                    errors.AppendFormat("<br/>Line {0},{1}\t: {2}\n", error.Line - numberOfLinesDelay, error.Column, error.ErrorText);
                }
                throw new Exception(errors.ToString());
            }
            else
            {
                return results.CompiledAssembly;
            }
        }

        private object ExecuteMethod(MethodInfo method, object instance, int timeLimit, params object[] args)
        {
            var tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;
            var task = Task.Factory.StartNew(() => method.Invoke(instance, args));

            if (!task.Wait(timeLimit, token))
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

        private object ExecuteMethod(MethodInfo method, object instance, params object[] args)
        {
            return method.Invoke(instance, args);
        }
    }
}
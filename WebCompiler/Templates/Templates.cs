using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebCompiler
{

    public abstract class CodeTemplate
    {
        public abstract string GetTemplate(params object[] args);
        public abstract string ClassName { get; }
        public string Namespace => "CodeCompiler";
    }

    public class ParamsTemplates : CodeTemplate
    {
        const string template = @"
using System;

namespace CodeCompiler
{{
    class Params 
    {{
                      
        unsafe public object[] GetParams() {{
            return new object[] {{
                {0}
            }};
        }}
    }}
}}
";

        public override string ClassName => "Params";

        public override string GetTemplate(params object[] args)
        {
            return String.Format(template, args);
        }
    }

    public class MethodTemplate : CodeTemplate
    {
        const string template = @"
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace CodeCompiler
{{
    unsafe public class Code
    {{

        {0}
    }}
}}";

        public override string ClassName => "Code";

        public override string GetTemplate(params object[] args)
        {
            return String.Format(template, args);
        }
    }
}
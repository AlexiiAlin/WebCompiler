using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebCompiler
{
    public class Templates
    {
        public const string Params = @"
            using System;
            namespace Templates
            {{
                class Params 
                {{
                       
                   public object[] GetParams() {{
                        return new object[] {{
                            {0}
                        }};
                   }}
                }}
            }}
        ";

        public static string GenerateTemplateForCode(string code)
        {
            var template = @"using System;
                            namespace HelloWorld
                            {
                            class Hello 
                            {
                            ";
            template = template + code;
            template = template + @"
                                    }
                                 }";
            return template;
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebCompiler.Models
{
    public class CodeResult
    {
        public object Result { get; set; }

        public string Errors { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace simplates
{
    public static class Tokens
    {
        public static Token If      { get { return new Token("if",      null, s => s); } }
        public static Token Eval    { get { return new Token("eval",    null, s => s); } }
        public static Token Foreach { get { return new Token("foreach", null, s => s); } }
    }
}

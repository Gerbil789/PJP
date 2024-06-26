﻿using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PJP_Project
{
    public class VerboseListener : BaseErrorListener
    {
        public override void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] IToken offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
            IList<string> stack = ((Parser)recognizer).GetRuleInvocationStack();
            stack.Reverse();

            Console.Error.WriteLine("rule stack: " + String.Join(", ", stack));
            Console.Error.WriteLine("line " + line + ":" + charPositionInLine + " at " + offendingSymbol + ": " + msg);
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace simplates.tests
{
    [TestClass]
    public class BasicOperations
    {
        [TestMethod]
        public void Templates_SimpleReplace()
        {
            var tokens = new TokensSet()
                .Add("x1", "Hello")
                .Add("x2", "world")
                .Add("em", "!");

            var template = "{{x1}} {{x2}}{{em}}";
            var rez = Templates.Process(template, tokens);

            Assert.AreEqual("Hello world!", rez);
        }
        
        [TestMethod]
        public void Templates_NestedReplace()
        {
            var tokens = new TokensSet()
                .Add("x1", "Hello {{x2}}")
                .Add("x2", "world{{em}}")
                .Add("em", "!");

            var template = "{{x1}}";
            var rez = Templates.Process(template, tokens);

            Assert.AreEqual("Hello world!", rez);
        } 

        [TestMethod]
        public void Template_FuncTokens()
        {
            var tokens = new TokensSet()
                .Add("x1", "Hello")
                .Add("x2", "world")
                .Add("em", "!");

            var funcs = new TokensSet()
                .Add("upper", s => s.ToUpper())
                .Add("em", () => "!");

            var template = "{{upper:{{x1}} {{x2}}}}{{em}}";
            var rez = Templates.Process(template, tokens, funcs);

            Assert.AreEqual("HELLO WORLD!", rez);
        }
    }
}

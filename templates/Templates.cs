using System;
using System.Collections.Generic;
using System.Text;

namespace templates
{
    public class Token
    {
        private readonly string _name;
        private readonly Func<string, string> _value;

        public Token(string name, Func<string, string> value)
        {
            _name = name;
            _value = value;
        }

        public Token(string name, Func<string> value)
        {
            _name = name;
            _value = s => value();
        }

        public Token(string name, string value)
        {
            _name = name;
            _value = s => value;
        }

        public string GetValue(string body)
        {
            return _value(body);
        }
    }

    public class TokensSet
    {
        private readonly Dictionary<string, Token> _tokens;

        public TokensSet()
        {
            _tokens = new Dictionary<string, Token>();
        }

        public TokensSet Add(string name, string value)
        {
            _tokens.Add(name, new Token(name, value));
            return this;
        }

        public TokensSet Add(string name, Func<string> value)
        {
            _tokens.Add(name, new Token(name, value));
            return this;
        }

        public TokensSet Add(string name, Func<string, string> value)
        {
            _tokens.Add(name, new Token(name, value));
            return this;
        }

        public bool Contains(string name)
        {
            return _tokens.ContainsKey(name);
        }

        public Token this[string name]
        {
            get { return _tokens[name]; }
        }
    }

    public static class Templates
    {
        public static string Process(string template, params TokensSet[] tokens)
        {
            var index = 0;
            var token_index = FindFirstToken(template);
            if (!TokenFound(token_index)) return template;
            var source = new TemplateSource(template);
            var rez = new TemplateRezult(template.Length);
            while (TokenFound(token_index))
            {
                rez.Append(source.Substring(from: index, to: token_index - 1));
                var token = ReadTokenAt(source, token_index);
                var token_value = CalculateToken(token, tokens);
                source.Replace(from: token.StartsAt, to: token.EndsAt, value: token_value);
                index = token.StartsAt;
                token_index = FindNextToken(in_template: source, starting_on: index);
            }
            rez.Append(source.Substring(from: index));
            return rez.ToString();
        }

        private class TemplateSource
        {
            public TemplateSource(string template)
            {
                throw new NotImplementedException();
            }

            public int IndexOf(string s, int starting_on, StringComparison comparison)
            {
                throw new NotImplementedException();
            }

            public string Substring(int from, int to)
            {
                throw new NotImplementedException();
            }

            public string Substring(int from)
            {
                throw new NotImplementedException();
            }

            public void Replace(int from, int to, string value)
            {
                throw new NotImplementedException();
            }
        }

        private class TemplateRezult
        {
            private readonly StringBuilder _builder;

            public TemplateRezult(int initial_size)
            {
                _builder = new StringBuilder(initial_size);
            }

            public void Append(string substring)
            {
                _builder.Append(substring);
            }

            public override string ToString()
            {
                return _builder.ToString();
            }
        }

        private class TokenEntry
        {
            public string Name;
            public string Body;
            public int StartsAt;
            public int EndsAt;
        }

        private static int FindFirstToken(string in_template)
        {
            return in_template.IndexOf("{{", StringComparison.Ordinal);
        }

        private static int FindNextToken(TemplateSource in_template, int starting_on)
        {
            return in_template.IndexOf("{{", starting_on, StringComparison.Ordinal);
        }

        private static bool TokenFound(int token_index)
        {
            return token_index >= 0;
        }

        private static TokenEntry ReadTokenAt(TemplateSource source, int token_index)
        {
            throw new NotImplementedException();
        }

        private static string CalculateToken(TokenEntry token, params TokensSet[] tokens)
        {
            foreach (var tokens_set in tokens)
                if (tokens_set.Contains(token.Name))
                    return tokens_set[token.Name].GetValue(Process(token.Body, tokens));
            throw new Exception(string.Format("Token {0} not found in data", token.Name));
        }

    }
}

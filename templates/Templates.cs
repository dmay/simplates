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
            private string _template;

            public TemplateSource(string template)
            {
                _template = template;
            }

            public int Length
            {
                get { return _template.Length; }
            }

            public int IndexOf(string s, int starting_on, StringComparison comparison)
            {
                return _template.IndexOf(s, starting_on, comparison);
            }

            public string Substring(int from, int to)
            {
                return _template.Substring(from, to - from + 1);
            }

            public string Substring(int from)
            {
                return _template.Substring(from);
            }

            public void Replace(int from, int to, string value)
            {
                _template = _template
                    .Remove(from, to - from + 1)
                    .Insert(from, value);
            }

            public char this[int char_index]
            {
                get { return _template[char_index]; }
            }

            public override string ToString()
            {
                return _template;
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

        private enum ReaderStates
        {
            Name, Splitter, Body, OpenBracket, CloseBracket, Done
        }

        private static TokenEntry ReadTokenAt(TemplateSource source, int token_index)
        {
            var char_index = token_index + 1;
            var brackets_count = 1;
            var state = ReaderStates.Name;
            var prev_state = state;
            var last_index = source.Length - 1;

            var name = "";
            var body = "";

            while (state != ReaderStates.Done && last_index > char_index)
            {
                char_index++;
                var ch = source[char_index];

                switch (state)
                {
                    case ReaderStates.Name:
                        if (ch == ':') state = ReaderStates.Splitter;
                        else if (ch == '}') state = ReaderStates.CloseBracket;
                        else name += ch;
                        break;
                    case ReaderStates.Splitter:
                        prev_state = state = ReaderStates.Body;
                        if (ch == '{') state = ReaderStates.OpenBracket;
                        else if (ch == '}') state = ReaderStates.CloseBracket;
                        else body += ch;
                        break;
                    case ReaderStates.Body:
                        if (ch == '{') state = ReaderStates.OpenBracket;
                        else if (ch == '}') state = ReaderStates.CloseBracket;
                        else body += ch;
                        break;
                    case ReaderStates.OpenBracket:
                        if (ch == '{') brackets_count++;
                        state = prev_state;
                        if (state == ReaderStates.Body) body = string.Concat(body, '{', ch);
                        else if (state == ReaderStates.Name) name = string.Concat(name, '{', ch);
                        break;
                    case ReaderStates.CloseBracket:
                        if (ch == '}')
                        {
                            brackets_count--;
                            if(brackets_count==0)state = ReaderStates.Done;
                        }
                        if (state != ReaderStates.Done)
                        {
                            state = prev_state;
                            if (state == ReaderStates.Body) body = string.Concat(body, '}', ch);
                            else if (state == ReaderStates.Name) name = string.Concat(name, '}', ch);
                        }
                        break;
                    case ReaderStates.Done:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if(state!=ReaderStates.Done)
                throw new Exception("Can't read token: "+source.ToString());

            return new TokenEntry
                {
                    Name = name,
                    Body = body,
                    StartsAt = token_index,
                    EndsAt = char_index
                };
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Kturker.UnityBase.Core.Domain
{
    // A small strict JSON reader (update files, string tables, saves): objects become Dictionary<string, object>, arrays List<object>,
    // numbers double, plus string, bool and null. Anything that is not exactly one JSON value (an HTML error page,
    // truncated text, trailing garbage, nesting deeper than 32) fails instead of throwing.
    public static class JsonText
    {
        private const int MaxDepth = 32;

        public static bool TryParse(string text, out object value)
        {
            value = null;
            if (string.IsNullOrEmpty(text)) return false;
            var reader = new Reader(text);
            try
            {
                reader.SkipWhitespace();
                var parsed = reader.ReadValue(0);
                reader.SkipWhitespace();
                if (!reader.AtEnd) return false;
                value = parsed;
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
            catch (OverflowException)
            {
                return false;
            }
        }

        private sealed class Reader
        {
            private readonly string text;
            private int position;

            public Reader(string text) => this.text = text;

            public bool AtEnd => position >= text.Length;

            public void SkipWhitespace()
            {
                while (!AtEnd && (text[position] == ' ' || text[position] == '\t' || text[position] == '\n' || text[position] == '\r')) position++;
            }

            public object ReadValue(int depth)
            {
                if (depth > MaxDepth) throw new FormatException("nested too deeply");
                if (AtEnd) throw new FormatException("unexpected end");
                switch (text[position])
                {
                    case '{': return ReadObject(depth);
                    case '[': return ReadArray(depth);
                    case '"': return ReadString();
                    case 't': Expect("true"); return true;
                    case 'f': Expect("false"); return false;
                    case 'n': Expect("null"); return null;
                    default: return ReadNumber();
                }
            }

            private Dictionary<string, object> ReadObject(int depth)
            {
                var result = new Dictionary<string, object>(StringComparer.Ordinal);
                position++;
                SkipWhitespace();
                if (Peek('}'))
                {
                    position++;
                    return result;
                }
                while (true)
                {
                    SkipWhitespace();
                    if (!Peek('"')) throw new FormatException("key expected");
                    var key = ReadString();
                    SkipWhitespace();
                    Consume(':');
                    SkipWhitespace();
                    result[key] = ReadValue(depth + 1);
                    SkipWhitespace();
                    if (Peek(','))
                    {
                        position++;
                        continue;
                    }
                    Consume('}');
                    return result;
                }
            }

            private List<object> ReadArray(int depth)
            {
                var result = new List<object>();
                position++;
                SkipWhitespace();
                if (Peek(']'))
                {
                    position++;
                    return result;
                }
                while (true)
                {
                    SkipWhitespace();
                    result.Add(ReadValue(depth + 1));
                    SkipWhitespace();
                    if (Peek(','))
                    {
                        position++;
                        continue;
                    }
                    Consume(']');
                    return result;
                }
            }

            private string ReadString()
            {
                position++;
                var builder = new StringBuilder();
                while (true)
                {
                    if (AtEnd) throw new FormatException("unterminated string");
                    var c = text[position++];
                    if (c == '"') return builder.ToString();
                    if (c < ' ') throw new FormatException("control character in a string");
                    if (c != '\\')
                    {
                        builder.Append(c);
                        continue;
                    }
                    if (AtEnd) throw new FormatException("unterminated escape");
                    var escaped = text[position++];
                    switch (escaped)
                    {
                        case '"': builder.Append('"'); break;
                        case '\\': builder.Append('\\'); break;
                        case '/': builder.Append('/'); break;
                        case 'b': builder.Append('\b'); break;
                        case 'f': builder.Append('\f'); break;
                        case 'n': builder.Append('\n'); break;
                        case 'r': builder.Append('\r'); break;
                        case 't': builder.Append('\t'); break;
                        case 'u':
                            if (position + 4 > text.Length) throw new FormatException("short unicode escape");
                            if (!int.TryParse(text.Substring(position, 4), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var code))
                                throw new FormatException("bad unicode escape");
                            builder.Append((char)code);
                            position += 4;
                            break;
                        default: throw new FormatException("bad escape");
                    }
                }
            }

            private double ReadNumber()
            {
                var start = position;
                if (Peek('-')) position++;
                if (!ReadDigits()) throw new FormatException("value expected");
                if (Peek('.'))
                {
                    position++;
                    if (!ReadDigits()) throw new FormatException("digits expected");
                }
                if (Peek('e') || Peek('E'))
                {
                    position++;
                    if (Peek('+') || Peek('-')) position++;
                    if (!ReadDigits()) throw new FormatException("exponent expected");
                }
                return double.Parse(text.Substring(start, position - start), NumberStyles.Float, CultureInfo.InvariantCulture);
            }

            private bool ReadDigits()
            {
                var start = position;
                while (!AtEnd && text[position] >= '0' && text[position] <= '9') position++;
                return position > start;
            }

            private void Expect(string word)
            {
                if (position + word.Length > text.Length || string.CompareOrdinal(text, position, word, 0, word.Length) != 0)
                    throw new FormatException(word + " expected");
                position += word.Length;
            }

            private bool Peek(char c) => !AtEnd && text[position] == c;

            private void Consume(char c)
            {
                if (!Peek(c)) throw new FormatException(c + " expected");
                position++;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
#pragma warning disable  CS1591

namespace Nistec.Web
{
    public class HtmlUtil
    {
        public static string HtmlDecode(string message)
        {
            if (message == null)
                return "";
            message = System.Web.HttpUtility.HtmlDecode(message);
            message = message.Replace("&quot;", "\"");
            message = message.Replace("&apos;", "'");
            message = message.Replace("&amp;", "&");

            return message;
        }
        public static string HtmlEncode(string message)
        {
            if (message == null)
                return "";
            message = System.Web.HttpUtility.HtmlEncode(message);

            return message;
        }

        public static string ToHtmlTable(NameValueCollection data, params string[] excludeKeys)
        {

            StringBuilder sb = new StringBuilder("<table>");

            var totalRows = data.GetValues(0).Count();
            for (int i = 0; i < totalRows; i++)
            {
                sb.AppendLine("<tr>");
                foreach (var key in data.AllKeys)
                {
                    if (excludeKeys != null && excludeKeys.Contains(key))
                        continue;
                    sb.AppendLine("<td>" + key + "</td>");
                    sb.AppendLine("<td>" + data.GetValues(key)[i] + "</td>");
                }
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</table>");
            return sb.ToString();
        }

        public static List<string> ParseHtmlTags(string inputHTML)
        {

            string pattern2 = @"(<[^>]*>)";
            var tagList = new List<string>();

            var patternMatch = Regex.Matches(inputHTML, pattern2);
            for (int i = 0; i < patternMatch.Count; i++)
            {
                tagList.Add(patternMatch[i].ToString());
            }
            return tagList;
        }

    }

    public class HtmlTag
    {
        /// <summary>
        /// Name of this tag
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Collection of attribute names and values for this tag
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; }

        /// <summary>
        /// True if this tag contained a trailing forward slash
        /// </summary>
        public bool TrailingSlash { get; set; }

        /// <summary>
        /// Indicates if this tag contains the specified attribute. Note that
        /// true is returned when this tag contains the attribute even when the
        /// attribute has no value
        /// </summary>
        /// <param name="name">Name of attribute to check</param>
        /// <returns>True if tag contains attribute or false otherwise</returns>
        public bool HasAttribute(string name)
        {
            return Attributes.ContainsKey(name);
        }
    };

    public class HtmlParser : TextParser
    {
        public HtmlParser()
        {
        }

        public HtmlParser(string html) : base(html)
        {
        }

        /// <summary>
        /// Parses the next tag that matches the specified tag name
        /// </summary>
        /// <param name="name">Name of the tags to parse ("*" = parse all tags)</param>
        /// <param name="tag">Returns information on the next occurrence of the
        /// specified tag or null if none found</param>
        /// <returns>True if a tag was parsed or false if the end of the document was reached</returns>
        public bool ParseNext(string name, out HtmlTag tag)
        {
            // Must always set out parameter
            tag = null;

            // Nothing to do if no tag specified
            if (String.IsNullOrEmpty(name))
                return false;

            // Loop until match is found or no more tags
            MoveTo('<');
            while (!EOF)
            {
                // Skip over opening '<'
                MoveAhead();

                // Examine first tag character
                char c = Peek();
                if (c == '!' && Peek(1) == '-' && Peek(2) == '-')
                {
                    // Skip over comments
                    const string endComment = "-->";
                    MoveTo(endComment);
                    MoveAhead(endComment.Length);
                }
                else if (c == '/')
                {
                    // Skip over closing tags
                    MoveTo('>');
                    MoveAhead();
                }
                else
                {
                    bool result, inScript;

                    // Parse tag
                    result = ParseTag(name, ref tag, out inScript);
                    // Because scripts may contain tag characters, we have special
                    // handling to skip over script contents
                    if (inScript)
                        MovePastScript();
                    // Return true if requested tag was found
                    if (result)
                        return true;
                }
                // Find next tag
                MoveTo('<');
            }
            // No more matching tags found
            return false;
        }

        /// <summary>
        /// Parses the contents of an HTML tag. The current position should be at the first
        /// character following the tag's opening less-than character.
        /// 
        /// Note: We parse to the end of the tag even if this tag was not requested by the
        /// caller. This ensures subsequent parsing takes place after this tag
        /// </summary>
        /// <param name="reqName">Name of the tag the caller is requesting, or "*" if caller
        /// is requesting all tags</param>
        /// <param name="tag">Returns information on this tag if it's one the caller is
        /// requesting</param>
        /// <param name="inScript">Returns true if tag began, and did not end, and script
        /// block</param>
        /// <returns>True if data is being returned for a tag requested by the caller
        /// or false otherwise</returns>
        protected bool ParseTag(string reqName, ref HtmlTag tag, out bool inScript)
        {
            bool doctype, requested;
            doctype = inScript = requested = false;

            // Get name of this tag
            string name = ParseTagName();

            // Special handling
            if (String.Compare(name, "!DOCTYPE", true) == 0)
                doctype = true;
            else if (String.Compare(name, "script", true) == 0)
                inScript = true;

            // Is this a tag requested by caller?
            if (reqName == "*" || String.Compare(name, reqName, true) == 0)
            {
                // Yes
                requested = true;
                // Create new tag object
                tag = new HtmlTag();
                tag.Name = name;
                tag.Attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            // Parse attributes
            MovePastWhitespace();
            while (Peek() != '>' && Peek() != NullChar)
            {
                if (Peek() == '/')
                {
                    // Handle trailing forward slash
                    if (requested)
                        tag.TrailingSlash = true;
                    MoveAhead();
                    MovePastWhitespace();
                    // If this is a script tag, it was closed
                    inScript = false;
                }
                else
                {
                    // Parse attribute name
                    name = (!doctype) ? ParseAttributeName() : ParseAttributeValue();
                    MovePastWhitespace();
                    // Parse attribute value
                    string value = String.Empty;
                    if (Peek() == '=')
                    {
                        MoveAhead();
                        MovePastWhitespace();
                        value = ParseAttributeValue();
                        MovePastWhitespace();
                    }
                    // Add attribute to collection if requested tag
                    if (requested)
                    {
                        // This tag replaces existing tags with same name
                        if (tag.Attributes.ContainsKey(name))
                            tag.Attributes.Remove(name);
                        tag.Attributes.Add(name, value);
                    }
                }
            }
            // Skip over closing '>'
            MoveAhead();

            return requested;
        }

        /// <summary>
        /// Parses a tag name. The current position should be the first character of the name
        /// </summary>
        /// <returns>Returns the parsed name string</returns>
        protected string ParseTagName()
        {
            int start = Position;
            while (!EOF && !Char.IsWhiteSpace(Peek()) && Peek() != '>')
                MoveAhead();
            return Substring(start, Position);
        }

        /// <summary>
        /// Parses an attribute name. The current position should be the first character
        /// of the name
        /// </summary>
        /// <returns>Returns the parsed name string</returns>
        protected string ParseAttributeName()
        {
            int start = Position;
            while (!EOF && !Char.IsWhiteSpace(Peek()) && Peek() != '>' && Peek() != '=')
                MoveAhead();
            return Substring(start, Position);
        }

        /// <summary>
        /// Parses an attribute value. The current position should be the first non-whitespace
        /// character following the equal sign.
        /// 
        /// Note: We terminate the name or value if we encounter a new line. This seems to
        /// be the best way of handling errors such as values missing closing quotes, etc.
        /// </summary>
        /// <returns>Returns the parsed value string</returns>
        protected string ParseAttributeValue()
        {
            int start, end;
            char c = Peek();
            if (c == '"' || c == '\'')
            {
                // Move past opening quote
                MoveAhead();
                // Parse quoted value
                start = Position;
                MoveTo(new char[] { c, '\r', '\n' });
                end = Position;
                // Move past closing quote
                if (Peek() == c)
                    MoveAhead();
            }
            else
            {
                // Parse unquoted value
                start = Position;
                while (!EOF && !Char.IsWhiteSpace(c) && c != '>')
                {
                    MoveAhead();
                    c = Peek();
                }
                end = Position;
            }
            return Substring(start, end);
        }

        /// <summary>
        /// Locates the end of the current script and moves past the closing tag
        /// </summary>
        protected void MovePastScript()
        {
            const string endScript = "</script";

            while (!EOF)
            {
                MoveTo(endScript, true);
                MoveAhead(endScript.Length);
                if (Peek() == '>' || Char.IsWhiteSpace(Peek()))
                {
                    MoveTo('>');
                    MoveAhead();
                    break;
                }
            }
        }

        public static List<HtmlTag> ScanTags(string html, string tagName)
        {
            // Download page
            //WebClient client = new WebClient();
            //string html = client.DownloadString(url);
            List<HtmlTag> list = new List<HtmlTag>();
            HtmlTag tag;
            HtmlParser parse = new HtmlParser(html);
            while (parse.ParseNext(tagName, out tag))
            {
                list.Add(tag);
            }
            return list;
        }

        public static List<HtmlTag> ScanLinksTags(string html)
        {
            // Download page
            //WebClient client = new WebClient();
            //string html = client.DownloadString(url);
            List<HtmlTag> list = new List<HtmlTag>();
            HtmlTag tag;
            HtmlParser parse = new HtmlParser(html);
            while (parse.ParseNext("a", out tag))
            {
                list.Add(tag);
            }
            return list;
        }

        public static void ScanLinksTags(string html, Action<HtmlTag> actioTag)
        {
            // Download page
            //WebClient client = new WebClient();
            //string html = client.DownloadString(url);

            // Scan links on this page
            HtmlTag tag;
            HtmlParser parse = new HtmlParser(html);
            while (parse.ParseNext("a", out tag))
            {
                actioTag(tag);
            }
        }
        public static void ScanLinksHref(string html, Action<string> actionHref)
        {
            // Download page
            //WebClient client = new WebClient();
            //string html = client.DownloadString(url);

            // Scan links on this page
            HtmlTag tag;
            HtmlParser parse = new HtmlParser(html);
            while (parse.ParseNext("a", out tag))
            {
                // See if this anchor links to us
                string value;

                if (tag.Attributes.TryGetValue("href", out value))
                {
                    // value contains URL referenced by this link
                    actionHref(value);
                }
            }
        }
    }

    public class TextParser
    {
        private string _text;
        private int _pos;

        public string Text { get { return _text; } }
        public int Position { get { return _pos; } }
        public int Remaining { get { return _text.Length - _pos; } }
        public static char NullChar = (char)0;

        public TextParser()
        {
            Reset(null);
        }

        public TextParser(string text)
        {
            Reset(text);
        }

        /// <summary>
        /// Resets the current position to the start of the current document
        /// </summary>
        public void Reset()
        {
            _pos = 0;
        }

        /// <summary>
        /// Sets the current document and resets the current position to the start of it
        /// </summary>
        /// <param name="text"></param>
        public void Reset(string text)
        {
            _text = (text != null) ? text : String.Empty;
            _pos = 0;
        }

        /// <summary>
        /// Indicates if the current position is at the end of the current document
        /// </summary>
        public bool EOF
        {
            get { return (_pos >= _text.Length); }
        }

        /// <summary>
        /// Returns the character at the current position, or a null character if we're
        /// at the end of the document
        /// </summary>
        /// <returns>The character at the current position</returns>
        public char Peek()
        {
            return Peek(0);
        }

        /// <summary>
        /// Returns the character at the specified number of characters beyond the current
        /// position, or a null character if the specified position is at the end of the
        /// document
        /// </summary>
        /// <param name="ahead">The number of characters beyond the current position</param>
        /// <returns>The character at the specified position</returns>
        public char Peek(int ahead)
        {
            int pos = (_pos + ahead);
            if (pos < _text.Length)
                return _text[pos];
            return NullChar;
        }

        /// <summary>
        /// Extracts a substring from the specified position to the end of the text
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public string Substring(int start)
        {
            return Substring(start, _text.Length);
        }

        /// <summary>
        /// Extracts a substring from the specified range of the current text
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public string Substring(int start, int end)
        {
            return _text.Substring(start, end - start);
        }

        /// <summary>
        /// Moves the current position ahead one character
        /// </summary>
        public void MoveAhead()
        {
            MoveAhead(1);
        }

        /// <summary>
        /// Moves the current position ahead the specified number of characters
        /// </summary>
        /// <param name="ahead">The number of characters to move ahead</param>
        public void MoveAhead(int ahead)
        {
            _pos = Math.Min(_pos + ahead, _text.Length);
        }

        /// <summary>
        /// Moves to the next occurrence of the specified string
        /// </summary>
        /// <param name="s">String to find</param>
        /// <param name="ignoreCase">Indicates if case-insensitive comparisons are used</param>
        public void MoveTo(string s, bool ignoreCase = false)
        {
            _pos = _text.IndexOf(s, _pos,
                ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
            if (_pos < 0)
                _pos = _text.Length;
        }

        /// <summary>
        /// Moves to the next occurrence of the specified character
        /// </summary>
        /// <param name="c">Character to find</param>
        public void MoveTo(char c)
        {
            _pos = _text.IndexOf(c, _pos);
            if (_pos < 0)
                _pos = _text.Length;
        }

        /// <summary>
        /// Moves to the next occurrence of any one of the specified
        /// characters
        /// </summary>
        /// <param name="carr">Array of characters to find</param>
        public void MoveTo(char[] carr)
        {
            _pos = _text.IndexOfAny(carr, _pos);
            if (_pos < 0)
                _pos = _text.Length;
        }

        /// <summary>
        /// Moves the current position to the first character that is part of a newline
        /// </summary>
        public void MoveToEndOfLine()
        {
            char c = Peek();
            while (c != '\r' && c != '\n' && !EOF)
            {
                MoveAhead();
                c = Peek();
            }
        }

        /// <summary>
        /// Moves the current position to the next character that is not whitespace
        /// </summary>
        public void MovePastWhitespace()
        {
            while (Char.IsWhiteSpace(Peek()))
                MoveAhead();
        }
    }

    public class HtmlParser2
    {
        protected string _html;
        protected int _pos;
        protected bool _scriptBegin;

        public HtmlParser2(string html)
        {
            Reset(html);
        }

        /// <summary>
        /// Resets the current position to the start of the current document
        /// </summary>
        public void Reset()
        {
            _pos = 0;
        }

        /// <summary>
        /// Sets the current document and resets the current position to the
        /// start of it
        /// </summary>
        /// <param name="html"></param>
        public void Reset(string html)
        {
            _html = html;
            _pos = 0;
        }

        /// <summary>
        /// Indicates if the current position is at the end of the current
        /// document
        /// </summary>
        public bool EOF
        {
            get { return (_pos >= _html.Length); }
        }

        /// <summary>
        /// Parses the next tag that matches the specified tag name
        /// </summary>
        /// <param name="name">Name of the tags to parse ("*" = parse all
        /// tags)</param>
        /// <param name="tag">Returns information on the next occurrence
        /// of the specified tag or null if none found</param>
        /// <returns>True if a tag was parsed or false if the end of the
        /// document was reached</returns>
        public bool ParseNext(string name, out HtmlTag tag)
        {
            tag = null;

            // Nothing to do if no tag specified
            if (String.IsNullOrEmpty(name))
                return false;

            // Loop until match is found or there are no more tags
            while (MoveToNextTag())
            {
                // Skip opening '<'
                Move();

                // Examine first tag character
                char c = Peek();
                if (c == '!' && Peek(1) == '-' && Peek(2) == '-')
                {
                    // Skip over comments
                    const string endComment = "-->";
                    _pos = _html.IndexOf(endComment, _pos);
                    NormalizePosition();
                    Move(endComment.Length);
                }
                else if (c == '/')
                {
                    // Skip over closing tags
                    _pos = _html.IndexOf('>', _pos);
                    NormalizePosition();
                    Move();
                }
                else
                {
                    // Parse tag
                    bool result = ParseTag(name, ref tag);

                    // Because scripts may contain tag characters,
                    // we need special handling to skip over
                    // script contents
                    if (_scriptBegin)
                    {
                        const string endScript = "</script";
                        _pos = _html.IndexOf(endScript, _pos,
                          StringComparison.OrdinalIgnoreCase);
                        NormalizePosition();
                        Move(endScript.Length);
                        SkipWhitespace();
                        if (Peek() == '>')
                            Move();
                    }

                    // Return true if requested tag was found
                    if (result)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Parses the contents of an HTML tag. The current position should
        /// be at the first character following the tag's opening less-than
        /// character.
        /// 
        /// Note: We parse to the end of the tag even if this tag was not
        /// requested by the caller. This ensures subsequent parsing takes
        /// place after this tag
        /// </summary>
        /// <param name="name">Name of the tag the caller is requesting,
        /// or "*" if caller is requesting all tags</param>
        /// <param name="tag">Returns information on this tag if it's one
        /// the caller is requesting</param>
        /// <returns>True if data is being returned for a tag requested by
        /// the caller or false otherwise</returns>

        protected bool ParseTag(string name, ref HtmlTag tag)
        {
            // Get name of this tag
            string s = ParseTagName();

            // Special handling
            bool doctype = _scriptBegin = false;
            if (String.Compare(s, "!DOCTYPE", true) == 0)
                doctype = true;
            else if (String.Compare(s, "script", true) == 0)
                _scriptBegin = true;

            // Is this a tag requested by caller?
            bool requested = false;
            if (name == "*" || String.Compare(s, name, true) == 0)
            {
                // Yes, create new tag object
                tag = new HtmlTag();
                tag.Name = s;
                tag.Attributes = new Dictionary<string, string>();
                requested = true;
            }

            // Parse attributes
            SkipWhitespace();
            while (Peek() != '>')
            {
                if (Peek() == '/')
                {
                    // Handle trailing forward slash
                    if (requested)
                        tag.TrailingSlash = true;
                    Move();
                    SkipWhitespace();
                    // If this is a script tag, it was closed
                    _scriptBegin = false;
                }
                else
                {
                    // Parse attribute name
                    s = (!doctype) ? ParseAttributeName() : ParseAttributeValue();
                    SkipWhitespace();
                    // Parse attribute value
                    string value = String.Empty;
                    if (Peek() == '=')
                    {
                        Move();
                        SkipWhitespace();
                        value = ParseAttributeValue();
                        SkipWhitespace();
                    }
                    // Add attribute to collection if requested tag
                    if (requested)
                    {
                        // This tag replaces existing tags with same name
                        if (tag.Attributes.Keys.Contains(s))
                            tag.Attributes.Remove(s);
                        tag.Attributes.Add(s, value);
                    }
                }
            }
            // Skip over closing '>'
            Move();

            return requested;
        }

        /// <summary>
        /// Parses a tag name. The current position should be the first
        /// character of the name
        /// </summary>
        /// <returns>Returns the parsed name string</returns>
        protected string ParseTagName()
        {
            int start = _pos;
            while (!EOF && !Char.IsWhiteSpace(Peek()) && Peek() != '>')
                Move();
            return _html.Substring(start, _pos - start);
        }

        /// <summary>
        /// Parses an attribute name. The current position should be the
        /// first character of the name
        /// </summary>
        /// <returns>Returns the parsed name string</returns>
        protected string ParseAttributeName()
        {
            int start = _pos;
            while (!EOF && !Char.IsWhiteSpace(Peek()) && Peek() != '>'
              && Peek() != '=')
                Move();
            return _html.Substring(start, _pos - start);
        }

        /// <summary>
        /// Parses an attribute value. The current position should be the
        /// first non-whitespace character following the equal sign.
        /// 
        /// Note: We terminate the name or value if we encounter a new line.
        /// This seems to be the best way of handling errors such as values
        /// missing closing quotes, etc.
        /// </summary>
        /// <returns>Returns the parsed value string</returns>
        protected string ParseAttributeValue()
        {
            int start, end;
            char c = Peek();
            if (c == '"' || c == '\'')
            {
                // Move past opening quote
                Move();
                // Parse quoted value
                start = _pos;
                _pos = _html.IndexOfAny(new char[] { c, '\r', '\n' }, start);
                NormalizePosition();
                end = _pos;
                // Move past closing quote
                if (Peek() == c)
                    Move();
            }
            else
            {
                // Parse unquoted value
                start = _pos;
                while (!EOF && !Char.IsWhiteSpace(c) && c != '>')
                {
                    Move();
                    c = Peek();
                }
                end = _pos;
            }
            return _html.Substring(start, end - start);
        }

        /// <summary>
        /// Moves to the start of the next tag
        /// </summary>
        /// <returns>True if another tag was found, false otherwise</returns>

        protected bool MoveToNextTag()
        {
            _pos = _html.IndexOf('<', _pos);
            NormalizePosition();
            return !EOF;
        }

        /// <summary>
        /// Returns the character at the current position, or a null
        /// character if we're at the end of the document
        /// </summary>
        /// <returns>The character at the current position</returns>
        public char Peek()
        {
            return Peek(0);
        }

        /// <summary>
        /// Returns the character at the specified number of characters
        /// beyond the current position, or a null character if the
        /// specified position is at the end of the document
        /// </summary>
        /// <param name="ahead">The number of characters beyond the
        /// current position</param>
        /// <returns>The character at the specified position</returns>
        public char Peek(int ahead)
        {
            int pos = (_pos + ahead);
            if (pos < _html.Length)
                return _html[pos];
            return (char)0;
        }

        /// <summary>
        /// Moves the current position ahead one character
        /// </summary>
        protected void Move()
        {
            Move(1);
        }

        /// <summary>
        /// Moves the current position ahead the specified number of characters
        /// </summary>
        /// <param name="ahead">The number of characters to move ahead</param>
        protected void Move(int ahead)
        {
            _pos = Math.Min(_pos + ahead, _html.Length);
        }

        /// <summary>
        /// Moves the current position to the next character that is
        /// not whitespace
        /// </summary>
        protected void SkipWhitespace()
        {
            while (!EOF && Char.IsWhiteSpace(Peek()))
                Move();
        }

        /// <summary>
        /// Normalizes the current position. This is primarily for handling
        /// conditions where IndexOf(), etc. return negative values when
        /// the item being sought was not found
        /// </summary>
        protected void NormalizePosition()
        {
            if (_pos < 0)
                _pos = _html.Length;
        }
    }

}

using System.Text.RegularExpressions;

public class Token
{
    public string classPart;
    public string valuePart;
    public int lineNo;

    public Token(string classPart, string valuePart, int lineNo)
    {
        this.classPart = classPart;
        this.valuePart = valuePart;
        this.lineNo = lineNo;
    }
}



public class Lexer
{
    public string[] punctuators = { "[", "]", "(", ")", "{", "}", ",", ";", ":" };
    public string[] compoundOperators = { "++", "--", "+=", "-=", "==", "!=", ">=", "<=", "/=", "*=", "%=", "^=" };
    public string[] operators = { "+", "-", "*", "/", "^", "%", "=", "==", "!=", "<", ">", ">=", "<=", "+=", "-=", "/=", "*=", "%=", "^=", "++", "--", "&", "!", "||" };
    private string[] separators = { "[", "]" ,"(", ")", "{", "}", ",", ";", ":", "+", "-", "*", "/", "^", "%", "=", "==", "!=", "<", ">", ">=", "<=", "+=", "-=", "/=", "*=", "%=", "^=", "++", "--", "&", "!", "||" };

    public List<string> Splitter(string sourceCode)
    {
        var words = new List<string>();
        int position = 0;
        int length = sourceCode.Length;
        //int currLine = 1;

        while (position < length)
        {
            while (position < length && char.IsWhiteSpace(sourceCode[position]))
            {
                if (sourceCode[position] == '\n')
                {
                    words.Add("newline");
                    //Console.WriteLine(currLine);
                }
                position++;
            }

            if (position < length)
            {
                if (sourceCode[position] == '~')
                {
                    // Check for single-line comments
                    position = sourceCode.IndexOf('\n', position);
                    if (position == -1)
                    {
                        position = length;
                    }
                    else
                    {
                        position++;
                    }
                }
                else if (sourceCode.Substring(position).StartsWith("*~"))
                {
                    // Check for multi-line comments
                    position += 2; // Skip the opening "*~"

                    while (position < length && !sourceCode.Substring(position).StartsWith("*~"))
                    {
                        position++;
                    }

                    if (position < length)
                    {
                        position += 2; // Skip the closing "*~"
                    }
                    else
                    {
                        Console.WriteLine("Error: Unclosed multi-line comment.");
                        break;
                    }
                }
                else if (sourceCode[position] == '\"')
                {
                    // Check for string literals
                    int start = position;
                    position++; // Move past the opening double quote

                    while (position < length && sourceCode[position] != '\"')
                    {
                        position++;
                    }

                    if (position < length)
                    {
                        position++; // Move past the closing double quote
                        string strLiteral = sourceCode.Substring(start, position - start);
                        words.Add(strLiteral);
                    }
                    else
                    {
                        Console.WriteLine("Error: Unclosed string literal.");
                        break;
                    }
                }
                else
                {
                    // Check for separators and compound operators
                    foreach (var cop in compoundOperators)
                    {
                        if (position + cop.Length <= sourceCode.Length)
                        {
                            string temp = sourceCode.Substring(position, cop.Length);

                            if (temp == cop)
                            {
                                words.Add(temp);
                                position += temp.Length;
                                //break;
                            }
                        }
                    }

                    foreach (var separator in separators)
                    {
                        if (sourceCode.Substring(position).StartsWith(separator))
                        {
                            words.Add(separator);
                            position += separator.Length;
                            //break;
                        }
                    }


                    // If not a separator or comment or string literal, extract the word
                    if (position < length && !char.IsWhiteSpace(sourceCode[position]))
                    {
                        int start = position;
                        while (position < length && !char.IsWhiteSpace(sourceCode[position]) && !Array.Exists(separators, sep => sourceCode.Substring(position).StartsWith(sep)))
                        {
                            position++;
                        }
                        string word = sourceCode.Substring(start, position - start);
                        words.Add(word);
                        if (string.IsNullOrEmpty(word))
                        {
                            words.Remove(word);
                        }



                    }
                }
            }
        }

        return words;
    }




    public List<Token> Tokenize(List<string> words)
    {
        List<Token> tokens = new List<Token>();
        int currLine = 1;



        foreach (string word in words)
        {
            if (word == "newline")
            {

                currLine++;
                continue;
            }
            else if (word == "$")
            {
                continue;
            }
            string classPart = GetTokenType(word);
            tokens.Add(new Token(classPart, word, currLine));
        }

        return tokens;
    }


    private string GetTokenType(string word)
    {
        string dataTypePattern = @"\b(int|string|float|double|char)\b";
        string booleanPattern = @"\b(true|false)\b";
        string floatConstantPattern = @"[-+]?\d*\.\d+([eE][-+]?\d+)?";
        string accessModifierPattern = @"\b(public|private|protected)\b";
        string loopControlPattern = @"\b(break|continue)\b";
        string otherKeywordPattern = @"\b(var|let|const|if|else|elif|while|function|return|null|main|Dyno|this|class|enum|case|switch|super|try|except|finally|catch|new|interface)\b";
        string identifierPattern = @"[a-zA-Z_]\w*";
        string integerPattern = @"\b\d+\b";
        string arithmeticOperatorPattern = @"[\+\-\*/%^]";
        string comparisonOperatorPattern = @"==|!=|<|>|<=|>=";
        string logicalOperatorPattern = @"(\&|\|\||!)";
        string assignmentOperatorPattern = @"[+\-*/%^]?=";
        //string[] punctuatorPatterns = { @"[\(\)\[\]\{\},;:]" };


        if (Regex.IsMatch(word, dataTypePattern))
        {
            return "DataType";
        }
        else if (Regex.IsMatch(word, booleanPattern))
        {
            return "Boolean";
        }
        else if (Regex.IsMatch(word, floatConstantPattern)) 
        {
            return "Float";
        }
        else if (word[0] == '\"' && word[word.Length - 1] == '\"')
        {
            return "StringLiteral";
        }
        else if (word == "(" || word == ")" || word == "{" || word == "}" || word == "[" || word == "]" || word == "," || word == ":" || word == ";"|| word =="."  || word == "'\n'")
        {
            return word;
        }
        else if (Regex.IsMatch(word, accessModifierPattern))
        {
            return "AccessModifier";
        }
        else if (Regex.IsMatch(word, loopControlPattern))
        {
            return "LoopControl";
        }
        else if (Regex.IsMatch(word, otherKeywordPattern))
        {
            return word;
        }
        else if (Regex.IsMatch(word, identifierPattern))
        {
            return "Identifier";
        }
        else if (Regex.IsMatch(word, integerPattern))
        {
            return "IntegerConstant";
        }
        else if (Regex.IsMatch(word, arithmeticOperatorPattern))
        {
            return "ArithmeticOperator";
        }
        else if (Regex.IsMatch(word, comparisonOperatorPattern))
        {
            return "ComparisonOperator";
        }
        else if (Regex.IsMatch(word, logicalOperatorPattern))
        {
            return "LogicalOperator";
        }
        else if (Regex.IsMatch(word, assignmentOperatorPattern))
        {
            return "AssignmentOperator";
        }
        else
        {
            return "Invalid token";
        }
    }

}

class Program
{
    static void Main()
    {
        try
        {
            // Read the source code from a file
            string filePath = "C:\\BSCS 6TH SEMESTER\\code.txt";
            string sourceCode = File.ReadAllText(filePath);


            var lexer = new Lexer();
            var words = lexer.Splitter(sourceCode);

            //Display words in the list
            Console.WriteLine("List of Words: ");
            foreach (var word in words)
            {
                if (word == "$" || word == "newline")
                {
                    continue;
                }
                Console.WriteLine(word);
            }



            var tokens = lexer.Tokenize(words);
            Console.WriteLine("----------Tokenization----------"); 
            Console.WriteLine("");
            foreach (var token in tokens)
            {
                Console.WriteLine($"ClassPart: {token.classPart}  ValuePart: {token.valuePart}   LineNo: {token.lineNo}");

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text.RegularExpressions;

//public class Token
//{
//    public string classPart;
//    public string valuePart;
//    public int lineNo;

//    public Token(string classPart, string valuePart, int lineNo)
//    {
//        this.classPart = classPart;
//        this.valuePart = valuePart;
//        this.lineNo = lineNo;
//    }
//}



//public class Lexer
//{
//    public string[] punctuators = { "[", "]", "(", ")", "{", "}", ",", ";", ":" };
//    public string[] compoundOperators = { "++", "--", "+=", "-=", "==", "!=", ">=", "<=", "/=", "*=", "%=", "^=" };
//    public string[] operators = { "+", "-", "*", "/", "^", "%", "=", "==", "!=", "<", ">", ">=", "<=", "+=", "-=", "/=", "*=", "%=", "^=", "++", "--", "&", "!", "||" };
//    private string[] separators = { "[", "]", "(", ")", "{", "}", ",", ";", ":", "+", "-", "*", "/", "^", "%", "=", "==", "!=", "<", ">", ">=", "<=", "+=", "-=", "/=", "*=", "%=", "^=", "++", "--", "&", "!", "||" };

//    public List<string> Splitter(string sourceCode)
//    {
//        var words = new List<string>();
//        int position = 0;
//        int length = sourceCode.Length;
//        //int currLine = 1;

//        while (position < length)
//        {
//            while (position < length && char.IsWhiteSpace(sourceCode[position]))
//            {
//                if (sourceCode[position] == '\n')
//                {
//                    words.Add("newline");
//                    //Console.WriteLine(currLine);
//                }
//                position++;
//            }

//            if (position < length)
//            {

//                if (sourceCode[position] == '~')
//                {
//                    // Single-line comment
//                    words.Add("single_line_comment");
//                    position = sourceCode.IndexOf('\n', position);
//                    if (position == -1)
//                    {
//                        position = length;
//                    }
//                    else
//                    {
//                        position++;
//                    }
//                }
//                else if (sourceCode.Substring(position).StartsWith("*~"))
//                {
//                    // Multi-line comment
//                    words.Add("multi_line_comment_start");
//                    position += 2; // Skip the opening "*~"

//                    while (position < length && !sourceCode.Substring(position).StartsWith("*~"))
//                    {
//                        if (sourceCode[position] == '\n')
//                        {
//                            words.Add("newline");
//                        }
//                        position++;
//                    }

//                    if (position < length )
//                    {
//                        position += 2; // Skip the closing "*~"
//                        words.Add("multi_line_comment_end");
//                    }
//                    else
//                    {
//                        Console.WriteLine("Error: Unclosed multi-line comment.");
//                        break;
//                    }
//                }

//                else if (sourceCode[position] == '\"')
//                {
//                    // Check for string literals
//                    int start = position;
//                    position++; // Move past the opening double quote

//                    while (position < length && sourceCode[position] != '\"')
//                    {
//                        position++;
//                    }

//                    if (position < length)
//                    {
//                        position++; // Move past the closing double quote
//                        string strLiteral = sourceCode.Substring(start, position - start);
//                        words.Add(strLiteral);
//                    }
//                    else
//                    {
//                        Console.WriteLine("Error: Unclosed string literal.");
//                        break;
//                    }
//                }
//                else
//                {
//                    // Check for separators and compound operators
//                    foreach (var cop in compoundOperators)
//                    {
//                        if (position + cop.Length <= sourceCode.Length)
//                        {
//                            string temp = sourceCode.Substring(position, cop.Length);

//                            if (temp == cop)
//                            {
//                                words.Add(temp);
//                                position += temp.Length;
//                                break;
//                            }
//                        }
//                    }

//                    foreach (var separator in separators)
//                    {
//                        if (sourceCode.Substring(position).StartsWith(separator))
//                        {
//                            words.Add(separator);
//                            position += separator.Length;
//                        }
//                    }

//                    // If not a separator or comment or string literal, extract the word
//                    if (position < length && !char.IsWhiteSpace(sourceCode[position]))
//                    {
//                        int start = position;
//                        while (position < length && !char.IsWhiteSpace(sourceCode[position]) && !Array.Exists(separators, sep => sourceCode.Substring(position).StartsWith(sep)))
//                        {
//                            position++;
//                        }
//                        string word = sourceCode.Substring(start, position - start);
//                        words.Add(word);
//                    }
//                }
//            }
//        }

//        return words;
//    }




//    public List<Token> Tokenize(List<string> words)
//    {
//        List<Token> tokens = new List<Token>();
//        int currLine = 1;



//        foreach (string word in words)
//        {

//            if (word == "newline")
//            {
//                //continue;
//                currLine++;
//                continue;
//            }
//            else if (word == "single_line_comment" ||  word == "multi_line_comment_start" || word == "multi_line_comment_end")
//            {

//                currLine++;
//                continue;
//            }
//            else if (word == "$")
//            {
//                continue;
//            }
//            string classPart = GetTokenType(word);
//            tokens.Add(new Token(classPart, word, currLine)); 
//        }

//        return tokens;
//    }


//    private string GetTokenType(string word)
//    {
//        string dataTypePattern = @"\b(int|string|float|double)\b";
//        string booleanPattern = @"\b(true|false)\b";
//        string floatConstantPattern = @"[-+]?\d*\.\d+([eE][-+]?\d+)?";
//        string accessModifierPattern = @"\b(public|private|protected)\b";
//        string loopControlPattern = @"\b(break|continue)\b";
//        string otherKeywordPattern = @"\b(var|let|const|if|else|elif|while|function|return|null|main|Dyno|this|class|enum|case|switch|super|try|except|finally|catch|new)\b";
//        string identifierPattern = @"[a-zA-Z_]\w*";
//        string integerPattern = @"\b\d+\b";
//        string arithmeticOperatorPattern = @"[\+\-\*/%^]";
//        string comparisonOperatorPattern = @"==|!=|<|>|<=|>=";
//        string logicalOperatorPattern = @"(\&|\|\||!)";
//        string assignmentOperatorPattern = @"[+\-*/%^]?=";
//        //string[] punctuatorPatterns = { @"[\(\)\[\]\{\},;:]" };


//        if (Regex.IsMatch(word, dataTypePattern))
//        {
//            return "DataType";
//        }
//        else if (Regex.IsMatch(word, booleanPattern))
//        {
//            return "Boolean";
//        }
//        else if (Regex.IsMatch(word, floatConstantPattern))
//        {
//            return "Float";
//        }
//        else if (word[0] == '\"' && word[word.Length - 1] == '\"')
//        {
//            return "StringLiteral";
//        }
//        else if (word == "(" || word == ")" || word == "{" || word == "}" || word == "[" || word == "]" || word == "," || word == ":" || word == ";")
//        {
//            return word;
//        }
//        else if (Regex.IsMatch(word, accessModifierPattern))
//        {
//            return "AccessModifier";
//        }
//        else if (Regex.IsMatch(word, loopControlPattern))
//        {
//            return "LoopControl";
//        }
//        else if (Regex.IsMatch(word, otherKeywordPattern))
//        {
//            return word;
//        }
//        else if (Regex.IsMatch(word, identifierPattern))
//        {
//            return "Identifier";
//        }
//        else if (Regex.IsMatch(word, integerPattern))
//        {
//            return "IntegerConstant";
//        }
//        else if (Regex.IsMatch(word, arithmeticOperatorPattern))
//        {
//            return "ArithmeticOperator";
//        }
//        else if (Regex.IsMatch(word, comparisonOperatorPattern))
//        {
//            return "ComparisonOperator";
//        }
//        else if (Regex.IsMatch(word, logicalOperatorPattern))
//        {
//            return "LogicalOperator";
//        }
//        else if (Regex.IsMatch(word, assignmentOperatorPattern))
//        {
//            return "AssignmentOperator";
//        }
//        else
//        {
//            return "Invalid token";
//        }
//    }

//}


//class Program
//{
//    static void Main()
//    {
//        try
//        {
//            // Read the source code from a file
//            string filePath = "C:\\BSCS 6TH SEMESTER\\code.txt";
//            string sourceCode = File.ReadAllText(filePath);


//            var lexer = new Lexer();
//            var words = lexer.Splitter(sourceCode);

//            //Display words in the list
//            Console.WriteLine("List of Words: ");
//            foreach (var word in words)
//            {
//                if (word == "$" || word == "newline" || word == "single_line_comment" || word == "multi_line_comment_start" || word == "multi_line_comment_end")
//                {
//                    continue;
//                }
//                Console.WriteLine(word);
//            }



//            var tokens = lexer.Tokenize(words);
//            Console.WriteLine("----------Tokenization----------");
//            Console.WriteLine("");
//            foreach (var token in tokens)
//            {
//                Console.WriteLine($"ClassPart: {token.classPart}  ValuePart: {token.valuePart}   LineNo: {token.lineNo}");

//            }
//        }
//        catch (Exception ex)
//        {
//            Console.WriteLine($"An error occurred: {ex.Message}");
//        }
//    }
//}




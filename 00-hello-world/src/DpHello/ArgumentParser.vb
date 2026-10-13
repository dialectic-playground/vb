Imports System.Collections.Generic

''' <summary>
''' Hand-written command-line parser (CONV-07 ... CONV-12). It recognizes syntax only: option
''' values are recorded and operands collected, and both are checked later by
''' <see cref="Validator"/>, so they never take precedence over --help or --version.
''' </summary>
Public Module ArgumentParser

    ''' <summary>
    ''' Scans <paramref name="args"/> (without the program name) from left to right. Returns
    ''' Help or Version as soon as --help or --version is reached, or the first syntax error
    ''' met before that point.
    ''' </summary>
    Public Function Parse(args As IReadOnlyList(Of String)) As ParseResult
        ArgumentNullException.ThrowIfNull(args)
        Dim name As OptionValue = Nothing
        Dim count As OptionValue = Nothing
        Dim firstOperand As String = Nothing
        Dim index = 0
        While index < args.Count
            Dim token = args(index)
            index += 1
            If String.Equals(token, "--", StringComparison.Ordinal) Then
                ' CONV-09: every later token is an operand.
                If firstOperand Is Nothing AndAlso index < args.Count Then firstOperand = args(index)
                Exit While
            End If

            ' Option values overwrite earlier ones: the last occurrence wins (CONV-11).
            Dim action = ParseAction.Greet
            Dim failure As UsageError = Nothing
            If token.StartsWith("--", StringComparison.Ordinal) Then
                failure = ParseLong(token, args, index, name, count, action)
            ElseIf token.Length > 1 AndAlso token(0) = "-"c Then
                failure = ParseShort(token, args, index, name, count)
            ElseIf firstOperand Is Nothing Then
                firstOperand = token
            End If

            If failure IsNot Nothing Then Return ParseResult.Fail(failure)
            If action <> ParseAction.Greet Then
                Return ParseResult.Success(New ParsedArguments(action, Nothing, Nothing, Nothing))
            End If
        End While
        Return ParseResult.Success(New ParsedArguments(ParseAction.Greet, name, count, firstOperand))
    End Function

    ''' <summary>One --long[=value] token (CONV-08). Advances <paramref name="index"/> past a separate value.</summary>
    Private Function ParseLong(token As String, args As IReadOnlyList(Of String), ByRef index As Integer,
                               ByRef name As OptionValue, ByRef count As OptionValue,
                               ByRef action As ParseAction) As UsageError
        Dim body = token.Substring(2)
        Dim equalsAt = body.IndexOf("="c, StringComparison.Ordinal)
        Dim longName = If(equalsAt < 0, body, body.Substring(0, equalsAt))
        Dim attached = If(equalsAt < 0, Nothing, body.Substring(equalsAt + 1))

        Select Case longName
            Case "help", "version"
                If attached IsNot Nothing Then Return UsageError.ArgumentNotAllowed(longName)
                action = If(longName = "help", ParseAction.Help, ParseAction.Version)
                Return Nothing
            Case "name", "count"
                Dim value = attached
                If value Is Nothing Then
                    If index >= args.Count Then Return UsageError.LongOptionRequiresArgument(longName)
                    value = args(index)
                    index += 1
                End If
                Dim recorded = New OptionValue(value, "--" & longName)
                If longName = "name" Then name = recorded Else count = recorded
                Return Nothing
            Case Else
                Return UsageError.UnrecognizedOption(token)
        End Select
    End Function

    ''' <summary>
    ''' One -x... token (CONV-07). Both short options take a value, which is the rest of the
    ''' token or, when that is empty, the next token.
    ''' </summary>
    Private Function ParseShort(token As String, args As IReadOnlyList(Of String), ByRef index As Integer,
                                ByRef name As OptionValue, ByRef count As OptionValue) As UsageError
        ' Report a whole code point, never half of a surrogate pair.
        Dim letterLength = If(Char.IsSurrogatePair(token, 1), 2, 1)
        Dim letter = token.Substring(1, letterLength)
        If letter <> "n" AndAlso letter <> "c" Then
            Return UsageError.InvalidOption(letter)
        End If

        Dim value = token.Substring(2)
        If value.Length = 0 Then
            If index >= args.Count Then Return UsageError.ShortOptionRequiresArgument(letter)
            value = args(index)
            index += 1
        End If
        Dim recorded = New OptionValue(value, "-" & letter)
        If letter = "n" Then name = recorded Else count = recorded
        Return Nothing
    End Function

End Module

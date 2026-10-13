Imports Xunit

Public Class ArgumentParserTests

    Private Shared Function ParseOk(ParamArray args As String()) As ParsedArguments
        Dim result = ArgumentParser.Parse(args)
        Assert.Null(result.Failure)
        Return result.Arguments
    End Function

    Private Shared Function ParseError(ParamArray args As String()) As String
        Dim result = ArgumentParser.Parse(args)
        Assert.Null(result.Arguments)
        Return result.Failure.Message
    End Function

    <Fact>
    Public Sub NoArgumentsGreetWithDefaults()
        Dim parsed = ParseOk()
        Assert.Equal(ParseAction.Greet, parsed.Action)
        Assert.Null(parsed.Name)
        Assert.Null(parsed.Count)
        Assert.Null(parsed.FirstOperand)
    End Sub

    <Theory>
    <InlineData("-nAda")>
    <InlineData("-n", "Ada")>
    <InlineData("--name=Ada")>
    <InlineData("--name", "Ada")>
    Public Sub NameSpellingsAreEquivalent(ParamArray args As String())
        Assert.Equal("Ada", ParseOk(args).Name.Value)
    End Sub

    <Theory>
    <InlineData("-n", "Ada", "-n")>
    <InlineData("-nAda", "-n")>
    <InlineData("--name", "Ada", "--name")>
    <InlineData("--name=Ada", "--name")>
    Public Sub SpellingIsRecordedAsWritten(ParamArray argsAndSpelling As String())
        Dim args = argsAndSpelling.Take(argsAndSpelling.Length - 1).ToArray()
        Assert.Equal(argsAndSpelling.Last(), ParseOk(args).Name.Spelling)
    End Sub

    <Fact>
    Public Sub BundledShortOptionsWithAttachedValues()
        Dim parsed = ParseOk("-c2", "-nKen")
        Assert.Equal("2", parsed.Count.Value)
        Assert.Equal("-c", parsed.Count.Spelling)
        Assert.Equal("Ken", parsed.Name.Value)
    End Sub

    <Fact>
    Public Sub ShortOptionConsumesRestOfBundle()
        Assert.Equal("c3", ParseOk("-nc3").Name.Value)
    End Sub

    <Theory>
    <InlineData("--name=-x", "-x")>
    <InlineData("-n-x", "-x")>
    <InlineData("--name=", "")>
    <InlineData("--name=a=b", "a=b")>
    Public Sub AttachedValuesAreVerbatim(token As String, expected As String)
        Assert.Equal(expected, ParseOk(token).Name.Value)
    End Sub

    <Theory>
    <InlineData("--name", "-x")>
    <InlineData("--name", "--help")>
    <InlineData("-n", "--")>
    Public Sub SeparateValueMayStartWithDash(optionToken As String, value As String)
        Assert.Equal(value, ParseOk(optionToken, value).Name.Value)
    End Sub

    <Fact>
    Public Sub LastOccurrenceWins()
        Dim parsed = ParseOk("-n", "A", "--name=B", "-c", "5", "--count", "7")
        Assert.Equal("B", parsed.Name.Value)
        Assert.Equal("7", parsed.Count.Value)
        Assert.Equal("--count", parsed.Count.Spelling)
    End Sub

    <Fact>
    Public Sub OperandsAreCollectedNotRejected()
        Dim parsed = ParseOk("extra", "-n", "Ada", "more")
        Assert.Equal("extra", parsed.FirstOperand)
        Assert.Equal("Ada", parsed.Name.Value)
    End Sub

    <Fact>
    Public Sub SingleDashIsAnOperand()
        Assert.Equal("-", ParseOk("-").FirstOperand)
    End Sub

    <Fact>
    Public Sub TerminatorMakesFollowingTokensOperands()
        Assert.Equal("-n", ParseOk("--", "-n").FirstOperand)
        Assert.Equal("--help", ParseOk("--", "--help").FirstOperand)
        Assert.Null(ParseOk("--").FirstOperand)
    End Sub

    <Theory>
    <InlineData("--help")>
    <InlineData("--help", "extra", "-x")>
    <InlineData("extra", "--help")>
    <InlineData("--count", "0", "--help")>
    <InlineData("--name=", "--help")>
    Public Sub HelpWinsWhenReachedBeforeASyntaxError(ParamArray args As String())
        Assert.Equal(ParseAction.Help, ParseOk(args).Action)
    End Sub

    <Fact>
    Public Sub FirstOfHelpAndVersionWins()
        Assert.Equal(ParseAction.Version, ParseOk("--version", "--help").Action)
        Assert.Equal(ParseAction.Help, ParseOk("--help", "--version").Action)
    End Sub

    <Fact>
    Public Sub SyntaxErrorBeforeHelpIsReported()
        Assert.Equal("invalid option -- 'x'", ParseError("-x", "--help"))
    End Sub

    <Theory>
    <InlineData("invalid option -- 'x'", "-x")>
    <InlineData("invalid option -- 'x'", "-xn")>
    <InlineData("invalid option -- 'N'", "-N", "Ada")>
    <InlineData("invalid option -- 'ë'", "-ë")>
    <InlineData("invalid option -- '😀'", "-😀")>
    <InlineData("unrecognized option '--nme'", "--nme", "Ada")>
    <InlineData("unrecognized option '--nam'", "--nam", "Ada")>
    <InlineData("unrecognized option '--x=1'", "--x=1")>
    <InlineData("unrecognized option '---name'", "---name")>
    <InlineData("option requires an argument -- 'n'", "-n")>
    <InlineData("option requires an argument -- 'c'", "-c")>
    <InlineData("option '--name' requires an argument", "--name")>
    <InlineData("option '--count' requires an argument", "--count")>
    <InlineData("option '--help' doesn't allow an argument", "--help=")>
    <InlineData("option '--version' doesn't allow an argument", "--version=1")>
    Public Sub SyntaxErrors(expected As String, ParamArray args As String())
        Assert.Equal(expected, ParseError(args))
    End Sub

End Class

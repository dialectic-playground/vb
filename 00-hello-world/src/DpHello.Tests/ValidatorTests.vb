Imports Xunit

Public Class ValidatorTests

    Private Shared Function Arguments(Optional name As OptionValue = Nothing,
                                      Optional count As OptionValue = Nothing,
                                      Optional operand As String = Nothing) As ParsedArguments
        Return New ParsedArguments(ParseAction.Greet, name, count, operand)
    End Function

    <Fact>
    Public Sub DefaultsAreWorldAndOne()
        Dim settings = Validator.Validate(Arguments()).Settings
        Assert.Equal("World", settings.Name)
        Assert.Equal(1, settings.Count)
    End Sub

    <Theory>
    <InlineData("Ada Lovelace")>
    <InlineData("Zoë")>
    <InlineData("-x")>
    <InlineData(" ")>
    Public Sub NonEmptyNamesAreKeptVerbatim(name As String)
        Assert.Equal(name, Validator.Validate(Arguments(name:=New OptionValue(name, "-n"))).Settings.Name)
    End Sub

    <Theory>
    <InlineData("-n")>
    <InlineData("--name")>
    Public Sub EmptyNameIsRejected(spelling As String)
        Dim result = Validator.Validate(Arguments(name:=New OptionValue("", spelling)))
        Assert.Null(result.Settings)
        Assert.Equal("invalid argument '' for '" & spelling & "'", result.Failure.Message)
    End Sub

    <Theory>
    <InlineData("1", 1)>
    <InlineData("2", 2)>
    <InlineData("999", 999)>
    <InlineData("1000", 1000)>
    <InlineData("007", 7)>
    <InlineData("0001000", 1000)>
    Public Sub ValidCounts(text As String, expected As Integer)
        Assert.Equal(expected, Validator.Validate(Arguments(count:=New OptionValue(text, "-c"))).Settings.Count)
    End Sub

    <Theory>
    <InlineData("0")>
    <InlineData("000")>
    <InlineData("1001")>
    <InlineData("99999999999999999999999")>
    <InlineData("")>
    <InlineData("abc")>
    <InlineData("-1")>
    <InlineData("+1")>
    <InlineData(" 1")>
    <InlineData("1 ")>
    <InlineData("1.0")>
    <InlineData("1e3")>
    <InlineData("٣")>
    <InlineData("３")>
    Public Sub InvalidCountsAreRejectedWithTheValueAndSpelling(text As String)
        Dim result = Validator.Validate(Arguments(count:=New OptionValue(text, "--count")))
        Assert.Null(result.Settings)
        Assert.Equal("invalid argument '" & text & "' for '--count'", result.Failure.Message)
    End Sub

    <Fact>
    Public Sub OperandIsReportedBeforeInvalidValues()
        Dim result = Validator.Validate(Arguments(New OptionValue("", "-n"), New OptionValue("0", "-c"), "extra"))
        Assert.Equal("extra operand 'extra'", result.Failure.Message)
    End Sub

    <Fact>
    Public Sub NameIsCheckedBeforeCount()
        Dim result = Validator.Validate(Arguments(New OptionValue("", "-n"), New OptionValue("0", "-c")))
        Assert.Equal("invalid argument '' for '-n'", result.Failure.Message)
    End Sub

End Class

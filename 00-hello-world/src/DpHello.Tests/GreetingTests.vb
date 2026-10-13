Imports Xunit

Public Class GreetingTests

    <Fact>
    Public Sub LineIsHelloNameBangLf()
        Assert.Equal("Hello, World!" & vbLf, Greeting.BuildLine("World"))
    End Sub

    <Theory>
    <InlineData("Ada Lovelace")>
    <InlineData("Zoë")>
    <InlineData("Hi!")>
    <InlineData("-x")>
    Public Sub NameIsVerbatim(name As String)
        Assert.Equal("Hello, " & name & "!" & vbLf, Greeting.BuildLine(name))
    End Sub

    <Theory>
    <InlineData(1)>
    <InlineData(3)>
    <InlineData(1000)>
    Public Sub TextRepeatsTheLineCountTimes(count As Integer)
        Dim expected = String.Concat(Enumerable.Repeat("Hello, Ken!" & vbLf, count))
        Assert.Equal(expected, Greeting.BuildText("Ken", count))
    End Sub

    <Theory>
    <InlineData(0)>
    <InlineData(-1)>
    Public Sub NonPositiveCountGivesEmptyText(count As Integer)
        Assert.Equal("", Greeting.BuildText("Ken", count))
    End Sub

End Class

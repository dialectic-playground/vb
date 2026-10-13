Imports System.IO
Imports Xunit

Public Class OutputTests

    <Fact>
    Public Sub EncodeIsUtf8WithoutBom()
        Assert.Equal(New Byte() {&H5A, &H6F, &HC3, &HAB, &HA}, Output.Encode("Zoë" & vbLf))
    End Sub

    <Fact>
    Public Sub EncodeReplacesUnpairedSurrogateInsteadOfThrowing()
        Assert.Equal(New Byte() {&HEF, &HBF, &HBD}, Output.Encode(ChrW(&HD800)))
    End Sub

    <Fact>
    Public Sub DiagnosticHasTwoLfTerminatedLines()
        Assert.Equal("dp-hello: extra operand 'x'" & vbLf & "Try 'dp-hello --help' for more information." & vbLf,
                     Output.Diagnostic(UsageError.ExtraOperand("x")))
    End Sub

    <Fact>
    Public Sub WriteAllWritesTheExactBytes()
        Using stream As New MemoryStream()
            Assert.True(Output.WriteAll(stream, New Byte() {1, 10, 13}))
            Assert.Equal(New Byte() {1, 10, 13}, stream.ToArray())
        End Using
    End Sub

    <Fact>
    Public Sub WriteAllReportsAFailingStream()
        Assert.False(Output.WriteAll(New BrokenStream(), New Byte() {1}))
    End Sub

    <Fact>
    Public Sub WriteAllReportsAClosedStream()
        Dim stream As New MemoryStream()
        stream.Dispose()
        Assert.False(Output.WriteAll(stream, New Byte() {1}))
    End Sub

    ''' <summary>A stream whose writes fail like a pipe whose reader has gone away.</summary>
    Private NotInheritable Class BrokenStream
        Inherits MemoryStream

        Public Overrides Sub Write(buffer() As Byte, offset As Integer, count As Integer)
            Throw New IOException("Broken pipe")
        End Sub
    End Class

End Class

Imports System.Text
Imports Xunit

''' <summary>End-to-end tests of the pure core: exact stdout and stderr bytes and exit status.</summary>
Public Class CommandLineToolTests

    Private Const TryLine As String = "Try 'dp-hello --help' for more information." & vbLf

    Private Shared Function Run(ParamArray args As String()) As RunResult
        Return CommandLineTool.Run(args)
    End Function

    Private Shared Sub AssertRun(stdout As String, stderr As String, status As Integer, ParamArray args As String())
        Dim result = Run(args)
        Assert.Equal(stdout, Encoding.UTF8.GetString(result.StandardOutput))
        Assert.Equal(stderr, Encoding.UTF8.GetString(result.StandardError))
        Assert.Equal(status, result.ExitCode)
    End Sub

    Private Shared Sub AssertUsageError(firstLine As String, ParamArray args As String())
        AssertRun("", "dp-hello: " & firstLine & vbLf & TryLine, 2, args)
    End Sub

    <Fact>
    Public Sub DefaultGreeting()
        AssertRun("Hello, World!" & vbLf, "", 0)
    End Sub

    <Fact>
    Public Sub NamedAndRepeatedGreeting()
        AssertRun("Hello, Ken!" & vbLf & "Hello, Ken!" & vbLf, "", 0, "-c2", "-n", "Ken")
    End Sub

    <Fact>
    Public Sub NonAsciiNameIsUtf8()
        Dim result = Run("--name", "Zoë")
        Assert.Equal(Encoding.UTF8.GetBytes("Hello, Zoë!" & vbLf), result.StandardOutput)
    End Sub

    <Fact>
    Public Sub MaximumCountPrintsOneThousandLines()
        Dim result = Run("--count=1000")
        Assert.Equal(1000 * ("Hello, World!" & vbLf).Length, result.StandardOutput.Length)
        Assert.DoesNotContain(CByte(13), result.StandardOutput)
    End Sub

    <Fact>
    Public Sub HelpIsTheUserManualText()
        Dim help = "Usage: dp-hello [OPTION]..." & vbLf &
            "Print a greeting to standard output." & vbLf &
            vbLf &
            "Options:" & vbLf &
            "  -n, --name=NAME     greet NAME instead of World" & vbLf &
            "  -c, --count=COUNT   print the greeting COUNT times (1-1000, default 1)" & vbLf &
            "      --help          display this help and exit" & vbLf &
            "      --version       output version information and exit" & vbLf
        AssertRun(help, "", 0, "--help")
        AssertRun(help, "", 0, "extra", "--help")
        AssertRun(help, "", 0, "--count", "0", "--help")
    End Sub

    <Fact>
    Public Sub Version()
        AssertRun("dp-hello (dialectic-playground) 1.0.0" & vbLf, "", 0, "--version")
    End Sub

    <Fact>
    Public Sub UsageErrorsWriteNothingToStdout()
        AssertUsageError("invalid option -- 'x'", "-x")
        AssertUsageError("invalid option -- 'x'", "-x", "--help")
        AssertUsageError("extra operand 'extra'", "extra")
        AssertUsageError("extra operand '-n'", "--", "-n")
        AssertUsageError("unrecognized option '--nme'", "--nme", "Ada")
        AssertUsageError("option '--name' requires an argument", "--name")
        AssertUsageError("option '--version' doesn't allow an argument", "--version=1")
        AssertUsageError("invalid argument '' for '--name'", "--name", "")
        AssertUsageError("invalid argument 'abc' for '-c'", "-c", "abc")
        AssertUsageError("invalid argument '1001' for '-c'", "-c", "1001")
        AssertUsageError("invalid argument '0' for '--count'", "--count=0")
    End Sub

    <Fact>
    Public Sub OnlyTheWinningValueIsValidated()
        AssertRun("Hello, B!" & vbLf, "", 0, "-c", "0", "-c", "1", "-n", "", "-n", "B")
        AssertUsageError("invalid argument '0' for '-c'", "-c", "1", "-c", "0")
    End Sub

End Class

Imports System.Collections.Generic

''' <summary>The outcome of one run: the exact bytes for stdout and stderr, and the exit status.</summary>
Public NotInheritable Class RunResult

    ''' <summary>Creates a run result.</summary>
    Public Sub New(standardOutput As Byte(), standardError As Byte(), exitCode As Integer)
        Me.StandardOutput = standardOutput
        Me.StandardError = standardError
        Me.ExitCode = exitCode
    End Sub

    ''' <summary>The bytes to write to standard output.</summary>
    Public ReadOnly Property StandardOutput As Byte()

    ''' <summary>The bytes to write to standard error.</summary>
    Public ReadOnly Property StandardError As Byte()

    ''' <summary>The exit status (0 or 2).</summary>
    Public ReadOnly Property ExitCode As Integer

End Class

''' <summary>
''' The pure core of dp-hello: arguments in, output bytes and exit status out. It wires the
''' parser, the validator, and greeting construction together and performs no I/O.
''' </summary>
Public Module CommandLineTool

    ''' <summary>Exit status for success, --help, and --version.</summary>
    Public Const ExitSuccess As Integer = 0

    ''' <summary>Exit status for usage errors (CONV-14).</summary>
    Public Const ExitUsage As Integer = 2

    ''' <summary>The version line (CONV-02, FR-08).</summary>
    Public ReadOnly Property VersionText As String = ToolName & " (dialectic-playground) 1.0.0" & vbLf

    ''' <summary>The help text from the user manual, byte for byte (CONV-23, FR-07).</summary>
    Public ReadOnly Property HelpText As String = String.Join(vbLf,
        "Usage: dp-hello [OPTION]...",
        "Print a greeting to standard output.",
        "",
        "Options:",
        "  -n, --name=NAME     greet NAME instead of World",
        "  -c, --count=COUNT   print the greeting COUNT times (1-1000, default 1)",
        "      --help          display this help and exit",
        "      --version       output version information and exit",
        "")

    ''' <summary>Runs dp-hello with <paramref name="args"/> (without the program name).</summary>
    Public Function Run(args As IReadOnlyList(Of String)) As RunResult
        Dim parsed = ArgumentParser.Parse(args)
        If parsed.Failure IsNot Nothing Then Return UsageFailure(parsed.Failure)

        Select Case parsed.Arguments.Action
            Case ParseAction.Help
                Return Success(HelpText)
            Case ParseAction.Version
                Return Success(VersionText)
        End Select

        Dim validated = Validator.Validate(parsed.Arguments)
        If validated.Failure IsNot Nothing Then Return UsageFailure(validated.Failure)
        Return Success(Greeting.BuildText(validated.Settings.Name, validated.Settings.Count))
    End Function

    Private Function Success(text As String) As RunResult
        Return New RunResult(Encode(text), Array.Empty(Of Byte)(), ExitSuccess)
    End Function

    Private Function UsageFailure(failure As UsageError) As RunResult
        Return New RunResult(Array.Empty(Of Byte)(), Encode(Diagnostic(failure)), ExitUsage)
    End Function

End Module

''' <summary>
''' The dp-hello executable: runs the pure core and writes its bytes to the raw standard
''' streams (never Console.Out, whose encoding and newline handling depend on the platform).
''' </summary>
Public Module Program

    ' CONV-25 leaves the status after a broken pipe unspecified; use the shell's SIGPIPE status.
    Private Const ExitBrokenPipe As Integer = 141

    ''' <summary>Entry point. .NET passes the arguments as UTF-16 strings on every OS (CONV-05).</summary>
    Public Function Main(args As String()) As Integer
        Dim result = CommandLineTool.Run(args)
        Using standardError = Console.OpenStandardError()
            Output.WriteAll(standardError, result.StandardError)
        End Using
        Using standardOutput = Console.OpenStandardOutput()
            If Not Output.WriteAll(standardOutput, result.StandardOutput) Then Return ExitBrokenPipe
        End Using
        Return result.ExitCode
    End Function

End Module

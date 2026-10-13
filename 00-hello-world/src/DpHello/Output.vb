Imports System.IO
Imports System.Text

''' <summary>
''' Byte-exact output (CONV-03, CONV-05, CONV-14, CONV-25). Text is encoded to UTF-8 here and
''' written as raw bytes to the standard streams, so there is no newline translation and no
''' console code page involved on any operating system.
''' </summary>
Public Module Output

    ''' <summary>The canonical tool name used in every diagnostic (CONV-01).</summary>
    Public Const ToolName As String = "dp-hello"

    ' UTF-8 without a byte-order mark; unpaired surrogates become U+FFFD instead of throwing.
    Private ReadOnly Utf8 As New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False, throwOnInvalidBytes:=False)

    ''' <summary>The UTF-8 bytes of <paramref name="text"/>, without a byte-order mark.</summary>
    Public Function Encode(text As String) As Byte()
        Return Utf8.GetBytes(text)
    End Function

    ''' <summary>The two CONV-13/CONV-14 diagnostic lines for <paramref name="failure"/>.</summary>
    Public Function Diagnostic(failure As UsageError) As String
        ArgumentNullException.ThrowIfNull(failure)
        Return ToolName & ": " & failure.Message & vbLf &
            "Try '" & ToolName & " --help' for more information." & vbLf
    End Function

    ''' <summary>
    ''' Writes and flushes <paramref name="data"/>. Returns False instead of throwing when the
    ''' stream fails, e.g. because the reader of a pipe went away (CONV-25).
    ''' </summary>
    Public Function WriteAll(stream As Stream, data As Byte()) As Boolean
        ArgumentNullException.ThrowIfNull(stream)
        ArgumentNullException.ThrowIfNull(data)
        If data.Length = 0 Then Return True
        Try
            stream.Write(data, 0, data.Length)
            stream.Flush()
            Return True
        Catch ex As IOException
            Return False
        Catch ex As ObjectDisposedException
            Return False
        Catch ex As NotSupportedException
            Return False
        End Try
    End Function

End Module

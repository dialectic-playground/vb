Imports System.Text

''' <summary>Pure greeting construction (FR-01, FR-02, FR-04, NFR-05).</summary>
Public Module Greeting

    ''' <summary>One greeting line: "Hello, " + name + "!" + LF.</summary>
    Public Function BuildLine(name As String) As String
        Return "Hello, " & name & "!" & vbLf
    End Function

    ''' <summary>
    ''' <paramref name="count"/> greeting lines for <paramref name="name"/>; empty when
    ''' <paramref name="count"/> is zero or negative.
    ''' </summary>
    Public Function BuildText(name As String, count As Integer) As String
        Dim line = BuildLine(name)
        Dim text As New StringBuilder(line.Length * Math.Max(count, 0))
        For i = 1 To count
            text.Append(line)
        Next
        Return text.ToString()
    End Function

End Module

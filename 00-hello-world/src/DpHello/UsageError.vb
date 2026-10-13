''' <summary>
''' A usage error (CONV-13). Every diagnostic text of dp-hello is built here, and only here.
''' </summary>
Public NotInheritable Class UsageError

    Private Sub New(message As String)
        Me.Message = message
    End Sub

    ''' <summary>The diagnostic without the "dp-hello: " prefix, e.g. "extra operand 'x'".</summary>
    Public ReadOnly Property Message As String

    ''' <summary>Unknown short option: <c>invalid option -- 'x'</c>.</summary>
    Public Shared Function InvalidOption(letter As String) As UsageError
        Return New UsageError("invalid option -- '" & letter & "'")
    End Function

    ''' <summary>Unknown long option (the whole token, including any =value).</summary>
    Public Shared Function UnrecognizedOption(token As String) As UsageError
        Return New UsageError("unrecognized option '" & token & "'")
    End Function

    ''' <summary>Short option <paramref name="letter"/> without its argument.</summary>
    Public Shared Function ShortOptionRequiresArgument(letter As String) As UsageError
        Return New UsageError("option requires an argument -- '" & letter & "'")
    End Function

    ''' <summary>Long option <paramref name="longName"/> (without dashes) without its argument.</summary>
    Public Shared Function LongOptionRequiresArgument(longName As String) As UsageError
        Return New UsageError("option '--" & longName & "' requires an argument")
    End Function

    ''' <summary>A flag such as --help given an =value.</summary>
    Public Shared Function ArgumentNotAllowed(longName As String) As UsageError
        Return New UsageError("option '--" & longName & "' doesn't allow an argument")
    End Function

    ''' <summary>Invalid option value; <paramref name="spelling"/> is the option as written.</summary>
    Public Shared Function InvalidArgument(value As String, spelling As String) As UsageError
        Return New UsageError("invalid argument '" & value & "' for '" & spelling & "'")
    End Function

    ''' <summary>The first surplus operand.</summary>
    Public Shared Function ExtraOperand(operand As String) As UsageError
        Return New UsageError("extra operand '" & operand & "'")
    End Function

End Class

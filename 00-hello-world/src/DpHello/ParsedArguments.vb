''' <summary>What an invocation asks for.</summary>
Public Enum ParseAction
    ''' <summary>Print the greeting (after validation).</summary>
    Greet
    ''' <summary>Print the help text (--help).</summary>
    Help
    ''' <summary>Print the version line (--version).</summary>
    Version
End Enum

''' <summary>A raw, unvalidated option value and the option as the user spelled it.</summary>
Public NotInheritable Class OptionValue

    ''' <summary>Creates an option value.</summary>
    Public Sub New(value As String, spelling As String)
        Me.Value = value
        Me.Spelling = spelling
    End Sub

    ''' <summary>The value exactly as given (may be empty).</summary>
    Public ReadOnly Property Value As String

    ''' <summary>The option without its value: -n, --name, -c, or --count.</summary>
    Public ReadOnly Property Spelling As String

End Class

''' <summary>The syntactic result of a successful scan; values are not validated yet.</summary>
Public NotInheritable Class ParsedArguments

    ''' <summary>Creates a parse result.</summary>
    Public Sub New(action As ParseAction, name As OptionValue, count As OptionValue, firstOperand As String)
        Me.Action = action
        Me.Name = name
        Me.Count = count
        Me.FirstOperand = firstOperand
    End Sub

    ''' <summary>Greet, Help, or Version.</summary>
    Public ReadOnly Property Action As ParseAction

    ''' <summary>The last --name/-n value, or Nothing.</summary>
    Public ReadOnly Property Name As OptionValue

    ''' <summary>The last --count/-c value, or Nothing.</summary>
    Public ReadOnly Property Count As OptionValue

    ''' <summary>The first operand, or Nothing when there is none.</summary>
    Public ReadOnly Property FirstOperand As String

End Class

''' <summary>Either <see cref="ParsedArguments"/> or a <see cref="UsageError"/>.</summary>
Public NotInheritable Class ParseResult

    Private Sub New(arguments As ParsedArguments, failure As UsageError)
        Me.Arguments = arguments
        Me.Failure = failure
    End Sub

    ''' <summary>The parsed arguments; Nothing when <see cref="Failure"/> is set.</summary>
    Public ReadOnly Property Arguments As ParsedArguments

    ''' <summary>The syntax error; Nothing on success.</summary>
    Public ReadOnly Property Failure As UsageError

    ''' <summary>A successful parse.</summary>
    Public Shared Function Success(arguments As ParsedArguments) As ParseResult
        Return New ParseResult(arguments, Nothing)
    End Function

    ''' <summary>A failed parse.</summary>
    Public Shared Function Fail(failure As UsageError) As ParseResult
        Return New ParseResult(Nothing, failure)
    End Function

End Class

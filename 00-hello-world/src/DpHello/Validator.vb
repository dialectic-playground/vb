''' <summary>A validated greeting request: Name is non-empty and Count is 1 ... 1000.</summary>
Public NotInheritable Class GreetingSettings

    ''' <summary>Creates validated settings.</summary>
    Public Sub New(name As String, count As Integer)
        Me.Name = name
        Me.Count = count
    End Sub

    ''' <summary>The greeted name.</summary>
    Public ReadOnly Property Name As String

    ''' <summary>The number of greeting lines.</summary>
    Public ReadOnly Property Count As Integer

End Class

''' <summary>Either <see cref="GreetingSettings"/> or a <see cref="UsageError"/>.</summary>
Public NotInheritable Class ValidationResult

    Private Sub New(settings As GreetingSettings, failure As UsageError)
        Me.Settings = settings
        Me.Failure = failure
    End Sub

    ''' <summary>The settings; Nothing when <see cref="Failure"/> is set.</summary>
    Public ReadOnly Property Settings As GreetingSettings

    ''' <summary>The value or operand error; Nothing on success.</summary>
    Public ReadOnly Property Failure As UsageError

    ''' <summary>Valid settings.</summary>
    Public Shared Function Success(settings As GreetingSettings) As ValidationResult
        Return New ValidationResult(settings, Nothing)
    End Function

    ''' <summary>A validation error.</summary>
    Public Shared Function Fail(failure As UsageError) As ValidationResult
        Return New ValidationResult(Nothing, failure)
    End Function

End Class

''' <summary>
''' Checks what the parser recorded (FR-03, FR-05, FR-06, FR-09): operands first, then the
''' winning --name and --count values.
''' </summary>
Public Module Validator

    ''' <summary>The name used when --name is absent.</summary>
    Public Const DefaultName As String = "World"

    ''' <summary>The count used when --count is absent.</summary>
    Public Const DefaultCount As Integer = 1

    ''' <summary>The smallest valid count.</summary>
    Public Const MinimumCount As Integer = 1

    ''' <summary>The largest valid count.</summary>
    Public Const MaximumCount As Integer = 1000

    ''' <summary>Turns a Greet parse result into settings or a usage error.</summary>
    Public Function Validate(arguments As ParsedArguments) As ValidationResult
        ArgumentNullException.ThrowIfNull(arguments)
        If arguments.FirstOperand IsNot Nothing Then
            Return ValidationResult.Fail(UsageError.ExtraOperand(arguments.FirstOperand))
        End If

        Dim name = DefaultName
        If arguments.Name IsNot Nothing Then
            If arguments.Name.Value.Length = 0 Then Return ValidationResult.Fail(Invalid(arguments.Name))
            name = arguments.Name.Value
        End If

        Dim count = DefaultCount
        If arguments.Count IsNot Nothing Then
            Dim parsed = ParseCount(arguments.Count.Value)
            If Not parsed.HasValue Then Return ValidationResult.Fail(Invalid(arguments.Count))
            count = parsed.Value
        End If

        Return ValidationResult.Success(New GreetingSettings(name, count))
    End Function

    ''' <summary>
    ''' Returns the count that <paramref name="text"/> denotes, or Nothing unless it is ASCII
    ''' digits only (leading zeros allowed) with a value from 1 to 1000 (FR-04, FR-05).
    ''' </summary>
    Public Function ParseCount(text As String) As Integer?
        ArgumentNullException.ThrowIfNull(text)
        If text.Length = 0 Then Return Nothing
        Dim value = 0
        For Each ch In text
            If ch < "0"c OrElse ch > "9"c Then Return Nothing
            ' Saturate above the maximum, so arbitrarily long digit strings never overflow.
            value = Math.Min(value * 10 + (AscW(ch) - AscW("0"c)), MaximumCount + 1)
        Next
        If value < MinimumCount OrElse value > MaximumCount Then Return Nothing
        Return value
    End Function

    Private Function Invalid(value As OptionValue) As UsageError
        Return UsageError.InvalidArgument(value.Value, value.Spelling)
    End Function

End Module

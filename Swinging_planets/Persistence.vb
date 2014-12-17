Public Class Persistence
    Public frames() As Image
    Public scores() As Integer
    Public persistenceLength As Short = 0

    Public Sub New()
    End Sub

    Public Sub addFrame(ByVal aFrame As Image, ByVal score As Integer)
        ReDim Preserve frames(persistenceLength)
        frames(persistenceLength) = aFrame
        ReDim Preserve scores(persistenceLength)
        scores(persistenceLength) = score
        persistenceLength += 1
    End Sub

End Class

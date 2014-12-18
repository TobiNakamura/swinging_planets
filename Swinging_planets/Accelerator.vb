Public Class Accelerator
    Dim position As PVector
    Dim sngAcceleration As Single

    Sub New(ByVal position As PVector, ByVal sngAcceleration As Single)
        Me.position = position.getThis
        Me.sngAcceleration = sngAcceleration
    End Sub



End Class

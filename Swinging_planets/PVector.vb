'=========================vector calculations and storage==============='

Public Class PVector
    Public x, y As Double

    Public Sub New(ByVal x As Double, ByVal y As Double)
        Me.x = x
        Me.y = y
    End Sub

    Public Function getThis() As PVector
        Return DirectCast(Me.MemberwiseClone, PVector)
    End Function


    Public Function mag() As Double
        Return Math.Sqrt(x ^ 2 + y ^ 2)
    End Function

    Public Sub setMagnitude(ByVal magnitude As Double)
        Me.divide(mag) 'what happens if divide by zero and why does not happen
        Me.multiply(magnitude)
    End Sub

    Public Function distance(ByVal other As PVector) As Double
        Dim dblX As Double = Math.Abs(other.x - x)
        Dim dblY As Double = Math.Abs(other.y - y)
        Return Math.Sqrt(dblX ^ 2 + dblY ^ 2)
    End Function

    Public Sub multiply(ByVal value As Double)
        x *= value
        y *= value
    End Sub

    Public Sub divide(ByVal value As Double)
        x /= value
        y /= value
    End Sub

    Public Sub add(ByVal value As PVector)
        x += value.x
        y += value.y
    End Sub

    Public Function subtract(ByVal other As PVector) As PVector
        Dim sum As PVector = New PVector(0, 0)
        sum.x = other.x - x
        sum.y = other.y - y
        Return sum
    End Function

    Public Function rotate(ByVal radians As Double) As PVector
        Dim px, py As Double
        px = x
        py = y
        Return New PVector(px * Math.Cos(radians) - py * Math.Sin(radians), px * Math.Sin(radians) + py * Math.Cos(radians))
    End Function

    Public Shared Function toPoint(ByVal vector As PVector) As Point
        Return New Point(vector.x, vector.y)
    End Function
End Class

Imports System.Drawing.Drawing2D
'=============================celestial objects======================'
Public Class Celestial
    Public position As PVector
    Public radii As Double
    Public atmosphereRadii As Double
    Public densityZero As Single
    Private atmoColor(1) As Color
    Private surfaceColor As Brush
    Private orbit(2) As Double '{orbit center position x, orbit center position y, orbital speed}

    Public Sub New(ByVal position As PVector, ByVal radii As Double, ByVal orbitParameters() As Double, ByVal atmoHeight As Double, ByVal atmoColor() As Color,
                   ByVal surfaceColor As Brush, ByVal surfaceDencity As Single)
        Me.position = position
        Me.radii = radii
        Me.atmosphereRadii = atmoHeight
        Me.densityZero = surfaceDencity
        Me.atmoColor = atmoColor
        Me.surfaceColor = surfaceColor
        Me.orbit = orbitParameters
    End Sub

    Public Sub Draw(ByRef buffer As Graphics)
        Dim recAtmosphere As Rectangle = New Rectangle(position.x - atmosphereRadii, position.y - atmosphereRadii,
                                                    atmosphereRadii * 2, atmosphereRadii * 2)
        Dim pth As New GraphicsPath() 'adds gradient to the atmosphere
        pth.AddEllipse(recAtmosphere)
        Dim pgb As New PathGradientBrush(pth)
        pgb.SurroundColors = New Color() {atmoColor(0)}
        pgb.CenterColor = atmoColor(1)
        buffer.FillEllipse(pgb, recAtmosphere) 'multiply by two to get diameter
        position = move()
        buffer.FillEllipse(surfaceColor, New Rectangle(position.x - radii, position.y - radii, radii * 2, radii * 2))
    End Sub

    Private Function move() As PVector
        Dim rtnPotision As PVector = New PVector(0, 0)
        Dim p As PVector = position.getThis 'get copy of current position
        p.subtract(New PVector(orbit(0), orbit(1))) 'translate orbital center to origin(0, 0)
        'calculate rotation
        rtnPotision = New PVector(p.x * Math.Cos(orbit(2)) - p.y * Math.Sin(orbit(2)), p.x * Math.Sin(orbit(2)) + p.y * Math.Cos(orbit(2)))
        rtnPotision.add(New PVector(orbit(0), orbit(1))) 'translate back to proper position
        Return rtnPotision
    End Function
End Class

'===============================moving ship================='

Public Class Ship
    Dim coefficenceDrag As Double = 0.02
    Dim thrusterForce As Double = 0.01
    Dim G As Double = 0.01
    Dim shtLength As Short = 300 'length of leading line
    Dim currentKeys As New List(Of Keys)
    Dim imgCrashIcon As Image
    Public position, velocity As PVector
    Public Event crashed()

    Public Sub New(ByVal x As Double, ByVal y As Double, ByRef keys As List(Of Keys))
        position = New PVector(0, 0)
        position.x = x
        position.y = y
        velocity = New PVector(0, 0)
        currentKeys = keys
        imgCrashIcon = Image.FromFile("crashIcon.png")
    End Sub

    Private Sub futurePath(ByVal planets() As Celestial, ByRef buffer As Graphics)
        Dim nominalPen As New Pen(Brushes.Blue)
        Dim warningPen As New Pen(Brushes.Red)
        Dim rtnPoints() As Point 'initiate with 2 indicies beacause drawlines() doesnt like less than 2
        Dim blnInAtmosphere As Boolean = False
        Dim p As PVector = position.getThis 'copy the spaceship properties
        Dim v As PVector = velocity.getThis
        Dim lineIndex As Short = 0
        For x = 0 To shtLength
            v.add(Grav(planets, p)) 'add gravity to velocity
            'supports multiplanet atmospheric entry
            Dim closestPlanet As Celestial = findClosestPlanet(planets, p)
            '-----------------------------------------------
            Dim drag As PVector = New PVector(0, 0)
            If p.distance(closestPlanet.position) < (closestPlanet.atmosphereRadii) Then 'in the atmosphere
                drag = v.getThis
                drag.multiply(-1) 'drag is opposite to prograde
                drag.setMagnitude(dragMagnitude(closestPlanet, p.getThis, v.getThis)) 'magnitude of force
                If blnInAtmosphere = False Then 'entry into atmosphere, draw line of trajectory before atmosphric entry
                    printLines(nominalPen, rtnPoints, buffer) 'the null ref error will never come up beacause 
                    lineIndex = 0
                    ReDim rtnPoints(lineIndex) 'reset array
                    blnInAtmosphere = True
                End If
            ElseIf blnInAtmosphere = True Then 'exsiting atmosphere, draw the line in the atmosphere
                printLines(warningPen, rtnPoints, buffer)
                lineIndex = 0
                ReDim rtnPoints(lineIndex) 'reset array
                blnInAtmosphere = False
            End If

            If p.distance(closestPlanet.position) < closestPlanet.radii Then 'crashed onto surface
                buffer.DrawImage(imgCrashIcon, New Point(p.x - (imgCrashIcon.Width / 2), p.y - (imgCrashIcon.Height / 2)))
                x = shtLength 'end the loop
            End If

            v.add(drag)
            p.add(v) 'move the spacecraft
            ReDim Preserve rtnPoints(lineIndex) 'extend the array to add new point
            rtnPoints(lineIndex) = New Point(p.x, p.y)
            lineIndex += 1
        Next

        If blnInAtmosphere = True Then 'final segment
            printLines(Pens.Red, rtnPoints, buffer)
        ElseIf blnInAtmosphere = False Then
            printLines(Pens.Blue, rtnPoints, buffer)
        End If

    End Sub

    Private Sub printLines(ByVal lineColor As Pen, ByVal linePoints() As Point, ByRef buffer As Graphics)
        Try
            buffer.DrawLines(lineColor, linePoints) 'draw atmosphere line
        Catch e As ArgumentException
            'in the event that there are not enough points
        End Try
    End Sub

    '=======================finds and returns the closes planet ========================================  
    Private Function findClosestPlanet(ByVal planets() As Celestial, ByVal currentPosition As PVector) As Celestial
        Dim closestPlanet As Celestial = planets(0)
        For i = 1 To planets.Length - 1
            If currentPosition.distance(closestPlanet.position) > currentPosition.distance(planets(i).position) Then
                closestPlanet = planets(i)
            End If
        Next
        Return closestPlanet
    End Function

    Public Sub Move(ByVal planets() As Celestial)
        velocity.add(Grav(planets, position))
        velocity.add(Thrusters(velocity.getThis))
        Dim drag As PVector = New PVector(0, 0)
        Dim closestPlanet As Celestial = findClosestPlanet(planets, position)
        If position.distance(closestPlanet.position) < (closestPlanet.atmosphereRadii) Then
            drag = velocity.getThis
            drag.multiply(-1) 'drag is opposite to prograde
            drag.setMagnitude(dragMagnitude(closestPlanet, position.getThis, velocity.getThis)) 'magnitude of force
        End If
        If position.distance(closestPlanet.position) < closestPlanet.radii Then 'ship crashed
            GameScreen.blnCrashProcedure = True
        End If
        velocity.add(drag)
        position.add(velocity) 'move the spacecraft
    End Sub

    Dim currentFlameSize As Single = 10
    Dim flameVelocity As Single = 0.5
    Dim maxFlameSize As Single = 20
    Dim minFlameSize As Single = 10

    Public Sub Draw(ByRef buffer As Graphics, ByVal planets() As Celestial)
        futurePath(planets, buffer)
        buffer.FillEllipse(Brushes.Beige, New Rectangle(position.x - 5, position.y - 5, 10, 10)) 'draw the space ship

        Dim closestPlanet As Celestial = findClosestPlanet(planets, position)
        If position.distance(closestPlanet.position) < closestPlanet.atmosphereRadii Then
            'below calculates the opacity of the flame(ARGB), using 255*(1-(altitude/atmospheric height))
            'absolute value used in case the player glitches into the ground
            Dim sngOpacity As Single = 255 * (1 - (
                                            (position.distance(closestPlanet.position) - closestPlanet.radii) /
                                            (closestPlanet.atmosphereRadii - closestPlanet.radii)))
            If sngOpacity > 255 Or sngOpacity < 0 Then 'incase the player glitches into the ground
                sngOpacity = 0
            End If
            Dim flameColor As Color = Color.FromArgb(sngOpacity, 255, 82, 13)

            buffer.FillEllipse(New SolidBrush(flameColor),
                                   New Rectangle(position.x - (currentFlameSize / 2), position.y - (currentFlameSize / 2),
                                                                    currentFlameSize, currentFlameSize))
            currentFlameSize += flameVelocity
            If currentFlameSize >= maxFlameSize Then
                flameVelocity *= -1
            ElseIf currentFlameSize <= minFlameSize Then
                flameVelocity *= -1
            End If
        End If

    End Sub

    'adds the thrust from user input
    Private Function Thrusters(ByVal referenceVector As PVector) As PVector
        Dim rtnVector As PVector = New PVector(0, 0)
        Dim refVect As PVector = referenceVector.getThis
        If currentKeys.Contains(Keys.Up) Then
            referenceVector.setMagnitude(thrusterForce)
            rtnVector.add(referenceVector)
        ElseIf currentKeys.Contains(Keys.Down) Then
            referenceVector.setMagnitude(-thrusterForce)
            rtnVector.add(referenceVector)
        End If
        referenceVector = refVect.getThis 'to cancel the rotation above
        If currentKeys.Contains(Keys.Left) Then
            referenceVector.setMagnitude(thrusterForce)
            rtnVector.add(referenceVector.rotate(-Math.PI / 2))
        ElseIf currentKeys.Contains(Keys.Right) Then
            referenceVector.setMagnitude(thrusterForce)
            rtnVector.add(referenceVector.rotate(Math.PI / 2))
        End If
        Return rtnVector
    End Function

    'adds gravity from all the bodies
    Private Function Grav(ByVal planets() As Celestial, ByVal pos As PVector) As PVector
        Dim totalA As PVector = New PVector(0, 0) 'calculate spaceship kinematics
        For i = 0 To planets.Length - 1 'grav from all planets
            Dim r As Double = pos.distance(planets(i).position) 'distance
            Dim mag As Double = G / (r + 1) 'magnitude of force
            mag *= planets(i).radii * planets(i).sngSolidDensity 'mass multiplyer
            Dim a As PVector
            a = pos.subtract(planets(i).position) 'unit vector
            a.divide(r) 'unit vector
            a.multiply(mag) 'the full force(acceleration: assume mass 1) vector
            totalA.add(a) 'add to exsisting grav forces
        Next
        Return totalA
    End Function

    Private Function dragMagnitude(ByVal planet As Celestial, ByVal pos As PVector, ByVal vel As PVector) As Double
        Dim scalingHeight As Double = planet.atmosphereRadii - planet.radii
        Dim altitude As Double = pos.distance(planet.position) - planet.radii
        Dim densityZero As Double = 1
        Dim density As Double = Math.Pow(Math.E, altitude * -1 / scalingHeight) * densityZero
        Return density * vel.mag ^ 2 * coefficenceDrag
    End Function

    Dim shtCrashClock As Short = 0
    Dim fadeSpeed As Single = 3
    Public Sub explode(ByRef buffer As Graphics)
        Dim flameColor As Color = Color.FromArgb(100, 255, 82, 13)
        Dim shtFlameRadius As Short = shtCrashClock / 3
        buffer.FillEllipse(New SolidBrush(flameColor),
                           New Rectangle(position.x - (shtFlameRadius / 2), position.y - (shtFlameRadius / 2), shtFlameRadius, shtFlameRadius))
        If shtCrashClock < 50 Then
            Dim shockWaveColor As Color = Color.FromArgb(20, 255, 255, 255)
            Dim shtShockRadius As Short = shtCrashClock * 50
            buffer.FillEllipse(New SolidBrush(shockWaveColor),
                               New Rectangle(position.x - (shtShockRadius / 2), position.y - (shtShockRadius / 2), shtShockRadius, shtShockRadius))
        ElseIf shtCrashClock < (255 / fadeSpeed) Then 'the limit for ARGB value is 255 so: 255 + 50 -20
            Dim fadeColor As Color = Color.FromArgb(20 + (shtCrashClock * fadeSpeed) - 50, 255, 255, 255) 'to match the opacity of the shock wave
            buffer.FillRectangle(New SolidBrush(fadeColor),
                               New Rectangle(position.x - (Form1.Width / 2), position.y - (Form1.Height / 2), Form1.Width, Form1.Height))
        Else
            RaiseEvent crashed()
        End If
        shtCrashClock += 1
    End Sub

End Class

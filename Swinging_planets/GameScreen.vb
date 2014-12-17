Imports System.Drawing.Drawing2D

Module GameScreen

    Dim WithEvents clock As Timer
    Dim form As Form1
    Public thisScreen As screenState = screenState.unloaded
    Dim currentKeys As New List(Of Keys)
    Dim lblFpsCounter As Label
    Dim lblScoreCounter As Label
    Dim lastCall As DateTime
    Dim dblTotalScore As Double
    Dim imgCrashIcon As Image
    Dim count As Integer
    Dim saveFrames As Persistence
    Dim blnCrashProcedure As Boolean
    Dim lblTimer As Label

    '=======game parameters======='
    Dim planets(3) As Celestial
    Dim WithEvents spaceChip As Ship
    Dim shtFPS As Short = 33
    Dim G As Double = 0.01
    Dim thrusterForce As Double = 0.01
    Dim shtScoreScale As Short = 5
    Dim coefficenceDrag As Double = 0.02
    Dim playTime As TimeSpan = New TimeSpan(1, 0, 0)
    

    '============================================screen controls============================='
    Public Sub load(ByRef parent As Form1)
        lastCall = New DateTime()
        lastCall = DateTime.Now
        form = parent
        clock = New Timer()
        clock.Interval = 1000 / shtFPS

        planets(0) = New Celestial(New PVector(5100, 500), 50, New Double{0, 0}, 70, New Color() {Color.FromArgb(20, 50, 90, 255), Color.Blue}, Brushes.GreenYellow, 1)
        planets(1) = New Celestial(New PVector(5500, 700), 50, New Double{0, 0}, 70, New Color() {Color.FromArgb(20, 50, 90, 255), Color.Blue}, Brushes.GreenYellow, 1)
        planets(2) = New Celestial(New PVector(5000, 300), 50, New Double{0, 0},  70, New Color() {Color.FromArgb(20, 50, 90, 255), Color.Blue}, Brushes.GreenYellow, 1)
        planets(3) = New Celestial(New PVector(0, 0), 400, 450, New Double{0, 0},  New Color() {Color.FromArgb(20, 50, 90, 255), Color.Blue}, Brushes.GreenYellow, 1)
        spaceChip = New Ship(5000, 100)
        spaceChip.velocity.x = 1.5
        spaceChip.velocity.y = 0

        lblFpsCounter = New Label()
        lblFpsCounter.Location = New Point(10, 10)
        lblFpsCounter.Width = 40
        form.Controls.Add(lblFpsCounter)
        lblScoreCounter = New Label()
        lblScoreCounter.Location = New Point(300, 10)
        form.Controls.Add(lblScoreCounter)
        lblTimer = New Label()
        lblTimer.Location = New Point(600, 10)
        form.Controls.Add(lblTimer)

        imgCrashIcon = Image.FromFile("crashIcon.png")

        saveFrames = New Persistence()

        thisScreen = screenState.loaded
    End Sub

    Public Sub run()
        clock.Start()
        thisScreen = screenState.running
    End Sub


    Public Sub unload()
        form.Controls.Remove(lblScoreCounter)
        form.Controls.Remove(lblFpsCounter)
        form.Controls.Remove(lblTimer)
        form.Update()
        thisScreen = screenState.unloaded
    End Sub
    '====================================end screen controls==========================================================='

    '===================================input control====================================================================='
    Public Sub key_down(ByVal sender As Object, ByVal e As KeyEventArgs) 'key is selected - add to key list
        If e.KeyValue = Keys.Escape Then
            clock.Stop()
        ElseIf e.KeyValue = Keys.Enter Then
            clock.Start()

        ElseIf e.KeyValue = Keys.Up Then
            KeyStates.up = True
        ElseIf e.KeyValue = Keys.Left Then
            KeyStates.left = True
        ElseIf e.KeyValue = Keys.Down Then
            KeyStates.down = True
        ElseIf e.KeyValue = Keys.Right Then
            KeyStates.right = True
        End If
        If currentKeys.Contains(e.KeyCode) = False Then
            currentKeys.Add(e.KeyCode)
        End If
    End Sub

    Public Sub key_up(ByVal sender As Object, ByVal e As KeyEventArgs) 'key released - remove from list
        If e.KeyValue = Keys.Up Then
            KeyStates.up = False
        ElseIf e.KeyValue = Keys.Left Then
            KeyStates.left = False
        ElseIf e.KeyValue = Keys.Down Then
            KeyStates.down = False
        ElseIf e.KeyValue = Keys.Right Then
            KeyStates.right = False
        End If
        currentKeys.Remove(e.KeyCode)
    End Sub

    Public Sub mouseWheel(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs)
        zoomState.mouseWheelMove(e.Delta)
    End Sub
    '===============================================end input control============================'


    '==============================================instruction cycle==============================================='
    Private Sub clock_tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles clock.Tick

        fpsCounter()

        Dim currentContext As BufferedGraphicsContext
        Dim myBuffer As BufferedGraphics
        ' Gets a reference to the current BufferedGraphicsContext.
        currentContext = BufferedGraphicsManager.Current
        ' Creates a BufferedGraphics instance associated with Form1, and with  
        ' dimensions the same size as the drawing surface of Form1.
        myBuffer = currentContext.Allocate(form.CreateGraphics, form.DisplayRectangle)

        myBuffer.Graphics.ScaleTransform(zoomState.zoom, zoomState.zoom)
        myBuffer.Graphics.TranslateTransform((form.DisplayRectangle.Width / 2) * (1 / zoomState.zoom) - spaceChip.position.x,
                                             (form.DisplayRectangle.Height / 2) * (1 / zoomState.zoom) - spaceChip.position.y)
        'offsets the screen to center spacecraf(pan camera), accounting for motion of space craft and 
        'zooming(the scalingtransform() pivots at top left corner)

        drawGraphics(myBuffer.Graphics)

        If blnCrashProcedure = False Then
            spaceChip.Move(planets) 'move the space ship
            addPoints()
        ElseIf blnCrashProcedure = True Then
            spaceChip.explode(myBuffer.Graphics)
        End If



        ' Renders the contents of the buffer to the specified drawing surface.
        myBuffer.Render(form.CreateGraphics)
        myBuffer.Dispose()


        If count = 10 Then
            Dim img As New Bitmap(form.DisplayRectangle.Width, form.DisplayRectangle.Height) 'memory leak danger, dont save too many(800ish seems to be limit)
            Dim aFrame As Graphics = Graphics.FromImage(img)
            aFrame.TranslateTransform((form.DisplayRectangle.Width / 2) * (1 / zoomState.zoom) - spaceChip.position.x,
                                             (form.DisplayRectangle.Height / 2) * (1 / zoomState.zoom) - spaceChip.position.y)
            drawGraphics(aFrame)
            saveFrames.addFrame(img, dblTotalScore)
            aFrame.Dispose()
        End If
        count += 1

        countDown()

    End Sub
    '===================================================================================================================================='

    Private Sub drawGraphics(ByRef buffer As Graphics)
        buffer.SmoothingMode = Drawing2D.SmoothingMode.HighQuality
        buffer.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality 'pixel offset doesnt seem to be doing much
        buffer.Clear(Color.FromArgb(255, 93, 93, 93)) 'clears background
        For i = 0 To planets.Length - 1 'draw planets
            planets(i).Draw(buffer)
        Next
        If blnCrashProcedure = False Then
            spaceChip.Draw(buffer, planets)
        End If
    End Sub

    '=======================handles player crashing into celestia==============================
    Private Sub playerCrashed() Handles spaceChip.crashed
        Form1.loadAndDisplayDeathScreen(saveFrames)
    End Sub

    '==============================keeps track of total points================='
    Private Sub addPoints()
        For i = 0 To planets.Length - 1
            dblTotalScore += shtScoreScale / (spaceChip.position.distance(planets(i).position) - planets(i).radii)
        Next
        lblScoreCounter.Text = Math.Round(dblTotalScore)
    End Sub

    Dim startTime As Date = DateTime.Now
    Dim elapsedTime As TimeSpan
    Private Sub countDown()
        elapsedTime = DateTime.Now - startTime
        If elapsedTime > playTime Then
            clock.Stop()
            playerCrashed()
        End If
        lblTimer.Text = (playTime - elapsedTime).ToString
    End Sub

    '=============================fps counter==========================='
    Private Sub fpsCounter()
        Dim dblDeltaT = 1000 / (DateTime.Now - lastCall).TotalMilliseconds
        dblDeltaT = Math.Round(dblDeltaT)
        lblFpsCounter.Text = dblDeltaT.ToString + " fps"
        lastCall = DateTime.Now
    End Sub




    '===============================moving ship================='
    Private Class Ship

        Public position, velocity As PVector
        Public Event crashed()

        Public Sub New(ByVal x As Double, ByVal y As Double)
            position = New PVector(0, 0)
            position.x = x
            position.y = y
            velocity = New PVector(0, 0)
        End Sub

        Private Sub futurePath(ByVal planets() As Celestial, ByRef buffer As Graphics)
            Dim nominalPen As New Pen(Brushes.Blue)
            Dim warningPen As New Pen(Brushes.Red)
            Dim shtLength As Short = 300 'length of leading line
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
                    If blnInAtmosphere = False Then 'entry into atmosphere
                        printLines(nominalPen, rtnPoints, buffer) 'the null ref error will never come up beacause 
                        lineIndex = 0
                        ReDim rtnPoints(lineIndex) 'reset array
                        blnInAtmosphere = True
                    ElseIf p.distance(closestPlanet.position) < closestPlanet.radii Then 'crashed onto surface
                        buffer.DrawImage(imgCrashIcon, New Point(p.x - (imgCrashIcon.Width / 2), p.y - (imgCrashIcon.Height / 2)))
                        x = shtLength 'end the loop
                    End If
                ElseIf blnInAtmosphere = True Then 'exsiting atmosphere
                    printLines(warningPen, rtnPoints, buffer)
                    lineIndex = 0
                    ReDim rtnPoints(lineIndex) 'reset array
                    blnInAtmosphere = False
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
                If position.distance(closestPlanet.position) < closestPlanet.radii Then 'ship crashed
                    blnCrashProcedure = True
                End If
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
                'absolute value used incase the player glitches into the ground
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
                mag *= planets(i).radii 'mass multiplyer
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
                                   New Rectangle(position.x - (form.Width / 2), position.y - (form.Height / 2), form.Width, form.Height))
            Else
                RaiseEvent crashed()
            End If
            shtCrashClock += 1
        End Sub

    End Class


End Module
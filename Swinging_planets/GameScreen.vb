Module GameScreen

    Dim WithEvents clock As Timer
    Dim form As Form1
    Public thisScreen As screenState = screenState.unloaded
    Dim currentKeys As New List(Of Keys)
    Dim lblFpsCounter As Label
    Dim lblScoreCounter As Label
    Dim lastCall As DateTime

    '=======game parameters======='
    Dim planets(2) As Celestial
    Dim spaceChip As Ship
    Dim shtFPS As Short = 30
    Dim G As Double = 0.01
    Dim thrusterForce As Double = 0.01
    Dim shtScoreScale As Short = 5
    Dim dblTotalScore As Double

    '============================================screen controls============================='
    Public Sub load(ByRef parent As Form1)
        lastCall = New DateTime()
        lastCall = DateTime.Now
        form = parent
        clock = New Timer()
        clock.Interval = 1000 / shtFPS

        Dim p(2) As PVector
        p(0) = New PVector(500, 200)
        p(1) = New PVector(200, 500)
        p(2) = New PVector(800, 300)
        For x = 0 To planets.Length - 1
            planets(x) = New Celestial(p(x), 50)
        Next
        spaceChip = New Ship(500, 100)
        spaceChip.velocity.x = 2
        spaceChip.velocity.y = 0

        lblFpsCounter = New Label()
        lblFpsCounter.Location = New Point(10, 10)
        lblScoreCounter = New Label
        lblScoreCounter.Location = New Point(300, 10)

        thisScreen = screenState.loaded
    End Sub

    Public Sub run()
        clock.Start()
        thisScreen = screenState.running
    End Sub

    Public Sub unload()
        clock.Dispose()
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
    '===============================================end input control============================'


    '==============================================instruction cycle==============================================='
    Private Sub clock_tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles clock.Tick
        Dim dblDeltaT = 1000 / (DateTime.Now - lastCall).TotalMilliseconds
        dblDeltaT = Math.Round(dblDeltaT)
        lblFpsCounter.Text = dblDeltaT.ToString + " fps"
        lblFpsCounter.Width = 40
        form.Controls.Add(lblFpsCounter)
        lastCall = DateTime.Now

        lblScoreCounter.Text = Math.Round(dblTotalScore)
        form.Controls.Add(lblScoreCounter)
        dblTotalScore += addPoints()


        Dim myPen As Pen = New Pen(Drawing.Color.Blue, 2)
        Dim currentContext As BufferedGraphicsContext
        Dim myBuffer As BufferedGraphics
        ' Gets a reference to the current BufferedGraphicsContext.
        currentContext = BufferedGraphicsManager.Current
        ' Creates a BufferedGraphics instance associated with Form1, and with  
        ' dimensions the same size as the drawing surface of Form1.
        myBuffer = currentContext.Allocate(form.CreateGraphics, form.DisplayRectangle)

        myBuffer.Graphics.Clear(Color.White) 'clears background with white

        spaceChip.Move(planets) 'move the space ship

        myBuffer.Graphics.TranslateTransform((form.DisplayRectangle.Width / 2) - spaceChip.position.x,
                                             (form.DisplayRectangle.Height / 2) - spaceChip.position.y) 'offsets the screen to center spacecraf(pan camera)

        Dim bounds As Rectangle = New Rectangle(spaceChip.position.x - 5, spaceChip.position.y - 5, 10, 10)
        myBuffer.Graphics.DrawEllipse(myPen, bounds) 'draw the space ship

        myBuffer.Graphics.DrawLines(myPen, spaceChip.tracer(planets)) 'draws the future path of the space ship


        For i = 0 To planets.Length - 1 'draw planets
            myBuffer.Graphics.DrawEllipse(myPen, CInt(planets(i).position.x - (planets(i).radii)),
                                          CInt(planets(i).position.y - (planets(i).radii)),
                                          CSng(planets(i).radii * 2), CSng(planets(i).radii * 2))
        Next






        ' Renders the contents of the buffer to the specified drawing surface.
        myBuffer.Render(form.CreateGraphics)
        myBuffer.Dispose()
    End Sub
    '===================================================================================================================================='

    Private Function addPoints() As Double
        Dim dblReturn As Double
        For i = 0 To planets.Length - 1
            dblReturn += shtScoreScale / (spaceChip.position.distance(planets(i).position) - planets(i).radii)
        Next
        Return dblReturn
    End Function

    '=============================celestial objects======================'
    Private Class Celestial
        Public position As PVector
        Public radii As Double

        Public Sub New(ByVal position As PVector, ByVal radii As Double)
            Me.position = position
            Me.radii = radii
        End Sub

    End Class



    '===============================moving ship================='
    Private Class Ship

        Public position, velocity As PVector
        Dim count As Integer

        Public Sub New(ByVal x As Double, ByVal y As Double)
            position = New PVector(0, 0)
            position.x = x
            position.y = y
            velocity = New PVector(0, 0)
        End Sub

        Public Function tracer(ByVal planets() As Celestial) As Point()
            Dim shtLength As Short = 300 'length of leading line
            Dim rtnPoints(shtLength) As Point
            Dim p As PVector = position.getThis 'copy the spaceship properties
            Dim v As PVector = velocity.getThis
            For x = 0 To shtLength
                v.add(Grav(planets, p)) 'add to velocity
                p.add(v) 'move the spacecraft
                rtnPoints(x) = New Point(p.x, p.y)
            Next
            Return rtnPoints
        End Function

        Public Sub Move(ByVal planets() As Celestial)
            velocity.add(Grav(planets, position)) 'add to velocity
            Dim referenceVector As PVector = velocity.getThis
            If currentKeys.Contains(Keys.Up) Then
                referenceVector.setMagnitude(thrusterForce)
                velocity.add(referenceVector)
            ElseIf currentKeys.Contains(Keys.Down) Then
                referenceVector.setMagnitude(-thrusterForce)
                velocity.add(referenceVector)
            End If
            referenceVector = velocity.getThis
            If currentKeys.Contains(Keys.Left) Then
                referenceVector.setMagnitude(thrusterForce)
                velocity.add(referenceVector.rotate(-Math.PI / 2))
            ElseIf currentKeys.Contains(Keys.Right) Then
                referenceVector.setMagnitude(thrusterForce)
                velocity.add(referenceVector.rotate(Math.PI / 2))
            End If
            position.add(velocity) 'move the spacecraft
        End Sub

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

        Public Function unitVector(ByVal planets() As Celestial) As Point
            Dim r As Double = position.distance(planets(0).position)
            Dim a As PVector
            a = position.subtract(planets(0).position) 'unit vector
            a.divide(r) 'unit vector
            a.multiply(100)
            a.add(New PVector(200, 200))
            Return New Point(a.x, a.y)
        End Function

    End Class



    '=========================vector calculations and storage==============='
    Private Class PVector
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

End Module


Module KeyStates
    Public up, down, left, right As Boolean

    Public Sub reset()
        up = False
        down = False
        left = False
        right = False
    End Sub
End Module
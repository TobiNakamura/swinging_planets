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
    Public blnCrashProcedure As Boolean
    Dim count As Integer
    Dim saveFrames As Persistence

    Dim lblTimer As Label

    '=======game parameters======='
    Dim planets(8) As Celestial
    Dim WithEvents spaceChip As Ship
    Dim shtFPS As Short = 33
    Dim shtScoreScale As Short = 5
    Dim playTime As TimeSpan = New TimeSpan(1, 0, 0)
    

    '============================================screen controls============================='
    Public Sub load(ByRef parent As Form1)
        

        planets(0) = New Celestial(New PVector(5100, 500), 50, 1, Brushes.GreenYellow)
        planets(0).atmosphericAttributes(70, 1, Color.FromArgb(20, 5, 11, 255), Color.Blue)
        planets(1) = New Celestial(New PVector(5500, 700), 50, 1, Brushes.GreenYellow)
        planets(2) = New Celestial(New PVector(5000, 300), 50, 1, Brushes.GreenYellow)
        planets(3) = New Celestial(New PVector(0, 0), 400, 1, Brushes.GreenYellow)
        planets(4) = New Celestial(New PVector(1000, 0), 50, 1, Brushes.GreenYellow)
        planets(5) = New Celestial(New PVector(5200, 500), 10, 1, Brushes.Brown)
        planets(5).orbitalAttributes(5100, 500, 0.05)
        planets(6) = New Celestial(New PVector(5000, 500), 10, 1, Brushes.Brown)
        planets(6).orbitalAttributes(5100, 500, 0.05)
        planets(7) = New Celestial(New PVector(5100, 400), 10, 1, Brushes.Brown)
        planets(7).orbitalAttributes(5100, 500, 0.05)
        planets(8) = New Celestial(New PVector(5100, 300), 10, 1, Brushes.Brown)
        planets(8).orbitalAttributes(5100, 500, 0.01)
        spaceChip = New Ship(5000, 100, currentKeys)
        spaceChip.velocity.x = 1.5
        spaceChip.velocity.y = 0

        lastCall = New DateTime()
        lastCall = DateTime.Now
        form = parent
        clock = New Timer()
        clock.Interval = 1000 / shtFPS

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
        clock.Stop()
        Form1.loadAndDisplayDeathScreen(saveFrames)
    End Sub

    '==============================keeps track of total points================='
    Private Sub addPoints()
        For i = 0 To planets.Length - 1
            Dim shtAtmosphericMultiplyer As Short = 1 'add one to prevent becoming zero
            If spaceChip.position.distance(planets(i).position) < planets(i).atmosphereRadii Then
                shtAtmosphericMultiplyer += planets(i).densityZero
            End If
            dblTotalScore += shtAtmosphericMultiplyer * shtScoreScale / (spaceChip.position.distance(planets(i).position) - planets(i).radii)
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

End Module
Module deathScreen
    Dim pastFrames As Persistence
    Dim lblScore As Label
    Dim WithEvents sclHistoryScroll As HScrollBar
    Dim szeImage As New Size(Form1.Width / 2.3, Form1.Height / 2.3)
    Dim colBackground As Color = Color.FromArgb(255, 73, 73, 73)

    Public Sub load(ByRef frames As Persistence)
        pastFrames = frames


        lblScore = New Label()
        lblScore.Width = 40
        lblScore.BackColor = colBackground
        lblScore.Location = New Point((Form1.Width / 2) - (lblScore.Width / 2), 500)
        Form1.Controls.Add(lblScore) 'lable doesnt show unless drawn first
        lblScore.Text = "hi"


        sclHistoryScroll = New HScrollBar()
        sclHistoryScroll.Dock = DockStyle.Bottom
        sclHistoryScroll.Maximum = 100
        Form1.Controls.Add(sclHistoryScroll)


        thisScreen = screenState.loaded
    End Sub

    Public Sub run()

        Form1.Update()
        drawGallery(0)

        thisScreen = screenState.running
    End Sub

    Public Sub unload()
        thisScreen = screenState.unloaded
    End Sub

    Private Sub drawGallery(ByVal intLoc As Integer)
        Dim currentContext As BufferedGraphicsContext
        Dim myBuffer As BufferedGraphics
        ' Gets a reference to the current BufferedGraphicsContext.
        currentContext = BufferedGraphicsManager.Current
        ' Creates a BufferedGraphics instance associated with Form1, and with  
        ' dimensions the same size as the drawing surface of Form1.
        myBuffer = currentContext.Allocate(Form1.CreateGraphics, Form1.DisplayRectangle)
        myBuffer.Graphics.Clear(colBackground) 'clears background
        For i = 0 To pastFrames.persistenceLength - 1
            Dim intXLocation As Integer = i * szeImage.Width - (intLoc * (pastFrames.persistenceLength + 1) * szeImage.Width / 100) + Form1.Width / 2 - szeImage.Width / 2
            myBuffer.Graphics.DrawImage(pastFrames.frames(i),
                                        New Rectangle(New Point(intXLocation, 100), szeImage))
            If intXLocation > Form1.Width / 2 - szeImage.Width And intXLocation < Form1.Width / 2 Then
                lblScore.Text = pastFrames.scores(i)
                lblScore.Update() 'forces update so the score lable doesnt lag behind
            End If
        Next

        ' Renders the contents of the buffer to the specified drawing surface.
        myBuffer.Render(Form1.CreateGraphics)
        myBuffer.Dispose()
    End Sub


    Private Sub sclHistoryScroll_Scroll(sender As Object, e As System.Windows.Forms.ScrollEventArgs) Handles sclHistoryScroll.Scroll
        drawGallery(e.NewValue)
    End Sub

End Module

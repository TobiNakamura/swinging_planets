
Imports System.Windows.Forms

'Tobi needs to put the information on the project down
'shame on Tobi, now he gets this random text. Also
'NASA faked the moon landing and Mr. Blake sucks at
'donkey kong
'♥♥♥☺☺☺♥♥♥
'no need to comment in code, il make a 300 paged documentation of all the classes, modules and function'


Public Class Form1

    Private Sub Form1_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        GameScreen.load(Me)
        GameScreen.run()
    End Sub

    Sub Form1_KeyPress(ByVal sender As Object, ByVal e As KeyEventArgs) Handles Me.KeyDown
        If GameScreen.thisScreen = screenState.running Then
            GameScreen.key_down(sender, e)
        End If
    End Sub

    Private Sub Form1_KeyUp(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles Me.KeyUp
        If GameScreen.thisScreen = screenState.running Then
            GameScreen.key_up(sender, e)
        End If
    End Sub


    Private Sub Form1_MouseWheel(sender As Object, e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseWheel
        If GameScreen.thisScreen = screenState.running Then
            GameScreen.mouseWheel(sender, e)
        End If
    End Sub

    Public Sub loadAndDisplayDeathScreen(ByRef savedFrames As Persistence)
        GameScreen.unload()
        deathScreen.load(savedFrames)
        deathScreen.run()
    End Sub



End Class

Enum screenState
    loaded
    running
    unloaded
End Enum

Module KeyStates
    Public up, down, left, right As Boolean

    Public Sub reset()
        up = False
        down = False
        left = False
        right = False
    End Sub
End Module

Module zoomState
    Public zoom As Single = 1

    Public Sub mouseWheelMove(delta As Integer)
        Dim sngP As Single = zoom
        zoom += delta / (1000 * (1 / zoom))
        If zoom = 0 Then
            zoom = sngP
        End If
    End Sub
End Module
using UnityEngine;

enum GameState { Instructions, CodeEditor, Game }

class Main:MonoBehaviour
{
    public static GameState gameState = GameState.Instructions;
    public static GUIStyle buttonStyle;
    public static GUIStyle labelStyle;
    public static GUIStyle textboxStyle;
    public static float lineSize;
    public static float buttonWidth;
    public static bool waitFrame;

    private void OnGUI()
    {
        lineSize = Screen.height * 0.06f;
        buttonWidth = Screen.width * 0.2f;
        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = (int)(Screen.height*0.04f);
        labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = (int)(Screen.height*0.04f);
        textboxStyle = new GUIStyle(GUI.skin.textField);
        textboxStyle.fontSize = (int)(Screen.height*0.04f);

        if (gameState == GameState.Instructions || waitFrame)
        {
            var text = @"This is a Codeeditor and language. Github repo 'https://github.com/DragonJT/DragonJTLang'
In the CodeEditor there is a textbox at the bottom where you input lines of code.
Enter 'Print(2)' or any line of code of your choice. Then press return.
Use the up and down arrows to select where to place the line of code.
Press the > button to run the code.
Press control/command backspace to delete previous line.

example code...
var x = 20+20*2+4/2
var y = 1+2+3+4+5
Print(x+y)
if(x>y)
Print(2)";
            GUILayout.Label(text, labelStyle);

            if(Event.current.type == EventType.Repaint)
            {
                waitFrame = false;
            }
            if (GUILayout.Button("Continue", buttonStyle) || Event.current.type == EventType.KeyDown || Event.current.type == EventType.MouseDown)
            {
                CodeEditor.Begin();
                waitFrame = true;
            }
        }
        else if(gameState == GameState.CodeEditor)
        {
            CodeEditor.OnGUI();
        }
        else if(gameState == GameState.Game)
        {
            Game.OnGUI();
        }
    }
}


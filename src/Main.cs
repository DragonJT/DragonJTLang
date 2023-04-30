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
    private void OnGUI()
    {
        lineSize = Screen.height * 0.075f;
        buttonWidth = Screen.width * 0.2f;
        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = (int)(Screen.height*0.05f);
        labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = (int)(Screen.height*0.05f);
        textboxStyle = new GUIStyle(GUI.skin.textField);
        textboxStyle.fontSize = (int)(Screen.height*0.05f);

        if (gameState == GameState.Instructions)
        {
            var text = @"This is a Codeeditor and language. Github repo 'https://github.com/DragonJT/DragonJTLang'
In the CodeEditor there is a textbox at the bottom where you input lines of code.
Enter 'Print(2)' or any line of code of your choice. Then press return.
Use the up and down arrows to select where to place the line of code.
Press the > button to run the code.
This project is early in development so not much code can be written.

example code...
var x = 20+20*2+4/2
var y = 1+2+3+4+5
Print(x+y)
if(x>y)
Print(2)";
            GUILayout.Label(text, labelStyle);

            if (GUILayout.Button("Continue", buttonStyle))
            {
                gameState = GameState.CodeEditor;
            }
        }
        if(gameState == GameState.CodeEditor)
        {
            CodeEditor.OnGUI();
        }
        else if(gameState == GameState.Game)
        {
            Game.OnGUI();
        }
    }
}


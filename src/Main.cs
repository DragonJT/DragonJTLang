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
    public static Main main;

    private void Awake()
    {
        Camera.main.clearFlags = CameraClearFlags.SolidColor;
        Camera.main.backgroundColor = Color.black;
        main = this;
    }

    private void Update()
    {
        if(gameState == GameState.Game)
        {
            Game.Update();
        }
    }

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
Press control/command backspace to delete previous line.";
            GUILayout.Label(text, labelStyle);

            if(Event.current.type == EventType.Repaint)
            {
                waitFrame = false;
            }
            GUI.color = new Color(0.5f, 0.5f, 1);
            var style = new GUIStyle(labelStyle);
            style.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label("Examples", style);
            GUI.color = Color.white;
            if (GUILayout.Button("Simple loop and print", buttonStyle))
            {
                CodeEditor.SetExample(@"var i=0
while(i<10){
    if(i>5){
        Print(i*i+1)
    }
    i=i+1
}");
            }
            if(GUILayout.Button("Moving triangle", buttonStyle))
            {
                CodeEditor.SetExample(@"var x=0
while(true){
    x=x+0.01
    if(x>1){
        x=0-1
    }
    DrawTriangle(x,0,0.2,0.3,1,0.3)
    yield
}");
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


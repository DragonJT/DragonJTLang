using UnityEngine;
using System.Collections.Generic;

static class Game
{
    static List<string> console = new List<string>();

    public static void Print(string message)
    {
        console.Add(message);
    }

    public static void Begin()
    {
        Main.gameState = GameState.Game;
        console.Clear();
    }

    public static void OnGUI()
    {
        GUI.Box(new Rect(20, 40, Screen.width - 40, Screen.height - 60), "");

        var y = 40;
        var x = 20;
        var lineSize = 30;
        foreach (var m in console)
        {
            GUI.Label(new Rect(x, y, Screen.width - 40, lineSize), m, Main.labelStyle);
            y += lineSize;
        }

        if (GUI.Button(new Rect(0, 0, 100, 40), "x", Main.buttonStyle))
        {
            Main.gameState = GameState.CodeEditor;
        }
    }
}
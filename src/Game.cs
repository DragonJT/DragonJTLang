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
        GUI.Box(new Rect(Main.lineSize, Main.lineSize, Screen.width - Main.lineSize*2, Screen.height - Main.lineSize*2), "");

        var y = Main.lineSize;
        var x = Main.lineSize;
        foreach (var m in console)
        {
            GUI.Label(new Rect(x, y, Screen.width - Main.lineSize*2, Main.lineSize), m, Main.labelStyle);
            y += Main.lineSize;
        }

        if (GUI.Button(new Rect(0, 0, Main.buttonWidth, Main.lineSize), "x", Main.buttonStyle))
        {
            CodeEditor.Begin();
        }
    }
}
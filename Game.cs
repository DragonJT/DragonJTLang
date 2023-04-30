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
        GUI.Box(new Rect(20, 20, Screen.width - 40, Screen.height - 40), "");

        var y = 20;
        var x = 20;
        var lineSize = 30;
        var style = new GUIStyle(GUI.skin.label);
        style.fontSize = 20;
        foreach (var m in console)
        {
            GUI.Label(new Rect(x, y, Screen.width - 40, lineSize), m, style);
        }

        if (GUI.Button(new Rect(0, 0, 100, 20), "x"))
        {
            Main.gameState = GameState.IDE;
        }
    }
}
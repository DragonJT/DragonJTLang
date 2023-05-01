using UnityEngine;
using System.Collections.Generic;

static class Game
{
    static List<string> console = new List<string>();
    static Mesh mesh;
    static Material material;
    static List<Vector3> vertices = new List<Vector3>();
    static List<Color> colors = new List<Color>();
    static List<int> triangles = new List<int>();

    public static void Print(string message)
    {
        console.Add(message);
    }

    public static void Begin()
    {
        Main.gameState = GameState.Game;
        console.Clear();
    }

    static int AddVertex(Vector2 position, Color color)
    {
        vertices.Add(position);
        colors.Add(color);
        return vertices.Count-1;
    }

    static void AddTriangle(int a, int b, int c)
    {
        triangles.AddRange(new int[] { a, b, c });
    }

    public static void DrawTriangle(float x, float y, float radius, Color color)
    {
        var a = AddVertex(new Vector2(x-radius, x-radius), color);
        var b = AddVertex(new Vector2(x, x+radius), color);
        var c = AddVertex(new Vector2(x+radius, x-radius), color);
        AddTriangle(a, b, c);
    }


    public static void Update()
    {
        DrawTriangle(0,0,0.3f, Color.red);
        VM.Update();
        if (mesh == null)
        {
            mesh = new Mesh();
            material = new Material(Shader.Find("Custom/VColorOpaque"));
            Camera.main.transform.position = new Vector3(0, 0, -10);
            Camera.main.transform.LookAt(new Vector3(0, 0, 0));
            Camera.main.orthographic = true;
            Camera.main.orthographicSize = 1;
        }
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.colors = colors.ToArray();
        mesh.triangles = triangles.ToArray();
        Graphics.DrawMesh(mesh, Matrix4x4.identity, material, 0);
        vertices.Clear();
        colors.Clear();
        triangles.Clear();
    }

    public static void OnGUI()
    {
        //GUI.Box(new Rect(Main.lineSize, Main.lineSize, Screen.width - Main.lineSize*2, Screen.height - Main.lineSize*2), "");

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
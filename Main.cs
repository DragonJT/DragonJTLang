using UnityEngine;

enum GameState { IDE, Game }

class Main:MonoBehaviour
{
    public static GameState gameState = GameState.IDE;

    private void OnGUI()
    {
        if(gameState == GameState.IDE)
        {
            IDE.OnGUI();
        }
        else if(gameState == GameState.Game)
        {
            Game.OnGUI();
        }
    }
}


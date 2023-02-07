using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Main.Scripts.Managers
{
    public enum GameState
    {
        Start,
        InGame,
        Complete,
        Failed
    }

    public class GameStateManager : Singleton<GameStateManager>
    {
        public GameState gameState;

        public static event Action<GameState> OnGameStateChanged; 

        public void UpdateGameState(GameState newGameState)
        {
            gameState = newGameState;
            switch (gameState)
            {
                case GameState.Start:
                    break;
                case GameState.InGame:
                    break;
                case GameState.Complete:
                    break;
                case GameState.Failed:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            OnGameStateChanged?.Invoke(newGameState);
        }
    }
}
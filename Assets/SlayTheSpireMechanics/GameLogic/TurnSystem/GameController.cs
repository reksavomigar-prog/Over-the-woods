using System;
using System.Collections;
using Assets.SlayTheSpireMechanics.GameLogic.TurnSystem.StateMachine.Realizations;
using Assets.SlayTheSpireMechanics.UI;
using SlayTheSpireMechanics.Actions;
using SlayTheSpireMechanics.VisualLogic.CardContainer;
using SlayTheSpireMechanics.VisualLogic.GameControllers.GameStates;
using UnityEngine;

namespace SlayTheSpireMechanics.VisualLogic.GameControllers
{
    public class GameController : MonoBehaviour
    {
        private GameState _gameState;

        [SerializeField] private CardViewContainer cardViewContainer;
        public CardViewContainer CardViewContainer => cardViewContainer;

        [SerializeField] private BattleController battleController;
        public BattleController BattleController => battleController;

        [SerializeField] private WindowManager windowManager;
        public WindowManager WindowManager => windowManager;

        public void Init(CardViewContainer container, BattleController controller)
        {
            cardViewContainer = container;
            battleController = controller;
        }

        private void Update()
        {
            _gameState?.OnUpdate();
        }

        public void MakeTransition()
        {
            if (_gameState == null)
            {
                ChangeGameState(GameStateEnum.BattleEnter);
            }
            else if (_gameState is GameEnemyTurnState)
            {
                ChangeGameState(GameStateEnum.PlayerTurn);
            }
            else if (_gameState is GamePlayerTurnState)
            {
                ChangeGameState(GameStateEnum.EnemyTurn);
            }
            else if (_gameState is GameBattleEnterState)
            {
                ChangeGameState(GameStateEnum.PlayerTurn);
            }
        }


        public void CheckWinCondition()
        {
            if (battleController.CurrentEnemies.Count == 0)
            {
                battleController.Player.DestroyTrash();
                cardViewContainer.DestroyTrash();
                ChangeGameState(GameStateEnum.BattleEnter);
            } 
        }


        public void ChangeGameState(GameStateEnum newGameState)
        {
            _gameState?.OnEnd();
            ActionSystem.Instance.AddMethodToQueue(() =>
            {

                switch (newGameState)
                {
                    case GameStateEnum.PlayerTurn:
                        _gameState = new GamePlayerTurnState(this);
                        break;
                    case GameStateEnum.EnemyTurn:
                        _gameState = new GameEnemyTurnState(this);
                        break;
                    case GameStateEnum.BattleEnter:
                        _gameState = new GameBattleEnterState(this);
                        break;

                }
                _gameState?.OnStart();
                Debug.Log(newGameState);
                ChangeTurnGA changeTurnGA = new ChangeTurnGA(newGameState);
                ActionSystem.Instance.AddCallbackToQueue(changeTurnGA);
                return null;
            });
        }
    }
}
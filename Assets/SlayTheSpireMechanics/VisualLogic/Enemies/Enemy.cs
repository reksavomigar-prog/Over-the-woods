using System;
using System.Collections.Generic;
using Assets.SlayTheSpireMechanics.Enums;
using SlayTheSpireMechanics.Actions;
using SlayTheSpireMechanics.VisualLogic.Enemies.EnemyActions;
using SlayTheSpireMechanics.VisualLogic.Enemies.EnemyTypes;
using SlayTheSpireMechanics.VisualLogic.GameControllers;
using SlayTheSpireMechanics.VisualLogic.ObjectInterfaces;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;


namespace SlayTheSpireMechanics.VisualLogic.Enemies
{
    public abstract class Enemy : MonoBehaviour, ITargetable
    {
        [SerializeField] private ReactiveProperty<int> _health;
        [SerializeField] private GameObject healthBarPrefab;

        private BattleController _battleController;
        public IReadOnlyReactiveProperty<int> Health => _health;

        public EnemySetting enemySetting;
        
        public Dictionary<EnemyChoseVariants, IEnemyAction> EnemyActions = new();

        public SerializedDictionary<EnemyChoseVariants, int> ActionsPercentage = new();
        public SerializedDictionary<EnemyChoseVariants, int> MaxActionRepeat = new();
        public SerializedDictionary<EnemyChoseVariants, int> ActionStartTurn = new();
        public SerializedDictionary<EnemyChoseVariants, BattleSituationsEnum> ActionConditionEnter = new();



        private List<EnemyChoseVariants> _choseVariantsSet = new();
        
        private List<EnemyChoseVariants> previousActions = new();

        [SerializeField] private ReactiveProperty<int> _maxHealth = new ReactiveProperty<int>();
        public IReadOnlyReactiveProperty<int> MaxHealth => _maxHealth;
        

        public void Init(BattleController bc)
        {
            _battleController = bc;
            _health.Value = enemySetting.Health;
            _maxHealth.Value = enemySetting.Health;
            EnemyActions = EnemyActionBinding.GetActionListFromEnemySetting(enemySetting);
            SetChances();

            HealthBarUI healthBar = Instantiate(healthBarPrefab, transform).GetComponentInChildren<HealthBarUI>();
            if (healthBar != null) healthBar.Init(this);
        }

        public void Act()
        {
            EnemyChoseVariants enemySetting = MakeChoice();
            ActionSystem.Instance.AddActionToQueue(EnemyActions[enemySetting]);
        }
        
        public void SetChances()
        {
            foreach (var action in ActionsPercentage)
            {
                for (int i = 0; i < action.Value; i++)
                {
                    _choseVariantsSet.Add(action.Key);
                }
            }
        }

        public EnemyChoseVariants RollChoice()
        {
            return _choseVariantsSet[Random.Range(0, _choseVariantsSet.Count)];
        }

        public EnemyChoseVariants MakeChoice()
        {
            int attempts = 0;
            bool isAllGood;
            EnemyChoseVariants potential;
            do
            {
                attempts++;
                if (attempts > 1000)
                {
                    throw new Exception($"StackOverflow in {gameObject.name}");
                }
                isAllGood = true;
                potential = RollChoice();
                
                if (ActionConditionEnter.ContainsKey(potential))
                {
                    
                    if (!_battleController.BattleSituations.Contains(ActionConditionEnter[potential]))
                    {
                        isAllGood = false;
                        continue;
                    }
                }
                if (ActionStartTurn.ContainsKey(potential))
                {
                   
                    if (_battleController.CurrentTurn < ActionStartTurn[potential])
                    {
                        isAllGood = false;
                        continue;
                    }
                }
                for (int i = 0; i < MaxActionRepeat[potential]; i++)
                {
                    
                    if (previousActions.Count < i + 1) {break;}
                    if (previousActions[previousActions.Count - 1 - i] == potential)
                    {
                        isAllGood = false;
                        continue;
                    }
                }
                
            } while (!isAllGood);
            return potential;
        }
        
        public void GetDamage(int damage)
        {
            _health.Value = _health.Value - damage > 0 ? _health.Value - damage : 0;
        }
        
    }
}
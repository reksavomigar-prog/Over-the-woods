using Assets.SlayTheSpireMechanics.GameLogic.PlayerSystem;
using SlayTheSpireMechanics.Actions;
using SlayTheSpireMechanics.VisualLogic.ObjectInterfaces;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace SlayTheSpireMechanics.VisualLogic.CardContainer
{
    public class Player : MonoBehaviour, ITargetable
    {
        [SerializeField] private GameObject healthBarPrefab;
        [SerializeField] private GameObject playerWrapper;
        [SerializeField] private Transform playerPosition;

        [SerializeField] private int _maxCardsHold;
        public int MaxCardsHold => _maxCardsHold;

        [SerializeField] private ReactiveProperty<int> _maxHealth = new ReactiveProperty<int>(80);
        

        [SerializeField] private int maxMana;

        public Inventory Inventory { get; private set; } = new Inventory();
        public CardModelContainer CardModelContainer { get; private set; }
        public ManaContainer ManaContainer { get; private set; }



        [SerializeField] private ReactiveProperty<int> _health = new ReactiveProperty<int>(80);
        public IReadOnlyReactiveProperty<int> Health => _health;
        public IReadOnlyReactiveProperty<int> MaxHealth => _maxHealth;

        public void Init()
        {
            CardModelContainer = new CardModelContainer(MaxCardsHold, this);
            ManaContainer = new ManaContainer(maxMana);


            Debug.Log(playerWrapper.name);
            Debug.Log(healthBarPrefab.name);
            GameObject wrapper = Instantiate(playerWrapper, playerPosition);
            GameObject hb = Instantiate(healthBarPrefab, wrapper.transform);
            HealthBarUI healthBar = hb.GetComponentInChildren<HealthBarUI>();
            if (healthBar != null) healthBar.Init(this);
        }
        public void DestroyTrash()
        {
            foreach (Transform trashObj in playerPosition.GetComponentsInChildren<Transform>())
            {
                Destroy(trashObj.gameObject);
            }
        }
        public void GetDamage(int damage)
        {
            if (damage < 0) { return; }
            _health.Value = _health.Value - damage > 0 ? _health.Value - damage : 0;
        }

        public void GetHeal(int heal)
        {
            if (heal < 0) { return; }
            _health.Value = _health.Value + heal < _maxHealth.Value ? _health.Value + heal : _maxHealth.Value;
        }

        public void IncreaseMaxHealth(int amount)
        {
            if (amount < 0) { return; }
            _maxHealth.Value = _maxHealth.Value + amount < 999 ? _maxHealth.Value + amount : 999;
        }

        public void DecreaseMaxHealth(int amount)
        {
            if (amount < 0) { return; }
            _maxHealth.Value = _maxHealth.Value - amount > 1 ? _maxHealth.Value - amount : 1;
            if (_health.Value > _maxHealth.Value) _health.Value = _maxHealth.Value;
        }
    }
}
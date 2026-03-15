using System;
using System.Collections;
using System.Collections.Generic;
using SlayTheSpireMechanics.VisualLogic.CardActionsCode;
using SlayTheSpireMechanics.VisualLogic.CardContainer;
using SlayTheSpireMechanics.VisualLogic.Enemies;
using SlayTheSpireMechanics.VisualLogic.Enemies.EnemyActions;
using UnityEngine;

namespace SlayTheSpireMechanics.VisualLogic.GameControllers
{
    public class DamageHandler : MonoBehaviour
    {
        public List<Enemy> enemyList = new();
        private Player _player;
        private void OnEnable()
        {
            ActionSystem.Subscribe<DamageCA>(DamageCardActionPerformer, ReactionTiming.Post);
            ActionSystem.Subscribe<DamageEA>(DamageEnemyActionPerformer, ReactionTiming.Post);
            ActionSystem.Subscribe<HealEA>(HealEnemyActionPerformer, ReactionTiming.Post);
        }

        public void Init(BattleController enemyController, Player player)
        {
            enemyController.OnEnemyAppeared += a => enemyList.Add(a);
            enemyController.OnEnemyDestroyed += a => enemyList.Remove(a);
            _player = player;
        }
        
        private IEnumerator DamageCardActionPerformer(DamageCA ca)
        {
            if (ca.Target != null)
            {
                for (int i = 0; i < ca.Repeat; i++)
                {
                    Debug.Log("damage received");
                    ca.Target.GetDamage(ca.Damage);
                }
            }
            else
            {
                foreach (var enemy in enemyList)
                {
                    for (int i = 0; i < ca.Repeat; i++)
                    {
                        Debug.Log("damage received by all");
                        enemy.GetDamage(ca.Damage);
                    }
                }
            }
            yield break;
        }

        private IEnumerator DamageEnemyActionPerformer(DamageEA ea)
        {
            for (int i = 0; i < ea.Repeat; i++)
            {
                Debug.Log("damage received by player");
                _player.GetDamage(ea.Damage);
            }
            yield break;
        }

        private IEnumerator HealEnemyActionPerformer(HealEA ea)
        {
            if (ea.Target == null)
                foreach (var enemy in enemyList)
                {

                    for (int i = 0; i < ea.Repeat; i++)
                    {
                        Debug.Log("enemy healed");
                        enemy.GetDamage(-ea.Heal);
                    }
                }
            else
            {
                for (int i = 0; i < ea.Repeat; i++)
                {
                    Debug.Log("enemy healed");
                    ea.Target.GetDamage(-ea.Heal);
                }
            }

                yield break;
        }
    }
}
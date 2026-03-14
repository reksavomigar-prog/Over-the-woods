using Assets.SlayTheSpireMechanics.ActionSystemLogic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;


namespace SlayTheSpireMechanics
{
    public class ActionSystem : Singleton<ActionSystem>
    {
        private static Dictionary<Type, IActionBinding> ActionsDictionary = new();
        private static Dictionary<Type, ICallbackBinding> CallbacksDictionary = new();

        private LinkedList<Func<IEnumerator>> ActionQueue = new();

        [SerializeField] private bool isQueueRunning;
        
        public static ActionBinding<T> GetActionBinding<T>() where T : IAction
        {
            Type type = typeof(T);
            if (ActionsDictionary.TryGetValue(type, out var binding))
            {
                if (binding is ActionBinding<T> rightBinding)
                {
                    return rightBinding;
                }
            }
            ActionBinding<T> newActionBinding = new ActionBinding<T>();

            ActionsDictionary[type] = newActionBinding;

            return newActionBinding;
        }
        public static CallbackBinding<T> GetCallbackBindind<T>() where T : ICallback
        {
            Type type = typeof(T);
            if (CallbacksDictionary.TryGetValue(type, out var binding))
            {
                if (binding is CallbackBinding<T> rightBinding)
                {
                    return rightBinding;
                }
            }
            
            CallbackBinding<T> newCallbackBinding = new CallbackBinding<T>();

            CallbacksDictionary[type] = newCallbackBinding;

            return newCallbackBinding;
        }

        public static void AttachPerformer<T>(Func<T, IEnumerator> performer) where T : IAction
        {
            var binding = GetActionBinding<T>();
            if (binding != null)
            {
                binding.BindPerformer(performer);
            }
        }
        public static void DetachPerformer<T>(Func<T, IEnumerator> performer) where T : IAction
        {
            var binding = GetActionBinding<T>();
            if (binding != null)
            {
                binding.UnbindPerformer(performer);
            }
        }
        public static void Subscribe<T>(Func<T, IEnumerator> reaction, ReactionTiming timing) where T : IAction
        {
            var binding = GetActionBinding<T>();
            if (binding != null)
            {
                binding.BindSubscribtion(reaction, timing);
            }
        }
        public static void Subscribe<T>(Func<T, IEnumerator> reaction) where T : ICallback
        {
            var binding = GetCallbackBindind<T>();
            if (binding != null)
            {
                binding.Bind(reaction);
            }
        }

        public static void Unsubscribe<T>(Func<T, IEnumerator> reaction, ReactionTiming timing) where T : IAction
        {
            var binding = GetActionBinding<T>();
            if (binding != null)
            {
                binding.UnbindSubscribtion(reaction, timing);
            }
        }
        public static void UnSubscribe<T>(Func<T, IEnumerator> reaction) where T : ICallback
        {
            var binding = GetCallbackBindind<T>();
            if (binding != null)
            {
                binding.RemoveReaction(reaction);
            }
        }


        public void AddReactionsToQueue(IAction action, ReactionTiming timing)
        {
            if (ActionsDictionary.TryGetValue(action.GetType(), out var binding))
            {
                foreach (Func<IEnumerator> item in binding.GainReactions(action, timing))
                {
                    ActionQueue.AddLast(item);
                }
            }
        }
        public void AddReactionsToQueue(ICallback callback)
        {
            if (CallbacksDictionary.TryGetValue(callback.GetType(), out var binding))
            {
                foreach(Func<IEnumerator> item in binding.GainReactions(callback))
                {
                    ActionQueue.AddFirst(item);
                }
            }
        }
        public void AddPerformerToQueue(IAction action)
        {
            if (ActionsDictionary.TryGetValue(action.GetType(), out var binding))
            {
                ActionQueue.AddLast(binding.GainPerformer(action));
            }
        }

        public void AddActionToQueue(IAction action)
        {
            AddReactionsToQueue(action, ReactionTiming.Pre);

            AddPerformerToQueue(action);

            AddReactionsToQueue(action, ReactionTiming.Post);
        }
        public void AddCallbackToQueue(ICallback callback)
        {
            AddReactionsToQueue(callback);
        }
        public void AddMethodToQueue(Func<IEnumerator> method)
        {
            ActionQueue.AddLast(method);
        }

        public IEnumerator CheckQueue()
        {
            while (ActionQueue.Count > 0)
            {
                Func<IEnumerator> action = ActionQueue.First.Value;
                ActionQueue.RemoveFirst();
                yield return action?.Invoke();
                StartCoroutine(CheckQueue());
            }
            isQueueRunning = false;
        }



        private void Update()
        {
            if (!isQueueRunning)
            {
                Debug.Log(ActionQueue?.First?.Value != null);
                isQueueRunning = true;
                StartCoroutine(CheckQueue());
            }
        }
       


    }
}
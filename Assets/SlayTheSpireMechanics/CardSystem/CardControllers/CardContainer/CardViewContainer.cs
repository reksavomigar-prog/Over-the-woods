
using SlayTheSpireMechanics.VisualLogic.Card;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UniRx;
using UnityEngine;
using UnityEngine.Splines;

namespace SlayTheSpireMechanics.VisualLogic.CardContainer
{
    public class CardViewContainer : MonoBehaviour
    {
        public CardHoverSystem cardHoverSystem;
        public CardAnimator cardAnimator;

        
        public GameObject cardPrefab;

        public SplineContainer currentCardSpline;
        public SplineContainer hiddenCardSpline;
        
        
        private bool _isShowing = true;
        
        private readonly Queue<CardModel> _drawCardQueue = new Queue<CardModel>();
        private readonly Queue<CardModel> _discardCardQueue = new Queue<CardModel>();
        


        public List<CardView> cardList = new List<CardView>();
        private int _maxCards;

        public event Action<Dictionary<CardView, Vector3>, Dictionary<CardView, Vector3>> OnCardPositionChanged;
        
        public void Init(Player player, CardHoverSystem cardHover, CardAnimator animator)
        {
            cardHoverSystem = cardHover;
            cardAnimator = animator;
            OnCardPositionChanged += cardAnimator.UpdateCardStartPositions;
        }
        public void Init(Player player)
        {
            _maxCards = player.MaxCardsHold;
            player.CardModelContainer.OnRefillFinished += AcceptDrawCardList;
            player.CardModelContainer.OnDiscardFinished += AcceptDiscardCardList;
            player.CardModelContainer.OnCardPlayed += (cm) => AcceptDiscardCardList(new List<CardModel> { cm });
        }
        public void DestroyTrash()
        {
            _drawCardQueue.Clear();
            _discardCardQueue.Clear();
            cardList.Clear();
            foreach (var card in GetComponentsInChildren<Transform>())
            {
                Destroy(card.gameObject);

            }
        }

        private void AcceptDiscardCardList(List<CardModel> cm)
        {
            foreach (var CardModel in cm)
            {
                _discardCardQueue.Enqueue(CardModel);
            }
            ActionSystem.Instance.AddMethodToQueue(DeleteCardsInQueue);
        }
        private void AcceptDrawCardList(List<CardModel> cm)
        {
            foreach (var CardModel in cm)
            {
                _drawCardQueue.Enqueue(CardModel);
            }
            ActionSystem.Instance.AddMethodToQueue(VisualizeCardsInQueue);
        }
        

        private IEnumerator DeleteCardsInQueue()
        {
            while (_discardCardQueue.Count > 0)
            {
                CardModel card = _discardCardQueue.Dequeue();
                CardView cv = cardList.Find(a=> a.CardInfo == card);
                if (cv != null) 
                { 
                    DiscardCard(cv);
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        private IEnumerator VisualizeCardsInQueue()
        {
            while (_drawCardQueue.Count > 0)
            {
                AddCardByCardModelInHand(_drawCardQueue.Dequeue());
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        private IEnumerator DestroyCard(CardModel cardModel)
        {
            CardView cv = cardList.Find((a) => a.CardInfo == cardModel);
            cardList.Remove(cv);
            cardAnimator.PlayDeleteAnimation(cv);
            yield return new WaitForSeconds(0.4f);
            Destroy(cv.gameObject);
        }

        private void DiscardCard(CardView cv)
        {
            cardList.Remove(cv);

            cardAnimator.PlayDiscard(cv, () => Destroy(cv.gameObject));
            UpdateCardsPositionOnCurve();
        }
       
        
        private void AddCardByCardModelInHand(CardModel cardModel)
        {
            CardView cv = VisualizeCardModel(cardModel);
            cardAnimator.PlayCreateAnimation(cv);
            cardList.Add(cv);
            cv.SortOrder = 50 + cardList.IndexOf(cv) * 2;
            UpdateCardsPositionOnCurve();
        }

        private CardView VisualizeCardModel(CardModel cardModel)
        {
            GameObject go = Instantiate(cardPrefab, transform);
            if (go.TryGetComponent(out CardView cardView))
            {
                cardView.Init(cardModel);
                cardView.OnEnter = cardHoverSystem.OnMouseEnt;
                cardView.OnExit = cardHoverSystem.OnMouseExit;
                cardView.DragEnd = cardHoverSystem.DragEnd;
                cardView.DragStart = cardHoverSystem.DragStart;
                cardView.DragContinue = cardHoverSystem.DragContinue;
                return cardView;
            }
            return null;
        }

        
        private void UpdateCardsPositionOnCurve()
        {
            if (cardList.Count == 0) { return; }
            
            Dictionary<CardView, Vector3> cardPositions = new Dictionary<CardView, Vector3>();
            Dictionary<CardView, Vector3> cardRotations = new Dictionary<CardView, Vector3>();

            SplineContainer spline = _isShowing ? currentCardSpline : hiddenCardSpline;
            float step = 1f / _maxCards;
            float firstCardPosition = 0.5f - (cardList.Count - 1) * step / 2;

            for (int i = 0; i < cardList.Count; i++)
            {
                float cardSplineLocation = firstCardPosition + step * i;
                Vector3 p = spline.EvaluatePosition(cardSplineLocation);
                Vector3 up = spline.EvaluateUpVector(cardSplineLocation);
                Vector3 fw = spline.EvaluateTangent(cardSplineLocation);
                Quaternion r = Quaternion.LookRotation(-up, Vector3.Cross(-up, fw));
                Vector3 euler = r.eulerAngles;
                
                cardPositions[cardList[i]] = p;
                cardRotations[cardList[i]] = euler;       
            }
            OnCardPositionChanged?.Invoke(cardPositions, cardRotations);
            foreach (var cv in cardList)
            {

                cv.canBeHovered = false; cv.canBeDragged = false;
                cardAnimator.PlayReturnToSpline(cv, () =>
                {
                    cv.canBeHovered = true;
                    cv.canBeDragged = true;
                });

            }
        }
        
    }
}

using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace SlayTheSpireMechanics.VisualLogic.Card
{
    public class CardAnimator : MonoBehaviour
    {
        [SerializeField] private Transform activeCardLocation;
        [SerializeField] private Transform discardPile;
        [SerializeField] private Transform drawPile;

        private Dictionary<CardView, Vector3> _cardStartPositions = new Dictionary<CardView, Vector3>();
        private Dictionary<CardView, Vector3> _cardStartRotations = new Dictionary<CardView, Vector3>();



        public Tween MoveOneCard(CardView card, Vector3 targetPosition, float moveDuration)
        {

            Tween moveTween = card.Wrapper.DOMove(targetPosition, moveDuration);
            card._cardTweens.moveTween = moveTween;

            return moveTween;
  
        }

        public Tween RotateOneCard(CardView card, Vector3 targetAngle, float rotateDuration)
        {
            Tween rotateTween = card.Wrapper.DORotate(targetAngle, rotateDuration);
            card._cardTweens.rotationTween = rotateTween;
            return rotateTween;
        }


        public Tween ScaleOneCard(CardView card, float scaleFactor, float duration)
        {
            Vector3 targetScale = Vector3.one * scaleFactor;
            
            Tween scaleTween = card.Wrapper.DOScale(targetScale, duration);
            card._cardTweens.scaleTween = scaleTween;

            return scaleTween;
        }


        public Tween PlayCreateAnimation(CardView card)
        {
            float startScaleFactor = card.Wrapper.localScale.x;
           
            card.Wrapper.localScale = Vector3.zero;

            return ScaleOneCard(card, startScaleFactor, 0.3f);
        }

        public Tween PlayDeleteAnimation(CardView card)
        {
            card.Wrapper.localScale = Vector3.one;
            
            return ScaleOneCard(card, 0, 0.3f);
        }

        public Tween PlayMoveToActiveSlot(CardView cardView, Action onComplete)
        {
            cardView._cardTweens.moveTween?.Kill();
            return MoveOneCard(cardView, activeCardLocation.position, 0.3f).OnComplete(() => onComplete?.Invoke());
        }
        public void PlayHover(CardView cv, Action onComplete)
        {
            cv.SortOrder *= 2;


            cv._cardTweens.moveTween?.onComplete?.Invoke();
            cv._cardTweens.moveTween?.Kill();
            cv._cardTweens.scaleTween?.onComplete?.Invoke();
            cv._cardTweens.scaleTween?.Kill();
            Tween scaleTween = ScaleOneCard(cv, 1.2f, 0.3f);
            Tween moveTween = MoveOneCard(cv, cv.Wrapper.position + cv.Wrapper.up, 0.3f);
     


            if (onComplete != null) scaleTween.OnComplete(() =>  onComplete.Invoke() );
        }
        public void PlayDragStartRotation(CardView cv, Action onComplete)
        {
            cv._cardTweens.moveTween?.Kill();
            cv._cardTweens.rotationTween?.Kill();
            cv._cardTweens.scaleTween?.onComplete?.Invoke();
            cv._cardTweens.scaleTween?. Kill();


            Tween tween = RotateOneCard(cv, Vector3.zero, 0.3f);
            Tween scaleTween = ScaleOneCard(cv, 1.2f, 0.3f);
            if (onComplete != null) tween.OnComplete(() => onComplete.Invoke());
        }

        public void UpdateCardStartPositions(Dictionary<CardView, Vector3> cardStartPositions, Dictionary<CardView, Vector3> cardStartRotations)
        {
            _cardStartPositions = cardStartPositions;
            _cardStartRotations = cardStartRotations;
           
        }

        public void PlayReturnToSpline(CardView cv, Action onComplete = null)
        {
            cv._cardTweens.moveTween?.Kill();
            cv._cardTweens.rotationTween?.Kill();
            cv._cardTweens.scaleTween?.Kill();
            ScaleOneCard(cv, 1f, 0.3f);

            cv.SortOrder = cv.SortOrder / 2 < 40 ? cv.SortOrder : cv.SortOrder / 2; // -----------------------------------------
            
            Tween tween = null;
            if (_cardStartPositions.TryGetValue(cv, out var position)) { tween = MoveOneCard(cv, position, 0.3f);  }
            if (_cardStartRotations.TryGetValue(cv, out var rotation)) { tween = RotateOneCard(cv, rotation, 0.3f); }
            tween?.OnComplete(() =>  onComplete?.Invoke());
        }

        public void PlayDiscard(CardView cv, Action onComplete)
        {
            if (cv == null) return;
            cv._cardTweens.moveTween?.Kill();
            cv._cardTweens.rotationTween?.Kill();
            cv._cardTweens.scaleTween?.Kill();
            MoveOneCard(cv, discardPile.position, 0.3f);
            RotateOneCard(cv, discardPile.rotation.eulerAngles, 0.3f);
            Tween scaleTween = ScaleOneCard(cv, 0, 0.3f);
            scaleTween?.OnComplete(() => onComplete?.Invoke());
        }
    }

}
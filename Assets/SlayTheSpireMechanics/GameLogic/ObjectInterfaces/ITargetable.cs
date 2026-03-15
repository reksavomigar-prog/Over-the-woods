using SlayTheSpireMechanics.VisualLogic.CardActionsCode;
using UniRx;

namespace SlayTheSpireMechanics.VisualLogic.ObjectInterfaces
{
    public interface ITargetable
    {
        public IReadOnlyReactiveProperty<int> Health { get; }
        public IReadOnlyReactiveProperty<int> MaxHealth { get; }

       
        public void GetDamage(int damageCa);
    }
}
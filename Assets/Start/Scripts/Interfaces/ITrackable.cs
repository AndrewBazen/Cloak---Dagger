
namespace Start.Scripts.Interfaces
{
    public interface ITrackable
    {
        bool HasTurn { get; set; }
        bool HasAction { get; set; }
        bool HasMovement { get; set; }
        bool HasBonusAction { get; set; }
        void StartTurn();
        void EndTurn();
    }
}

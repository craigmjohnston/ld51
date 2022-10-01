namespace Oatsbarley.LD51.Interfaces
{
    using Oatsbarley.LD51.Data;

    public interface IReceiver
    {
        bool CanReceive(Item item);
        void Receive(Item item);
    }
}
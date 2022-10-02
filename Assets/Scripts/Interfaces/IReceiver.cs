namespace Oatsbarley.LD51.Interfaces
{
    using System.Collections.Generic;
    using Oatsbarley.LD51.Data;

    public interface IReceiver
    {
        bool CanReceive(Item item, int inputIndex);
        void Receive(Item item, int inputIndex);
        // Item[] FilterValidItems(IEnumerable<Item> items);
    }
}
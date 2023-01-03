using GoRogue.GameFramework;
using SadRogue.Primitives;

namespace GoRogue_ncurses_tutorial
{
    internal class Player : GameObject
    {
        public readonly char displayChar;
        public Player(Point position, char displayChar, int layer) : base(position, layer, false)
        {
            this.displayChar = displayChar;
        }
    }
}

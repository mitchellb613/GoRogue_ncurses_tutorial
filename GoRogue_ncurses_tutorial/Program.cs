using GoRogue.MapGeneration;
using SadRogue.Primitives.GridViews;
using Mindmagma.Curses;

namespace GoRogue_ncurses_tutorial
{
    internal class Program
    {
        static void Main(string[] args)
        {
            NCurses.InitScreen();
            var generator = new Generator(80, 24);
            generator.ConfigAndGenerateSafe(gen =>
            {
                gen.AddSteps(DefaultAlgorithms.DungeonMazeMapSteps());
            });
            var wallFloorValues = generator.Context.GetFirst<ISettableGridView<bool>>("WallFloor");
            foreach (var pos in wallFloorValues.Positions())
            {
                if (wallFloorValues[pos])
                {
                    NCurses.MoveAddChar(pos.Y, pos.X, '.');
                }
                else
                {
                    NCurses.MoveAddChar(pos.Y, pos.X, '#');
                }
            }
            NCurses.Refresh();
            NCurses.GetChar();
            NCurses.EndWin();
        }
    }
}



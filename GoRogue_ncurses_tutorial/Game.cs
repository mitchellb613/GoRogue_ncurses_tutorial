using Mindmagma.Curses;
using SadRogue.Primitives;

namespace GoRogue_ncurses_tutorial
{
    internal class Game
    {
        private readonly IntPtr _window;
        private readonly Player _player;
        public Game()
        {
            _window = NCurses.InitScreen();
            _player = new Player(new Point(0, 0), '@', 1);
            NCurses.Keypad(_window, true);
            NCurses.NoEcho();
            NCurses.CBreak();
        }

        public void Run()
        {
            while (true)
            {
                NCurses.Erase();
                NCurses.MoveAddChar(_player.Position.Y, _player.Position.X, _player.displayChar);
                NCurses.Refresh();
                var input = NCurses.GetChar();
                switch (input)
                {
                    case CursesKey.UP:
                        _player.Position += Direction.Up;
                        break;
                    case CursesKey.DOWN:
                        _player.Position += Direction.Down;
                        break;
                    case CursesKey.LEFT:
                        _player.Position += Direction.Left;
                        break;
                    case CursesKey.RIGHT:
                        _player.Position += Direction.Right;
                        break;
                    case CursesKey.ESC:
                        NCurses.EndWin();
                        Environment.Exit(0);
                        break;
                }
            }
        }
    }
}

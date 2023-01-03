using Mindmagma.Curses;

namespace GoRogue_ncurses_tutorial
{
    internal class Game
    {
        private readonly IntPtr _window;
        private int _count;
        public Game()
        {
            _count = 0;
            _window = NCurses.InitScreen();
            NCurses.Keypad(_window, true);
            NCurses.NoEcho();
            NCurses.CBreak();
        }

        public void Run()
        {
            while (true)
            {
                NCurses.Erase();
                NCurses.AddString(_count.ToString());
                NCurses.Refresh();
                var input = NCurses.GetChar();
                switch (input)
                {
                    case CursesKey.UP:
                        _count++;
                        break;
                    case CursesKey.DOWN:
                        _count--;
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

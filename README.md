# GoRogue + dotnet-curses Tutorial

## 0. Requirements

- Visual Studio 2022 or any editor you are familiar with
- GoRogue v3 and dotnet-curses added to your project
- ncurses installed on your system

## 1. Hello dotnet-curses

Let's start by getting some text on the screen with dotnet-curses.

```csharp
using Mindmagma.Curses;

namespace GoRogue_ncurses_tutorial
{
    internal class Program
    {
        static void Main(string[] args)
        {
            NCurses.InitScreen();
            NCurses.AddString("Hello World!");
            NCurses.Refresh();
            NCurses.GetChar();
            NCurses.EndWin();
        }
    }
}
```

- `InitScreen()` initializes the base ncurses window and gets info about the terminals capabilities.
- `AddString()` adds a string to be displayed the next time `Refresh()` is called.
- `GetChar()` waits for user input and returns an integer representing the input received.
- `EndWin()` restores the terminal to the way it was before we ran our program.

## 2. Hello GoRogue

Now that we can display text let's generate a map using GoRogue and display it with ncurses! First, we need a `Generator` instance to generate our map. We can create one by providing a width and height. We will use a width of 80 and height of 24; a standard terminal size.
```csharp
var generator = new Generator(80, 24);
```

The `Generator` class provides us with a function `ConfigAndGenerateSafe()` which can be used to run generation steps and will automatically regenerate our map if a `RegenerateMapException` is thrown. We can use it as follows:
```csharp
generator.ConfigAndGenerateSafe(gen =>
{
    gen.AddSteps(DefaultAlgorithms.DungeonMazeMapSteps());
});
```

`DungeonMazeMapSteps()` is a function in GoRogue that provides us with steps needed to make a maze map. These steps add some components to the `generator.Context`. One of these components is `WallFloor` and provides us a grid view where true values represent floors and false values represent walls. We can loop over this `WallFloor` component to display the generated map. Let's use '#' for walls and '.' for floors.

```csharp
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
```

`MoveAddChar()` is an ncurses function which moves the cursor to a specified position and adds a character to be displayed the next time `Refresh()` is called. All together the code to display our map looks like:

```csharp
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
```

# 3. Scaffolding a game

Let's begin by making a `Game` class with a method that runs itself. We'll have a `_count` integer that the user can increment by pressing the up arrow and decrement by pressing down arrow. This will help us understand the core ncurses functions we need to get a game loop going.
```csharp
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
            _window = NCurses.InitScreen(); //we will keep a pointer to our ncurses window this time
            NCurses.Keypad(_window, true); //set ncurses to receive input from the keypad
            NCurses.NoEcho(); //do not display the users input directly to the terminal
            NCurses.CBreak(); //disable line buffering so that the user doesn't need to submit input with enter
        }

        public void Run()
        {
            while (true)
            {
                NCurses.Erase(); //fill the screen with space characters; this is how we clear our window with ncurses
                NCurses.AddString(_count.ToString());
                NCurses.Refresh(); //display the current value of count
                var input = NCurses.GetChar(); //wait for the users input
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
```

Now we need to create a `Game` instance and call `Run()` in our `Main` method.
```csharp
namespace GoRogue_ncurses_tutorial
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var game = new Game();
            game.Run();
        }
    }
}
```


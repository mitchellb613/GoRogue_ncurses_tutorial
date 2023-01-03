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

## 3. Scaffolding a game

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

## 4. Creating the player

To make an actual game we will need a player object. Let's make a Player class that extends the GoRogue `GameObject` class. `GameObjects`'s can easily be added to `Map` which we will do in the next section. Along with what `GameObject` provides let's also give our Player a `displayChar` that we can use to render to the screen.

 ```csharp
 using GoRogue.GameFramework;
 using SadRogue.Primitives;
 
 namespace GoRogue_ncurses_tutorial
 {
     internal class Player : GameObject
     {
         public readonly char displayChar;
         public Player(Point position, char displayChar, int layer) : base(position, layer, false)
         {
             this.displayChar= displayChar;
         }
     }
 }
 ```

Now we can update our game loop to move the player around the console. Note that if you move outside the bounds of the terminal with this code it will crash with an error. Don't worry we will fix this later when we start using an actual `Map`.

```csharp
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
            _player = new Player(new Point(0, 0), '@', 1); //create the player; I will use the traditional @ for it's display
            NCurses.Keypad(_window, true);
            NCurses.NoEcho();
            NCurses.CBreak();
        }

        public void Run()
        {
            while (true)
            {
                NCurses.Erase();
                NCurses.MoveAddChar(_player.Position.Y, _player.Position.X, _player.displayChar); //add the player to screen
                NCurses.Refresh();
                var input = NCurses.GetChar();
                switch (input)
                {
                    case CursesKey.UP:
                        _player.Position += Direction.Up; //GoRogue provides us Directions to use with a grid
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
```

## 5. Bringing back the map

- add map and move player around it
- add doors to the map and open them
- add monsters that chase the player when found
- add attacks to kill monsters/player takes damage
- add color

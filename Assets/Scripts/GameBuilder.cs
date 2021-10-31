abstract class GameBuilder<TGame>
    where TGame : Game
{
    protected int _boardSize = 8;
    protected int _level = 1;

    public GameBuilder<TGame> BoardSize(int boardSize)
    {
        _boardSize = boardSize;
        return this;
    }

    public GameBuilder<TGame> Level(int level)
    {
        _level = level;
        return this;
    }

    public abstract TGame Build();
}

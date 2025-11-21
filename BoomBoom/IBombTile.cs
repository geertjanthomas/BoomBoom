namespace BoomBoom;

public interface IBombTile
{
    void Expose(bool endOfGame = false);
    void ToggleFlag();
}

namespace KriegspielTicTacToe.Model;

public record Player(string Value)
{
    public static Player FromChar(char value) => new(value.ToString());
    
    public static implicit operator Player(char value) => new(value.ToString());
    
    public static implicit operator char(Player player) => player.Value[0];
    
    public static implicit operator string(Player player) => player.Value;
}

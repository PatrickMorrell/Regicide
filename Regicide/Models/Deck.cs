namespace Regicide.Models;

public class Deck
{
	public string? Name { get; set; }
	public List<PlayingCard> Cards { get; set; } = [];

	public Deck(string? name = null)
	{
		Name = name;
	}
}

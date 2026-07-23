using Regicide.Models;

namespace Regicide.Services;

public class StandardService
{
	public Deck StandardDeck = new("Standard");
	public List<Suit> Suits = [Suit.Clubs, Suit.Diamonds, Suit.Spades, Suit.Hearts];
	public List<string> Enemies = [Face.Jack.ToString(), Face.Queen.ToString(), Face.King.ToString()];

	public void BuildStandardDeck()
	{
		List<string> values = [Face.Ace, "2", "3", "4", "5", "6", "7", "8", "9", "10", Face.Jack, Face.Queen, Face.King];


		foreach (string value in values)
		{
			foreach (Suit suit in Suits)
			{
				StandardDeck.Cards.Add(new PlayingCard(suit, value));
			}
		}
	}
}

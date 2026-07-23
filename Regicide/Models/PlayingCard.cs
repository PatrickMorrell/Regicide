namespace Regicide.Models;

public class PlayingCard
{
	public Suit Suit { get; set; }
	public string Value { get; set; }
	public bool Active { get; set; }

	public int Strength
	{
		get
		{
			switch (Value)
			{
				case Face.Jack:
					return 10;
				case Face.Queen:
					return 15;
				case Face.King:
					return 20;
				case Face.Ace:
					return 1;
				default:
					if (int.TryParse(Value, out var v)) return v;
					return 0;
			}
		}
	}

	public int? Health
	{
		get
		{
			switch (Value)
			{
				case Face.Jack:
				case Face.Queen:
				case Face.King:
					return Strength * 2;
				default:
					return null;
			}
		}
	}

	public PlayingCard(Suit suit, string value)
	{
		Suit = suit;
		Value = value;
	}
}

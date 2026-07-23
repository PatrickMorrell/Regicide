using Regicide.Extensions;
using Regicide.Models;

namespace Regicide.Services;

public class CastleService(StandardService standardService)
{
	public Deck CastleDeck = new("Castle");

	public void BuildCastleDeck()
	{
		CastleDeck.Cards = [];

		var jacks = standardService.StandardDeck.Cards.Where(c => c.Value.StringEquals(Face.Jack));
		CastleDeck.Cards.AddRange(jacks);

		var queens = standardService.StandardDeck.Cards.Where(c => c.Value.StringEquals(Face.Queen));
		CastleDeck.Cards.AddRange(queens);

		var kings = standardService.StandardDeck.Cards.Where(c => c.Value.StringEquals(Face.King));
		CastleDeck.Cards.AddRange(kings);
	}

	public PlayingCard? CurrentEnemy => CastleDeck.Cards.FirstOrDefault();
}

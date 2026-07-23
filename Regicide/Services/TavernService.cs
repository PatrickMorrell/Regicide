using Regicide.Extensions;
using Regicide.Models;

namespace Regicide.Services;

public class TavernService(StandardService standardService, CastleService castleService)
{
	public Deck TavernDeck = new("Tavern");
	public Deck Hand = new("Hand");
	public List<List<PlayingCard>> Fight = [];
	public Deck DiscardPile = new("Discard");

	public Character Turn = Character.Hero;
	public int JestersUsed;

	private readonly int handLimit = 8;

	public event Action Updated;

	public void BuildTavernDeck()
	{
		TavernDeck.Cards = [];

		var tavern = standardService.StandardDeck.Cards.Where(c => !standardService.Enemies.Contains(c.Value));
		TavernDeck.Cards.AddRange(tavern);
	}

	public void BuildHand()
	{
		Hand.Cards = [];

		for (int i = 0; i < handLimit; i++)
		{
			DrawCard();
		}

		if (Hand.Cards.Count(c => c.Suit == Suit.Diamonds) == 0)
		{
			standardService.StandardDeck.Cards.ShuffleDeck();
			BuildTavernDeck();
			BuildHand();
		}
	}
	public void InitializeGame()
	{
		Turn = Character.Hero;
		standardService.StandardDeck.Cards.ShuffleDeck();

		castleService.BuildCastleDeck();
		BuildTavernDeck();
		BuildHand();

		Fight = [];
		DiscardPile.Cards = [];
	}

	public void RedrawHand()
	{
		JestersUsed++;
		var hand = Hand.Cards.ToList();
		foreach (var card in hand)
		{
			card.Active = false;
			DiscardCard(card);
		}

		for (int i = 0; i < handLimit; i++)
		{
			DrawCard();
		}

		Updated.Invoke();
	}

	public void MoveCard(Deck source, Deck destination, PlayingCard? card = null)
	{
		if (source.Cards.Count == 0)
		{
			return;
		}

		card ??= source.Cards.First();
		destination.Cards.Add(card);
		source.Cards.Remove(card);
	}

	public void DrawCard() => MoveCard(TavernDeck, Hand);

	public void PlayCard(PlayingCard card)
	{
		if (CanPlay(card))
		{
			card.Active = true;
		}
	}

	public void UndoPlay(PlayingCard card)
	{
		card.Active = false;
	}

	public bool CanPlay(PlayingCard card)
	{
		if (Turn == Character.Enemy) return true;
		if (card.Active) return true;
		var attack = GetActiveCards();
		if (attack.Count == 0) return true;
		if (attack.Count(c => c.Value.StringEquals(Face.Ace)) > 1) return false;
		if ((attack[0].Strength == card.Strength) && (attack.Sum(c => c.Strength) + card.Strength <= 10)) return true;
		if (attack.Count == 1 && card.Value.StringEquals(Face.Ace)) return true;
		return false;
	}

	private List<PlayingCard> GetActiveCards()
	{
		return Hand.Cards.Where(c => c.Active).ToList();
	}

	public bool ShowConfirmPlay => Turn == Character.Hero && GetActiveCards().Count != 0;

	public void ConfirmPlay()
	{
		var attack = GetActiveCards();

		Fight.Add(attack);

		var totalPower = attack.Sum(c => c.Strength);
		bool restore = ActivatePower(attack, Suit.Diamonds);
		bool heal = ActivatePower(attack, Suit.Hearts);

		foreach (var card in attack)
		{
			card.Active = false;
			Hand.Cards.Remove(card);
		}

		if (restore)
		{
			RestoreHand(totalPower);
		}

		if (heal)
		{
			HealTavern(totalPower);
		}

		if (DefeatEnemy())
		{
			DiscardFight();
		}
		else if (GetDamageDealt() > 0)
		{
			Turn = Character.Enemy;
		}

		Updated.Invoke();
	}

	public void LoseCard(PlayingCard card)
	{
		card.Active = true;
	}

	public void UndoLoss(PlayingCard card)
	{
		card.Active = false;
	}

	public bool ShowConfirmDamage => Turn == Character.Enemy && (GetActiveCards().Sum(c => c.Strength) >= GetDamageDealt() || Hand.Cards.Count(c => !c.Active) == 0);

	public void ConfirmLoss()
	{
		TakeDamage();
		Turn = Character.Hero;
		Updated.Invoke();
	}

	private bool ActivatePower(List<PlayingCard> attack, Suit suit)
	{
		return castleService.CurrentEnemy.Suit != suit
			&& attack.Any(c => c.Suit == suit);
	}

	public int TotalAttack()
	{
		int totalAttack = 0;
		foreach (var attack in Fight)
		{
			bool doubled = ActivatePower(attack, Suit.Clubs);
			int totalHits = attack.Sum(c => c.Strength);
			totalAttack += totalHits * (doubled ? 2 : 1);
		}

		return totalAttack;
	}

	public int TotalShields()
	{
		int totalShields = 0;
		foreach (var attack in Fight)
		{
			if (ActivatePower(attack, Suit.Spades))
			{
				totalShields += attack.Sum(c => c.Strength);
			}
		}

		return totalShields;
	}

	public void DiscardCard(PlayingCard card) => MoveCard(Hand, DiscardPile, card);
	public void Heal() => MoveCard(DiscardPile, TavernDeck);

	public void HealTavern(int amount)
	{
		if (castleService.CurrentEnemy.Suit == Suit.Hearts)
		{
			return;
		}

		DiscardPile.Cards.ShuffleDeck();

		for (int i = 0; i < amount; i++)
		{
			Heal();
		}
	}

	public void DiscardFight()
	{
		foreach (var attack in Fight.ToList())
		{
			foreach (var card in attack.ToList())
			{
				DiscardPile.Cards.Add(card);
			}
		}

		Fight.Clear();
	}

	public void TakeDamage()
	{
		var damage = Hand.Cards.Where(c => c.Active).ToList();
		foreach (var card in damage)
		{
			card.Active = false;
			DiscardCard(card);
		}
	}

	public bool DefeatEnemy()
	{
		var enemy = castleService.CurrentEnemy;
		var total = TotalAttack();

		if (total == enemy.Health)
		{
			TavernDeck.Cards.Insert(0, enemy);
			castleService.CastleDeck.Cards.Remove(enemy);
			return true;
		}
		else if (total > enemy.Health)
		{
			MoveCard(castleService.CastleDeck, DiscardPile, enemy);
			return true;
		}

		return false;
	}

	public void RestoreHand(int amount)
	{
		if (castleService.CurrentEnemy.Suit == Suit.Diamonds)
		{
			return;
		}

		for (int i = 0; i < amount; i++)
		{
			if (Hand.Cards.Count == handLimit)
			{
				Console.WriteLine(Hand.Cards.Select(c => $"{c.Value} of {c.Suit}"));
				return;
			}

			DrawCard();
		}
	}

	public int GetDamageDealt()
	{
		var enemyStrength = castleService.CurrentEnemy.Strength;
		var shields = TotalShields();
		return shields > enemyStrength ? 0 : enemyStrength - shields;
	}

	public int GetArms()
	{
		double fullTavern = standardService.StandardDeck.Cards.Count - 12;
		return (int)((double)TavernDeck.Cards.Count / fullTavern * 15);
	}
}

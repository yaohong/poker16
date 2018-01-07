using System;
using System.Collections;
using System.Collections.Generic;

public enum CardColor
{
	COLOR_FANGKUAI = 1,
	COLOR_MEIHUA = 2,
	COLOR_HONGTAO = 3,
	COLOR_HEITAO = 4,
	COLOR_DA = 5,
}

public struct PlayerCard
{
	public PlayerCard(ushort sd)
	{
		serverData = sd;
		color = CardColor.COLOR_DA;
		value = 3;
	}

	public PlayerCard(CardColor c, ushort v)
	{
		color = c;
		value = v;
		serverData = 1;
	}

	public ushort serverData;
	public CardColor color;
	public ushort value;
}


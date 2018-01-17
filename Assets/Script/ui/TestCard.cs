using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TestCard : MonoBehaviour {

	public PlayerCardList pcl;
	public PlayerCardList tcc;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private System.Random random = new System.Random ();

	public void AddTestCard()
	{
		List<PlayerCard> cards = new List<PlayerCard>();
		for (int i = 0; i < 10; ++i) 
		{
			PlayerCard p = new PlayerCard ();
			p.color = (CardColor) random.Next (1, 4);
			p.value = (ushort)random.Next (3, 15);
			cards.Add (p);
		}

		pcl.ShowCardList (cards);
	}


	public void TheCard()
	{
		List<PlayerCard> card = pcl.PickSelectCards ();
		if (card.Count > 0) 
		{
			tcc.ShowCardList (card);
		}
	}
}

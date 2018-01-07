using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TestCard : MonoBehaviour {

	public PlayerCardList pcl;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private System.Random random = new System.Random ();

	public void AddTestCard()
	{
		PlayerCards pc = new PlayerCards ();

		for (int i = 0; i < 10; ++i) 
		{
			PlayerCard p = new PlayerCard ();
			p.color = (CardColor) random.Next (1, 4);
			p.value = (ushort)random.Next (3, 15);
			pc.cards.Add (p);
		}

		pcl.ShowCardList (pc);
	}


	public void TheCard()
	{
		List<PlayerCard> card = pcl.PickSelectCards ();
	}
}

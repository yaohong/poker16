using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCardList : MonoBehaviour 
{
	public GameObject cardTemplate;
	public List<GameObject> allCardObjects = new List<GameObject> ();

	void Start ()
	{

	}

	void ClearOldCard()
	{
		for (int i = 0; i < allCardObjects.Count; ++i) 
		{
			GameObject.Destroy (allCardObjects [i]);
		}
		allCardObjects.Clear ();
	}

	public List<PlayerCard> PickSelectCards()
	{
		List<PlayerCard> theCardlist = new List<PlayerCard> ();
		List<PlayerCard> overCardlist = new List<PlayerCard> ();
		for (int i = 0; i < allCardObjects.Count; ++i) 
		{
			PokerControl p = allCardObjects[i].GetComponent<PokerControl> ();
			if (p.isSelected) {
				theCardlist.Add (p.GetCard ());
			}
			else
			{
				overCardlist.Add (p.GetCard());
			}
		}

		ShowCardList (overCardlist);
		return theCardlist;
	}

	public void ShowCardList(List<PlayerCard> playcards)
	{
		ClearOldCard ();

		Vector3 pos = Vector3.zero;
		for (int i = 0; i < playcards.Count; ++i) 
		{
			GameObject childCard = GameObject.Instantiate (cardTemplate);
			childCard.transform.parent = gameObject.transform;
			childCard.transform.localPosition = pos;
			childCard.transform.localScale = Vector3.one;

			allCardObjects.Add (childCard);

			PokerControl p = childCard.GetComponent<PokerControl> ();
			p.SetCard (playcards [i]);
			p.SetDepth (i);

			pos.x = pos.x + 25;
		}
	}
}

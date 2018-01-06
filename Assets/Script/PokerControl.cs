using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


class PokerControl: MonoBehaviour
{
    private bool selected = false;
    private Vector3 unselectedPosition  = Vector3.zero;
    private Vector3 selectedPosition = Vector3.zero;
    
	public UISprite cardPicture;
    public UIWidget selectCardObj;
	private PlayerCard currentCard;

    public bool isSelected
    {
        get
        {
            return selected;
        }
    }

    void Start()
    {
        unselectedPosition = this.transform.localPosition;
		selectedPosition = unselectedPosition;
		selectedPosition.y = selectedPosition.y + 10;
    }

    public void SwitchSelectState()
    {
        selected = !selected;
		RefreshSelectState();
    }

    private void RefreshSelectState()
    {
        if (selected)
        {
			transform.localPosition = selectedPosition;
        }
        else
        {
			transform.localPosition = unselectedPosition;
        }
    }

	private void RefreshCardPicture()
	{
		string spriteName = string.Format ("{0}_{1}", (int)currentCard.color, (int)currentCard.value);
		cardPicture.spriteName = spriteName;
	}

	public void SetDepth(int depth)
	{
		cardPicture.depth = depth;
	}

	public void SetCard(PlayerCard card)
	{
		currentCard = card;
		RefreshCardPicture ();
	}

	public PlayerCard GetCard()
	{
		return currentCard;
	}
}

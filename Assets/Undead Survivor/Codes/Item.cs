using Assets.Undead_Survivor.Codes;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
	public ItemData data;
	public int level;

	Image icon;
	Text textLevel;
	Text textName;
	Text textDesc;
	bool isOneTime;

	void Awake()
	{
		icon = GetComponentsInChildren<Image>()[1];
		icon.sprite = data.itemIcon;

		Text[] texts = GetComponentsInChildren<Text>();
		textLevel = texts[0];
		textName = texts[1];
		textDesc = texts[2];
		textName.text = data.itemName;
		isOneTime = (data.options.Length == 0);
	}

	void OnEnable()
	{
		textLevel.text = "Lv." + (level + 1);

		if (isOneTime)
		{
			textDesc.text = string.Format(data.itemDesc);
		}
		else
		{
			textDesc.text = string.Format(data.itemDesc, data.options[level]);
		}
	}


	public void OnClick()
	{
		if (!isOneTime)
		{
			if (data.isUpgrade)
			{
				if (GameManager.upgrades.ContainsKey(data.itemId))
				{
					GameManager.upgrades[data.itemId] = data.options[level];
				}
				else
				{
					GameManager.upgrades.Add(data.itemId, data.options[level]);
				}
			}
			else
			{
				switch (data.itemId)
				{
					case 1:
						GameManager.Instance.player.speed = 6 * (1 + data.options[level] / 100);
						break;
				}
			}
			level++;
		}
		else
		{
			switch (data.itemId)
			{
				case 3:
                    AudioManager.instance.PlaySfx(AudioManager.Sfx.Bomb);
                    Collider2D[] hits = Physics2D.OverlapCircleAll(GameManager.Instance.player.transform.position, 5.0f);
					foreach (var hit in hits)
					{
						IObjectDameged target = hit.GetComponent<IObjectDameged>();
						if (target != null)
						{
							target.Dameged(999999);
						}
					}
					break;
				case 4:
					GameManager.Instance.health = GameManager.Instance.maxHealth;
					break;
			}
		}

		if (!isOneTime && data.options.Length == level)
		{
			GetComponent<Button>().interactable = false;
		}
	}
}

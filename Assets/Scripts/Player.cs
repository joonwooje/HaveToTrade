using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Player : Singleton<Player>
{
    [SerializeField] ItemData dItems;
    public List<Item> playerItems;
    public int money;
    public Image pImage;
    public TMP_Text priceText;
    
    /*
     * ���� �ڵ��� ������ : � ��Ȳ���� UI�� �������� �����Ұ��ΰ�
     * ���� �۵� ��� : item�� ���� -> money�� �����ϸ� �ٷ� ���� -> UI���� -> �� ���� �� ����Ʈ����
     */
    public void SetUI(Item item)
    {
        pImage.sprite = item.image;
        priceText.text = "" + item.price;
    }
    public void Buy()
    {
        Item item = dItems.items[Random.Range(0, dItems.items.Length)];
        if(money < item.price)
        {
            //�Ұ��� UI
            return;
        }
        SetUI(item);
        money -= item.price;
        for(int i = 0; i < playerItems.Count; i++)
        {
            if (playerItems[i].stuffName == item.stuffName)
            {
                playerItems[i].counts += 1;
                return;
            }
        }
        playerItems.Add(item);
        return;
    }
    public void Sell()
    {
       
    }

}

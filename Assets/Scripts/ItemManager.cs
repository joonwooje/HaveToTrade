using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemManager : Singleton<ItemManager>
{
    [Header("�����۵�����")]
    [SerializeField] ItemSO itemSO;
    [Header("�÷��̾� ������")]
    public List<pItem> playerItems;
    [Header("������ ��ȯ�� ����� ����Ʈ(UI����)")]
    public List<int> productIndex; // �����ε��� �������
    public List<int> itemCountIndex;
    public List<TMP_Text> productTexts;
    public List<Image> productImages;

    // ���� �������
    private int totalPrice = 0;
    private List<int> randIndex = new List<int>();

    private void Start()
    {
        SetupItems();
    }
    void SetupItems()
    {
        playerItems = new List<pItem>();
        // ScriptableObject�� �����͸� �������� Item ������ ����
        for (int i = 0; i < itemSO.items.Length; i++)
        {
            // ItemData�κ��� ���ο� Item�� ����
            pItem newItem = new pItem(itemSO.items[i]);

            // ����Ʈ�� �߰�
            playerItems.Add(newItem);
        }
    }

    public void RandomSetItem(int sortcount)
    {
        for (int i = 0; i < sortcount; i++)
        {
            int randnum;
            // ���ο� ���� ��ȣ�� ã�� ������ �ݺ�
            do
            {
                randnum = Random.Range(0, playerItems.Count);
            }
            while (randIndex.Contains(randnum)); // �̹� ���� ��ȣ��� �ٽ� �̱�
            randIndex.Add(randnum);
            productIndex.Add(randnum);
        }
        randIndex.Clear();
    }
    public void SetUI()
    {
        for(int i = 0; i <productIndex.Count; i++)
        {
            int randCount = Random.Range(1, 3);
            productImages[i].sprite = playerItems[productIndex[i]].image;
            itemCountIndex.Add(randCount);
            productTexts[i].text = "" + itemCountIndex[i];
        }
    }
    public void BuyProduct()
    {
        for(int i = 0; i < productIndex.Count; i++)
        {
            totalPrice += playerItems[productIndex[i]].price * itemCountIndex[i];
        }
        if(Player.Instance.money < totalPrice)
        {
            //�Ұ��� �ۼ�
            return;
        }
        Player.Instance.money -= totalPrice;
        for(int i = 0; i < productIndex.Count; i++)
        {
            playerItems[productIndex[i]].counts += itemCountIndex[i]; 
        }
        ListClear();
    }
    public void ListClear()
    {
        productIndex.Clear();
        itemCountIndex.Clear();
    }

}

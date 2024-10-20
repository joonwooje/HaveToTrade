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
    public List<int> itemCountIndex;// ������ ���� ���� ����
    public TMP_Text productTexts; // ��ǰ ���� �ؽ�Ʈ
    public Image productImages; // ��ǰ �̹��� ����Ʈ

    // ���� �������
    
    public int productCount = 0;//UI ������� �����
    private List<int> randIndex = new List<int>();// �ٸ� ������ �������� �̱����� ����Ʈ

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

    public void RandomSetItem(int sortcount)//Customer��ũ��Ʈ���� sortCount���޾ƿ�
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
            randIndex.Add(randnum); // ������ȣ ����
            productIndex.Add(randnum); //��ǰ �߰�, ����Ʈ�� ��ȣ�� ������ ��(���� ��������..�Ƹ�)
        }
        randIndex.Clear();//������ȣ ����
    }
    public void SetUI()
    {
     int randCount = Random.Range(1, 4);//��ǰ�� ������ �󸶳� �ŷ��Ұ��� �������� ����
     itemCountIndex.Add(randCount); //� ����� �߰�
     productImages.sprite = playerItems[productIndex[productCount]].image; //��ǰ�� �̹���, ���� �ִ� 3���μ���
     productTexts.text = "" + itemCountIndex[productCount];//���� �ؽ�Ʈ�� �ݿ�
    }
    public void BuyProduct() //���Ž�
    {
     
        if(Player.Instance.money < playerItems[productIndex[productCount]].price)
        {
            //�Ұ��� �ۼ�����
            return;
        }
        Player.Instance.money -= playerItems[productIndex[productCount]].price; // ���� ��� ����
        playerItems[productIndex[productCount]].counts += itemCountIndex[productCount]; // �Ȱ��� productIndex�� ������ ���� �� ���� ���� 
        productCount++;
        
    }
    public void ListClear()//����Ʈ ���ſ�
    {
        productIndex.Clear();
        itemCountIndex.Clear();
        productCount = 0;
    }

}

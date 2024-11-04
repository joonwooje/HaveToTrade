using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemManager : Singleton<ItemManager>
{
    [Header("�����۵�����")]
    [SerializeField] ItemSO itemSO;
    [Header("�÷��̾� ������ ������")]
    public List<pItem> playerItems;
    [Header("ǰ�� ���� ����")]
    public int itemCountLimit;// ǰ�� ���� ����
    [Header("������ ��ȯ�� ����� ����Ʈ(UI����)")]
    public List<int> productIndex; // �����ε��� �������
    public List<int> itemCountIndex;// ������ ���� ���� ����
    public TMP_Text productTexts; // ��ǰ ���� �ؽ�Ʈ
    public Image productImages; // ��ǰ �̹��� ����Ʈ
    

    // ���� �������
    public int productCount = 0;//��ǰ ���� �з�
    private List<int> randIndex = new List<int>();// �ٸ� ������ �������� �̱����� ����Ʈ

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
    private void Start()
    {
        
    }
    #region �ŷ��� ������ ����
    public void RandomSetItem(int sortcount)//Customer��ũ��Ʈ���� sortCount���޾ƿ�
    {
        for (int i = 0; i < sortcount; i++)
        {
            int randnum;
            // ���ο� ���� ��ȣ�� ã�� ������ �ݺ�
            do
            {
                randnum = Random.Range(0, itemSO.items.Length);
            }
            while (randIndex.Contains(randnum)); // �̹� ���� ��ȣ��� �ٽ� �̱�
            randIndex.Add(randnum); // ������ȣ ����
            productIndex.Add(randnum); //��ǰ �߰�, ����Ʈ�� ��ȣ�� ������ ��(���� ��������..�Ƹ�)
        }
        randIndex.Clear();//������ȣ ����
    }
    public void RandomSetItemSell(int sortcount)//�ǸŽ� ���� ������ ����
    {
        if(sortcount > playerItems.Count)//���� �޾ƿ� ������ �÷��̾� �����ۿ� �ִ� �������� ���� ���
            sortcount = playerItems.Count;
        for(int i = 0; i< sortcount; i++)
        {
            int randnum;
            // ���ο� ���� ��ȣ�� ã�� ������ �ݺ�
            do
            {
                randnum = Random.Range(0, playerItems.Count);
            }
            while (randIndex.Contains(randnum) || playerItems[randnum].counts == 0); // �̹� ���� ��ȣ Ȥ�� �÷��̾ ������ ������ ���� ���� ��� �ٽû̱�
            randIndex.Add(randnum); // ������ȣ ����
            productIndex.Add(randnum); //��ǰ �߰�, ����Ʈ�� ��ȣ�� ������ ��(���� ��������..�Ƹ�)
        }
        randIndex.Clear();//������ȣ ����
    }
    #endregion

    #region �ŷ� UI����
    public void SetUI()//���Ž� UI
    {
     int randCount = Random.Range(1, itemCountLimit+1);//��ǰ�� ������ �󸶳� �ŷ��Ұ��� �������� ����
     itemCountIndex.Add(randCount); //� ����� �߰�
     productImages.sprite = itemSO.items[productIndex[productCount]].image; //��ǰ�� �̹���
     productTexts.text = "" + itemCountIndex[productCount];//���� �ؽ�Ʈ�� �ݿ�
    }
    public void SetSellUI()//�ǸŽ� UI
    {
        int randCount;
        if(productIndex[productCount] >= playerItems.Count)
            productIndex[productCount] = Random.Range(0, playerItems.Count);
        int currentProductIndex = productIndex[productCount];
        if(playerItems[currentProductIndex].counts > itemCountLimit)
        {
            randCount = Random.Range(1,itemCountLimit+1);
        }
        else
        {
            randCount = Random.Range(1, playerItems[currentProductIndex].counts + 1);
        }
        itemCountIndex.Add(randCount); //� �Ȱ��� �߰�
        productImages.sprite = playerItems[currentProductIndex].image;
        productTexts.text = "" + itemCountIndex[productCount];//���� �ؽ�Ʈ�� �ݿ�
    }
    #endregion


    #region ���Ź� �Ǹ� ���� ����
    public void BuyProduct() //���Ž�
    {
        int currentProductIndex = productIndex[productCount];
        int currentPrice = itemSO.items[currentProductIndex].price * itemCountIndex[productCount];
        if (Player.Instance.money < currentPrice)
        {
            //�Ұ��� �ۼ�����
            return;
        }
        Player.Instance.money -= currentPrice; // ���� ��� ����

        pItem newItem = new pItem(itemSO.items[currentProductIndex]);

        // playerItems ����Ʈ�� ���� �������� �ִ��� Ȯ��
        pItem existingItem = playerItems.Find(item => item.stuffName == newItem.stuffName);

        if (existingItem != null)
        {
            // ���� �������� �̹� �ִٸ� ������ ����
            existingItem.counts += itemCountIndex[productCount];
        }
        else
        {
            // ���ο� �������̶�� ����Ʈ�� �߰�
            newItem.counts = itemCountIndex[productCount];
            playerItems.Add(newItem);
        }
        productCount++;
    }
    public void SellProduct()//��ǰ �Ǹ�
    {
        int currentProductIndex = productIndex[productCount];
        int currentPrice = playerItems[currentProductIndex].price * itemCountIndex[productCount];
        Player.Instance.money += currentPrice;
        playerItems[currentProductIndex].counts -= itemCountIndex[productCount];
        if(playerItems[currentProductIndex].counts <= 0)
            playerItems.RemoveAt(currentProductIndex);
        productCount++;
    }

    #endregion
    public void ListClear()//����Ʈ ���ſ�
    {
        productIndex.Clear();
        itemCountIndex.Clear();
        productCount = 0;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditorInternal.Profiling.Memory.Experimental;

public class ItemManager : Singleton<ItemManager>
{
    [Header("�����۵�����")]
    [SerializeField] ItemSO itemSO;
    [Header("�÷��̾� ������ ������")]
    //public List<pItem> playerItems;
    public InventoryContainer playerInventory;
    public InventoryUI inventoryUI;
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
        playerInventory.InitWeight();
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
        if(sortcount > playerInventory.inventory.Count)//���� �޾ƿ� ������ �÷��̾� �����ۿ� �ִ� �������� ���� ���
            sortcount = playerInventory.inventory.Count;
        for(int i = 0; i< sortcount; i++)
        {
            int randnum;
            // ���ο� ���� ��ȣ�� ã�� ������ �ݺ�
            do
            {
                randnum = Random.Range(0, playerInventory.inventory.Count);
            }
            while (randIndex.Contains(randnum) || playerInventory.inventory[randnum].counts == 0); // �̹� ���� ��ȣ Ȥ�� �÷��̾ ������ ������ ���� ���� ��� �ٽû̱�
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
        if(productIndex[productCount] >= playerInventory.inventory.Count)
            productIndex[productCount] = Random.Range(0, playerInventory.inventory.Count);
        int currentProductIndex = productIndex[productCount];
        if(playerInventory.inventory[currentProductIndex].counts > itemCountLimit)
        {
            randCount = Random.Range(1,itemCountLimit+1);
        }
        else
        {
            randCount = Random.Range(1, playerInventory.inventory[currentProductIndex].counts + 1);
        }
        itemCountIndex.Add(randCount); //� �Ȱ��� �߰�
        productImages.sprite = playerInventory.inventory[currentProductIndex].image;
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

        #region ���� ��� �κ�
        float itemTotalWeight = itemSO.items[currentProductIndex].weight * itemCountIndex[productCount];

        playerInventory.sortWeight[itemSO.items[currentProductIndex].sort].CurrentWeight += itemTotalWeight;
        if(playerInventory.sortWeight[itemSO.items[currentProductIndex].sort].CurrentWeight > playerInventory.sortWeight[itemSO.items[currentProductIndex].sort].MaxWeight)
        {
            float over = playerInventory.sortWeight[itemSO.items[currentProductIndex].sort].CurrentWeight - playerInventory.sortWeight[itemSO.items[currentProductIndex].sort].MaxWeight;
            over = Mathf.Min(over, itemTotalWeight);

            if (playerInventory.PublicCurrentWeight + over > playerInventory.PublicMaxWeight)
            {
                playerInventory.sortWeight[itemSO.items[currentProductIndex].sort].CurrentWeight -= itemTotalWeight;
                Debug.LogWarning("Cannot add item");
                return;
            }
            playerInventory.PublicCurrentWeight += over;
        }
        #endregion

        Player.Instance.money -= currentPrice; // ���� ��� ����

        pItem newItem = new pItem(itemSO.items[currentProductIndex]);

        // playerItems ����Ʈ�� ���� �������� �ִ��� Ȯ��
        pItem existingItem = playerInventory.inventory.Find(item => item.stuffName == newItem.stuffName);

        if (existingItem != null)
        {
            // ���� �������� �̹� �ִٸ� ������ ����
            existingItem.counts += itemCountIndex[productCount];
            inventoryUI.InitUI(true);
        }
        else
        {
            // ���ο� �������̶�� ����Ʈ�� �߰�
            newItem.counts = itemCountIndex[productCount];
            playerInventory.inventory.Add(newItem);
            inventoryUI.GenerateSlot();
            inventoryUI.InitUI(true);
        }
        productCount++;
    }
    public void SellProduct()//��ǰ �Ǹ�
    {
        int currentProductIndex = productIndex[productCount];
        pItem itemToRemove = playerInventory.inventory[currentProductIndex];
        int currentPrice = itemToRemove.price * itemCountIndex[productCount];

        Player.Instance.money += currentPrice;
        playerInventory.inventory[currentProductIndex].counts -= itemCountIndex[productCount];
        if (playerInventory.inventory[currentProductIndex].counts <= 0)
        {
            playerInventory.inventory.RemoveAt(currentProductIndex);
            inventoryUI.DeleteSlot(currentProductIndex);
        }

        inventoryUI.InitUI(false);

        #region ���� ��� �κ�
        float itemTotalWeight = itemToRemove.weight * itemCountIndex[productCount];
        if (playerInventory.sortWeight[itemToRemove.sort].CurrentWeight <= playerInventory.sortWeight[itemToRemove.sort].MaxWeight)
        {
            playerInventory.sortWeight[itemToRemove.sort].CurrentWeight -= itemTotalWeight;
        }
        else
        {
            if(playerInventory.sortWeight[itemToRemove.sort].CurrentWeight - itemTotalWeight >= playerInventory.sortWeight[itemToRemove.sort].MaxWeight)
            {
                playerInventory.sortWeight[itemToRemove.sort].CurrentWeight -= itemTotalWeight;
                playerInventory.PublicCurrentWeight -= itemTotalWeight;
            }
            else
            {
                playerInventory.sortWeight[itemToRemove.sort].CurrentWeight -= itemTotalWeight;
                float margin = playerInventory.sortWeight[itemToRemove.sort].MaxWeight - playerInventory.sortWeight[itemToRemove.sort].CurrentWeight;
                float over = itemTotalWeight - margin;
                playerInventory.PublicCurrentWeight -= over;
            }
        }

        #endregion
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

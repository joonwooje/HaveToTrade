using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


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
    
    
    

    // ���� �������
    public int productCount = 0;//��ǰ ���� �з�
    private List<int> randIndex = new List<int>();// �ٸ� ������ �������� �̱����� ����Ʈ
    private int bargainPrice = 0; //������ ����
    public bool bargainSuccess = false;
    public int currentProductIndex = 0;
    public pItem buyItem;
   private static ItemManager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
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
     if (sortcount > playerItems.Count)//���� �޾ƿ� ������ �÷��̾� �����ۿ� �ִ� �������� ���� ���
     sortcount = playerItems.Count;
     for (int i = 0; i < sortcount; i++)
     {
        int randnum;
                // ���ο� ���� ��ȣ�� ã�� ������ �ݺ�
        do
        {
           randnum = Random.Range(0, playerItems.Count);
        }
        while (randIndex.Contains(randnum)); // �̹� ���� ��ȣ Ȥ�� �÷��̾ ������ ������ ���� ���� ��� �ٽû̱�
        randIndex.Add(randnum); // ������ȣ ����
        productIndex.Add(randnum); //��ǰ �߰�, ����Ʈ�� ��ȣ�� ������ ��(���� ��������..�Ƹ�)
     }
     randIndex.Clear();//������ȣ ����
     
    }
#endregion
    
    #region �ŷ� UI����
    public void SetUI()//���Ž� UI
    {
        currentProductIndex = productIndex[productCount];
        int randCount = Random.Range(1, itemCountLimit+1);//��ǰ�� ������ �󸶳� �ŷ��Ұ��� �������� ����
        PutInfo(randCount);
    }
    public void SetSellUI()//�ǸŽ� UI
    {
        if (productIndex[productCount] >= playerItems.Count)
            productIndex[productCount] = Random.Range(0, playerItems.Count);
        currentProductIndex = productIndex[productCount];
        int randCount;
        if (playerItems[currentProductIndex].counts > itemCountLimit)
        {
            randCount = Random.Range(1,itemCountLimit+1);
        }
        else
        {
            randCount = Random.Range(1, playerItems[currentProductIndex].counts + 1);
        }
        PutInfo(randCount);
    }
    #endregion
    #region ���Ź� �Ǹ� ���� ����
    public void BuyProduct() //���Ž�
    {
        int currentPrice;

        if (bargainSuccess == true)
            currentPrice = bargainPrice * itemCountIndex[productCount];
        else
           currentPrice = itemSO.items[currentProductIndex].price * itemCountIndex[productCount];

        if (Player.Instance.money < currentPrice)
        {
            productCount++;
            BargainClear();
            //�Ұ��� �ۼ�����
            return;
        }
        Player.Instance.money -= currentPrice; // ���� ��� ����


        // playerItems ����Ʈ�� ���� �������� �ִ��� Ȯ��
        pItem existingItem = playerItems.Find(item => item.stuffName == buyItem.stuffName);

        if (existingItem != null)
        {
            // ���� �������� �̹� �ִٸ� ������ ����
            existingItem.counts += itemCountIndex[productCount];
        }
        else
        {
            // ���ο� �������̶�� ����Ʈ�� �߰�
            buyItem.counts = itemCountIndex[productCount];
            playerItems.Add(buyItem);
        }
        productCount++;
        buyItem = null;
        BargainClear();
    }
    public void SellProduct()//��ǰ �Ǹ�
    {
        int currentPrice; 
        if(bargainSuccess == true)
            currentPrice = bargainPrice * itemCountIndex[productCount];
        else
            currentPrice = playerItems[currentProductIndex].price * itemCountIndex[productCount];
        Player.Instance.money += currentPrice;
        playerItems[currentProductIndex].counts -= itemCountIndex[productCount];
        if(playerItems[currentProductIndex].counts <= 0)
            playerItems.RemoveAt(currentProductIndex);
        productCount++;
        BargainClear();
    }

    #endregion
    #region ���� ����
    public void SetBargainPrice(float initialChance,int bargainValue,int bargainPoint, float bargainPercent)// �ʱ�Ȯ��(%),�������ð���, ��������, ���� ������ %����
    {
        float randomValue = Random.Range(0f, 100f);
        int diff; //���̰��
        int chancePoint; // ������Ȯ���� �󸶸� ���Ұ���
        float totalChance; // ����Ȯ��
        if (Customer.buyOrSell== true)//�����϶�
        {
            diff = itemSO.items[currentProductIndex].price - bargainValue;//�޾ƿ� �������� �����۰� ���̸� ��� ��
        }
        else//�Ǹ��϶�
        {
           if(bargainValue < 0)//�����ȶ��� 0������ ����...
                bargainValue = 0;
            diff = bargainValue - playerItems[currentProductIndex].price; // �Ǹ�, �������� �� ���ƾ� �ϹǷ� ������ ���
        }
        chancePoint = diff / bargainPoint; //���̿� ������ ����
        totalChance = initialChance - (chancePoint * bargainPercent); //�ʱ�Ȯ���� ���� Ȯ�������� ����������%������ ���� ���, ���� ���� Ȯ��

        if (totalChance >= randomValue)//��� Ȯ���� random���� ���� ������ ������ ����, ex) Ȯ���� 30�̸� random������ 31�� �������� ����, �������� 29�̸� ����
        {
            bargainPrice = bargainValue;
            Customer.costText.text = "price : " + bargainPrice;
            bargainSuccess = true;
        }
        else
        {
            bargainSuccess = false;
        }
    }
    #endregion
    #region �ʱ�ȭ ���� �� �ڵ� �����
    public void ListClear()//����Ʈ ���ſ�
    {
        productIndex.Clear();
        itemCountIndex.Clear();
        productCount = 0;
    }
    public void BargainClear()//�������� �ʱ�ȭ��
    {
        bargainPrice = 0;
        bargainSuccess = false;
    }
    public void PutInfo(int randCount)//SetUI�������� �ߺ��Ǵºκ� �ڵ����̱�
    {
        itemCountIndex.Add(randCount); //� ����� �߰�
        if (Customer.buyOrSell == true)
        {
            buyItem = new pItem (itemSO.items[currentProductIndex]);
            pItem countItem = playerItems.Find(item => item.stuffName == buyItem.stuffName); // ������ϴ� ������ ã��
            if (countItem != null)//���� �ִٸ�
            {
                Customer.playerCountTexts.text = "" + countItem.counts;
            }
            else //���ٸ�
            {
                Customer.playerCountTexts.text = "" + 0; //�÷��̾ ����ִ� �����ؽ�Ʈ �� 0 ����
            }
            Customer.productImages.sprite = itemSO.items[currentProductIndex].image; //��ǰ�� �̹���
            Customer.costText.text = "price : " + itemSO.items[currentProductIndex].price;//; ���� �ؽ�Ʈ�� �ݿ�
        }
        else
        {
            Customer.playerCountTexts.text = "" + playerItems[currentProductIndex].counts;
            Customer.productImages.sprite = playerItems[currentProductIndex].image;
            Customer.costText.text = "price : " + playerItems[currentProductIndex].price;
        }
        CusProductCountSet();
        Customer.productTexts.text = "" + itemCountIndex[productCount];//���� �ؽ�Ʈ�� �ݿ�
    }
    #endregion
    private void CusProductCountSet()
    {
        switch (Customer.randcusnum)
        {
            case 1:
                if (buyItem.sort == ItemSorts.food && Customer.buyOrSell == true)
                {
                    itemCountIndex[productCount] = itemCountIndex[productCount] + 5;
                }
               // else if (playerItems == null || playerItems.Count == 0)
                  //  return;
                else if (Customer.buyOrSell == false && playerItems[currentProductIndex].sort == ItemSorts.food)//��������. ��¥�� buyorsell�� �Ǹŷ� �������̻� playerItem���� ������ �������� �ϳ��� �����Ƿ�.
                {
                    itemCountIndex[productCount] = itemCountIndex[productCount] + 5;
                    if (itemCountIndex[productCount] >= playerItems[currentProductIndex].counts)
                        itemCountIndex[productCount] = playerItems[currentProductIndex].counts;
                }
                break;
        }
    }
}

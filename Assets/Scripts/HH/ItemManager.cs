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
    private int currentProductIndex = 0;

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
        currentProductIndex = productIndex[productCount];
        int randCount = Random.Range(1, itemCountLimit+1);//��ǰ�� ������ �󸶳� �ŷ��Ұ��� �������� ����
        PutInfo(randCount);
    }
    public void SetSellUI()//�ǸŽ� UI
    {
        currentProductIndex = productIndex[productCount];
        int randCount;
        if(productIndex[productCount] >= playerItems.Count)
            productIndex[productCount] = Random.Range(0, playerItems.Count);
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
        int currentPrice = playerItems[currentProductIndex].price * itemCountIndex[productCount];
        Player.Instance.money += currentPrice;
        playerItems[currentProductIndex].counts -= itemCountIndex[productCount];
        if(playerItems[currentProductIndex].counts <= 0)
            playerItems.RemoveAt(currentProductIndex);
        productCount++;
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
            chancePoint = diff / bargainPoint; //���̿� ������ ����
            totalChance = initialChance -(chancePoint * bargainPercent); //�ʱ�Ȯ���� ���� Ȯ�������� ����������%������ ���� ���, ���� ���� Ȯ��
            
            if(totalChance >= randomValue)//��� Ȯ���� random���� ���� ������ ������ ����, ex) Ȯ���� 30�̸� random������ 31�� �������� ����, �������� 29�̸� ����
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
        else//�Ǹ��϶�
        {

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
        Customer.productImages.sprite = itemSO.items[productIndex[productCount]].image; //��ǰ�� �̹���
        Customer.productTexts.text = "" + itemCountIndex[productCount];//���� �ؽ�Ʈ�� �ݿ�
        Customer.costText.text = "price : " + itemSO.items[productIndex[productCount]].price;//; ���� �ؽ�Ʈ�� �ݿ�
    }
    #endregion

}

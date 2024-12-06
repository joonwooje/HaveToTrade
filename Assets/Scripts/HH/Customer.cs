using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * �ŷ� ���� �� ���� ��, �ŷ� ��ǰ�� ���� �� ���ڸ� ���� ����
 * �մ� �ൿ���� ��� : ����(Start) -> ���(Idle) -> ���� or �Ǹ� -> ����(End) -> ����, �մԼ� ��ŭ �ݺ�
 */
public class Customer : MonoBehaviour
{
    [Header("�մԼ�, ����� ���۽� ���� ����")]
    public int cusCount;
    [Header("�մ� ���� �ɼ�")]
    public float speed; //�մ� �̵��ӵ�
    public float tradeDelay; // �մ� �ŷ� ������
    public float rejectDelay; // �մ� ���� �� ���� �ŷ� ������
    public float fadeDuration; // ���̵� �ƿ� ���� �ð�
    [Header("��� ������ �ŷ��Ұ���")]
    public int tradeSortCount; // �մ��� ��� ������ �ŷ��Ұ���
    [Header("�մ� ���� ������, ���� �� ���� ��ġ ����")]
    public List<GameObject> customerPrefab;//�մ� ������
    public List<Transform> customerTransform;// ����, �ŷ���ġ, ����


    [Header("�մ� �ŷ�â")]
    public GameObject CustomerUI;
    public GameObject BuyUI;
    public GameObject SellUI;
    public Data<CustomerState> cState = new Data<CustomerState>();//���º� �̺�Ʈ

    [SerializeField] GameObject GoTownButton;
    private GameObject newCustomer;
    private bool buyOrSell;//���϶�����, �����϶��Ǹ�
    void Start()
    {
        cusCount = Random.Range(3, 6);
        Player.Instance.RenewMoney();
        cState.Value = CustomerState.Start;
    }
    private void Awake()//���º�ȭ ���� ����
    {
        cState.onChange += SetCustomer;
        cState.onChange += CustomerSetItem;
        cState.onChange += CustomerSetUI;
        cState.onChange += BuyItem;
        cState.onChange += SellItem;
        cState.onChange += RejectItem;
        cState.onChange += CustomerExit;
    }
    #region ��ư���� �ൿ���� ��ȭ
    public void CustomerBuy()
    {
        cState.Value = CustomerState.Buy;
    }
    public void CustomerSell()
    {
        cState.Value = CustomerState.Sell;
    }
    public void CustomerReject()
    {
        cState.Value = CustomerState.Reject;
    }
    #endregion
    #region �մ� �ൿ����
    private void SetCustomer(CustomerState _cState)
    {
        if(_cState == CustomerState.Start)//�մ� ��ü ������ �̵�
        {
            int randnum = Random.Range(0,customerPrefab.Count);
            newCustomer = Instantiate(customerPrefab[randnum], customerTransform[0]);
            StartCoroutine(MoveCustomerToPosition(newCustomer, customerTransform[1].position));
        }
    }
    private void CustomerSetItem(CustomerState _cState)
    {
        if(_cState == CustomerState.ItemSet)//�մԴ��� �÷��̾�� ����/�Ǹ� �� ������ ���� 
        {
            int randsort = Random.Range(1, tradeSortCount+1);
            BuyOrSell();//���� or �Ǹ� �������� ������
            if (buyOrSell == true)
                ItemManager.Instance.RandomSetItem(randsort);
            else
                ItemManager.Instance.RandomSetItemSell(randsort);
            cState.Value = CustomerState.SetUI;
        }
    }
    private void CustomerSetUI(CustomerState _cState)
    {
        if(_cState == CustomerState.SetUI)//UI�� ǥ�� �� ItemManager�� productCount�� ���� �Ǵ�
        {
            if (buyOrSell == true)
            {
                ItemManager.Instance.SetUI();
                BuyUI.SetActive(true);
                CustomerUI.SetActive(true);
            }
            else
            {
                ItemManager.Instance.SetSellUI();
                SellUI.SetActive(true);
                CustomerUI.SetActive(true);
            }

        }
    }
    private void BuyItem(CustomerState _cState)
    {
        if(_cState == CustomerState.Buy)//����
        {
            StartCoroutine(DelayBuy());
        }
    }
    private void SellItem(CustomerState _cState)
    {
        if(_cState == CustomerState.Sell)//�Ǹ�
        {
            StartCoroutine(DelaySell());
        }
    }
    private void RejectItem(CustomerState _cState)
    {
        if(_cState == CustomerState.Reject)//����
        {
            StartCoroutine(DelayReject());
        }
    }
    private void CustomerExit(CustomerState _cState)
    {
        if(_cState == CustomerState.End)//����
        {
            StartCoroutine(MoveAndFadeOutCustomer(newCustomer, customerTransform[2].position, fadeDuration)); //���� �� ���̵� �ƿ�
            cusCount--;
            StartCoroutine(TradeEnd());
        }
    }
    #endregion
    #region �մ� ���
    public void BuyOrSell()//���� Ȥ�� �Ǹ� ��ȣ(����)
    {
        if (ItemManager.Instance.playerInventory.inventory.Count == 0)
        {
            buyOrSell = true;
            return;
        }
        if (Random.value > 0.5)
            buyOrSell = true;
        else
            buyOrSell = false;

    }
    IEnumerator MoveCustomerToPosition(GameObject customer, Vector3 targetPosition)//�մ� ����
    {
        while (customer.transform.position != targetPosition)
        {
            customer.transform.position = Vector2.MoveTowards(customer.transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
        cState.Value = CustomerState.ItemSet;
    }
    IEnumerator MoveAndFadeOutCustomer(GameObject customer, Vector3 targetPosition, float duration)//�մ�����
    {
        SpriteRenderer spriteRenderer = customer.GetComponent<SpriteRenderer>();
        Color originalColor = spriteRenderer.color;
        float elapsed = 0f;

        Vector3 startPosition = customer.transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // �̵� ó��
            float t = elapsed / duration;
            customer.transform.position = Vector2.Lerp(startPosition, targetPosition, t);

            // ���̵� �ƿ� ó��
            float alpha = Mathf.Lerp(1f, 0f, t);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            yield return null;
        }

        // ������ ��ġ�� ���� ����
        customer.transform.position = targetPosition;
        spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        Destroy(customer); // ���̵� �ƿ� �� ������Ʈ ����
    }
    IEnumerator DelayBuy()//��������
    {
        ItemManager.Instance.BuyProduct();
        CustomerUI.SetActive(false);
        BuyUI.SetActive(false);
        Player.Instance.RenewMoney();
        yield return new WaitForSecondsRealtime(tradeDelay);
        if (ItemManager.Instance.productCount == ItemManager.Instance.productIndex.Count)
            cState.Value = CustomerState.End;
        else
        cState.Value = CustomerState.SetUI;
    }
    IEnumerator DelaySell()//�Ǹ�����
    {
        ItemManager.Instance.SellProduct();
        CustomerUI.SetActive(false);
        SellUI.SetActive(false);
        Player.Instance.RenewMoney();
        yield return new WaitForSecondsRealtime(tradeDelay);
        if (ItemManager.Instance.productCount == ItemManager.Instance.productIndex.Count)
            cState.Value = CustomerState.End;
        else
            cState.Value = CustomerState.SetUI;
    }
    IEnumerator TradeEnd()//�ŷ� ����
    {
        ItemManager.Instance.ListClear();
        yield return new WaitForSeconds(fadeDuration*1.5f);
        if (cusCount == 0)
        {
            cState.Value = CustomerState.Idle;
            GoTownButton.SetActive(true);
            //����� ���� UI�� ���̵�
        }
        else
        {
            cState.Value = CustomerState.Start;
        }
    }
    IEnumerator DelayReject()//���� ������
    {
        ItemManager.Instance.productCount++;//������ ������ǰ���� �ѱ�
        CustomerUI.SetActive(false);
        if(buyOrSell == true)
            BuyUI.SetActive(false);
        else
            SellUI.SetActive(false);
        yield return new WaitForSeconds(rejectDelay);
        if (ItemManager.Instance.productCount == ItemManager.Instance.productIndex.Count)
            cState.Value = CustomerState.End;
        else
            cState.Value = CustomerState.SetUI;
    }
    #endregion

}

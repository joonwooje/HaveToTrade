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
    public float buyDelay; // �մ� ���� ������
    public List<GameObject> customerPrefab;//�մ� ������
    public List<Transform> customerTransform;// ����, �ŷ���ġ, ����


    [Header("�մ� �ŷ�â")]
    public GameObject CustomerUI;
    public Data<CustomerState> cState = new Data<CustomerState>();//���º� �̺�Ʈ


    void Start()
    {
        cusCount = Random.Range(3, 5);
        cState.Value = CustomerState.Start;
    }
    private void Awake()
    {
        cState.onChange += SetCustomer;
        cState.onChange += CustomerSetItem;
        cState.onChange += CustomerSetUI;
        cState.onChange += BuyItem;
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
    #endregion
    #region �մ� �ൿ����
    private void SetCustomer(CustomerState _cState)
    {
        if(_cState == CustomerState.Start)//�մ� ��ü ������ �̵�
        {
            int randnum = Random.Range(0,customerPrefab.Count);
            GameObject newCustomer = Instantiate(customerPrefab[randnum], customerTransform[0]);
            StartCoroutine(MoveCustomerToPosition(newCustomer, customerTransform[1].position));
        }
    }
    private void CustomerSetItem(CustomerState _cState)
    {
        if(_cState == CustomerState.ItemSet)//�մԴ��� �÷��̾�� ����/�Ǹ� �� ������ ���� 
        {
            int randsort = Random.Range(1, 4);
            ItemManager.Instance.RandomSetItem(randsort);
            cState.Value = CustomerState.SetUI;
        }
    }
    private void CustomerSetUI(CustomerState _cState)
    {
        if(_cState == CustomerState.SetUI)//UI�� ǥ�� �� ItemManager�� productCount�� ���� �Ǵ�
        {
            if (ItemManager.Instance.productCount == ItemManager.Instance.productIndex.Count)
                cState.Value = CustomerState.End;
            ItemManager.Instance.SetUI();
            CustomerUI.SetActive(true);
        }
    }
    private void BuyItem(CustomerState _cState)
    {
        if(_cState == CustomerState.Buy)//����
        {
            StartCoroutine(DelayBuy());
            
        }
    }
    private void CustomerExit(CustomerState _cState)
    {
        if(_cState == CustomerState.End)
        {
            ItemManager.Instance.ListClear();
            //���̵�ƿ� �߰�
            //�̵��߰�
            cusCount--;
            TradeEnd();
        }
    }
    #endregion
    #region �մ� ���
    IEnumerator MoveCustomerToPosition(GameObject customer, Vector3 targetPosition)//�մ� �̵� ���
    {
        while (customer.transform.position != targetPosition)
        {
            customer.transform.position = Vector2.MoveTowards(customer.transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
        cState.Value = CustomerState.ItemSet;
    }
    IEnumerator DelayBuy()//��������
    {
        ItemManager.Instance.BuyProduct();
        CustomerUI.SetActive(false);
        yield return new WaitForSecondsRealtime(buyDelay);
        cState.Value = CustomerState.SetUI;
    }
    private void TradeEnd()
    {
        if(cusCount == 0)
        {
            cState.Value = CustomerState.Idle;
            //����� ���� UI�� ���̵�
        }
        else
        {
            cState.Value = CustomerState.Start;
        }
    }
    #endregion

}

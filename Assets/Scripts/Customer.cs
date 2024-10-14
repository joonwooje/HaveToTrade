using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 
 * �ŷ� ���� �� ���� ��, �ŷ� ��ǰ�� ���� �� ���ڸ� ���� ����
 * 
 */
public class Customer : MonoBehaviour
{
    public int cusCount; // �մ� ��
    public float speed; //�մ� �̵��ӵ�
    public List<GameObject> customerPrefab;//�մ� ������
    public List<Transform> customerTransform;// ����, �ŷ���ġ, ����
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
        cState.onChange += BuyItem;
    }
    public void CustomerBuy()
    {
        cState.Value = CustomerState.Buy;
    }
    public void CustomerSell()
    {
        cState.Value = CustomerState.Sell;
    }

    private void SetCustomer(CustomerState _cState)
    {
        if(_cState == CustomerState.Start)
        {
            int randnum = Random.Range(0,customerPrefab.Count);
            GameObject newCustomer = Instantiate(customerPrefab[randnum], customerTransform[0]);
            StartCoroutine(MoveCustomerToPosition(newCustomer, customerTransform[1].position));
        }
    }

    IEnumerator MoveCustomerToPosition(GameObject customer, Vector3 targetPosition)//�մ� �̵� ���
    {
        while (customer.transform.position != targetPosition)
        {
            customer.transform.position = Vector2.MoveTowards(customer.transform.position, targetPosition, speed * Time.deltaTime);
            yield return null; 
        }
        cState.Value = CustomerState.Idle; 
    }
    private void CustomerSetItem(CustomerState _cState)
    {
        if(_cState == CustomerState.Idle)
        {
            int randsort = Random.Range(1, 4);
            ItemManager.Instance.RandomSetItem(randsort);
            ItemManager.Instance.SetUI();
        }
    }
    private void BuyItem(CustomerState _cState)
    {
        if(_cState == CustomerState.Buy)
        {
            ItemManager.Instance.BuyProduct();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 거래 진입 시 고객의 수, 거래 물품의 종류 및 숫자를 정할 공간
 * 손님 행동패턴 요약 : 입장(Start) -> 대기(Idle) -> 구매 or 판매 -> 퇴장(End) -> 입장, 손님수 만큼 반복
 */
public class Customer : MonoBehaviour
{
    [Header("손님수, 현재는 시작시 랜덤 지정")]
    public int cusCount;
    [Header("손님 세부 옵션")]
    public float speed; //손님 이동속도
    public float tradeDelay; // 손님 거래 딜레이
    public float rejectDelay; // 손님 거절 후 다음 거래 딜레이
    public float fadeDuration; // 페이드 아웃 지속 시간
    [Header("몇개의 종류를 거래할건지")]
    public int tradeSortCount; // 손님이 몇개의 종류를 거래할건지
    [Header("손님 외형 프리팹, 생성 및 퇴장 위치 설정")]
    public List<GameObject> customerPrefab;//손님 프리팹
    public List<Transform> customerTransform;// 생성, 거래위치, 퇴장


    [Header("손님 거래창")]
    public GameObject CustomerUI;
    public GameObject BuyUI;
    public GameObject SellUI;
    public Data<CustomerState> cState = new Data<CustomerState>();//상태별 이벤트

    [SerializeField] GameObject GoTownButton;
    private GameObject newCustomer;
    private bool buyOrSell;//참일때구매, 거짓일때판매
    void Start()
    {
        cusCount = Random.Range(3, 6);
        Player.Instance.RenewMoney();
        cState.Value = CustomerState.Start;
    }
    private void Awake()//상태변화 구독 위주
    {
        cState.onChange += SetCustomer;
        cState.onChange += CustomerSetItem;
        cState.onChange += CustomerSetUI;
        cState.onChange += BuyItem;
        cState.onChange += SellItem;
        cState.onChange += RejectItem;
        cState.onChange += CustomerExit;
    }
    #region 버튼으로 행동패턴 변화
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
    #region 손님 행동패턴
    private void SetCustomer(CustomerState _cState)
    {
        if(_cState == CustomerState.Start)//손님 객체 생성후 이동
        {
            int randnum = Random.Range(0,customerPrefab.Count);
            newCustomer = Instantiate(customerPrefab[randnum], customerTransform[0]);
            StartCoroutine(MoveCustomerToPosition(newCustomer, customerTransform[1].position));
        }
    }
    private void CustomerSetItem(CustomerState _cState)
    {
        if(_cState == CustomerState.ItemSet)//손님대기시 플레이어에게 구매/판매 할 아이템 설정 
        {
            int randsort = Random.Range(1, tradeSortCount+1);
            BuyOrSell();//구매 or 판매 랜덤으로 돌리기
            if (buyOrSell == true)
                ItemManager.Instance.RandomSetItem(randsort);
            else
                ItemManager.Instance.RandomSetItemSell(randsort);
            cState.Value = CustomerState.SetUI;
        }
    }
    private void CustomerSetUI(CustomerState _cState)
    {
        if(_cState == CustomerState.SetUI)//UI로 표현 및 ItemManager의 productCount로 퇴장 판단
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
        if(_cState == CustomerState.Buy)//구매
        {
            StartCoroutine(DelayBuy());
        }
    }
    private void SellItem(CustomerState _cState)
    {
        if(_cState == CustomerState.Sell)//판매
        {
            StartCoroutine(DelaySell());
        }
    }
    private void RejectItem(CustomerState _cState)
    {
        if(_cState == CustomerState.Reject)//거절
        {
            StartCoroutine(DelayReject());
        }
    }
    private void CustomerExit(CustomerState _cState)
    {
        if(_cState == CustomerState.End)//종료
        {
            StartCoroutine(MoveAndFadeOutCustomer(newCustomer, customerTransform[2].position, fadeDuration)); //퇴장 및 페이드 아웃
            cusCount--;
            StartCoroutine(TradeEnd());
        }
    }
    #endregion
    #region 손님 기능
    public void BuyOrSell()//구매 혹은 판매 신호(랜덤)
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
    IEnumerator MoveCustomerToPosition(GameObject customer, Vector3 targetPosition)//손님 입장
    {
        while (customer.transform.position != targetPosition)
        {
            customer.transform.position = Vector2.MoveTowards(customer.transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
        cState.Value = CustomerState.ItemSet;
    }
    IEnumerator MoveAndFadeOutCustomer(GameObject customer, Vector3 targetPosition, float duration)//손님퇴장
    {
        SpriteRenderer spriteRenderer = customer.GetComponent<SpriteRenderer>();
        Color originalColor = spriteRenderer.color;
        float elapsed = 0f;

        Vector3 startPosition = customer.transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // 이동 처리
            float t = elapsed / duration;
            customer.transform.position = Vector2.Lerp(startPosition, targetPosition, t);

            // 페이드 아웃 처리
            float alpha = Mathf.Lerp(1f, 0f, t);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            yield return null;
        }

        // 마지막 위치와 투명도 설정
        customer.transform.position = targetPosition;
        spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        Destroy(customer); // 페이드 아웃 후 오브젝트 삭제
    }
    IEnumerator DelayBuy()//구매지연
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
    IEnumerator DelaySell()//판매지연
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
    IEnumerator TradeEnd()//거래 종료
    {
        ItemManager.Instance.ListClear();
        yield return new WaitForSeconds(fadeDuration*1.5f);
        if (cusCount == 0)
        {
            cState.Value = CustomerState.Idle;
            GoTownButton.SetActive(true);
            //종료시 나올 UI및 씬이동
        }
        else
        {
            cState.Value = CustomerState.Start;
        }
    }
    IEnumerator DelayReject()//거절 딜레이
    {
        ItemManager.Instance.productCount++;//거절시 다음상품으로 넘김
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

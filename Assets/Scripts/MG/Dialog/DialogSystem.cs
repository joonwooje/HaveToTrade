using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogSystem : MonoBehaviour
{
    public DialogData DialogDB;

    [Header("������ �ϳ� ���� ��Ŀ� ����")]
    [SerializeField] private int branch; // ����Ʈ���� ������ ��� �б� �ε���

    [Header("����Ʈ ���� ��Ŀ� ����")]
    public string seletedDialogName; // ������ ����Ʈ �̸�
    private List<TextData> currentList; // ������ ����Ʈ�� ���� �� ����Ʈ

    [Header("���� ����")]
    [SerializeField] private Speaker[] speakers; // ��ȭ�� �����ϴ� ĳ���� UI �迭

    [SerializeField] private List<DialogData_S> dialogs; // ��ȭ ������ ��� �迭

    public Image dialogUI;
    public GameObject DialogBG;

    [SerializeField] private bool isAutoStart = true; // �ڵ� ���� ����
    private bool isFirst = true; // ���� 1ȸ�� ȣ���ϱ� ���� bool ��
    private int currentDialogIndex = -1; // ���� ��� ����
    private int currentSpeakerIndex = 0; // ���� ȭ�� speakers �迭 ����

    [Header("Ÿ���� ȿ�� ���� ����")]
    [SerializeField] private float typingSpeed = 0.1f; // Ÿ���� �ӵ�
    [SerializeField] private bool isTypingEffect = false; // Ÿ���� ȿ�� ���� ����

    private void Awake()
    {
        dialogs.Clear();

        #region ����Ʈ ���� ���
        // ���� : �׼� ���� ������ ����
        // ���� : ��ũ��Ʈ ���ݾ� ���� �ʿ�
        var field = DialogDB.GetType().GetField(seletedDialogName);
        if (field != null && field.FieldType == typeof(List<TextData>))
        {
            currentList = (List<TextData>)field.GetValue(DialogDB);

            int index = 0;
            for(int i = 0; i < currentList.Count; ++i)
            {
                DialogData_S newDialog = new DialogData_S
                {
                    name = currentList[i].Name,
                    dialog = currentList[i].Dialog
                };
                dialogs.Add(newDialog);
                index++;
            }
        }
        #endregion

        #region ������ �ϳ��� �����ϴ� ���
        // ���� : ��ũ��Ʈ ���� �ʿ� ����
        // ���� : ��� ��ȭ�� �׼� �� ��Ʈ������ �����ؾ��ؼ� ���� ������ ������� ����
        /*
        int index = 0;
        for(int i = 0; i < DialogDB.TextEX.Count; ++i)
        {
            if (DialogDB.TextEX[i].Branch == branch)
            {
                dialogs[index].name = DialogDB.TextEX[i].Name;
                dialogs[index].dialog = DialogDB.TextEX[i].Dialog;
                index++;
            }
        }*/
        #endregion

        Setting();
    }

    private void Setting()
    {
        for(int i = 0; i < speakers.Length; ++i)
        {
            SetActiveObjects(speakers[i], false);

            speakers[i].SpeakerImage.gameObject.SetActive(true);
        }
    }

    public bool UpdateDialog()
    {
        if(isFirst == true)
        {
            Setting();

            if (isAutoStart)
            {
                dialogUI.gameObject.SetActive(true);
                SetNextDialog();
            }
            
            isFirst = false;
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))/* || Input.GetMouseButtonDown(0))*/
        {
            if(isTypingEffect == true)
            {
                isTypingEffect = false;

                StopCoroutine("OnTypingText");
                speakers[currentSpeakerIndex].Dialog.text = dialogs[currentDialogIndex].dialog;
                //speakers[currentSpeakerIndex].Cursor.SetActive(true);

                return false;
            }

            if(dialogs.Count > currentDialogIndex + 1)
            {
                SetNextDialog();
            }
            else
            {
                for(int i = 0; i < speakers.Length; ++i)
                {
                    SetActiveObjects(speakers[i], false);
                    speakers[i].SpeakerImage.gameObject.SetActive(false);
                }
                dialogUI.gameObject.SetActive(false);
                DialogBG.SetActive(false);

                return true;
            }
        }

        return false;
    }

    private void SetNextDialog()
    {
        SetActiveObjects(speakers[currentSpeakerIndex], false);

        currentDialogIndex++;

        currentSpeakerIndex = dialogs[currentDialogIndex].speakerIndex;

        SetActiveObjects(speakers[currentSpeakerIndex], true);

        speakers[currentSpeakerIndex].Name.text = dialogs[currentDialogIndex].name;

        speakers[currentSpeakerIndex].Dialog.text = dialogs[currentDialogIndex].dialog;

        StartCoroutine("OnTypingText");
    }

    private void SetActiveObjects(Speaker speaker, bool isActive)
    {
        speaker.Name.gameObject.SetActive(isActive);
        speaker.Dialog.gameObject.SetActive(isActive);

        //speaker.Cursor.SetActive(false);

        Color color = speaker.SpeakerImage.color;
        color.a = isActive == true ? 1 : 0.2f;
        speaker.SpeakerImage.color = color;
    }

    private IEnumerator OnTypingText()
    {
        int index = 0;

        isTypingEffect = true;

        while(index <= dialogs[currentDialogIndex].dialog.Length)
        {
            speakers[currentSpeakerIndex].Dialog.text = dialogs[currentDialogIndex].dialog.Substring(0, index);

            index++;

            yield return new WaitForSeconds(typingSpeed);
        }

        isTypingEffect = false;

        //speakers[currentSpeakerIndex].Cursor.SetActive(true);
    }
}

[System.Serializable]
public struct Speaker
{
    public Image SpeakerImage;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Dialog;
    //public GameObject Cursor;
}

[System.Serializable]
public struct DialogData_S
{
    public int speakerIndex;
    public string name;
    public string dialog;
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogManager : MonoBehaviour
{
    private static DialogManager instance;
    public static DialogManager Instance => instance;

    private Dialog dialog;

    [SerializeField] private TextMeshProUGUI actorText;
    [SerializeField] private TextMeshProUGUI bodyText;

    [SerializeField] private Animator imageAnimator;

    private bool isLastDialog = false;
    [SerializeField] private GameObject lastScreen;

    private void Awake()
    {
        instance = this;
    }

    public void StartDialog(Dialog dialog)
    {
        gameObject.SetActive(true);
        GameManager.Instance.Pause();
        this.dialog = dialog;
        DisplayDialog();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NextDialog();
        }
    }

    public void DisplayDialog()
    {
        imageAnimator.Rebind();
        imageAnimator.SetBool(dialog.actor.ToString(), true);

        actorText.text = dialog.actor.ToString();
        bodyText.text = dialog.text;
    }

    public void NextDialog()
    {
        dialog = dialog.nextDialog;

        if (dialog == null)
        {

            EndDialog();
            return;
        }

        DisplayDialog();
    }

    public void EndLastDialog()
    {
        gameObject.SetActive(false);
        lastScreen.SetActive(true);
    }

    public void EndDialog()
    {

        if (isLastDialog)
        {
            EndLastDialog();
            return;
        }
        gameObject.SetActive(false);
        GameManager.Instance.Unpause();
    }

    public void StartEndDialog(Dialog dialog)
    {
        StartDialog(dialog);
        isLastDialog = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotHolder : MonoBehaviour
{
    [SerializeField] private Image slotImage;
    [SerializeField] private Image slotImageBackground;
    [SerializeField] private Image letterBackground;

    [SerializeField] private TextMeshProUGUI cooldownLetter;

    private bool toggled = true;

    [SerializeField] private Color disabledColor;
    private Color originalColor;

    [SerializeField] private Button button;

    public void Toggle(bool toggle)
    {
        if (toggled == toggle)
        {
            return;
        }

        if (toggle)
        {
            Enable();
        }
        else
        {
            Disable();
        }

        toggled = toggle;
    }

    public void Disable()
    {
        slotImageBackground.color = disabledColor;
        letterBackground.color = disabledColor;

        button.interactable = false;
    }

    public void Enable()
    {
        slotImageBackground.color = originalColor;
        letterBackground.color = originalColor;

        button.interactable = true;
    }

    public void UpdateCooldown(int cooldown)
    {
        cooldownLetter.text = cooldown + "";
        cooldownLetter.enabled = cooldown > 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        originalColor = letterBackground.color;
    }

    // Update is called once per frame
    void Update()
    {

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


[RequireComponent(typeof(Slider))]
public class SliderText : MonoBehaviour
{
    private Slider slider;
    [SerializeField] private TextMeshProUGUI minText, maxText, currentText;
    [SerializeField] private int currentValueDecimal;
    private string currentValueFormat = "f";
    private const string minMaxFormat = "f0";

    // Start is called before the first frame update
    void Start()
    {
        slider = GetComponent<Slider>();
        //slider.onValueChanged.AddListener(delegate { ValueChange(); });
        
        currentValueFormat +=  currentValueDecimal.ToString();

        minText.text = slider.minValue.ToString(minMaxFormat);
        maxText.text = slider.maxValue.ToString(minMaxFormat);
        currentText.text = slider.value.ToString(currentValueFormat);
    }

    public void ValueChange()
    {
        currentText.text = slider.value.ToString(currentValueFormat);
    }
}

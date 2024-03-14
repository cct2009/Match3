using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CheckBox : MonoBehaviour
{
    public Image Checkbox;
    public Image selImage;
 
    public BoxSubType subType;
    public TMP_Text subTypeOut;
    public void Show(Button button)
    {
        RectTransform rect = button.GetComponent<RectTransform>();
        RectTransform rect2 = Checkbox.GetComponent<RectTransform>();

        rect2.localPosition = new Vector2(rect.localPosition.x+15, rect.localPosition.y-15);
        selImage.sprite = button.GetComponent<Image>().sprite;
        subTypeOut.text = ((int) subType).ToString();        
    }
 
}

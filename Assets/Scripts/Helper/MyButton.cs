using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyButton : Button
{
public int value1;    
protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);
        CheckBox checkBox = GetComponent<CheckBox>();
 
        switch(state)
        {
            case SelectionState.Selected:
                checkBox.Show(this);
                break;
            default:
                
                break;
        }
    }
 
}

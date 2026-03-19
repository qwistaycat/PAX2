using UnityEngine;

public class ClickToCall : MonoBehaviour
{
    public ColorRoller roller;   // or any script you want to call

    void OnMouseDown()
    {
        roller.StartSpin();
    }
}
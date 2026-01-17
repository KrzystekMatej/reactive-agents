using UnityEngine;

public class FoodManager : MonoBehaviour
{
    [SerializeField] private int amount = 100;

    public void TakeFood()
    {
        amount--;
    }

    public bool HasFood()
    {
        return amount > 0;
    }
}

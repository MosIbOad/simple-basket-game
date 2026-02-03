using UnityEngine;

public class BasketDetector : MonoBehaviour
{
    private int basketCount = 0;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            basketCount++;
            // Debug.Log("Basket!");
        }
        // Debug.Log("OnTriggerEnter: " + other.name);
        //if(other.CompareTag("IPlayer"))
        //{
        //    Debug.Log("player OnTriggerEnter");
        //}
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 24;
        style.normal.textColor = Color.green;
        style.alignment = TextAnchor.MiddleRight;

        GUI.Label(new Rect(Screen.width - 200, 90, 180, 30), $"Basket Sayısı: {basketCount}", style);
    }
}

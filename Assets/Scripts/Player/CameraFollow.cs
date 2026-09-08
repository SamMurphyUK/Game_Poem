using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset;

    void Awake()
    {
        //// SplitScreenCamera owns both viewports. Following one player from here
        //// would drag the other half of the screen along with WASD.
        //if (GetComponent<SplitScreenCamera>() != null)
        //{
        //    enabled = false;
        //}
    }

    void Update()
    {
        if (player == null)
        {
            return;
        }

        transform.position = new Vector3(player.position.x + offset.x, player.position.y + offset.y, offset.z);
    }
}

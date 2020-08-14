using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class MapPreloader : MonoBehaviour
{
    [SerializeField] private SpriteAtlas Sprites;
    [SerializeField] private string Background_Sprite;
    [SerializeField] private string Stone_Sprite;
    // Start is called before the first frame update
    void Start()
    {
        foreach(SpriteRenderer Sprite in GetComponentsInChildren<SpriteRenderer>())
        {
            Sprite.sprite = Sprites.GetSprite(Stone_Sprite);
        }
        GetComponent<SpriteRenderer>().sprite = Sprites.GetSprite(Background_Sprite);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

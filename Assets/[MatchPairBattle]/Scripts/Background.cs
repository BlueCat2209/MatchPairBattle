using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Background : MonoBehaviour
{
    public Image background;

    public Sprite[]  Sprite;

    int x;
    // Start is called before the first frame update
    void Start()
    {
        x = Random.Range(0, Sprite.Length);
        background.sprite = Sprite[x];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

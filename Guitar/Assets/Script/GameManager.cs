using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public readonly KeyCode[] keyInput = { KeyCode.U, KeyCode.I, KeyCode.O, KeyCode.P };
    public bool[] isKeyPressed = new bool[4];
    public MusicData music;
    [SerializeField] private float curTime;
    [SerializeField] private int curTileIndex = 0;
    void Start()
    {
        StartCoroutine(GameStart());
        print(music.tiles.Count);
    }
    void Update()
    {
        for(int i = 0; i < keyInput.Length; i++)
        {
            isKeyPressed[i] = Input.GetKey(keyInput[i]);
        }
    }
    IEnumerator GameStart()
    {
        curTileIndex = 0;
        while(curTileIndex < music.tiles.Count)
        {
            yield return null;
            curTime += Time.deltaTime;
            if (music.tiles[curTileIndex].spawnTime <= curTime)
            {
                print($"{music.tiles[curTileIndex].spawnTime}\n{music.tiles[curTileIndex].Line}");
                curTileIndex++;
            }
        }
        StartCoroutine(MusicStart());
    }
    IEnumerator MusicStart()
    {
        yield return null;
    }
}

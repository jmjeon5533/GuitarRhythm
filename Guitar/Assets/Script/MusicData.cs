using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Music", menuName = "MusicData", order = 0)]
public class MusicData : ScriptableObject
{
    public AudioClip music;
    public List<TileData> tiles = new List<TileData>();
    public int BPM = 120; // BPM ±âº»°ª
    public void SaveTiles(List<TileData> newTiles)
    {
        tiles = newTiles;
    }
}

[System.Serializable]
public class TileData
{
    public float spawnTime;
    public TileState state;
    public int Line;
    public enum TileState
    {
        Normal,
        Long
    }
}

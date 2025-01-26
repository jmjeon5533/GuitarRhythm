using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MusicDataEditor : EditorWindow
{
    private MusicData musicData;
    private TileData selectedTile;
    private float timelineWidth = 800f;
    private float timelineHeight = 50f;
    private float zoomLevel = 1f; // Ÿ�Ӷ��� Ȯ��/��� ����
    private Vector2 scrollPosition; // ��ũ�� ��ġ

    [MenuItem("Window/Music Data Editor")]
    public static void OpenWindow()
    {
        GetWindow<MusicDataEditor>("Music Data Editor");
    }

    private void OnEnable()
    {
        // ó�� �����͸� �� �� zoomLevel�� 10���� �ʱ�ȭ
        zoomLevel = 10f;
    }

    private void OnGUI()
    {
        GUILayout.Label("Music Data Editor", EditorStyles.boldLabel);

        musicData = (MusicData)EditorGUILayout.ObjectField("Music Data", musicData, typeof(MusicData), false);

        if (musicData == null)
        {
            EditorGUILayout.HelpBox("MusicData�� �����ϼ���.", MessageType.Warning);
            return;
        }

        if (musicData.music == null)
        {
            EditorGUILayout.HelpBox("AudioClip�� MusicData�� �������� �ʾҽ��ϴ�.", MessageType.Warning);
            return;
        }

        GUILayout.Space(10);

        // BPM �Է� UI �߰�
        musicData.BPM = EditorGUILayout.IntField("BPM", musicData.BPM);

        // ��ũ�� �� ����
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
        DrawTimelines();
        EditorGUILayout.EndScrollView(); // ��ũ�� �� ��

        GUILayout.Space(20);

        if (selectedTile != null)
        {
            DrawTileEditor();
        }

        HandleTimelineZoom();

        // Save ��ư �߰�
        GUILayout.Space(20);
        if (GUILayout.Button("Save Music Data"))
        {
            SaveMusicData();
        }
    }


    private void SaveMusicData()
    {
        if (musicData != null)
        {
            // Undo ���
            Undo.RecordObject(musicData, "Save Music Data");

            // MusicData�� Ÿ�� �����͸� ����
            musicData.SaveTiles(musicData.tiles);

            // ����� ��ü�� dirty ���·� ��ŷ
            EditorUtility.SetDirty(musicData);

            // ������ ��ũ�� ����
            AssetDatabase.SaveAssets();

            // ���� ������ �����ߴٰ� �α� ǥ��
            Debug.Log("Music Data saved successfully!");
        }
        else
        {
            EditorGUILayout.HelpBox("MusicData�� �����ϼ���.", MessageType.Error);
        }
    }





    private void DrawTimelines()
    {
        GUILayout.Label("Timelines", EditorStyles.boldLabel);

        float duration = musicData.music.length;
        float pixelPerSecond = (timelineWidth * zoomLevel) / duration;

        // BPM�� ���� ���� ������ ���
        float secondsPerBeat = 60f / Mathf.Max(musicData.BPM, 1); // BPM 0 ����
        float majorTickInterval = secondsPerBeat; // ���� ���� = �� ���ڴ� �ð�

        for (int line = 0; line < 4; line++)
        {
            GUILayout.Space(20); // Ÿ�Ӷ��� ����

            // Ÿ�Ӷ����� ���� ���̸� Ȯ�뿡 ���� ����
            Rect timelineRect = GUILayoutUtility.GetRect(timelineWidth * zoomLevel, timelineHeight);
            EditorGUI.DrawRect(timelineRect, new Color(0.1f, 0.1f, 0.1f));

            // �ֿ� ���� �׸���
            for (float t = 0; t <= duration; t += majorTickInterval)
            {
                float x = timelineRect.x + t * pixelPerSecond;
                Handles.color = Color.gray;
                Handles.DrawLine(new Vector3(x, timelineRect.y), new Vector3(x, timelineRect.y + timelineHeight));
                GUI.Label(new Rect(x - 15, timelineRect.y + timelineHeight, 50, 20), $"{t:F2}s");
            }

            // Ÿ�� �׸���
            foreach (var tile in musicData.tiles)
            {
                if (tile.Line != line) continue;

                float x = timelineRect.x + tile.spawnTime * pixelPerSecond;
                Rect tileRect = new Rect(x - 5, timelineRect.y + 10, 10, timelineHeight - 20);
                EditorGUI.DrawRect(tileRect, Color.red);

                if (Event.current.type == EventType.MouseDown && tileRect.Contains(Event.current.mousePosition))
                {
                    selectedTile = tile;
                    Event.current.Use();
                }
            }

            // Ŭ�� �̺�Ʈ ó�� (�ش� Ÿ�Ӷ����� Line ����)
            if (Event.current.type == EventType.MouseDown && timelineRect.Contains(Event.current.mousePosition))
            {
                float clickTime = (Event.current.mousePosition.x - timelineRect.x) / pixelPerSecond;
                AddTile(clickTime, line);
                Event.current.Use();
            }
        }
    }


    private void AddTile(float spawnTime, int line)
    {
        TileData newTile = new TileData
        {
            spawnTime = Mathf.Clamp(spawnTime, 0, musicData.music.length),
            state = TileData.TileState.Normal,
            Line = line
        };
        musicData.tiles.Add(newTile);
        EditorUtility.SetDirty(musicData);
    }

    private void DrawTileEditor()
    {
        GUILayout.Label("Tile Editor", EditorStyles.boldLabel);
        selectedTile.spawnTime = EditorGUILayout.FloatField("Spawn Time", selectedTile.spawnTime);
        selectedTile.state = (TileData.TileState)EditorGUILayout.EnumPopup("Tile State", selectedTile.state);
        selectedTile.Line = EditorGUILayout.IntField("Line", selectedTile.Line);

        if (GUILayout.Button("Delete Tile"))
        {
            musicData.tiles.Remove(selectedTile);
            selectedTile = null;
            EditorUtility.SetDirty(musicData);
        }
    }

    private void HandleTimelineZoom()
    {
        Event e = Event.current;

        if (e.type == EventType.ScrollWheel)
        {
            zoomLevel = Mathf.Clamp(zoomLevel - e.delta.y * 0.25f, 10f, 50f); // Ȯ��/��� ����
            Repaint();
            e.Use();
        }
    }
}

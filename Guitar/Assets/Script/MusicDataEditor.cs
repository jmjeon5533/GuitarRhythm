using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MusicDataEditor : EditorWindow
{
    private MusicData musicData;
    private TileData selectedTile;
    private float timelineWidth = 800f;
    private float timelineHeight = 50f;
    private float zoomLevel = 1f; // 타임라인 확대/축소 레벨
    private Vector2 scrollPosition; // 스크롤 위치

    [MenuItem("Window/Music Data Editor")]
    public static void OpenWindow()
    {
        GetWindow<MusicDataEditor>("Music Data Editor");
    }

    private void OnEnable()
    {
        // 처음 에디터를 열 때 zoomLevel을 10으로 초기화
        zoomLevel = 10f;
    }

    private void OnGUI()
    {
        GUILayout.Label("Music Data Editor", EditorStyles.boldLabel);

        musicData = (MusicData)EditorGUILayout.ObjectField("Music Data", musicData, typeof(MusicData), false);

        if (musicData == null)
        {
            EditorGUILayout.HelpBox("MusicData를 선택하세요.", MessageType.Warning);
            return;
        }

        if (musicData.music == null)
        {
            EditorGUILayout.HelpBox("AudioClip이 MusicData에 설정되지 않았습니다.", MessageType.Warning);
            return;
        }

        GUILayout.Space(10);

        // BPM 입력 UI 추가
        musicData.BPM = EditorGUILayout.IntField("BPM", musicData.BPM);

        // 스크롤 뷰 시작
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
        DrawTimelines();
        EditorGUILayout.EndScrollView(); // 스크롤 뷰 끝

        GUILayout.Space(20);

        if (selectedTile != null)
        {
            DrawTileEditor();
        }

        HandleTimelineZoom();

        // Save 버튼 추가
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
            // Undo 기록
            Undo.RecordObject(musicData, "Save Music Data");

            // MusicData에 타일 데이터를 저장
            musicData.SaveTiles(musicData.tiles);

            // 변경된 객체를 dirty 상태로 마킹
            EditorUtility.SetDirty(musicData);

            // 에셋을 디스크에 저장
            AssetDatabase.SaveAssets();

            // 변경 사항을 저장했다고 로그 표시
            Debug.Log("Music Data saved successfully!");
        }
        else
        {
            EditorGUILayout.HelpBox("MusicData를 선택하세요.", MessageType.Error);
        }
    }





    private void DrawTimelines()
    {
        GUILayout.Label("Timelines", EditorStyles.boldLabel);

        float duration = musicData.music.length;
        float pixelPerSecond = (timelineWidth * zoomLevel) / duration;

        // BPM에 따라 눈금 단위를 계산
        float secondsPerBeat = 60f / Mathf.Max(musicData.BPM, 1); // BPM 0 방지
        float majorTickInterval = secondsPerBeat; // 눈금 단위 = 한 박자당 시간

        for (int line = 0; line < 4; line++)
        {
            GUILayout.Space(20); // 타임라인 간격

            // 타임라인의 가로 길이를 확대에 따라 변경
            Rect timelineRect = GUILayoutUtility.GetRect(timelineWidth * zoomLevel, timelineHeight);
            EditorGUI.DrawRect(timelineRect, new Color(0.1f, 0.1f, 0.1f));

            // 주요 눈금 그리기
            for (float t = 0; t <= duration; t += majorTickInterval)
            {
                float x = timelineRect.x + t * pixelPerSecond;
                Handles.color = Color.gray;
                Handles.DrawLine(new Vector3(x, timelineRect.y), new Vector3(x, timelineRect.y + timelineHeight));
                GUI.Label(new Rect(x - 15, timelineRect.y + timelineHeight, 50, 20), $"{t:F2}s");
            }

            // 타일 그리기
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

            // 클릭 이벤트 처리 (해당 타임라인의 Line 설정)
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
            zoomLevel = Mathf.Clamp(zoomLevel - e.delta.y * 0.25f, 10f, 50f); // 확대/축소 제한
            Repaint();
            e.Use();
        }
    }
}

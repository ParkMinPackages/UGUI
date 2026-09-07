using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ParkMinPackages.UGUI.Editor
{
    public sealed class PSDImportLayer
    {
        public string Name;
        public int ParentIndex;
        public bool IsGroup;
        public bool Visible;
        public Sprite Sprite;
        public PSDLayerData Data;
        public Rect Bounds;
    }

    public static class PSDUIConverter
    {
        // - Public Methods -
        public static List<PSDImportLayer> ReadImportedLayers(PSDDocument document) {
            AssetImporter importer = AssetImporter.GetAtPath(document.AssetPath);
            if (importer == null || importer.GetType().FullName != "UnityEditor.U2D.PSD.PSDImporter") throw new InvalidOperationException("PSD Importer로 임포트된 PSD/PSB를 선택해 주세요. Importer Override에서 PSD Importer를 선택할 수 있습니다.");
            using (SerializedObject serialized = new SerializedObject(importer)) {
                SerializedProperty layers = serialized.FindProperty("m_PsdLayers");
                if (layers == null || !layers.isArray || layers.arraySize == 0) throw new InvalidOperationException("PSD Importer 레이어 정보가 없습니다. 레이어별 Sprite 임포트 설정을 확인해 주세요.");
                Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
                foreach (Sprite sprite in AssetDatabase.LoadAllAssetsAtPath(document.AssetPath).OfType<Sprite>()) {
                    using (SerializedObject spriteObject = new SerializedObject(sprite)) {
                        SerializedProperty id = spriteObject.FindProperty("m_SpriteID");
                        if (id == null || id.propertyType != SerializedPropertyType.String) throw new InvalidOperationException("지원하지 않는 Unity Sprite 직렬화 구조입니다.");
                        sprites[id.stringValue] = sprite;
                    }
                }
                Dictionary<int, PSDLayerData> data = document.Layers.Where(layer => layer.Id != 0).GroupBy(layer => layer.Id).ToDictionary(group => group.Key, group => group.First());
                List<PSDImportLayer> result = new List<PSDImportLayer>();
                for (int i = 0; i < layers.arraySize; i++) {
                    SerializedProperty layer = layers.GetArrayElementAtIndex(i);
                    SerializedProperty name = layer.FindPropertyRelative("m_Name");
                    if (name == null) throw new InvalidOperationException("지원하지 않는 PSD Importer 레이어 구조입니다. PSD Importer 14.x가 필요합니다.");
                    int id = layer.FindPropertyRelative("m_LayerID").intValue;
                    sprites.TryGetValue(layer.FindPropertyRelative("m_SpriteID").stringValue, out Sprite sprite);
                    data.TryGetValue(id, out PSDLayerData source);
                    result.Add(new PSDImportLayer {
                        Name = name.stringValue, ParentIndex = layer.FindPropertyRelative("m_ParentIndex").intValue,
                        IsGroup = layer.FindPropertyRelative("m_IsGroup").boolValue,
                        Visible = source?.Visible ?? layer.FindPropertyRelative("m_IsVisible").boolValue,
                        Sprite = layer.FindPropertyRelative("m_IsImported").boolValue ? sprite : null, Data = source, Bounds = source?.Bounds ?? Rect.zero
                    });
                }
                if (!result.Any(layer => layer.Sprite != null)) throw new InvalidOperationException("레이어별 Sprite가 없습니다. PSD Importer의 레이어 임포트를 활성화해 주세요.");
                if (result.Any(layer => layer.Sprite != null && layer.Data == null)) throw new InvalidOperationException("PSD 레이어 ID와 임포트된 Sprite 정보를 연결할 수 없습니다. 원본 PSD를 다시 임포트해 주세요.");
                for (int i = 0; i < result.Count; i++) {
                    HashSet<int> ancestors = new HashSet<int> { i };
                    int parent = result[i].ParentIndex;
                    while (parent >= 0) {
                        if (parent >= result.Count || !ancestors.Add(parent)) throw new InvalidOperationException("잘못된 PSD 부모 레이어 정보입니다.");
                        parent = result[parent].ParentIndex;
                    }
                }
                for (int i = 0; i < result.Count; i++) {
                    PSDImportLayer group = result[i];
                    if (!group.IsGroup || group.Sprite == null || group.Bounds.width > 0 && group.Bounds.height > 0) continue;
                    bool hasBounds = false;
                    for (int child = 0; child < result.Count; child++) {
                        Rect bounds = result[child].Bounds;
                        if (bounds.width <= 0 || bounds.height <= 0) continue;
                        int parent = result[child].ParentIndex;
                        while (parent >= 0 && parent != i) parent = result[parent].ParentIndex;
                        if (parent != i) continue;
                        group.Bounds = hasBounds ? Rect.MinMaxRect(Mathf.Min(group.Bounds.xMin, bounds.xMin), Mathf.Min(group.Bounds.yMin, bounds.yMin), Mathf.Max(group.Bounds.xMax, bounds.xMax), Mathf.Max(group.Bounds.yMax, bounds.yMax)) : bounds;
                        hasBounds = true;
                    }
                    if (!hasBounds) throw new InvalidOperationException(group.Name + ": 병합 그룹의 위치를 계산할 수 없습니다. PSD Importer에서 그룹 병합을 해제해 주세요.");
                }
                return result;
            }
        }
        public static string GetBlockingReason(PSDDocument document, List<PSDImportLayer> layers, PSDConverterSettings settings, PSDTextOutput output) {
            if (document == null || layers == null) return "PSD를 먼저 분석해 주세요.";
            if (EditorApplication.isPlayingOrWillChangePlaymode) return "Edit Mode에서 변환해 주세요.";
            if (PrefabStageUtility.GetCurrentPrefabStage() != null) return "Prefab Mode를 닫고 대상 씬에서 변환해 주세요.";
            if (!settings.UsePSDResolution && (settings.ReferenceResolution.x < 1 || settings.ReferenceResolution.y < 1)) return "기준 해상도는 1 이상이어야 합니다.";
            if (settings.MissingFontPolicy != PSDMissingFontPolicy.StopConversion) return null;
            string[] missing = layers.Where(layer => layer.Sprite != null && layer.Data?.Text?.CanConvert == true).SelectMany(layer => layer.Data.Text.Fonts).Distinct().Where(font => settings.ResolveFont(font, output) == null).ToArray();
            return missing.Length > 0 ? "폰트 에셋을 연결해 주세요: " + string.Join(", ", missing) : null;
        }
        public static GameObject Convert(PSDDocument document, List<PSDImportLayer> layers, PSDConverterSettings settings, PSDTextOutput output, out string report) {
            string error = GetBlockingReason(document, layers, settings, output);
            if (error != null) throw new InvalidOperationException(error);
            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Convert PSD to " + output);
            List<string> notes = new List<string>();
            int textCount = 0, imageCount = 0;
            try {
                Scene scene = SceneManager.GetActiveScene();
                string name = System.IO.Path.GetFileNameWithoutExtension(document.AssetPath) + (output == PSDTextOutput.Legacy ? "_Legacy" : "_TMP");
                name = ObjectNames.GetUniqueName(scene.GetRootGameObjects().Select(root => root.name).ToArray(), name);
                GameObject canvasObject = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                Undo.RegisterCreatedObjectUndo(canvasObject, "Create PSD Canvas");
                SceneManager.MoveGameObjectToScene(canvasObject, scene);
                canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                Vector2 reference = settings.UsePSDResolution ? document.Size : settings.ReferenceResolution;
                scaler.referenceResolution = reference;
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
                RectTransform content = CreateRect("PSD Content", canvasObject.transform, document.Size);
                float scale = Mathf.Min(reference.x / document.Size.x, reference.y / document.Size.y);
                content.localScale = Vector3.one * scale;
                Dictionary<int, RectTransform> groups = new Dictionary<int, RectTransform>();
                for (int i = layers.Count - 1; i >= 0; i--) {
                    PSDImportLayer layer = layers[i];
                    bool flattenedParent = false;
                    for (int parent = layer.ParentIndex; parent >= 0; parent = layers[parent].ParentIndex) if (layers[parent].IsGroup && layers[parent].Sprite != null) { flattenedParent = true; break; }
                    if (flattenedParent) continue;
                    if (layer.Sprite == null) {
                        if (!layer.IsGroup && layer.Visible && layer.Data != null && layer.Data.Bounds.width > 0 && layer.Data.Bounds.height > 0) notes.Add(layer.Name + ": 임포트된 Sprite가 없어 제외됨");
                        continue;
                    }
                    RectTransform parentTransform = GetParent(layer.ParentIndex, layers, groups, content, document.Size);
                    Rect bounds = layer.Bounds;
                    RectTransform rect = CreateRect(layer.Name, parentTransform, bounds.size);
                    rect.anchoredPosition = new Vector2(bounds.center.x - document.Size.x * 0.5f, document.Size.y * 0.5f - bounds.center.y);
                    rect.gameObject.SetActive(layer.Visible);
                    PSDTextData text = layer.Data.Text;
                    UnityEngine.Object font = text?.CanConvert == true ? settings.ResolveFont(text.Fonts[0], output) : null;
                    if (font == null) {
                        Image image = Undo.AddComponent<Image>(rect.gameObject);
                        image.sprite = layer.Sprite;
                        image.raycastTarget = false;
                        image.useSpriteMesh = true;
                        imageCount++;
                        if (layer.Data.IsText) notes.Add(layer.Name + ": 이미지 유지 (" + (layer.Data.Warning ?? "폰트 에셋 미연결") + ")");
                        continue;
                    }
                    PSDFontMapping mapping = settings.GetMapping(text.Fonts[0]);
                    bool fallback = output == PSDTextOutput.Legacy ? mapping.LegacyFont == null : mapping.TMPFont == null;
                    if (fallback) notes.Add(layer.Name + ": " + text.Fonts[0] + " → 기본 대체 폰트 " + font.name);
                    Color color = text.Color;
                    color.a *= layer.Data.Opacity;
                    float fontSize = Mathf.Max(1, text.FontSize);
                    rect.localRotation = Quaternion.Euler(0, 0, text.Rotation);
                    rect.localScale = new Vector3(text.HorizontalScale, 1, 1);
                    rect.sizeDelta = new Vector2(Mathf.Max(1, bounds.width / Mathf.Max(0.01f, text.HorizontalScale)), Mathf.Max(bounds.height, fontSize * 1.5f));
                    if (output == PSDTextOutput.Legacy) {
                        Text component = Undo.AddComponent<Text>(rect.gameObject);
                        component.font = (Font)font;
                        component.text = text.Content;
                        component.fontSize = Mathf.Max(1, Mathf.RoundToInt(fontSize));
                        component.color = color;
                        component.supportRichText = false;
                        component.fontStyle = text.Bold ? text.Italic ? FontStyle.BoldAndItalic : FontStyle.Bold : text.Italic ? FontStyle.Italic : FontStyle.Normal;
                        component.alignment = text.Alignment == 1 ? TextAnchor.MiddleRight : text.Alignment == 2 ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;
                        component.horizontalOverflow = HorizontalWrapMode.Overflow;
                        component.verticalOverflow = VerticalWrapMode.Overflow;
                        component.raycastTarget = false;
                    }
                    else {
                        TextMeshProUGUI component = Undo.AddComponent<TextMeshProUGUI>(rect.gameObject);
                        component.font = (TMP_FontAsset)font;
                        component.text = text.Content;
                        component.fontSize = fontSize;
                        component.color = color;
                        component.richText = false;
                        component.enableAutoSizing = false;
                        component.textWrappingMode = TextWrappingModes.NoWrap;
                        component.overflowMode = TextOverflowModes.Overflow;
                        component.fontStyle = (text.Bold ? FontStyles.Bold : FontStyles.Normal) | (text.Italic ? FontStyles.Italic : FontStyles.Normal);
                        component.alignment = text.Alignment == 1 ? TextAlignmentOptions.MidlineRight : text.Alignment == 2 ? TextAlignmentOptions.Midline : TextAlignmentOptions.MidlineLeft;
                        component.raycastTarget = false;
                    }
                    textCount++;
                }
                Undo.CollapseUndoOperations(undoGroup);
                Selection.activeGameObject = canvasObject;
                EditorSceneManager.MarkSceneDirty(scene);
                report = "생성 완료: " + name + " · Text " + textCount + " · Image " + imageCount + (notes.Count > 0 ? "\n" + string.Join("\n", notes) : "");
                return canvasObject;
            }
            catch { Undo.RevertAllDownToGroup(undoGroup); throw; }
        }

        // - Internals -
        static RectTransform CreateRect(string name, Transform parent, Vector2 size) {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(gameObject, "Create PSD Layer");
            RectTransform rect = (RectTransform)gameObject.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            return rect;
        }
        static RectTransform GetParent(int index, List<PSDImportLayer> layers, Dictionary<int, RectTransform> groups, RectTransform root, Vector2 size) {
            if (index < 0) return root;
            if (groups.TryGetValue(index, out RectTransform existing)) return existing;
            PSDImportLayer layer = layers[index];
            RectTransform parent = GetParent(layer.ParentIndex, layers, groups, root, size);
            RectTransform group = CreateRect(layer.Name, parent, size);
            group.gameObject.SetActive(layer.Visible);
            groups[index] = group;
            return group;
        }
    }
}

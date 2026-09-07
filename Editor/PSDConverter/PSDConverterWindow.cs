using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Search;

namespace ParkMinPackages.UGUI.Editor
{
    public sealed class PSDConverterWindow : EditorWindow
    {
        // - Statics -
        [MenuItem("ParkMinPackages/PSD Converter")]
        public static void Open() {
            PSDConverterWindow window = GetWindow<PSDConverterWindow>();
            window.titleContent = new GUIContent("PSD Converter");
            window.minSize = new Vector2(540, 550);
            window.Show();
        }
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider() {
            return new SettingsProvider("Project/ParkMinPackages/PSD Converter", SettingsScope.Project) {
                label = "PSD Converter",
                guiHandler = search => DrawSettings(PSDConverterSettings.instance),
                keywords = new HashSet<string> { "PSD", "Fonts", "TextMeshPro", "Legacy", "Canvas" }
            };
        }

        // - Public Methods -
        public void SetSource(UnityEngine.Object source) {
            if (ChangeSource(source)) Analyze();
        }

        // - Handler -
        void OnEnable() {
            titleContent = new GUIContent("PSD Converter");
            minSize = new Vector2(540, 550);
            if (_source == null && IsPSD(AssetDatabase.GetAssetPath(Selection.activeObject))) _source = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(Selection.activeObject));
            if (_source != null) EditorApplication.delayCall += Analyze;
        }
        void OnDisable() {
            EditorApplication.delayCall -= Analyze;
            if (_sourcePicker is EditorWindow picker && picker != null) picker.Close();
            _sourcePicker = null;
            _sourceSearchContext?.Dispose();
            _sourceSearchContext = null;
        }
        void OnGUI() {
            PSDConverterSettings settings = PSDConverterSettings.instance;
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("PSD Converter", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope()) {
                EditorGUILayout.PrefixLabel("PSD Asset");
                string sourcePath = AssetDatabase.GetAssetPath(_source);
                GUIContent sourceContent = new GUIContent(_source != null ? Path.GetFileName(sourcePath) : "PSD 선택...", _source != null ? sourcePath : "Unity Search에서 PSD 선택 또는 프로젝트의 PSD를 드래그해 주세요.");
                Rect sourceRect = GUILayoutUtility.GetRect(sourceContent, EditorStyles.objectField, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(true));
                if (GUI.Button(sourceRect, sourceContent, EditorStyles.objectField)) OpenSourcePicker();
                Event current = Event.current;
                if ((current.type == EventType.DragUpdated || current.type == EventType.DragPerform) && sourceRect.Contains(current.mousePosition)) {
                    UnityEngine.Object dragged = DragAndDrop.objectReferences.Length == 1 ? DragAndDrop.objectReferences[0] : null;
                    bool accepted = dragged != null && EditorUtility.IsPersistent(dragged) && IsPSD(AssetDatabase.GetAssetPath(dragged));
                    DragAndDrop.visualMode = accepted ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
                    if (current.type == EventType.DragPerform && accepted) {
                        DragAndDrop.AcceptDrag();
                        ChangeSource(dragged);
                    }
                    current.Use();
                }
                using (new EditorGUI.DisabledScope(_source == null)) if (GUILayout.Button(new GUIContent("×", "PSD 선택 해제"), GUILayout.Width(22))) ChangeSource(null);
                using (new EditorGUI.DisabledScope(_source == null)) if (GUILayout.Button("분석", GUILayout.Width(65))) Analyze();
            }
            if (_document != null) EditorGUILayout.LabelField($"{_document.Size.x} × {_document.Size.y} · 레이어 {_document.Layers.Count}개 · 텍스트 {_document.Layers.Count(layer => layer.IsText)}개");
            if (!string.IsNullOrEmpty(_error)) EditorGUILayout.HelpBox(_error, MessageType.Error);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawSettings(settings);
            if (_document != null) {
                DrawSectionDivider();
                using (new EditorGUILayout.HorizontalScope()) {
                    EditorGUILayout.LabelField("Font Mapping / 폰트 연결", EditorStyles.boldLabel);
                    _missingOnly = GUILayout.Toggle(_missingOnly, "미연결만 보기", GUILayout.Width(110));
                    if (GUILayout.Button("자동 연결", GUILayout.Width(80))) {
                        try {
                            _fonts.Refresh();
                            foreach (string name in _usedFonts.Keys) _fonts.AutoConnect(settings.GetMapping(name));
                            settings.SaveSettings();
                        }
                        catch (Exception exception) { _error = exception.Message; }
                    }
                }
                EditorGUILayout.HelpBox("시스템 폰트는 참고 정보입니다. 변환에 사용할 Unity Font / TMP Font Asset을 연결해 주세요. 이름이 비슷한 폰트는 자동 연결하지 않습니다.", MessageType.Info);
                foreach (KeyValuePair<string, List<PSDLayerData>> usage in _usedFonts) DrawFont(usage.Key, usage.Value, settings);
                if (_usedFonts.Count == 0) EditorGUILayout.LabelField("분석 가능한 텍스트 폰트가 없습니다.");
                if (_document.Warnings.Count > 0) {
                    DrawSectionDivider();
                    _warningsExpanded = EditorGUILayout.Foldout(_warningsExpanded, $"분석 경고 {_document.Warnings.Count}개", true);
                    if (_warningsExpanded) foreach (string warning in _document.Warnings) EditorGUILayout.HelpBox(warning, MessageType.Warning);
                }
                EditorGUILayout.HelpBox("텍스트 렌더러의 글자 치수 차이로 배치가 달라질 수 있습니다. 혼합 스타일, 여러 줄/문단, 워프, 레이어 효과·마스크는 이미지로 유지합니다.", MessageType.Info);
            }
            if (!string.IsNullOrEmpty(_report)) {
                DrawSectionDivider();
                EditorGUILayout.LabelField("변환 결과", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(_report, MessageType.Info);
            }
            EditorGUILayout.EndScrollView();
            DrawSectionDivider();
            bool stale = _document != null && _analyzedHash != AssetDatabase.GetAssetDependencyHash(_document.AssetPath);
            if (stale) EditorGUILayout.HelpBox("PSD 또는 Importer 설정이 변경됐습니다. 다시 분석해 주세요.", MessageType.Warning);
            using (new EditorGUILayout.HorizontalScope()) {
                DrawConvertButton("Legacy UGUI로 변환", PSDTextOutput.Legacy, settings, stale);
                DrawConvertButton("TextMeshPro로 변환", PSDTextOutput.TextMeshPro, settings, stale);
            }
            EditorGUILayout.Space(8);
        }

        // - Internals -
        [SerializeField] UnityEngine.Object _source;
        [SerializeField] bool _missingOnly;
        [SerializeField] bool _warningsExpanded;
        Vector2 _scroll;
        PSDDocument _document;
        List<PSDImportLayer> _layers;
        PSDFontUtility _fonts = new PSDFontUtility();
        Dictionary<string, List<PSDLayerData>> _usedFonts = new Dictionary<string, List<PSDLayerData>>();
        readonly Dictionary<string, bool> _expanded = new Dictionary<string, bool>();
        Hash128 _analyzedHash;
        string _error;
        string _report;
        SearchContext _sourceSearchContext;
        ISearchView _sourcePicker;

        void OpenSourcePicker() {
            if (_sourcePicker is EditorWindow picker && picker != null) {
                picker.Focus();
                return;
            }
            _sourceSearchContext?.Dispose();
            _sourceSearchContext = SearchService.CreateContext("asset", "ext:psd", SearchFlags.FirstBatchAsync);
            SearchViewState state = SearchViewState.CreatePickerState(
                "PSD Asset",
                _sourceSearchContext,
                (SearchItem item, bool canceled) => {
                    if (canceled || this == null || item == null) return;
                    string path = SearchUtils.GetAssetPath(item);
                    if (!IsPSD(path)) return;
                    UnityEngine.Object source = AssetDatabase.LoadMainAssetAtPath(path);
                    if (source != null) ChangeSource(source);
                },
                (SearchItem item) => { },
                (SearchItem item) => item != null && IsPSD(SearchUtils.GetAssetPath(item)),
                SearchViewFlags.ListView | SearchViewFlags.DisableInspectorPreview | SearchViewFlags.OpenInTextMode | SearchViewFlags.DisableBuilderModeToggle | SearchViewFlags.IgnoreSavedSearches
            );
            state.position.size = new Vector2(640, 440);
            state.excludeClearItem = true;
            state.hideTabs = true;
            state.queryBuilderEnabled = false;
            try { _sourcePicker = SearchService.ShowPicker(state); }
            catch {
                _sourceSearchContext.Dispose();
                _sourceSearchContext = null;
                throw;
            }
        }
        bool ChangeSource(UnityEngine.Object source) {
            if (source != null) {
                string path = AssetDatabase.GetAssetPath(source);
                if (!EditorUtility.IsPersistent(source) || !IsPSD(path)) {
                    _error = "프로젝트의 .psd 파일만 선택할 수 있습니다.";
                    Repaint();
                    return false;
                }
                source = AssetDatabase.LoadMainAssetAtPath(path);
                if (source == null) {
                    _error = "PSD 에셋을 불러올 수 없습니다. 임포트 상태를 확인해 주세요.";
                    Repaint();
                    return false;
                }
            }
            EditorApplication.delayCall -= Analyze;
            _source = source;
            _document = null;
            _layers = null;
            _usedFonts.Clear();
            _error = null;
            _report = null;
            Repaint();
            return true;
        }
        void Analyze() {
            if (this == null || _source == null) return;
            _document = null;
            _layers = null;
            _error = null;
            _report = null;
            _usedFonts.Clear();
            try {
                string path = AssetDatabase.GetAssetPath(_source);
                if (!IsPSD(path)) throw new InvalidOperationException("PSD 파일을 선택해 주세요.");
                EditorUtility.DisplayProgressBar("PSD Converter", "레이어 및 폰트 정보 분석", 0.2f);
                PSDDocument document = PSDDocumentReader.Read(path);
                List<PSDImportLayer> layers = PSDUIConverter.ReadImportedLayers(document);
                EditorUtility.DisplayProgressBar("PSD Converter", "시스템 및 프로젝트 폰트 검색", 0.6f);
                _fonts.Refresh();
                foreach (PSDLayerData layer in document.Layers.Where(layer => layer.Text != null)) {
                    foreach (string name in layer.Text.Fonts.Distinct()) {
                        if (!_usedFonts.TryGetValue(name, out List<PSDLayerData> users)) _usedFonts[name] = users = new List<PSDLayerData>();
                        users.Add(layer);
                    }
                }
                PSDConverterSettings settings = PSDConverterSettings.instance;
                foreach (string name in _usedFonts.Keys) _fonts.AutoConnect(settings.GetMapping(name));
                settings.SaveSettings();
                _document = document;
                _layers = layers;
                _analyzedHash = AssetDatabase.GetAssetDependencyHash(path);
            }
            catch (Exception exception) { _error = exception.Message; }
            finally { EditorUtility.ClearProgressBar(); Repaint(); }
        }
        void DrawFont(string name, List<PSDLayerData> layers, PSDConverterSettings settings) {
            PSDFontMapping mapping = settings.GetMapping(name);
            if (_missingOnly && mapping.LegacyFont != null && mapping.TMPFont != null) return;
            PSDFontInfo systemFont = _fonts.FindSystemFont(name);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                using (new EditorGUILayout.HorizontalScope()) {
                    Color original = GUI.contentColor;
                    GUI.contentColor = systemFont != null ? new Color(0.4f, 0.9f, 0.4f) : new Color(1, 0.8f, 0.25f);
                    _expanded.TryGetValue(name, out bool expanded);
                    _expanded[name] = EditorGUILayout.Foldout(expanded, name + $"  ({layers.Count}개 레이어)", true);
                    GUI.contentColor = original;
                    if (GUILayout.Button("이름 복사", GUILayout.Width(75))) EditorGUIUtility.systemCopyBuffer = name;
                }
                EditorGUILayout.LabelField("시스템 상태", systemFont != null ? "파일 확인됨 · " + systemFont.DisplayName : "정확한 폰트를 찾지 못함 / 활성화 여부 미확인");
                if (systemFont != null) EditorGUILayout.SelectableLabel(systemFont.FilePath, EditorStyles.miniLabel, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                else {
                    string[] candidates = _fonts.FindCandidates(name);
                    if (candidates.Length > 0) EditorGUILayout.LabelField("이름 후보", string.Join(", ", candidates));
                }
                EditorGUI.BeginChangeCheck();
                mapping.LegacyFont = (Font)EditorGUILayout.ObjectField("Legacy Font", mapping.LegacyFont, typeof(Font), false);
                mapping.TMPFont = (TMP_FontAsset)EditorGUILayout.ObjectField("TMP Font Asset", mapping.TMPFont, typeof(TMP_FontAsset), false);
                if (EditorGUI.EndChangeCheck()) settings.SaveSettings();
                string folder = (settings.OutputFolder ?? "").Replace('\\', '/').TrimEnd('/');
                bool validFolder = PSDFontUtility.FolderError(folder) == null;
                bool canImportSystemFont = systemFont != null && systemFont.CanImport;
                using (new EditorGUILayout.HorizontalScope()) {
                    using (new EditorGUI.DisabledScope(!validFolder || !canImportSystemFont)) {
                        if (GUILayout.Button(new GUIContent("시스템 폰트 가져오기(레거시)", "시스템 폰트를 저장 폴더로 가져와 Legacy Font에 연결합니다."))) {
                            try {
                                Font font = _fonts.ImportFont(systemFont, folder);
                                if (font != null) { mapping.LegacyFont = font; settings.SaveSettings(); }
                            }
                            catch (Exception exception) { _error = exception.Message; }
                        }
                    }
                    using (new EditorGUI.DisabledScope(!validFolder || mapping.LegacyFont == null && !canImportSystemFont)) {
                        if (GUILayout.Button(new GUIContent("시스템 폰트 가져오기(TMP)", "연결된 Legacy Font가 있으면 재사용하고, 없으면 시스템 폰트를 가져온 뒤 TMP Font Asset을 생성·연결합니다."))) {
                            try {
                                Font font = mapping.LegacyFont != null ? mapping.LegacyFont : _fonts.ImportFont(systemFont, folder);
                                if (font != null) {
                                    mapping.LegacyFont = font;
                                    settings.SaveSettings();
                                    mapping.TMPFont = _fonts.CreateTMPFont(font, folder);
                                    settings.SaveSettings();
                                }
                            }
                            catch (Exception exception) { _error = exception.Message; }
                        }
                    }
                }
                EditorGUILayout.LabelField("Legacy 적용", FontResult(name, PSDTextOutput.Legacy, settings));
                EditorGUILayout.LabelField("TMP 적용", FontResult(name, PSDTextOutput.TextMeshPro, settings));
                int imageOnly = layers.Count(layer => layer.Text?.CanConvert != true);
                if (imageOnly > 0) EditorGUILayout.LabelField("서식 제한", $"{imageOnly}개 텍스트 레이어는 폰트와 관계없이 이미지로 유지");
                if (_expanded[name]) foreach (PSDLayerData layer in layers) EditorGUILayout.LabelField($"  {layer.Name}  [Layer ID: {layer.Id}]", EditorStyles.miniLabel);
            }
        }
        void DrawConvertButton(string label, PSDTextOutput output, PSDConverterSettings settings, bool stale) {
            string reason = PSDUIConverter.GetBlockingReason(_document, _layers, settings, output);
            using (new EditorGUI.DisabledScope(reason != null || stale)) {
                if (GUILayout.Button(new GUIContent(label, reason ?? "현재 씬 루트에 새 Canvas 생성"), GUILayout.Height(34))) {
                    try { PSDUIConverter.Convert(_document, _layers, settings, output, out _report); _error = null; }
                    catch (Exception exception) { _error = exception.Message; }
                }
            }
        }
        static void DrawSettings(PSDConverterSettings settings) {
            EditorGUI.BeginChangeCheck();
            DrawSectionDivider();
            EditorGUILayout.LabelField("Canvas Settings", EditorStyles.boldLabel);
            settings.UsePSDResolution = EditorGUILayout.Toggle("PSD 크기 사용", settings.UsePSDResolution);
            using (new EditorGUI.DisabledScope(settings.UsePSDResolution)) settings.ReferenceResolution = EditorGUILayout.Vector2IntField("Reference Resolution", settings.ReferenceResolution);
            DrawSectionDivider();
            EditorGUILayout.LabelField("시스템 폰트 가져오기 설정", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope()) {
                settings.OutputFolder = EditorGUILayout.TextField("저장 폴더", settings.OutputFolder);
                if (GUILayout.Button("선택", GUILayout.Width(50))) {
                    string selected = EditorUtility.OpenFolderPanel("폰트 저장 폴더", Application.dataPath, "");
                    if (!string.IsNullOrEmpty(selected)) {
                        string relative = FileUtil.GetProjectRelativePath(selected);
                        if (PSDFontUtility.FolderError(relative) == null) { settings.OutputFolder = relative; GUI.changed = true; }
                        else EditorUtility.DisplayDialog("저장 폴더", "프로젝트의 Assets 아래 폴더를 선택해 주세요.", "확인");
                    }
                }
                using (new EditorGUI.DisabledScope(PSDFontUtility.FolderError(settings.OutputFolder) != null || !Directory.Exists(settings.OutputFolder))) if (GUILayout.Button("열기", GUILayout.Width(50))) EditorUtility.RevealInFinder(Path.GetFullPath(settings.OutputFolder));
            }
            string folderError = PSDFontUtility.FolderError(settings.OutputFolder);
            if (folderError != null) EditorGUILayout.HelpBox(folderError + " 폰트 가져오기·생성에만 적용됩니다.", MessageType.Warning);
            DrawSectionDivider();
            EditorGUILayout.LabelField("Default Fallback Fonts", EditorStyles.boldLabel);
            settings.DefaultLegacyFont = (Font)EditorGUILayout.ObjectField("Legacy Font", settings.DefaultLegacyFont, typeof(Font), false);
            settings.DefaultTMPFont = (TMP_FontAsset)EditorGUILayout.ObjectField("TMP Font Asset", settings.DefaultTMPFont, typeof(TMP_FontAsset), false);
            settings.MissingFontPolicy = (PSDMissingFontPolicy)EditorGUILayout.EnumPopup("미연결 폰트 처리", settings.MissingFontPolicy);
            if (EditorGUI.EndChangeCheck()) settings.SaveSettings();
        }
        static void DrawSectionDivider() {
            EditorGUILayout.Space(6);
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? new Color(0.36f, 0.36f, 0.36f) : new Color(0.6f, 0.6f, 0.6f));
            EditorGUILayout.Space(6);
        }
        static string FontResult(string name, PSDTextOutput output, PSDConverterSettings settings) {
            UnityEngine.Object font = settings.ResolveFont(name, output);
            if (font == null) return settings.MissingFontPolicy == PSDMissingFontPolicy.StopConversion ? "미연결 · 변환 중단" : "미연결 · 이미지 유지";
            PSDFontMapping mapping = settings.GetMapping(name);
            bool direct = output == PSDTextOutput.Legacy ? mapping.LegacyFont != null : mapping.TMPFont != null;
            return font.name + (direct ? " · 연결된 에셋" : " · 기본 대체 폰트");
        }
        static bool IsPSD(string path) { return !string.IsNullOrEmpty(path) && path.EndsWith(".psd", StringComparison.OrdinalIgnoreCase); }
    }
}

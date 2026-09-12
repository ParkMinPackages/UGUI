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
            window.titleContent = new GUIContent("PSD 변환기");
            window.minSize = new Vector2(540, 550);
            window.Show();
        }
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider() {
            return new SettingsProvider("Project/ParkMinPackages/PSD Converter", SettingsScope.Project) {
                label = "PSD 변환기",
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
            titleContent = new GUIContent("PSD 변환기");
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
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("- PSD파일선택 -", SectionTitleStyle);
            using (new EditorGUILayout.HorizontalScope()) {
                EditorGUILayout.PrefixLabel("PSD 에셋");
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
            if (_document != null) {
                DrawSettings(settings);
                EditorGUILayout.Space(6);
                if (GUILayout.Button(new GUIContent("폰트 목록 새로고침", "검색 목록만 갱신하며 저장된 폰트 연결은 변경하지 않습니다."), GUILayout.Width(150))) {
                    try { _fonts.Refresh(); _error = null; }
                    catch (Exception exception) { _error = exception.Message; }
                }
                foreach (KeyValuePair<string, List<PSDLayerData>> usage in _usedFonts) DrawFont(usage.Key, usage.Value, settings);
                if (_usedFonts.Count == 0) EditorGUILayout.LabelField("분석 가능한 텍스트 폰트가 없습니다.");
                HashSet<string> layerWarnings = new HashSet<string>(_usedFonts.Values.SelectMany(layers => layers).Where(layer => layer.Text?.CanConvert == false && layer.Warning != null).Select(layer => layer.Name + ": " + layer.Warning));
                List<string> warnings = _document.Warnings.Where(warning => !layerWarnings.Contains(warning)).ToList();
                if (warnings.Count > 0) {
                    DrawSectionDivider();
                    _warningsExpanded = EditorGUILayout.Foldout(_warningsExpanded, $"분석 경고 {warnings.Count}개", true);
                    if (_warningsExpanded) foreach (string warning in warnings) EditorGUILayout.HelpBox(warning, MessageType.Warning);
                }
                EditorGUILayout.HelpBox("텍스트 렌더러의 글자 치수 차이로 배치가 달라질 수 있습니다. 혼합 스타일, 여러 줄/문단, 워프, 레이어 효과·마스크는 이미지로 유지합니다.", MessageType.Info);
            }
            if (!string.IsNullOrEmpty(_report)) {
                DrawSectionDivider();
                EditorGUILayout.LabelField("변환 결과", SectionTitleStyle);
                EditorGUILayout.HelpBox(_report, MessageType.Info);
            }
            bool stale = _document != null && _analyzedHash != AssetDatabase.GetAssetDependencyHash(_document.AssetPath);
            if (stale) EditorGUILayout.HelpBox("PSD 또는 Importer 설정이 변경됐습니다. 다시 분석해 주세요.", MessageType.Warning);
            EditorGUILayout.EndScrollView();
            DrawSectionDivider();
            DrawConvertButton(settings, stale);
            EditorGUILayout.Space(8);
        }

        // - Internals -
        static GUIStyle SectionTitleStyle => new GUIStyle(EditorStyles.boldLabel) { fontSize = Mathf.RoundToInt((EditorStyles.label.fontSize > 0 ? EditorStyles.label.fontSize : 12) * 1.2f), fontStyle = FontStyle.Bold, fixedHeight = 0, wordWrap = true };
        [SerializeField] UnityEngine.Object _source;
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
                "PSD 에셋",
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
                EditorUtility.DisplayProgressBar("PSD 변환기", "레이어 및 폰트 정보 분석", 0.2f);
                PSDDocument document = PSDDocumentReader.Read(path);
                List<PSDImportLayer> layers = PSDUIConverter.ReadImportedLayers(document);
                EditorUtility.DisplayProgressBar("PSD 변환기", "시스템 및 프로젝트 폰트 검색", 0.6f);
                _fonts.Refresh();
                foreach (PSDLayerData layer in document.Layers.Where(layer => layer.Text != null)) {
                    foreach (string name in layer.Text.Fonts.Distinct()) {
                        if (!_usedFonts.TryGetValue(name, out List<PSDLayerData> users)) _usedFonts[name] = users = new List<PSDLayerData>();
                        users.Add(layer);
                    }
                }
                _document = document;
                _layers = layers;
                _analyzedHash = AssetDatabase.GetAssetDependencyHash(path);
            }
            catch (Exception exception) { _error = exception.Message; }
            finally { EditorUtility.ClearProgressBar(); Repaint(); }
        }
        void DrawFont(string name, List<PSDLayerData> layers, PSDConverterSettings settings) {
            PSDFontMapping mapping = settings.GetMapping(name);
            bool legacy = settings.TextOutput == PSDTextOutput.Legacy;
            PSDFontInfo systemFont = _fonts.FindSystemFont(name);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                _expanded.TryGetValue(name, out bool expanded);
                using (new EditorGUILayout.HorizontalScope()) {
                    _expanded[name] = EditorGUILayout.Foldout(expanded, $"{layers.Count}개 레이어", true);
                    int unsupportedCount = layers.Count(layer => layer.Text?.CanConvert != true);
                    if (unsupportedCount > 0) {
                        Color original = GUI.contentColor;
                        GUI.contentColor = new Color(1, 0.8f, 0.25f);
                        GUILayout.Label($"({unsupportedCount}개변환미지원)", GUILayout.ExpandWidth(false));
                        GUI.contentColor = original;
                    }
                }
                if (_expanded[name]) {
                    EditorGUILayout.LabelField("이미지로 유지할 레이어를 선택하세요.", EditorStyles.miniLabel);
                    foreach (PSDLayerData layer in layers) {
                        bool unsupported = layer.Text?.CanConvert != true;
                        bool keepImage = settings.IsImageLayer(_document.AssetPath, layer.Id);
                        using (new EditorGUILayout.HorizontalScope()) {
                            using (new EditorGUI.DisabledScope(unsupported)) {
                                EditorGUI.BeginChangeCheck();
                                bool selected = EditorGUILayout.ToggleLeft(new GUIContent(layer.Name, $"레이어 ID: {layer.Id}"), unsupported || keepImage);
                                if (EditorGUI.EndChangeCheck()) settings.SetImageLayer(_document.AssetPath, layer.Id, selected);
                            }
                            if (unsupported) {
                                Color original = GUI.contentColor;
                                GUI.contentColor = new Color(1, 0.8f, 0.25f);
                                GUILayout.Label(new GUIContent("(변환미지원)", layer.Warning ?? "지원하지 않는 텍스트 서식입니다."), GUILayout.Width(100));
                                GUI.contentColor = original;
                            }
                        }
                    }
                }
                using (new EditorGUILayout.HorizontalScope()) {
                    EditorGUILayout.LabelField("PSD 폰트 이름", name);
                    if (GUILayout.Button("이름 복사", GUILayout.Width(75))) EditorGUIUtility.systemCopyBuffer = name;
                }
                string folder = (settings.OutputFolder ?? "").Replace('\\', '/').TrimEnd('/');
                string folderError = PSDFontUtility.FolderError(folder);
                bool canImportSystemFont = systemFont != null && systemFont.CanImport;
                using (new EditorGUILayout.HorizontalScope()) {
                    EditorGUILayout.PrefixLabel("적용할 폰트 에셋");
                    EditorGUI.BeginChangeCheck();
                    if (legacy) mapping.LegacyFont = (Font)EditorGUILayout.ObjectField(mapping.LegacyFont, typeof(Font), false, GUILayout.MinWidth(80));
                    else mapping.TMPFont = (TMP_FontAsset)EditorGUILayout.ObjectField(mapping.TMPFont, typeof(TMP_FontAsset), false, GUILayout.MinWidth(80));
                    if (EditorGUI.EndChangeCheck()) settings.SaveSettings();
                    bool connected = legacy ? mapping.LegacyFont != null : mapping.TMPFont != null;
                    Font[] projectFonts = _fonts.FindProjectFonts(name);
                    Font projectFont = mapping.LegacyFont != null ? mapping.LegacyFont : projectFonts.Length == 1 ? projectFonts[0] : null;
                    TMP_FontAsset[] tmpFonts = !legacy ? _fonts.FindTMPFonts(name, projectFont) : Array.Empty<TMP_FontAsset>();
                    TMP_FontAsset existingTMP = tmpFonts.Length == 1 ? tmpFonts[0] : null;
                    bool chooseExisting = legacy ? projectFonts.Length > 1 : tmpFonts.Length > 1 || tmpFonts.Length == 0 && projectFont == null && projectFonts.Length > 1;
                    bool canConnect = legacy ? projectFont != null : existingTMP != null;
                    bool canCreate = !legacy && projectFont != null;
                    bool importSystem = !connected && !chooseExisting && !canConnect && !canCreate && canImportSystemFont;
                    string buttonText = connected ? "폰트 연결됨" : chooseExisting ? "기존 폰트 선택" : canConnect ? "기존 폰트 연결" : canCreate ? "TMP 폰트 준비 및 연결" : importSystem ? "시스템 폰트 가져오기" : "폰트를 직접 선택해 주세요";
                    string tooltip = connected ? "적용할 폰트 에셋이 이미 연결되어 있습니다." : chooseExisting ? "후보 에셋을 선택하세요. TMP 원본 Font를 선택한 경우 다음 클릭에서 TMP 폰트를 준비합니다." : canConnect ? "프로젝트의 기존 폰트 에셋을 연결합니다." : canCreate ? "사용 가능한 TMP 에셋을 재사용하거나 생성하여 연결합니다." : importSystem ? "시스템 폰트를 가져와 선택한 텍스트 방식으로 연결합니다." : "자동으로 제안할 수 있는 폰트를 찾지 못했습니다. 폰트 선택기에서 직접 선택해 주세요.";
                    if (!connected && !chooseExisting && !canConnect && folderError != null) tooltip += "\n" + folderError;
                    float buttonWidth = Mathf.Clamp(position.width * 0.42f, 200, 420);
                    if (importSystem) {
                        string displayPath = systemFont.FilePath.Replace('\\', '/');
                        tooltip += "\n" + displayPath;
                        buttonText += " (" + displayPath + ")";
                        while (displayPath.Length > 4 && GUI.skin.button.CalcSize(new GUIContent(buttonText)).x > buttonWidth) {
                            displayPath = displayPath.Substring(1);
                            buttonText = "시스템 폰트 가져오기 (…" + displayPath + ")";
                        }
                    }
                    using (new EditorGUI.DisabledScope(connected || !chooseExisting && !canConnect && (!(canCreate || importSystem) || folderError != null))) {
                        if (GUILayout.Button(new GUIContent(buttonText, tooltip), GUILayout.Width(buttonWidth))) {
                            try {
                                if (chooseExisting) {
                                    GenericMenu menu = new GenericMenu();
                                    if (!legacy && tmpFonts.Length > 0) {
                                        foreach (TMP_FontAsset candidate in tmpFonts) menu.AddItem(new GUIContent(candidate.name + " — " + AssetDatabase.GetAssetPath(candidate).Replace('/', '／')), false, () => { if (candidate == null) return; mapping.TMPFont = candidate; settings.SaveSettings(); Repaint(); });
                                    }
                                    else {
                                        foreach (Font candidate in projectFonts) menu.AddItem(new GUIContent(candidate.name + " — " + AssetDatabase.GetAssetPath(candidate).Replace('/', '／')), false, () => { if (candidate == null) return; mapping.LegacyFont = candidate; settings.SaveSettings(); Repaint(); });
                                    }
                                    menu.DropDown(GUILayoutUtility.GetLastRect());
                                }
                                else if (canConnect) {
                                    if (legacy) mapping.LegacyFont = projectFont;
                                    else mapping.TMPFont = existingTMP;
                                    settings.SaveSettings();
                                }
                                else {
                                    Font font = canCreate ? projectFont : _fonts.ImportFont(systemFont, folder);
                                    if (font != null) {
                                        mapping.LegacyFont = font;
                                        settings.SaveSettings();
                                        if (!legacy) {
                                            mapping.TMPFont = _fonts.CreateTMPFont(font, folder);
                                            settings.SaveSettings();
                                        }
                                        _fonts.Refresh();
                                    }
                                }
                            }
                            catch (Exception exception) { _error = exception.Message; }
                        }
                    }
                }
                UnityEngine.Object resolvedFont = settings.ResolveFont(name, settings.TextOutput);
                bool direct = legacy ? mapping.LegacyFont != null : mapping.TMPFont != null;
                int textLayers = layers.Count(layer => layer.Text?.CanConvert == true && !settings.IsImageLayer(_document.AssetPath, layer.Id));
                using (new EditorGUILayout.HorizontalScope()) {
                    EditorGUILayout.PrefixLabel("적용될 결과");
                    Color original = GUI.contentColor;
                    GUI.contentColor = textLayers == 0 ? original : resolvedFont == null ? new Color(1, 0.3f, 0.3f) : direct ? new Color(0.4f, 0.9f, 0.4f) : new Color(1, 0.8f, 0.25f);
                    EditorGUILayout.LabelField(textLayers == 0 ? "모든 레이어를 이미지로 유지" : FontResult(name, settings.TextOutput, settings) + (textLayers < layers.Count ? $" · 텍스트 대상 {textLayers}개" : ""));
                    GUI.contentColor = original;
                }
            }
        }
        void DrawConvertButton(PSDConverterSettings settings, bool stale) {
            PSDTextOutput output = settings.TextOutput;
            string reason = PSDUIConverter.GetBlockingReason(_document, _layers, settings, output);
            using (new EditorGUI.DisabledScope(reason != null || stale)) {
                if (GUILayout.Button(new GUIContent("변환하고 현재 씬에 배치", reason ?? "현재 씬 루트에 새 캔버스 생성"), GUILayout.Height(34))) {
                    try { PSDUIConverter.Convert(_document, _layers, settings, output, out _report); _error = null; }
                    catch (Exception exception) { _error = exception.Message; }
                }
            }
        }
        static void DrawSettings(PSDConverterSettings settings) {
            EditorGUI.BeginChangeCheck();
            DrawSectionDivider();
            EditorGUILayout.LabelField("- 캔버스 설정 -", SectionTitleStyle);
            settings.UsePSDResolution = EditorGUILayout.Toggle("PSD 크기 사용", settings.UsePSDResolution);
            using (new EditorGUI.DisabledScope(settings.UsePSDResolution)) settings.ReferenceResolution = EditorGUILayout.Vector2IntField("기준 해상도", settings.ReferenceResolution);
            DrawSectionDivider();
            EditorGUILayout.LabelField("- PSD Text -> Unity Text 설정 -", SectionTitleStyle);
            settings.TextOutput = (PSDTextOutput)EditorGUILayout.EnumPopup("텍스트 방식", settings.TextOutput);
            settings.MissingFontPolicy = (PSDMissingFontPolicy)EditorGUILayout.EnumPopup("미연결 폰트 처리", settings.MissingFontPolicy);
            if (settings.MissingFontPolicy == PSDMissingFontPolicy.UseDefaultFallback) {
                if (settings.TextOutput == PSDTextOutput.Legacy) settings.DefaultLegacyFont = (Font)EditorGUILayout.ObjectField("기본대체폰트 설정", settings.DefaultLegacyFont, typeof(Font), false);
                else settings.DefaultTMPFont = (TMP_FontAsset)EditorGUILayout.ObjectField("기본대체폰트 설정", settings.DefaultTMPFont, typeof(TMP_FontAsset), false);
            }
            EditorGUILayout.Space(6);
            using (new EditorGUILayout.HorizontalScope()) {
                GUILayout.Label("시스템폰트가져오기 기능 사용시 저장할 경로", GUILayout.ExpandWidth(false));
                settings.OutputFolder = EditorGUILayout.TextField(settings.OutputFolder);
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
            if (font == null) return "폰트 적용 불가 · 이미지로 유지";
            PSDFontMapping mapping = settings.GetMapping(name);
            bool direct = output == PSDTextOutput.Legacy ? mapping.LegacyFont != null : mapping.TMPFont != null;
            return (direct ? "연결된 폰트 적용 · " : "기본 대체 폰트 적용 · ") + font.name;
        }
        static bool IsPSD(string path) { return !string.IsNullOrEmpty(path) && path.EndsWith(".psd", StringComparison.OrdinalIgnoreCase); }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace ParkMinPackages.UGUI.Editor
{
    public sealed class PSDFontInfo
    {
        public string PostScriptName;
        public string DisplayName;
        public string FilePath;
        public bool CanImport => File.Exists(FilePath) && (Path.GetExtension(FilePath).Equals(".ttf", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(FilePath).Equals(".otf", StringComparison.OrdinalIgnoreCase)) && FilePath.IndexOf("CoreSync", StringComparison.OrdinalIgnoreCase) < 0 && FilePath.IndexOf("livetype", StringComparison.OrdinalIgnoreCase) < 0;
    }

    public sealed class PSDFontUtility
    {
        // - Public Methods -
        public void Refresh() {
            _systemFonts.Clear();
            _projectFonts.Clear();
            _tmpFonts.Clear();
            _installedNames = Font.GetOSInstalledFontNames();
            IEnumerable<string> paths = Font.GetPathsToOSFonts();
            string[] folders = { Environment.GetFolderPath(Environment.SpecialFolder.Fonts), Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft/Windows/Fonts") };
            foreach (string folder in folders.Where(Directory.Exists)) paths = paths.Concat(Directory.EnumerateFiles(folder));
            foreach (string path in paths.Distinct(StringComparer.OrdinalIgnoreCase)) {
                foreach (PSDFontInfo info in ReadFontNames(path)) {
                    if (!_systemFonts.ContainsKey(info.PostScriptName) || !_systemFonts[info.PostScriptName].CanImport && info.CanImport) _systemFonts[info.PostScriptName] = info;
                }
            }
            foreach (string guid in AssetDatabase.FindAssets("t:Font")) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Font font = AssetDatabase.LoadAssetAtPath<Font>(path);
                if (font == null) continue;
                PSDFontInfo[] names = ReadFontNames(path).ToArray();
                if (names.Length != 1) continue;
                string name = names[0].PostScriptName;
                if (!_projectFonts.TryGetValue(name, out List<Font> fonts)) _projectFonts[name] = fonts = new List<Font>();
                if (!fonts.Contains(font)) fonts.Add(font);
            }
            foreach (string guid in AssetDatabase.FindAssets("t:TMP_FontAsset")) {
                TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
                if (font != null) _tmpFonts.Add(font);
            }
        }

        public PSDFontInfo FindSystemFont(string name) { _systemFonts.TryGetValue(name, out PSDFontInfo font); return font; }
        public string[] FindCandidates(string name) {
            string family = new string(name.TakeWhile(c => c != '-').ToArray());
            return _installedNames.Where(candidate => candidate.Replace(" ", "").IndexOf(family, StringComparison.OrdinalIgnoreCase) >= 0).Take(4).ToArray();
        }
        public void AutoConnect(PSDFontMapping mapping) {
            if (mapping.LegacyFont == null && _projectFonts.TryGetValue(mapping.PostScriptName, out List<Font> fonts) && fonts.Count == 1) mapping.LegacyFont = fonts[0];
            if (mapping.TMPFont != null) return;
            List<TMP_FontAsset> matches = _tmpFonts.Where(font => font.sourceFontFile != null && (font.sourceFontFile == mapping.LegacyFont || ReadFontNames(AssetDatabase.GetAssetPath(font.sourceFontFile)).Any(info => info.PostScriptName == mapping.PostScriptName))).ToList();
            if (matches.Count == 1) mapping.TMPFont = matches[0];
        }
        public Font ImportFont(PSDFontInfo info, string folder) {
            if (info == null || !info.CanImport) throw new InvalidOperationException("직접 가져올 수 있는 TTF/OTF 원본 파일이 없습니다.");
            string error = FolderError(folder);
            if (error != null) throw new InvalidOperationException(error);
            if (!EditorUtility.DisplayDialog("시스템 폰트 가져오기", info.DisplayName + "\n" + info.FilePath + "\n\n이 폰트 파일을 Unity 프로젝트로 복사합니다. 프로젝트 및 빌드에 포함할 수 있는 라이선스를 확보했는지 확인해 주세요.", "확인 후 가져오기", "취소")) return null;
            string hash = FileHash(info.FilePath);
            foreach (string guid in AssetDatabase.FindAssets("t:Font")) {
                string existing = AssetDatabase.GUIDToAssetPath(guid);
                if (File.Exists(existing) && string.Equals(Path.GetExtension(existing), Path.GetExtension(info.FilePath), StringComparison.OrdinalIgnoreCase) && FileHash(existing) == hash) return AssetDatabase.LoadAssetAtPath<Font>(existing);
            }
            EnsureFolder(folder);
            string path = AssetDatabase.GenerateUniqueAssetPath(folder + "/" + SafeName(info.PostScriptName) + Path.GetExtension(info.FilePath).ToLowerInvariant());
            File.Copy(info.FilePath, path, false);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            Font imported = AssetDatabase.LoadAssetAtPath<Font>(path);
            if (imported == null) throw new InvalidOperationException("복사한 파일을 Unity Font로 읽지 못했습니다: " + path);
            return imported;
        }
        public TMP_FontAsset CreateTMPFont(Font source, string folder) {
            if (source == null || !AssetDatabase.Contains(source)) throw new InvalidOperationException("프로젝트의 Font 에셋을 먼저 연결해 주세요.");
            TMP_FontAsset existing = _tmpFonts.FirstOrDefault(font => font != null && font.sourceFontFile == source && font.atlasPopulationMode == AtlasPopulationMode.Dynamic && font.atlasWidth == 1024 && font.atlasHeight == 1024 && font.atlasPadding == 9 && font.faceInfo.pointSize == 90);
            if (existing != null) return existing;
            string error = FolderError(folder);
            if (error != null) throw new InvalidOperationException(error);
            if (Shader.Find("TextMeshPro/Mobile/Distance Field") == null) throw new InvalidOperationException("TextMeshPro Essential Resources를 먼저 가져와 주세요. Window > TextMeshPro > Import TMP Essential Resources");
            EnsureFolder(folder);
            TMP_FontAsset font = TMP_FontAsset.CreateFontAsset(source);
            if (font == null) throw new InvalidOperationException("TMP 폰트 생성에 실패했습니다. 원본 Font의 Include Font Data 설정을 확인해 주세요.");
            string path = AssetDatabase.GenerateUniqueAssetPath(folder + "/" + SafeName(source.name) + " SDF.asset");
            font.name = Path.GetFileNameWithoutExtension(path);
            AssetDatabase.CreateAsset(font, path);
            font.material.name = font.name + " Material";
            AssetDatabase.AddObjectToAsset(font.material, font);
            foreach (Texture2D atlas in font.atlasTextures) {
                atlas.name = font.name + " Atlas";
                AssetDatabase.AddObjectToAsset(atlas, font);
            }
            EditorUtility.SetDirty(font);
            AssetDatabase.SaveAssetIfDirty(font);
            _tmpFonts.Add(font);
            return font;
        }
        public static string FolderError(string folder) {
            try {
                if (string.IsNullOrWhiteSpace(folder)) return "폰트를 생성할 폴더를 지정해 주세요.";
                string normalized = folder.Replace('\\', '/').TrimEnd('/');
                if (normalized != "Assets" && !normalized.StartsWith("Assets/", StringComparison.Ordinal)) return "Assets 아래의 폴더를 지정해 주세요.";
                string full = Path.GetFullPath(normalized);
                string assets = Path.GetFullPath(Application.dataPath);
                if (!full.Equals(assets, StringComparison.OrdinalIgnoreCase) && !full.StartsWith(assets + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) return "Assets 밖의 폴더는 사용할 수 없습니다.";
                if (File.Exists(full)) return "파일이 아닌 폴더를 지정해 주세요.";
                DirectoryInfo directory = new DirectoryInfo(full);
                while (directory != null && directory.FullName.Length >= assets.Length) {
                    if (directory.Exists && (directory.Attributes & (FileAttributes.ReparsePoint | FileAttributes.ReadOnly)) != 0) return "읽기 전용 또는 링크 폴더는 사용할 수 없습니다.";
                    directory = directory.Parent;
                }
                return null;
            }
            catch (Exception exception) when (exception is ArgumentException || exception is IOException || exception is NotSupportedException || exception is UnauthorizedAccessException) { return "유효하지 않은 저장 폴더입니다."; }
        }
        public static string SafeName(string name) { return string.Concat(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c)); }

        // - Internals -
        readonly Dictionary<string, PSDFontInfo> _systemFonts = new Dictionary<string, PSDFontInfo>(StringComparer.Ordinal);
        readonly Dictionary<string, List<Font>> _projectFonts = new Dictionary<string, List<Font>>(StringComparer.Ordinal);
        readonly List<TMP_FontAsset> _tmpFonts = new List<TMP_FontAsset>();
        string[] _installedNames = Array.Empty<string>();

        static void EnsureFolder(string folder) {
            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
        }
        static string FileHash(string path) {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(path)) return BitConverter.ToString(sha.ComputeHash(stream));
        }
        static IEnumerable<PSDFontInfo> ReadFontNames(string path) {
            List<PSDFontInfo> result = new List<PSDFontInfo>();
            try {
                string extension = Path.GetExtension(path).ToLowerInvariant();
                if (extension != ".ttf" && extension != ".otf" && extension != ".ttc" && extension != ".otc") return result;
                using (BinaryReader reader = new BinaryReader(File.OpenRead(path))) {
                    string signature = Encoding.ASCII.GetString(reader.ReadBytes(4));
                    List<uint> offsets = new List<uint> { 0 };
                    if (signature == "ttcf") {
                        Read32(reader);
                        uint count = Read32(reader);
                        if (count > 256) return result;
                        offsets.Clear();
                        for (int i = 0; i < count; i++) offsets.Add(Read32(reader));
                    }
                    foreach (uint offset in offsets) {
                        reader.BaseStream.Position = offset + 4;
                        int tables = Read16(reader);
                        reader.BaseStream.Position += 6;
                        uint nameOffset = 0;
                        if (tables > 1024) continue;
                        for (int i = 0; i < tables; i++) {
                            string tag = Encoding.ASCII.GetString(reader.ReadBytes(4));
                            Read32(reader);
                            uint tableOffset = Read32(reader);
                            Read32(reader);
                            if (tag == "name") nameOffset = tableOffset;
                        }
                        if (nameOffset == 0) continue;
                        reader.BaseStream.Position = nameOffset;
                        Read16(reader);
                        int count = Read16(reader);
                        int storage = Read16(reader);
                        string postScript = null, display = null;
                        for (int i = 0; i < count; i++) {
                            int platform = Read16(reader);
                            Read16(reader);
                            int language = Read16(reader);
                            int id = Read16(reader);
                            int length = Read16(reader);
                            int textOffset = Read16(reader);
                            long next = reader.BaseStream.Position;
                            if ((id == 6 || id == 4) && (platform == 0 || platform == 3 || platform == 1)) {
                                reader.BaseStream.Position = nameOffset + storage + textOffset;
                                byte[] bytes = reader.ReadBytes(length);
                                string value = (platform == 0 || platform == 3 ? Encoding.BigEndianUnicode : Encoding.ASCII).GetString(bytes).TrimEnd('\0');
                                if (id == 6 && (postScript == null || platform == 3 && language == 1033)) postScript = value;
                                if (id == 4 && (display == null || platform == 3 && language == 1033)) display = value;
                            }
                            reader.BaseStream.Position = next;
                        }
                        if (!string.IsNullOrEmpty(postScript)) result.Add(new PSDFontInfo { PostScriptName = postScript, DisplayName = display ?? postScript, FilePath = path });
                    }
                }
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is ArgumentException || exception is NotSupportedException) { }
            return result;
        }
        static int Read16(BinaryReader reader) { return reader.ReadByte() << 8 | reader.ReadByte(); }
        static uint Read32(BinaryReader reader) { return (uint)Read16(reader) << 16 | (uint)Read16(reader); }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ParkMinPackages.UGUI.Editor
{
    public sealed class PSDDocument
    {
        public string AssetPath;
        public Vector2Int Size;
        public readonly List<PSDLayerData> Layers = new List<PSDLayerData>();
        public readonly List<string> Warnings = new List<string>();
    }

    public sealed class PSDLayerData
    {
        public int Id;
        public string Name;
        public Rect Bounds;
        public float Opacity;
        public bool Visible;
        public bool IsText;
        public bool HasEffects;
        public string BlendMode;
        public PSDTextData Text;
        public string Warning;
    }

    public sealed class PSDTextData
    {
        public string Content;
        public string[] Fonts = Array.Empty<string>();
        public float FontSize;
        public float Rotation;
        public float HorizontalScale = 1;
        public Color Color = Color.white;
        public int Alignment;
        public bool Bold;
        public bool Italic;
        public bool CanConvert;
    }

    public static class PSDDocumentReader
    {
        // - Public Methods -
        public static PSDDocument Read(string assetPath) {
            using (FileStream stream = File.OpenRead(assetPath))
            using (BigEndian reader = new BigEndian(stream)) {
                if (reader.Tag() != "8BPS") throw new InvalidDataException("PSD 또는 PSB 파일이 아닙니다.");
                int version = reader.U16();
                if (version != 1 && version != 2) throw new NotSupportedException("지원하지 않는 PSD 버전입니다.");
                reader.Skip(8);
                int height = reader.I32();
                int width = reader.I32();
                int depth = reader.U16();
                int colorMode = reader.U16();
                if (width < 1 || height < 1) throw new InvalidDataException("잘못된 문서 크기입니다.");
                if (depth != 8 || colorMode != 3) throw new NotSupportedException("현재 텍스트 분석은 RGB / 8 bits PSD·PSB를 지원합니다. Photoshop에서 문서 모드를 변환해 주세요.");
                PSDDocument document = new PSDDocument { AssetPath = assetPath, Size = new Vector2Int(width, height) };
                reader.Skip(reader.U32());
                reader.Skip(reader.U32());
                long sectionLength = reader.Length(version == 2);
                long sectionEnd = reader.Position + sectionLength;
                if (sectionLength == 0) return document;
                long layerLength = reader.Length(version == 2);
                long layerEnd = reader.Position + layerLength;
                if (layerEnd > sectionEnd || sectionEnd > stream.Length) throw new InvalidDataException("잘못된 PSD 레이어 섹션 길이입니다.");
                if (layerLength == 0) return document;
                int count = Math.Abs((int)reader.I16());
                for (int i = 0; i < count; i++) {
                    PSDLayerData layer = new PSDLayerData();
                    int top = reader.I32();
                    int left = reader.I32();
                    int bottom = reader.I32();
                    int right = reader.I32();
                    layer.Bounds = Rect.MinMaxRect(left, top, right, bottom);
                    int channels = reader.U16();
                    reader.Skip(channels * (version == 2 ? 10L : 6L));
                    if (reader.Tag() != "8BIM") throw new InvalidDataException("잘못된 PSD 블렌드 시그니처입니다.");
                    layer.BlendMode = reader.Tag();
                    layer.Opacity = reader.Byte() / 255f;
                    if (reader.Byte() != 0) layer.HasEffects = true;
                    layer.Visible = (reader.Byte() & 2) == 0;
                    reader.Skip(1);
                    long extraLength = reader.U32();
                    long extraEnd = reader.Position + extraLength;
                    if (extraEnd > layerEnd) throw new InvalidDataException("잘못된 PSD 레이어 길이입니다.");
                    long maskLength = reader.U32();
                    if (maskLength > 0) layer.HasEffects = true;
                    reader.Skip(maskLength);
                    reader.Skip(reader.U32());
                    int nameLength = reader.Byte();
                    layer.Name = Encoding.Default.GetString(reader.Bytes(nameLength));
                    reader.Skip((4 - (nameLength + 1) % 4) % 4);
                    while (reader.Position + 12 <= extraEnd) {
                        string signature = reader.Tag();
                        if (signature != "8BIM" && signature != "8B64") break;
                        string key = reader.Tag();
                        long size = reader.Length(signature == "8B64" || version == 2 && LargeKeys.Contains(key));
                        long end = reader.Position + size;
                        if (end > extraEnd) throw new InvalidDataException("잘못된 PSD 추가 정보 길이입니다.");
                        if (key == "luni") layer.Name = reader.Unicode();
                        else if (key == "lyid") layer.Id = reader.I32();
                        else if (key == "lfx2" || key == "lrFX" || key == "lfxs" || key == "vmsk" || key == "vsms") layer.HasEffects = true;
                        else if (key == "TySh") {
                            layer.IsText = true;
                            try {
                                if (size > 16777216) throw new NotSupportedException("텍스트 메타데이터가 너무 큽니다.");
                                using (BigEndian textReader = new BigEndian(new MemoryStream(reader.Bytes((int)size)))) layer.Text = ReadText(textReader);
                            }
                            catch (Exception exception) when (exception is InvalidDataException || exception is NotSupportedException || exception is EndOfStreamException || exception is FormatException || exception is OverflowException) {
                                layer.Warning = "텍스트 분석 실패 → 이미지 유지: " + exception.Message;
                            }
                        }
                        reader.Position = end + size % 2;
                    }
                    reader.Position = extraEnd;
                    if (layer.Text != null && (layer.HasEffects || layer.BlendMode != "norm")) {
                        layer.Text.CanConvert = false;
                        layer.Warning = "레이어 효과·마스크·블렌딩이 있는 텍스트 → 이미지 유지";
                    }
                    if (layer.Text != null && !layer.Text.CanConvert && layer.Warning == null) layer.Warning = "혼합 스타일·변형·문단 서식이 있는 텍스트 → 이미지 유지";
                    if (layer.Warning != null) document.Warnings.Add(layer.Name + ": " + layer.Warning);
                    document.Layers.Add(layer);
                }
                return document;
            }
        }

        // - Internals -
        static readonly HashSet<string> LargeKeys = new HashSet<string> { "LMsk", "Lr16", "Lr32", "Layr", "Mt16", "Mt32", "Mtrn", "Alph", "FMsk", "lnk2", "FEid", "FXid", "PxSD" };

        static PSDTextData ReadText(BigEndian reader) {
            if (reader.U16() != 1) throw new NotSupportedException("TySh 버전");
            double xx = reader.Double(), xy = reader.Double(), yx = reader.Double(), yy = reader.Double();
            reader.Double();
            reader.Double();
            if (reader.U16() != 50 || reader.I32() != 16) throw new NotSupportedException("텍스트 Descriptor 버전");
            Dictionary<string, object> descriptor = reader.Descriptor();
            reader.U16();
            if (reader.I32() != 16) throw new NotSupportedException("Warp Descriptor 버전");
            Dictionary<string, object> warp = reader.Descriptor();
            byte[] engineBytes = Get(descriptor, "EngineData") as byte[];
            if (engineBytes == null) throw new NotSupportedException("EngineData가 없습니다.");
            Dictionary<string, object> root = new EngineReader(engineBytes).Read() as Dictionary<string, object>;
            object engine = Get(root, "EngineDict");
            object resource = Get(root, "ResourceDict");
            List<object> fontSet = List(Get(resource, "FontSet"));
            List<object> runs = List(Get(engine, "StyleRun", "RunArray"));
            List<object> lengths = List(Get(engine, "StyleRun", "RunLengthArray"));
            List<object> defaults = List(Get(resource, "StyleSheetSet"));
            Dictionary<string, object> defaultStyle = defaults.Count > 0 ? Get(defaults[0], "StyleSheetData") as Dictionary<string, object> : null;
            string content = Get(descriptor, "Txt ") as string ?? Get(engine, "Editor", "Text") as string;
            if (content == null) throw new InvalidDataException("문자열이 없습니다.");
            content = content.TrimEnd('\0');
            if (content.EndsWith("\r", StringComparison.Ordinal)) content = content.Substring(0, content.Length - 1);
            List<Dictionary<string, object>> styles = new List<Dictionary<string, object>>();
            HashSet<string> usedFonts = new HashSet<string>();
            int offset = 0;
            for (int i = 0; i < runs.Count; i++) {
                int length = i < lengths.Count ? (int)Number(lengths[i]) : content.Length;
                if (offset < content.Length && length > 0) {
                    Dictionary<string, object> style = defaultStyle != null ? new Dictionary<string, object>(defaultStyle) : new Dictionary<string, object>();
                    Dictionary<string, object> overrides = Get(runs[i], "StyleSheet", "StyleSheetData") as Dictionary<string, object>;
                    if (overrides != null) foreach (KeyValuePair<string, object> item in overrides) style[item.Key] = item.Value;
                    int index = (int)Number(Get(style, "Font"));
                    if (index < 0 || index >= fontSet.Count) throw new InvalidDataException("잘못된 폰트 인덱스입니다.");
                    string font = Get(fontSet[index], "Name") as string;
                    if (string.IsNullOrEmpty(font)) throw new InvalidDataException("폰트 이름이 없습니다.");
                    usedFonts.Add(font);
                    styles.Add(style);
                }
                offset += length;
            }
            if (styles.Count == 0) throw new NotSupportedException("사용 가능한 텍스트 스타일이 없습니다.");
            Dictionary<string, object> first = styles[0];
            List<object> paragraphs = List(Get(engine, "ParagraphRun", "RunArray"));
            object paragraph = paragraphs.Count > 0 ? Get(paragraphs[0], "ParagraphSheet", "Properties") : null;
            double scaleX = Math.Sqrt(xx * xx + xy * xy), scaleY = Math.Sqrt(yx * yx + yy * yy);
            Color color = ReadColor(Get(first, "FillColor"));
            float size = (float)Number(Get(first, "FontSize"), 12);
            int alignment = (int)Number(Get(paragraph, "Justification"));
            bool sameStyle = styles.All(style => Number(Get(style, "FontSize"), 12) == Number(Get(first, "FontSize"), 12) && Number(Get(style, "HorizontalScale"), 1) == Number(Get(first, "HorizontalScale"), 1) && Number(Get(style, "VerticalScale"), 1) == Number(Get(first, "VerticalScale"), 1) && ReadColor(Get(style, "FillColor")) == color && Equals(Get(style, "FauxBold"), Get(first, "FauxBold")) && Equals(Get(style, "FauxItalic"), Get(first, "FauxItalic")));
            bool simple = styles.All(style => Number(Get(style, "Tracking")) == 0 && Number(Get(style, "BaselineShift")) == 0 && Number(Get(style, "FontCaps")) == 0 && Number(Get(style, "FontBaseline")) == 0 && !Equals(Get(style, "StrokeFlag"), true) && !Equals(Get(style, "Underline"), true) && !Equals(Get(style, "Strikethrough"), true));
            bool transformSupported = scaleX > 0 && scaleY > 0 && xx * yy - xy * yx > 0 && Math.Abs(xx * yx + xy * yy) < 0.001 * scaleX * scaleY && Number(Get(first, "VerticalScale"), 1) > 0 && Number(Get(first, "HorizontalScale"), 1) > 0 && size > 0;
            string warpStyle = Get(warp, "warpStyle") as string;
            return new PSDTextData {
                Content = content.Replace("\r\n", "\n").Replace('\r', '\n'), Fonts = usedFonts.ToArray(),
                FontSize = size * (float)scaleY * (float)Number(Get(first, "VerticalScale"), 1),
                HorizontalScale = scaleY > 0 ? (float)(scaleX / scaleY * Number(Get(first, "HorizontalScale"), 1) / Number(Get(first, "VerticalScale"), 1)) : 1,
                Rotation = -(float)Math.Atan2(xy, xx) * Mathf.Rad2Deg, Color = color, Alignment = alignment,
                Bold = Equals(Get(first, "FauxBold"), true), Italic = Equals(Get(first, "FauxItalic"), true),
                CanConvert = usedFonts.Count == 1 && sameStyle && simple && transformSupported && alignment <= 2 && !content.Contains('\r') && !content.Contains('\n') && (warpStyle == null || warpStyle == "warpNone") && (Get(descriptor, "Ornt") as string != "Vrtc")
            };
        }

        static Color ReadColor(object value) {
            List<object> values = List(Get(value, "Values"));
            if (values.Count != 4 || Number(Get(value, "Type"), 1) != 1) throw new NotSupportedException("RGB 이외의 텍스트 색상");
            return new Color((float)Number(values[1]), (float)Number(values[2]), (float)Number(values[3]), (float)Number(values[0]));
        }
        static object Get(object current, params string[] keys) {
            foreach (string key in keys) {
                if (!(current is Dictionary<string, object> dictionary) || !dictionary.TryGetValue(key, out current)) return null;
            }
            return current;
        }
        static List<object> List(object value) { return value as List<object> ?? new List<object>(); }
        static double Number(object value, double fallback = 0) { return value == null ? fallback : Convert.ToDouble(value, CultureInfo.InvariantCulture); }

        sealed class BigEndian : IDisposable
        {
            public BigEndian(Stream stream) { _reader = new BinaryReader(stream); }
            public long Position { get => _reader.BaseStream.Position; set { if (value < 0 || value > _reader.BaseStream.Length) throw new EndOfStreamException(); _reader.BaseStream.Position = value; } }
            public byte Byte() { return _reader.ReadByte(); }
            public int U16() { return Byte() << 8 | Byte(); }
            public short I16() { return unchecked((short)U16()); }
            public uint U32() { return (uint)Byte() << 24 | (uint)Byte() << 16 | (uint)Byte() << 8 | Byte(); }
            public int I32() { return unchecked((int)U32()); }
            public long Length(bool large) { return large ? checked((long)((ulong)U32() << 32 | U32())) : U32(); }
            public void Skip(long count) { Position = checked(Position + count); }
            public byte[] Bytes(int count) {
                if (count < 0 || count > _reader.BaseStream.Length - Position) throw new EndOfStreamException();
                byte[] bytes = _reader.ReadBytes(count);
                if (bytes.Length != count) throw new EndOfStreamException();
                return bytes;
            }
            public string Tag() { return Encoding.ASCII.GetString(Bytes(4)); }
            public string Unicode() { return Encoding.BigEndianUnicode.GetString(Bytes(checked(I32() * 2))).TrimEnd('\0'); }
            public string Id() { int count = I32(); return Encoding.ASCII.GetString(Bytes(count == 0 ? 4 : count)); }
            public double Double() { byte[] bytes = Bytes(8); if (BitConverter.IsLittleEndian) Array.Reverse(bytes); return BitConverter.ToDouble(bytes, 0); }
            public Dictionary<string, object> Descriptor(int depth = 0) {
                if (depth > 64) throw new InvalidDataException("Descriptor 중첩 제한");
                Unicode(); Id();
                int count = I32();
                if (count < 0 || count > 100000) throw new InvalidDataException("Descriptor 항목 제한");
                Dictionary<string, object> items = new Dictionary<string, object>();
                for (int i = 0; i < count; i++) { string key = Id(); items[key] = Value(Tag(), depth + 1); }
                return items;
            }
            object Value(string type, int depth) {
                if (depth > 64) throw new InvalidDataException("Descriptor 중첩 제한");
                switch (type) {
                    case "TEXT": return Unicode();
                    case "Objc": case "GlbO": return Descriptor(depth);
                    case "enum": Id(); return Id();
                    case "doub": return Double();
                    case "UntF": Tag(); return Double();
                    case "long": return I32();
                    case "bool": return Byte() != 0;
                    case "tdta": case "alis": case "Pth ": return Bytes(I32());
                    case "type": case "GlbC": Unicode(); return Id();
                    case "comp": return Length(true);
                    case "VlLs":
                        int count = I32();
                        if (count < 0 || count > 100000) throw new InvalidDataException("Descriptor 목록 제한");
                        List<object> list = new List<object>();
                        for (int i = 0; i < count; i++) list.Add(Value(Tag(), depth + 1));
                        return list;
                    default: throw new NotSupportedException("Descriptor type: " + type);
                }
            }
            public void Dispose() { _reader.Dispose(); }
            readonly BinaryReader _reader;
        }

        sealed class EngineReader
        {
            public EngineReader(byte[] data) { _data = data; }
            public object Read(int depth = 0) {
                if (depth > 64) throw new InvalidDataException("EngineData 중첩 제한");
                Space();
                if (_position >= _data.Length) throw new EndOfStreamException();
                if (_data[_position] == '<' && _position + 1 < _data.Length && _data[_position + 1] == '<') {
                    _position += 2;
                    Dictionary<string, object> dictionary = new Dictionary<string, object>();
                    while (true) {
                        Space();
                        if (_position + 1 >= _data.Length) throw new EndOfStreamException();
                        if (_data[_position] == '>' && _data[_position + 1] == '>') { _position += 2; return dictionary; }
                        if (_data[_position++] != '/') throw new InvalidDataException("EngineData 키");
                        string key = Token();
                        dictionary[key] = Read(depth + 1);
                    }
                }
                if (_data[_position] == '[') {
                    _position++;
                    List<object> items = new List<object>();
                    while (true) {
                        Space();
                        if (_position >= _data.Length) throw new EndOfStreamException();
                        if (_data[_position] == ']') { _position++; return items; }
                        items.Add(Read(depth + 1));
                    }
                }
                if (_data[_position] == '(') {
                    _position++;
                    int nesting = 1;
                    List<byte> bytes = new List<byte>();
                    while (_position < _data.Length) {
                        byte value = _data[_position++];
                        if (value == '\\') {
                            if (_position >= _data.Length) throw new EndOfStreamException();
                            value = _data[_position++];
                            if (value == 'n') value = 10;
                            else if (value == 'r') value = 13;
                            else if (value == 't') value = 9;
                            else if (value == 'b') value = 8;
                            else if (value == 'f') value = 12;
                            else if (value >= '0' && value <= '7') {
                                int octal = value - '0';
                                for (int i = 0; i < 2 && _position < _data.Length && _data[_position] >= '0' && _data[_position] <= '7'; i++) octal = octal * 8 + _data[_position++] - '0';
                                value = (byte)octal;
                            }
                            bytes.Add(value);
                        }
                        else {
                            if (value == '(') nesting++;
                            if (value == ')' && --nesting == 0) {
                                byte[] result = bytes.ToArray();
                                return result.Length >= 2 && result[0] == 254 && result[1] == 255 ? Encoding.BigEndianUnicode.GetString(result, 2, result.Length - 2) : Encoding.UTF8.GetString(result);
                            }
                            bytes.Add(value);
                        }
                    }
                    throw new EndOfStreamException();
                }
                if (_data[_position] == '/') { _position++; return Token(); }
                string token = Token();
                if (token == "true") return true;
                if (token == "false") return false;
                if (token == "null") return null;
                if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out double number)) return number;
                return token;
            }
            void Space() { while (_position < _data.Length && (_data[_position] == 0 || _data[_position] == 9 || _data[_position] == 10 || _data[_position] == 13 || _data[_position] == 32)) _position++; }
            string Token() {
                int start = _position;
                while (_position < _data.Length && _data[_position] > 32 && _data[_position] != '[' && _data[_position] != ']' && _data[_position] != '<' && _data[_position] != '>' && _data[_position] != '(' && _data[_position] != ')') _position++;
                if (_position == start) throw new InvalidDataException("EngineData 토큰");
                return Encoding.ASCII.GetString(_data, start, _position - start);
            }
            readonly byte[] _data;
            int _position;
        }
    }
}

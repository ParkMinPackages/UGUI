using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace ParkMinPackages.UGUI.Editor
{
    public enum PSDMissingFontPolicy { UseDefaultFallback, KeepAsImage, StopConversion }
    public enum PSDTextOutput { Legacy, TextMeshPro }

    [Serializable]
    public sealed class PSDFontMapping
    {
        // - Construct -
        public PSDFontMapping(string postScriptName) { _postScriptName = postScriptName; }

        // - Public Properties -
        public string PostScriptName => _postScriptName;
        public Font LegacyFont { get => _legacyFont; set => _legacyFont = value; }
        public TMP_FontAsset TMPFont { get => _tmpFont; set => _tmpFont = value; }

        // - Internals -
        [SerializeField] string _postScriptName;
        [SerializeField] Font _legacyFont;
        [SerializeField] TMP_FontAsset _tmpFont;
    }

    [FilePath("ProjectSettings/ParkMinPackages.PSDConverter.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class PSDConverterSettings : ScriptableSingleton<PSDConverterSettings>
    {
        // - Public Methods -
        public void SaveSettings() { Save(true); }
        public PSDFontMapping GetMapping(string name) {
            PSDFontMapping mapping = _fontMappings.FirstOrDefault(item => item.PostScriptName == name);
            if (mapping != null) return mapping;
            mapping = new PSDFontMapping(name);
            _fontMappings.Add(mapping);
            return mapping;
        }
        public UnityEngine.Object ResolveFont(string name, PSDTextOutput output) {
            PSDFontMapping mapping = GetMapping(name);
            UnityEngine.Object font = output == PSDTextOutput.Legacy ? (UnityEngine.Object)mapping.LegacyFont : mapping.TMPFont;
            if (font != null || _missingFontPolicy != PSDMissingFontPolicy.UseDefaultFallback) return font;
            return output == PSDTextOutput.Legacy ? (UnityEngine.Object)_defaultLegacyFont : _defaultTMPFont;
        }

        // - Public Properties -
        public string OutputFolder { get => _outputFolder; set => _outputFolder = value; }
        public Font DefaultLegacyFont { get => _defaultLegacyFont; set => _defaultLegacyFont = value; }
        public TMP_FontAsset DefaultTMPFont { get => _defaultTMPFont; set => _defaultTMPFont = value; }
        public PSDMissingFontPolicy MissingFontPolicy { get => _missingFontPolicy; set => _missingFontPolicy = value; }
        public bool UsePSDResolution { get => _usePSDResolution; set => _usePSDResolution = value; }
        public Vector2Int ReferenceResolution { get => _referenceResolution; set => _referenceResolution = value; }

        // - Internals -
        [SerializeField] string _outputFolder = "Assets/Fonts/PSDConverter";
        [SerializeField] Font _defaultLegacyFont;
        [SerializeField] TMP_FontAsset _defaultTMPFont;
        [SerializeField] PSDMissingFontPolicy _missingFontPolicy;
        [SerializeField] bool _usePSDResolution = true;
        [SerializeField] Vector2Int _referenceResolution = new Vector2Int(1920, 1080);
        [SerializeField] List<PSDFontMapping> _fontMappings = new List<PSDFontMapping>();
    }
}

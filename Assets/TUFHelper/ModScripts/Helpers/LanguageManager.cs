using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TUFHelper
{
    public enum TUFHelperLanguage
    {
        English,
        Korean,
        Chinese
    }

    public static class LanguageManager
    {
        private static bool initialized;
        private static TUFHelperLanguage activeLanguage = TUFHelperLanguage.English;

        private static readonly Dictionary<int, string> OriginalTexts = new();
        private static readonly Dictionary<int, string> FixedTexts = new();
        private static readonly Dictionary<int, float> OriginalFontSizes = new();
        private static readonly Dictionary<int, TMP_FontAsset> OriginalFonts = new();

        private const string ChineseFontAssetPath = "assets/TUFHelper/Assets/Fonts/NotoSansSC-Regular SDF.asset";
        private static TMP_FontAsset cachedChineseFontAsset;

        private static readonly Dictionary<string, string[]> LocalizationRegistry = new(StringComparer.Ordinal)
        {
            { "SETTINGS", new[] { "설정", "设置" } },
            { "Settings", new[] { "설정", "设置" } },
            { "Mod Settings", new[] { "모드 설정", "模组设置" } },
            { "Bootstrap", new[] { "부트스트랩", "启动" } },
            { "Overlayer", new[] { "오버레이", "覆盖层" } },
            { "Audio", new[] { "오디오", "音频" } },
            { "Language", new[] { "언어", "语言" } },
            { "TUFHELPER MUSIC VOLUME", new[] { "TUFHelper 음악 볼륨", "TUFHelper 音乐音量" } },
            { "LEVEL SAVE PATH", new[] { "레벨 저장 경로", "关卡保存路径" } },
            { "START WITH GAME", new[] { "게임 시작 시 열기", "随游戏启动" } },
            { "SHOW & HANDLE OVERLAYER", new[] { "오버레이 표시 및 조정", "显示并调整覆盖层" } },
            { "Show PP Displayer", new[] { "PP 표시기 보이기", "显示 PP 显示器" } },
            { "Show Speed", new[] { "속도 표시 보이기", "显示速度" } },
            { "Show Leaderboard", new[] { "리더보드 보이기", "显示排行榜" } },
            { "Show Level Info", new[] { "레벨 정보 보이기", "显示关卡信息" } },
            { "Scale", new[] { "크기", "缩放" } },
            { "Auto (Game Language)", new[] { "자동 (게임 언어)", "自动（游戏语言）" } },
            { "English", new[] { "English", "English" } },
            { "Korean", new[] { "한국어", "한국어" } },
            { "Chinese", new[] { "简体中文", "简体中文" } },
            { "Follow the game's language setting.", new[] { "게임 언어 설정을 따라갑니다.", "跟随游戏的语言设置。" } },
            { "Select Language", new[] { "언어 선택", "选择语言" } },
            { "GROUP BY FOLDERS", new[] { "폴더별로 묶기", "按文件夹分组" } },
            { "SHOW ONLY FAVORITES", new[] { "즐겨찾기만 보기", "仅显示收藏" } },
            { "SHOW ONLY DOWNLOADED", new[] { "다운로드한 레벨만 보기", "仅显示已下载" } },
            { "SORT BY:", new[] { "정렬 기준:", "排序方式：" } },
            { "Ascending", new[] { "오름차순", "升序" } },
            { "Descending", new[] { "내림차순", "降序" } },
            { "ID", new[] { "ID", "ID" } },
            { "Difficulty", new[] { "난이도", "难度" } },
            { "Clears", new[] { "클리어 수", "通关数" } },
            { "Likes", new[] { "좋아요", "点赞数" } },
            { "Tags", new[] { "태그", "标签" } },
            { "Play Style", new[] { "플레이 스타일", "游玩风格" } },
            { "Key Limit", new[] { "키 제한", "按键限制" } },
            { "Judgement", new[] { "판정", "判定" } },
            { "Gimmick", new[] { "기믹", "机制" } },
            { "VFX", new[] { "VFX", "视觉效果" } },
            { "Length", new[] { "길이", "长度" } },
            { "Required Mods", new[] { "필요 모드", "所需模组" } },
            { "DLC", new[] { "DLC", "DLC" } },
            { "Misc", new[] { "기타", "其他" } },
            { "Special Difficulties", new[] { "특수 난이도", "特殊难度" } },
            { "Unranked", new[] { "언랭크", "未排名" } },
            { "HIDE RATED", new[] { "평가한 항목 숨기기", "隐藏已评级" } },
            { "4+ RATED", new[] { "4명 이상 평가", "4+ 评级" } },
            { "Hide", new[] { "숨기기", "隐藏" } },
            { "Show", new[] { "보이기", "显示" } },
            { "Only", new[] { "만 보기", "仅显示" } },
            { "PLAY", new[] { "플레이", "游玩" } },
            { "BACK", new[] { "뒤로", "返回" } },
            { "Close", new[] { "닫기", "关闭" } },
            { "CANCEL", new[] { "취소", "取消" } },
            { "ADD", new[] { "추가", "添加" } },
            { "ADD FOLDER", new[] { "폴더 추가", "添加文件夹" } },
            { "Create a new folder", new[] { "새 폴더 만들기", "创建新文件夹" } },
            { "Type a name folder", new[] { "폴더 이름 입력", "输入文件夹名称" } },
            { "Select Folder", new[] { "폴더 선택", "选择文件夹" } },
            { "Select Level", new[] { "레벨 선택", "选择关卡" } },
            { "Select the folder you want to add the level to.", new[] { "레벨을 추가할 폴더를 선택하세요.", "选择要添加关卡的文件夹。" } },
            { "The folder contains several levels. Which level will you open?", new[] { "어떤 레벨을 열까요?", "要打开哪一个？" } },
            { "Search by level name, artist...", new[] { "레벨 이름, 아티스트로 검색...", "按关卡名、艺术家搜索..." } },
            { "Type to search (Song, Artist, Creator, #ID)...", new[] { "검색어 입력 (곡, 아티스트, 제작자, #ID)...", "输入搜索内容（歌曲、艺术家、作者、#ID）..." } },
            { "Downloading", new[] { "다운로드 중", "下载中" } },
            { "LOADING...", new[] { "불러오는 중...", "加载中..." } },
            { "An error has occured!", new[] { "오류가 발생했습니다!", "发生错误！" } },
            { "Login", new[] { "로그인", "登录" } },
            { "SIGN IN", new[] { "로그인", "登录" } },
            { "Register", new[] { "가입", "注册" } },
            { "LOG OUT", new[] { "로그아웃", "退出登录" } },
            { "Email or Username", new[] { "이메일 또는 사용자 이름", "邮箱或用户名" } },
            { "Password", new[] { "비밀번호", "密码" } },
            { "LEADERBOARD", new[] { "리더보드", "排行榜" } },
            { "Leaderboard", new[] { "리더보드", "排行榜" } },
            { "Credits", new[] { "크레딧", "制作人员" } },
            { "CREDITS", new[] { "크레딧", "制作人员" } },
            { "PROGRAMMING", new[] { "프로그래밍", "程序" } },
            { "LEAD DEVELOPER", new[] { "리드 개발자", "主开发者" } },
            { "WEB BACKEND DEVELOPER", new[] { "웹 백엔드 개발자", "网页后端开发者" } },
            { "SUPPORT", new[] { "지원", "支持" } },
            { "FORUM", new[] { "포럼 사이트", "论坛网站" } },
            { "Downloader", new[] { "다운로더", "下载器" } },
            { "MUSIC", new[] { "음악", "音乐" } },
            { "UPDATE INFO", new[] { "정보 업데이트", "刷新" } },
            { "EXIT", new[] { "나가기", "退出" } },
            { "HIDDEN", new[] { "숨김", "隐藏" } },
            { "State:", new[] { "상태:", "状态：" } },
            { "Progress:", new[] { "진행률:", "进度：" } },
            { "ACCURACY:", new[] { "정확도:", "准确率：" } },
            { "SPEED:", new[] { "속도:", "速度：" } },
            { "BPM:", new[] { "BPM:", "BPM：" } },
            { "Tiles:", new[] { "타일:", "格数：" } },
            { "Length:", new[] { "길이:", "长度：" } }
        };

        public static void Init()
        {
            if (initialized) return;

            initialized = true;
            SceneManager.sceneLoaded += (_, _) => Apply();
        }

        public static TUFHelperLanguage CurrentLanguage
        {
            get
            {
                string mode = Main.Setting?.Language ?? "Auto";
                if (mode.Equals("Korean", StringComparison.OrdinalIgnoreCase)) return TUFHelperLanguage.Korean;
                if (mode.Equals("Chinese", StringComparison.OrdinalIgnoreCase)) return TUFHelperLanguage.Chinese;
                if (mode.Equals("English", StringComparison.OrdinalIgnoreCase)) return TUFHelperLanguage.English;

                return GetAutoLanguage();
            }
        }

        public static void SetLanguageMode(string mode)
        {
            if (Main.Setting == null) return;

            Main.Setting.Language = mode;
            Main.Setting.Save(Main.ModEntry);
            Apply();
        }

        public static string Translate(string english)
        {
            if (string.IsNullOrEmpty(english) || !LocalizationRegistry.TryGetValue(english, out string[] translations))
            {
                return english;
            }

            return activeLanguage switch
            {
                TUFHelperLanguage.Korean => translations[0],
                TUFHelperLanguage.Chinese => translations[1],
                _ => english
            };
        }
        public static void Apply()
        {
            RefreshActiveLanguage();
            foreach (TextMeshProUGUI text in UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None))
            {
                ApplyToText(text);
            }
        }

        public static void ApplyTo(GameObject root)
        {
            if (root == null) return;

            RefreshActiveLanguage();
            foreach (TextMeshProUGUI text in root.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                ApplyToText(text);
            }
        }

        public static void RememberCurrentFonts(GameObject root)
        {
            if (root == null) return;

            foreach (TextMeshProUGUI text in root.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (text == null) continue;

                int id = text.GetInstanceID();
                OriginalFonts[id] = text.font;
                OriginalFontSizes[id] = text.fontSize;
            }
        }

        private static void RefreshActiveLanguage()
        {
            activeLanguage = CurrentLanguage;
        }

        private static TUFHelperLanguage GetAutoLanguage()
        {
            return DetectGameLanguage();
        }

        public static void RememberOriginal(TextMeshProUGUI text, string english)
        {
            if (text == null) return;

            int id = text.GetInstanceID();
            FixedTexts.Remove(id);
            OriginalTexts[id] = english;
            OriginalFontSizes.TryAdd(id, text.fontSize);
            OriginalFonts.TryAdd(id, text.font);

            string translated = Translate(english);
            text.text = translated;
            ApplyChineseFont(text, translated);
        }

        public static void RememberFixedText(TextMeshProUGUI text, string value)
        {
            if (text == null) return;

            int id = text.GetInstanceID();
            OriginalTexts.Remove(id);
            FixedTexts[id] = value;
            OriginalFontSizes.TryAdd(id, text.fontSize);
            OriginalFonts.TryAdd(id, text.font);

            text.text = value;
            ApplyChineseJapaneseFont(text, value);
        }

        public static void ApplyChineseJapaneseFont(TextMeshProUGUI text)
        {
            if (text == null) return;
            ApplyChineseJapaneseFont(text, text.text);
        }

        public static void ApplyChineseJapaneseFont(TextMeshProUGUI text, string value)
        {
            if (text == null) return;

            int id = text.GetInstanceID();
            if (!OriginalFontSizes.TryGetValue(id, out float originalSize))
            {
                originalSize = text.fontSize;
                OriginalFontSizes[id] = originalSize;
            }
            if (!OriginalFonts.TryGetValue(id, out TMP_FontAsset originalFont))
            {
                originalFont = text.font;
                OriginalFonts[id] = originalFont;
            }

            text.fontSize = originalSize;
            if (ContainsChineseOrJapanese(value) && TryGetChineseFont(out TMP_FontAsset chineseFont))
            {
                text.font = chineseFont;
                text.fontSize *= 1.25f;
            }
            else
            {
                text.font = originalFont;
            }
        }

        private static void ApplyToText(TextMeshProUGUI text)
        {
            if (text == null) return;

            int id = text.GetInstanceID();
            if (FixedTexts.TryGetValue(id, out string fixedText))
            {
                text.text = fixedText;
                ApplyChineseJapaneseFont(text, fixedText);
                return;
            }

            if (!OriginalTexts.TryGetValue(id, out string original))
            {
                original = GetEnglishSource(text.text);
                if (original == null) return;

                OriginalTexts[id] = original;
            }

            string translated = Translate(original);
            text.text = translated;
            ApplyChineseFont(text, translated);
        }

        private static void ApplyChineseFont(TextMeshProUGUI text, string value)
        {
            if (text == null) return;

            int id = text.GetInstanceID();
            if (!OriginalFontSizes.TryGetValue(id, out float originalSize))
            {
                originalSize = text.fontSize;
                OriginalFontSizes[id] = originalSize;
            }
            if (!OriginalFonts.TryGetValue(id, out TMP_FontAsset originalFont))
            {
                originalFont = text.font;
                OriginalFonts[id] = originalFont;
            }

            text.fontSize = originalSize;

            bool shouldUseChineseFont = activeLanguage == TUFHelperLanguage.Chinese && ContainsCjk(value);
            if (shouldUseChineseFont && TryGetChineseFont(out TMP_FontAsset chineseFont))
            {
                text.font = chineseFont;
            }
            else
            {
                text.font = originalFont;
            }
        }

        private static bool TryGetChineseFont(out TMP_FontAsset fontAsset)
        {
            if (cachedChineseFontAsset != null)
            {
                fontAsset = cachedChineseFontAsset;
                return true;
            }

            fontAsset = null;
            if (Main.assets == null) return false;

            try
            {
                cachedChineseFontAsset = Main.assets.LoadAsset<TMP_FontAsset>(ChineseFontAssetPath);
                fontAsset = cachedChineseFontAsset;
                return fontAsset != null;
            }
            catch (Exception ex)
            {
                Main.Logger?.Error($"[LanguageManager] Critical failure pulling Chinese SDF asset path: {ex.Message}");
                return false;
            }
        }

        private static bool ContainsCjk(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;

            foreach (char c in value)
            {
                if ((c >= '\u3400' && c <= '\u4DBF') || (c >= '\u4E00' && c <= '\u9FFF'))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool ContainsChineseOrJapanese(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;

            foreach (char c in value)
            {
                if ((c >= '\u3040' && c <= '\u30FF') ||
                    (c >= '\u31F0' && c <= '\u31FF') ||
                    (c >= '\u3400' && c <= '\u4DBF') ||
                    (c >= '\u4E00' && c <= '\u9FFF') ||
                    (c >= '\uF900' && c <= '\uFAFF'))
                {
                    return true;
                }
            }
            return false;
        }

        private static string GetEnglishSource(string current)
        {
            if (string.IsNullOrEmpty(current)) return null;
            if (LocalizationRegistry.ContainsKey(current)) return current;

            foreach (KeyValuePair<string, string[]> pair in LocalizationRegistry)
            {
                if (pair.Value[0] == current || pair.Value[1] == current)
                {
                    return pair.Key;
                }
            }
            return null;
        }

        private static TUFHelperLanguage DetectGameLanguage()
        {
            string reflected = TryReadPlayerPrefsLanguage();
            if (LooksKorean(reflected)) return TUFHelperLanguage.Korean;
            if (LooksChineseLanguage(reflected)) return TUFHelperLanguage.Chinese;
            if (LooksEnglish(reflected)) return TUFHelperLanguage.English;

            return Application.systemLanguage switch
            {
                SystemLanguage.Korean => TUFHelperLanguage.Korean,
                SystemLanguage.Chinese => TUFHelperLanguage.Chinese,
                SystemLanguage.ChineseSimplified => TUFHelperLanguage.Chinese,
                SystemLanguage.ChineseTraditional => TUFHelperLanguage.Chinese,
                _ => TUFHelperLanguage.English
            };
        }

        private static string TryReadPlayerPrefsLanguage()
        {
            string[] keys = { "language", "Language", "lang", "Lang", "locale", "Locale" };

            foreach (string key in keys)
            {
                try
                {
                    if (PlayerPrefs.HasKey(key))
                    {
                        string value = PlayerPrefs.GetString(key);
                        if (!string.IsNullOrEmpty(value)) return value;
                    }
                }
                catch { }
            }
            return null;
        }

        private static bool LooksKorean(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            value = value.ToLowerInvariant();
            return value.Contains("korean") || value == "ko" || value.Contains("kr") || value.Contains("한국");
        }

        private static bool LooksEnglish(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            value = value.ToLowerInvariant();
            return value.Contains("english") || value == "en";
        }

        private static bool LooksChineseLanguage(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            value = value.ToLowerInvariant();
            return value.Contains("chinese") || value.Contains("simplified") || value.Contains("traditional") ||
                   value.Contains("schinese") || value.Contains("tchinese") || value.Contains("zh") ||
                   value.Contains("cn") || value.Contains("中文") || value.Contains("简体");
        }
    }

    // Helper extension fallback string validation structure block
    internal static class StringExtensions
    {
        public static bool MakeNullEmptyOrWhiteSpace(string value) => string.IsNullOrWhiteSpace(value);
    }
}
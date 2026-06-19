using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private static readonly Dictionary<int, float> OriginalFontSizes = new();
        private const float FallbackFontScale = 1.5f;

        private static readonly Dictionary<string, string> Korean = new(StringComparer.Ordinal)
        {
            { "SETTINGS", "설정" },
            { "Settings", "설정" },
            { "Mod Settings", "모드 설정" },
            { "Bootstrap", "부트스트랩" },
            { "Overlayer", "오버레이" },
            { "Audio", "오디오" },
            { "Language", "언어" },
            { "TUFHELPER MUSIC VOLUME", "TUFHelper 음악 볼륨" },
            { "LEVEL SAVE PATH", "레벨 저장 경로" },
            { "START WITH GAME", "게임 시작 시 열기" },
            { "SHOW & HANDLE OVERLAYER", "오버레이 표시 및 조정" },
            { "Show PP Displayer", "PP 표시기 보이기" },
            { "Show Speed", "속도 표시 보이기" },
            { "Show Leaderboard", "리더보드 보이기" },
            { "Show Level Info", "레벨 정보 보이기" },
            { "Scale", "크기" },
            { "Auto (Game Language)", "자동 (게임 언어)" },
            { "English", "English" },
            { "Korean", "한국어" },
            { "Chinese", "简体中文" },
            { "Follow the game's language setting.", "게임 언어 설정을 따라갑니다." },
            { "Select Language", "언어 선택" },
            { "GROUP BY FOLDERS", "폴더별로 묶기" },
            { "SHOW ONLY FAVORITES", "즐겨찾기만 보기" },
            { "SHOW ONLY DOWNLOADED", "다운로드한 레벨만 보기" },
            { "SORT BY:", "정렬 기준:" },
            { "Ascending", "오름차순" },
            { "Descending", "내림차순" },
            { "ID", "ID" },
            { "Difficulty", "난이도" },
            { "Clears", "클리어 수" },
            { "Likes", "좋아요" },
            { "Tags", "태그" },
            { "Play Style", "플레이 스타일" },
            { "Key Limit", "키 제한" },
            { "Judgement", "판정" },
            { "Gimmick", "기믹" },
            { "VFX", "VFX" },
            { "Length", "길이" },
            { "Required Mods", "필요 모드" },
            { "DLC", "DLC" },
            { "Misc", "기타" },
            { "Special Difficulties", "특수 난이도" },
            { "Unranked", "언랭크" },
            { "HIDE RATED", "평가한 항목 숨기기" },
            { "4+ RATED", "4명 이상 평가" },
            { "Hide", "숨기기" },
            { "Show", "보이기" },
            { "Only", "만 보기" },
            { "PLAY", "플레이" },
            { "BACK", "뒤로" },
            { "Close", "닫기" },
            { "CANCEL", "취소" },
            { "ADD", "추가" },
            { "ADD FOLDER", "폴더 추가" },
            { "Create a new folder", "새 폴더 만들기" },
            { "Type a name folder", "폴더 이름 입력" },
            { "Select Folder", "폴더 선택" },
            { "Select Level", "레벨 선택" },
            { "Select the folder you want to add the level to.", "레벨을 추가할 폴더를 선택하세요." },
            { "The folder contains several levels. Which level will you open?", "어떤 레벨을 열까요?" },
            { "Search by level name, artist...", "레벨 이름, 아티스트로 검색..." },
            { "Type to search (Song, Artist, Creator, #ID)...", "검색어 입력 (곡, 아티스트, 제작자, #ID)..." },
            { "Downloading", "다운로드 중" },
            { "LOADING...", "불러오는 중..." },
            { "An error has occured!", "오류가 발생했습니다!" },
            { "Login", "로그인" },
            { "SIGN IN", "로그인" },
            { "Register", "가입" },
            { "LOG OUT", "로그아웃" },
            { "Email or Username", "이메일 또는 사용자 이름" },
            { "Password", "비밀번호" },
            { "LEADERBOARD", "리더보드" },
            { "Leaderboard", "리더보드" },
            { "Credits", "크레딧" },
            { "CREDITS", "크레딧" },
            { "PROGRAMMING", "프로그래밍" },
            { "LEAD DEVELOPER", "리드 개발자" },
            { "WEB BACKEND DEVELOPER", "웹 백엔드 개발자" },
            { "SUPPORT", "지원" },
            { "FORUM", "포럼 사이트" },
            { "Downloader", "다운로더" },
            { "MUSIC", "음악" },
            { "UPDATE INFO", "새로고침" },
            { "EXIT", "나가기" },
            { "HIDDEN", "숨김" },
            { "State:", "상태:" },
            { "Progress:", "진행률:" },
            { "ACCURACY:", "정확도:" },
            { "SPEED:", "속도:" },
            { "BPM:", "BPM:" },
            { "Tiles:", "타일:" },
            { "Length:", "길이:" }
        };

        private static readonly Dictionary<string, string> Chinese = new(StringComparer.Ordinal)
        {
            { "SETTINGS", "设置" },
            { "Settings", "设置" },
            { "Mod Settings", "模组设置" },
            { "Bootstrap", "启动" },
            { "Overlayer", "覆盖层" },
            { "Audio", "音频" },
            { "Language", "语言" },
            { "TUFHELPER MUSIC VOLUME", "TUFHelper 音乐音量" },
            { "LEVEL SAVE PATH", "关卡保存路径" },
            { "START WITH GAME", "随游戏启动" },
            { "SHOW & HANDLE OVERLAYER", "显示并调整覆盖层" },
            { "Show PP Displayer", "显示 PP 显示器" },
            { "Show Speed", "显示速度" },
            { "Show Leaderboard", "显示排行榜" },
            { "Show Level Info", "显示关卡信息" },
            { "Scale", "缩放" },
            { "Auto (Game Language)", "自动（游戏语言）" },
            { "English", "English" },
            { "Korean", "한국어" },
            { "Chinese", "简体中文" },
            { "Follow the game's language setting.", "跟随游戏的语言设置。" },
            { "Select Language", "选择语言" },
            { "GROUP BY FOLDERS", "按文件夹分组" },
            { "SHOW ONLY FAVORITES", "仅显示收藏" },
            { "SHOW ONLY DOWNLOADED", "仅显示已下载" },
            { "SORT BY:", "排序方式：" },
            { "Ascending", "升序" },
            { "Descending", "降序" },
            { "ID", "ID" },
            { "Difficulty", "难度" },
            { "Clears", "通关数" },
            { "Likes", "点赞数" },
            { "Tags", "标签" },
            { "Play Style", "游玩风格" },
            { "Key Limit", "按键限制" },
            { "Judgement", "判定" },
            { "Gimmick", "机制" },
            { "VFX", "视觉效果" },
            { "Length", "长度" },
            { "Required Mods", "所需模组" },
            { "DLC", "DLC" },
            { "Misc", "其他" },
            { "Special Difficulties", "特殊难度" },
            { "Unranked", "未排名" },
            { "HIDE RATED", "隐藏已评级" },
            { "4+ RATED", "4+ 评级" },
            { "Hide", "隐藏" },
            { "Show", "显示" },
            { "Only", "仅显示" },
            { "PLAY", "游玩" },
            { "BACK", "返回" },
            { "Close", "关闭" },
            { "CANCEL", "取消" },
            { "ADD", "添加" },
            { "ADD FOLDER", "添加文件夹" },
            { "Create a new folder", "创建新文件夹" },
            { "Type a name folder", "输入文件夹名称" },
            { "Select Folder", "选择文件夹" },
            { "Select Level", "选择关卡" },
            { "Select the folder you want to add the level to.", "选择要添加关卡的文件夹。" },
            { "The folder contains several levels. Which level will you open?", "要打开哪一个？" },
            { "Search by level name, artist...", "按关卡名、艺术家搜索..." },
            { "Type to search (Song, Artist, Creator, #ID)...", "输入搜索内容（歌曲、艺术家、作者、#ID）..." },
            { "Downloading", "下载中" },
            { "LOADING...", "加载中..." },
            { "An error has occured!", "发生错误！" },
            { "Login", "登录" },
            { "SIGN IN", "登录" },
            { "Register", "注册" },
            { "LOG OUT", "退出登录" },
            { "Email or Username", "邮箱或用户名" },
            { "Password", "密码" },
            { "LEADERBOARD", "排行榜" },
            { "Leaderboard", "排行榜" },
            { "Credits", "制作人员" },
            { "CREDITS", "制作人员" },
            { "PROGRAMMING", "程序" },
            { "LEAD DEVELOPER", "主开发者" },
            { "WEB BACKEND DEVELOPER", "网页后端开发者" },
            { "SUPPORT", "支持" },
            { "FORUM", "论坛网站" },
            { "Downloader", "下载器" },
            { "MUSIC", "音乐" },
            { "UPDATE INFO", "刷新" },
            { "EXIT", "退出" },
            { "HIDDEN", "隐藏" },
            { "State:", "状态：" },
            { "Progress:", "进度：" },
            { "ACCURACY:", "准确率：" },
            { "SPEED:", "速度：" },
            { "BPM:", "BPM：" },
            { "Tiles:", "格数：" },
            { "Length:", "长度：" }
        };

        public static void Init()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            SceneManager.sceneLoaded += (_, _) => Apply();
        }

        public static TUFHelperLanguage CurrentLanguage
        {
            get
            {
                string mode = Main.Setting?.Language ?? "Auto";
                if (mode.Equals("Korean", StringComparison.OrdinalIgnoreCase))
                {
                    return TUFHelperLanguage.Korean;
                }

                if (mode.Equals("Chinese", StringComparison.OrdinalIgnoreCase))
                {
                    return TUFHelperLanguage.Chinese;
                }

                if (mode.Equals("English", StringComparison.OrdinalIgnoreCase))
                {
                    return TUFHelperLanguage.English;
                }

                return GetAutoLanguage();
            }
        }

        public static void SetLanguageMode(string mode)
        {
            if (Main.Setting == null)
            {
                return;
            }

            Main.Setting.Language = mode;
            Main.Setting.Save(Main.ModEntry);
            Apply();
        }

        public static string Translate(string english)
        {
            Dictionary<string, string> translations = activeLanguage switch
            {
                TUFHelperLanguage.Korean => Korean,
                TUFHelperLanguage.Chinese => Chinese,
                _ => null
            };

            return translations != null && translations.TryGetValue(english, out string translated) ? translated : english;
        }

        public static void Apply()
        {
            RefreshActiveLanguage();
            foreach (TextMeshProUGUI text in UnityEngine.Object.FindObjectsOfType<TextMeshProUGUI>(true))
            {
                ApplyToText(text);
            }
        }

        public static void ApplyTo(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            RefreshActiveLanguage();
            foreach (TextMeshProUGUI text in root.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                ApplyToText(text);
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
            if (text == null)
            {
                return;
            }

            OriginalTexts[text.GetInstanceID()] = english;
            OriginalFontSizes.TryAdd(text.GetInstanceID(), text.fontSize);

            string translated = Translate(english);
            text.text = translated;
            ApplyFallbackFontScale(text, translated);
        }

        private static void ApplyToText(TextMeshProUGUI text)
        {
            if (text == null)
            {
                return;
            }

            int id = text.GetInstanceID();
            if (!OriginalTexts.TryGetValue(id, out string original))
            {
                original = GetEnglishSource(text.text);
                if (original == null)
                {
                    return;
                }

                OriginalTexts[id] = original;
            }

            string translated = Translate(original);
            text.text = translated;
            ApplyFallbackFontScale(text, translated);
        }

        private static void ApplyFallbackFontScale(TextMeshProUGUI text, string value)
        {
            if (text == null)
            {
                return;
            }

            int id = text.GetInstanceID();
            if (!OriginalFontSizes.TryGetValue(id, out float originalSize))
            {
                originalSize = text.fontSize;
                OriginalFontSizes[id] = originalSize;
            }

            bool shouldScale = activeLanguage == TUFHelperLanguage.Chinese
                && ContainsCjk(value)
                && (UsesFallbackFontFor(value, text.font) || IsSimplifiedChineseLanguageName(value));

            text.fontSize = shouldScale ? originalSize * FallbackFontScale : originalSize;
        }

        private static bool UsesFallbackFontFor(string value, TMP_FontAsset font)
        {
            if (font == null || string.IsNullOrEmpty(value))
            {
                return false;
            }

            try
            {
                return !font.HasCharacters(value);
            }
            catch
            {
                return false;
            }
        }

        private static bool ContainsCjk(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            foreach (char c in value)
            {
                if ((c >= '\u3400' && c <= '\u4DBF') || (c >= '\u4E00' && c <= '\u9FFF'))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsSimplifiedChineseLanguageName(string value)
        {
            return !string.IsNullOrEmpty(value) && value.Contains("\u7B80\u4F53\u4E2D\u6587");
        }

        private static string GetEnglishSource(string current)
        {
            if (string.IsNullOrEmpty(current))
            {
                return null;
            }

            if (Korean.ContainsKey(current))
            {
                return current;
            }

            if (Chinese.ContainsKey(current))
            {
                return current;
            }

            foreach (KeyValuePair<string, string> pair in Korean)
            {
                if (pair.Value == current)
                {
                    return pair.Key;
                }
            }

            foreach (KeyValuePair<string, string> pair in Chinese)
            {
                if (pair.Value == current)
                {
                    return pair.Key;
                }
            }

            return null;
        }

        private static TUFHelperLanguage DetectGameLanguage()
        {
            string reflected = TryReadGameLanguageValue();
            if (LooksKorean(reflected))
            {
                return TUFHelperLanguage.Korean;
            }

            if (LooksChineseLanguage(reflected))
            {
                return TUFHelperLanguage.Chinese;
            }

            if (LooksEnglish(reflected))
            {
                return TUFHelperLanguage.English;
            }

            return Application.systemLanguage switch
            {
                SystemLanguage.Korean => TUFHelperLanguage.Korean,
                SystemLanguage.Chinese => TUFHelperLanguage.Chinese,
                SystemLanguage.ChineseSimplified => TUFHelperLanguage.Chinese,
                SystemLanguage.ChineseTraditional => TUFHelperLanguage.Chinese,
                _ => TUFHelperLanguage.English
            };
        }

        private static string TryReadGameLanguageValue()
        {
            return TryReadPlayerPrefsLanguage();
        }

        private static string TryReadPlayerPrefsLanguage()
        {
            string[] keys =
            {
                "language",
                "Language",
                "lang",
                "Lang",
                "locale",
                "Locale",
                "currentLanguage",
                "CurrentLanguage",
                "selectedLanguage",
                "SelectedLanguage"
            };

            foreach (string key in keys)
            {
                try
                {
                    if (PlayerPrefs.HasKey(key))
                    {
                        string value = PlayerPrefs.GetString(key);
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            return value;
                        }
                    }
                }
                catch
                {
                }
            }

            return null;
        }

        private static bool LooksKorean(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            value = value.ToLowerInvariant();
            return value.Contains("korean") || value == "ko" || value.Contains("kr") || value.Contains("한국");
        }

        private static bool LooksEnglish(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            value = value.ToLowerInvariant();
            return value.Contains("english") || value == "en";
        }

        private static bool LooksChineseLanguage(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            value = value.ToLowerInvariant();
            return value.Contains("chinese")
                || value.Contains("simplified")
                || value.Contains("traditional")
                || value.Contains("schinese")
                || value.Contains("tchinese")
                || value.Contains("zh")
                || value.Contains("zhs")
                || value.Contains("zht")
                || value.Contains("cn")
                || value.Contains("tw")
                || value.Contains("中文")
                || value.Contains("简体")
                || value.Contains("簡體")
                || value.Contains("繁体")
                || value.Contains("繁體")
                || value.Contains("中国")
                || value.Contains("中國");
        }

        private static bool LooksChinese(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            value = value.ToLowerInvariant();
            return value.Contains("chinese") || value.Contains("zh") || value.Contains("cn") || value.Contains("tw") || value.Contains("中文") || value.Contains("简体") || value.Contains("繁體");
        }
    }
}

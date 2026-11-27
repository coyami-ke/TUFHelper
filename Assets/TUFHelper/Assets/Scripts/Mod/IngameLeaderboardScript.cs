using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Together.Utils;
using TUFHelper;
using TUFHelper.ModScripts.Json;
using TUFHelper.Utils;
using UnityEngine;
using UnityEngine.Networking;
using Utf8Json;
using static IngameLeaderboardScript;
using static PPDisplayerScript;

public sealed class ReadOnlyMemoryCharFormatter : IJsonFormatter<ReadOnlyMemory<char>>
{
    public void Serialize(ref Utf8Json.JsonWriter writer, ReadOnlyMemory<char> value, IJsonFormatterResolver formatterResolver)
    {
        if (value.IsEmpty)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteString(value.ToString());
    }

    public ReadOnlyMemory<char> Deserialize(ref Utf8Json.JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        if (reader.ReadIsNull())
        {
            return ReadOnlyMemory<char>.Empty;
        }

        string str = reader.ReadString();
        return str is null ? ReadOnlyMemory<char>.Empty : str.AsMemory();
    }
}
public sealed class IngameLeaderboardRankFormatter : IJsonFormatter<IngameLeaderboardRank>
{
    public void Serialize(ref Utf8Json.JsonWriter writer, IngameLeaderboardRank value, IJsonFormatterResolver formatterResolver)
    {
        writer.WriteBeginObject();

        writer.WritePropertyName("accuracy");
        writer.WriteSingle(value.accuracy);

        writer.WriteValueSeparator();
        writer.WritePropertyName("scoreV2");
        writer.WriteSingle(value.scoreV2);

        writer.WriteValueSeparator();
        writer.WritePropertyName("player");
        writer.WriteBeginObject();
        writer.WritePropertyName("name");
        formatterResolver.GetFormatterWithVerify<ReadOnlyMemory<char>>()
            .Serialize(ref writer, value.player.name, formatterResolver);
        writer.WriteEndObject();

        writer.WriteEndObject();
    }

    public IngameLeaderboardRank Deserialize(ref Utf8Json.JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        if (reader.ReadIsNull())
        {
            return default;
        }

        reader.ReadIsBeginObjectWithVerify();

        float accuracy = 0f;
        float scoreV2 = 0f;
        ReadOnlyMemory<char> playerName = ReadOnlyMemory<char>.Empty;

        int count = 0;
        while (!reader.ReadIsEndObject())
        {
            if (count > 0)
            {
                reader.ReadIsValueSeparatorWithVerify();
            }

            string propertyName = reader.ReadPropertyName();
            switch (propertyName)
            {
                case "accuracy":
                    accuracy = reader.ReadSingle();
                    break;
                case "scoreV2":
                    scoreV2 = reader.ReadSingle();
                    break;
                case "player":
                    playerName = ReadPlayerName(ref reader, formatterResolver);
                    break;
                default:
                    reader.ReadNextBlock();
                    break;
            }
            count++;
        }

        return new IngameLeaderboardRank(accuracy, scoreV2, new(new ReadOnlyMemory<char>(playerName.ToArray())));
    }

    private static ReadOnlyMemory<char> ReadPlayerName(ref Utf8Json.JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        if (reader.ReadIsNull())
        {
            return ReadOnlyMemory<char>.Empty;
        }

        reader.ReadIsBeginObjectWithVerify();
        ReadOnlyMemory<char> name = ReadOnlyMemory<char>.Empty;

        int count = 0;
        while (!reader.ReadIsEndObject())
        {
            if (count > 0)
            {
                reader.ReadIsValueSeparatorWithVerify();
            }

            if (reader.ReadPropertyName() == "name")
            {
                name = formatterResolver.GetFormatterWithVerify<ReadOnlyMemory<char>>()
                    .Deserialize(ref reader, formatterResolver);
            }
            else
            {
                reader.ReadNextBlock();
            }
            count++;
        }

        return name;
    }
}

[RegisterIngameElement("Leaderboard", "assets/tufhelper/assets/prefabs/ingameleaderboardprefab.prefab")]
public class IngameLeaderboardScript : BasicIngameElement
{
    public readonly struct IngameLeaderboardRank
    {
        public readonly float accuracy;
        public readonly float scoreV2;
        public readonly IngameLeaderboardPlayer player;
        public IngameLeaderboardRank(float accuracy, float scoreV2, IngameLeaderboardPlayer player)
        {
            this.accuracy = accuracy;
            this.scoreV2 = scoreV2;
            this.player = player;
        }
        public readonly struct IngameLeaderboardPlayer
        {
            public readonly ReadOnlyMemory<char> name;
            public IngameLeaderboardPlayer(ReadOnlyMemory<char> name)
            {
                this.name = name;
            }
        }
    }

    public GameObject parentList, prefab;
    public IngamerankPrefabScript PlayerRankPrefab { get; private set; }
    public IngamerankPrefabScript[] RankPrefabs { get; private set;  }
    private ReadOnlyMemory<IngameLeaderboardRank> ranks = new();

    public override bool IsShownOnlyInTUFHelper => true;
    public override string ID => "Leaderboard";
    public override Sprite Icon => Main.assets.LoadAsset<Sprite>("assets/tufhelper/assets/sprites/leaderboard.png");
    public override Anchor DefaultAnchor => Anchor.RightMiddle;

    private readonly PassData _cachedPassData = new();
    private LevelData _cachedLevelData;
    private Judgements _judgements;
    private CancellationTokenSource currentRequestToken;
    private const int MAX_RANKS = 120;
    private bool isInit = false;

    #region Self-Contained Gameplay Event Hooks

    protected override async void OnPlay(PlayButtonEventArgs e)
    {
        if (!ADOFAIGameplayHandler.IsFromTUFHelper) return;
        isInit = false;
        _cachedLevelData = new LevelData(e.CurrentLevelInfo);
        _judgements = new();
        _cachedPassData.Judgements = _judgements;
        ranks = await GetPassesAsync(e.CurrentLevelInfo.ID);
        LoadLeaderboard(ranks);
        isInit = true;
    }
    protected override void OnHit(HitMargin hit)
    {
        if (!isInit) return;
        _cachedPassData.IsNoHoldTap = Persistence.holdBehavior == HoldBehavior.NoHoldNeeded;
        _cachedPassData.Speed = scnGame.instance.levelData.pitch / 100f * scnEditor.instance.playbackSpeed;

        var span = ranks.Span;
        var player = span[span.Length - 1];

        switch (hit)
        {
            case HitMargin.TooEarly:
                _judgements.EarlyDouble++;
                break;
            case HitMargin.VeryEarly:
                _judgements.EarlySingle++;
                break;
            case HitMargin.EarlyPerfect:
                _judgements.EPerfect++;
                break;
            case HitMargin.PerfectMinus:
                _judgements.Perfect++;
                break;
            case HitMargin.XPerfect:
                _judgements.Perfect++;
                break;
            case HitMargin.PerfectPlus:
                _judgements.Perfect++;
                break;
            case HitMargin.LatePerfect:
                _judgements.LPerfect++;
                break;
            case HitMargin.VeryLate:
                _judgements.LateSingle++;
                break;
            case HitMargin.TooLate:
                _judgements.LateDouble++;
                break;
            case HitMargin.FailMiss:
                _judgements.Deaths++;
                break;
            case HitMargin.FailOverload:
                _judgements.Deaths++;
                break;
            default:
                break;
        }

        float acc = (float)ScoreCalculator.CalcAcc(_judgements);
        float score = (float)ScoreCalculator.GetScoreV2(_cachedPassData, _cachedLevelData);

        IngameLeaderboardRank newPlayer = new(acc, score, new(player.player.name));

        if (span.Length == 1)
        {
            PlayerRankPrefab.LoadPass(newPlayer, 0);
            return;
        }
        if (PlayerRankPrefab == null || RankPrefabs == null) return;

        int leaderboardCount = ranks.Length - 1;
        int playerIndex = 0;

        for (int i = 0; i < leaderboardCount; i++)
        {
            if (span[i].scoreV2 > score)
            {
                playerIndex++;
            }
            else
            {
                break;
            }
        }

        PlayerRankPrefab.LoadPass(newPlayer, playerIndex);

        int totalEntries = leaderboardCount + 1; 
        int startIndex = Mathf.Max(0, playerIndex - 2);
        int endIndex = Mathf.Min(totalEntries, startIndex + 5);

        for (int i = 0; i < leaderboardCount; i++)
        {
            var rankPrefab = RankPrefabs[i];

            int virtualIndex = i >= playerIndex ? i + 1 : i;
            bool shouldBeVisible = virtualIndex >= startIndex && virtualIndex < endIndex;

            if (rankPrefab.gameObject.activeSelf != shouldBeVisible)
                rankPrefab.gameObject.SetActive(shouldBeVisible);

            if (shouldBeVisible)
            {
                rankPrefab.SetPosition(virtualIndex - startIndex);
                rankPrefab.SetRank(virtualIndex + 1);
            }
        }

        bool youVisible = playerIndex >= startIndex && playerIndex < endIndex;
        if (PlayerRankPrefab.gameObject.activeSelf != youVisible)
            PlayerRankPrefab.gameObject.SetActive(youVisible);

        if (youVisible)
        {
            PlayerRankPrefab.SetPosition(playerIndex - startIndex);
            PlayerRankPrefab.UpdateVisual();
        }
    }

    public override void OnSettingsOpened()
    {
        ReadOnlyMemory<IngameLeaderboardRank> passes = new(new IngameLeaderboardRank[5]
        {
            new(0.99f , 777.27f, new("Player 1".AsMemory())),
            new(0.97f , 682.39f, new("Player 2".AsMemory())),
            new(0.932f, 521.43f, new("Player 3".AsMemory())),
            new(0.895f, 331.09f, new("Player 4".AsMemory())),
            new(0.78f , 52.25f,  new("Player 5".AsMemory()))
        });

        LoadLeaderboard(passes);
    }

    #endregion

    public void LoadLeaderboard(ReadOnlyMemory<IngameLeaderboardRank> ranks)
    {
        for (int i = parentList.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(parentList.transform.GetChild(i).gameObject);
        }

        var span = ranks.Span;
        IngamerankPrefabScript[] prefabs = new IngamerankPrefabScript[ranks.Length];
        for (int i = 0; i < ranks.Length; i++)
        {
            GameObject obj = Instantiate(prefab, parentList.transform);
            var script = obj.GetComponent<IngamerankPrefabScript>();

            obj.SetActive(false);
            script.LoadPass(span[i], i);

            if (new string(span[i].player.name.Span) == "YOU")
            {
                PlayerRankPrefab = script;
            }

            prefabs[i] = script;
        }

        RankPrefabs = prefabs;
    }
    private async Task<ReadOnlyMemory<IngameLeaderboardRank>> GetPassesAsync(int levelID)
    {
        currentRequestToken?.Cancel();
        currentRequestToken = new CancellationTokenSource();
        CancellationToken token = currentRequestToken.Token;

        string url = LeaderboardScript.GetDefaultUrl(levelID);
        Stream stream;
        HttpResponseMessage response = await Main.Client.GetAsync(url, token);

        response.EnsureSuccessStatusCode();
        stream = await response.Content.ReadAsStreamAsync();

        Stopwatch stopwatch = Stopwatch.StartNew();
        var ranks = await Utf8Json.JsonSerializer.DeserializeAsync<IngameLeaderboardRank[]>(stream);

        if (ranks != null && ranks.Length > 1)
        {
            Array.Sort(ranks, static (a, b) => b.scoreV2.CompareTo(a.scoreV2));
        }

        int count = Math.Min(MAX_RANKS, ranks?.Length ?? 0);
        var combined = new IngameLeaderboardRank[count + 1];

        if (count > 0)
        {
            Array.Copy(ranks, 0, combined, 0, count);
        }

        combined[count] = new IngameLeaderboardRank(0, 0, new("YOU".AsMemory()));

        return new(combined);
    }

    protected override void OnDestroy()
    {
        currentRequestToken?.Cancel();
        base.OnDestroy();
    }
}
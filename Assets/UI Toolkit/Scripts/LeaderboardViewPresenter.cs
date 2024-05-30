using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UIElements;

class User : IEquatable<User>, IComparable<User>
{
    public string name;
    public string flagUri;
    public int points;

    public int CompareTo(User other)
    {
        return other.points.CompareTo(points);
    }

    public bool Equals(User other)
    {
        return points == other.points;
    }
}

public class LeaderboardViewPresenter : MonoBehaviour, IDataPersistence
{
    const string DEFAULT_USERNAME = "Alessio";
    const int BASE_PLACEMENT = 1;
    private int userPoints;
    private VisualElement _leaderboardView;
    private List<User> _mockUsers;
    private User currentUser;
    private Label rankingLabel;
    private GameData gameData;
    public void LoadData(GameData data)
    {
        userPoints = data.userPoints;
        gameData = data;
    }

    public void SaveData(GameData data)
    {
        data.userPoints = userPoints;
    }

    void Awake()
    {
        _mockUsers = new List<User>
        {
            new() {
                name = "Aisha",
                flagUri = "FlagPakistan",
                points = 80,
            },
            new() {
                name = "Max",
                flagUri = "FlagNetherlands",
                points = 40,
            },
            new() {
                name = "Alessio",
                flagUri = "FlagItaly",
                points = 0,
            },
            new() {
                name = "Miguel",
                flagUri = "FlagSpain",
                points = 25
            },
            new() {
                name = "Yuki",
                flagUri = "FlagJapan",
                points = 10
            }
        };
        currentUser = _mockUsers.Find(x => x.name == DEFAULT_USERNAME);
    }

    private (Label, Label) GetPodiumPlaceLabels(VisualElement podium, string place)
    {
        var placeLabel = podium.Q<Label>($"{place}PlaceNameLabel");
        var pointLabel = podium.Q<Label>($"{place}PlacePointsLabel");
        return ( placeLabel, pointLabel );
    }

    public void RenderRows(int addedPoints)
    {
        userPoints += addedPoints;
        var podium = _leaderboardView.Q<VisualElement>("Podium");
        var leaderboardRows = _leaderboardView.Q<VisualElement>("LeaderboardRows"); //container
        var rowTemplate = Resources.Load<VisualTreeAsset>("LeaderboardRow");
        leaderboardRows.Clear();
        currentUser.points = userPoints;
        _mockUsers.Sort();
        gameData.leaderboardPlacement.Value = _mockUsers.FindIndex(x => x.name == DEFAULT_USERNAME) + BASE_PLACEMENT;

        var (firstPlaceNameLbl, firstPlacePointsLbl) = GetPodiumPlaceLabels(podium, "First");
        var (secondPlaceNameLbl, secondPlacePointsLbl) = GetPodiumPlaceLabels(podium, "Second");
        var (thirdPlaceNameLbl, thirdPlacePointsLbl) = GetPodiumPlaceLabels(podium, "Third");
        firstPlaceNameLbl.text = _mockUsers[0].name;
        firstPlacePointsLbl.text = $"{_mockUsers[0].points} pt";
        secondPlaceNameLbl.text = _mockUsers[1].name;
        secondPlacePointsLbl.text = $"{_mockUsers[1].points} pt";
        thirdPlaceNameLbl.text = _mockUsers[2].name;
        thirdPlacePointsLbl.text = $"{_mockUsers[2].points} pt";

        for (int i = 0; i < _mockUsers.Count; i++)
        {
            var user = _mockUsers[i];
            var row = rowTemplate.Instantiate();
            row.style.height = Length.Percent(100 / _mockUsers.Count);
            if (i == _mockUsers.Count - 1)
            {
                row.Q<VisualElement>("Row").style.borderBottomWidth = 0;
                row.style.borderBottomLeftRadius = 40;
                row.style.borderBottomRightRadius = 40;
            }
            var placement = row.Q<Label>("PlaceNumberLabel");
            placement.text = (BASE_PLACEMENT + i).ToString();
            var name = row.Q<Label>("NameLabel");
            name.text = user.name;
            var flag = row.Q<VisualElement>("Flag");
            flag.style.backgroundImage = new StyleBackground(Resources.Load<VectorImage>(user.flagUri));
            var pointLabel = row.Q<Label>("PointsLabel");
            pointLabel.text = user.points.ToString();
            if (ReferenceEquals(user, currentUser))
            {
                StyleColor orange = new(new Color32(249, 170, 51, 255));
                placement.style.unityFontStyleAndWeight = FontStyle.Bold;
                placement.style.color = orange;

                name.style.unityFontStyleAndWeight = FontStyle.Bold;
                name.style.color = orange;

                pointLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                pointLabel.style.color = orange;

                // set rank in profile view
                rankingLabel.text = $"Rank #{BASE_PLACEMENT + i}";

                row.style.backgroundColor = new StyleColor(new Color32(74, 101, 114, 255));
            }
            leaderboardRows.Add(row);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        RenderRows(0);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetLeaderboardView(VisualElement visualElement)
    {
        _leaderboardView = visualElement;
    }
    public void SetRankingLabel(Label lbl)
    {
        rankingLabel = lbl;
    }
}

using Engine;

namespace SuperAdventure
{
    public partial class SuperAdventure : Form
    {
        private Player _player;
        private Monster _currentMonster;
        private PlayerQuest _mainQuest;
        public SuperAdventure()
        {
            InitializeComponent();

            _player = new Player(10, 10, 20, 0, 1);
            MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            _player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));

            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            lblGold.Text = _player.Gold.ToString();
            lblExperience.Text = _player.ExperiencePoints.ToString();
            lblLevel.Text = _player.Level.ToString();
        }

        private void MoveTo(Location newLocation)
        {
            if (newLocation.ItemRequiredToEnter != null)
            {
                if (_player.Inventory.FirstOrDefault(item => item.Details.ID == newLocation.ItemRequiredToEnter.ID) == null)
                {
                    rtbMessages.Text = "You must have a " + newLocation.ItemRequiredToEnter.Name + " to enter this location." + Environment.NewLine;
                    return;
                }
            }

            _player.CurrentLocation = newLocation;

            SetMoveButtons(newLocation);
            DisplayLocationInfo(newLocation);
            UpdatePlayerStats();

            if (LocationHasQuest(newLocation))
            {
                PlayerQuest playerQuest = PlayerHaveQuest(newLocation.QuestAvailableHere, _player.Quests);

                if (playerQuest == null)
                {
                    playerQuest = new PlayerQuest(newLocation.QuestAvailableHere, false);
                    _player.Quests.Add(playerQuest);
                }
                else if (playerQuest.IsCompleted)
                {
                    //check items
                }

                _mainQuest = playerQuest;
                DisplayLocationInfo(newLocation);
            }

        }


        private PlayerQuest PlayerHaveQuest(Quest questAvailable, List<PlayerQuest> playerQuests)
        => playerQuests.SingleOrDefault(quest => quest.Details.ID == questAvailable.ID);


        private bool PlayerHaveThisQuest(Quest questAvailable, List<PlayerQuest> playerQuests)
            => playerQuests.Any(q => q.Details.ID == questAvailable.ID);

        private bool LocationHasQuest(Location location) => location.QuestAvailableHere != null;

        private void UpdatePlayerStats()
        {
            _player.CurrentHitPoints = _player.MaximumHitPoints;
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
        }

        private void DisplayLocationInfo(Location location)
        {
            rtbLocation.Text = location.Name + Environment.NewLine;
            rtbLocation.Text += location.Description + Environment.NewLine;

            rtbLocation.Text += "Main quest: " + (_mainQuest != null ? _mainQuest.Details.Name.ToString() : "");
        }

        private void SetMoveButtons(Location location)
        {
            btnNorth.Visible = location.LocationToNorth != null;
            btnEast.Visible = location.LocationToEast != null;
            btnSouth.Visible = location.LocationToSouth != null;
            btnWest.Visible = location.LocationToWest != null;
        }

        private void UpdatePlayerLocation(Location location)
        {

        }

        private void btnNorth_Click(object sender, EventArgs e) => MoveTo(_player.CurrentLocation.LocationToNorth);

        private void btnEast_Click(object sender, EventArgs e) => MoveTo(_player.CurrentLocation.LocationToEast);

        private void btnSouth_Click(object sender, EventArgs e) => MoveTo(_player.CurrentLocation.LocationToSouth);

        private void btnWest_Click(object sender, EventArgs e) => MoveTo(_player.CurrentLocation.LocationToWest);

        private void btnUseWeapon_Click(object sender, EventArgs e) { }

        private void btnUsePotion_Click(object sender, EventArgs e)
        {

        }
    }
}
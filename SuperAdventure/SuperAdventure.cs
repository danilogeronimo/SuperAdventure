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
            RenewPlayerStats();

            if (LocationHasQuest(newLocation))
            {
                PlayerQuest playerQuest = PlayerHaveQuest(newLocation.QuestAvailableHere, _player.Quests);

                if (playerQuest == null)
                {
                    playerQuest = new PlayerQuest(newLocation.QuestAvailableHere, false);
                    _player.Quests.Add(playerQuest);
                }
                else if (!playerQuest.IsCompleted)
                    if (CheckQuestItemCompletion(newLocation.QuestAvailableHere.QuestCompletionItems))
                        CompleteQuest(newLocation, ref playerQuest);

                _mainQuest = playerQuest;
                DisplayLocationInfo(newLocation);
            }

            CheckLocationMonsters(newLocation);

            RefreshPlayerStats();
            RefreshUI();

        }

        private void CompleteQuest(Location location, ref PlayerQuest playerQuest)
        {
            playerQuest.IsCompleted = true;
            RemovePlayerQuestItems(location.QuestAvailableHere.QuestCompletionItems);
            AddItemToPlayerIventory(location.QuestAvailableHere.RewardItem);
            DisplayQuestMsg(location);
        }

        private void RefreshUI()
        {
            FillCombatCmb();
            //TODO potions
        }

        private void RefreshPlayerStats()
        {
            RefreshPlayerInventory();
            RefreshPlayerQuest();
        }

        private void RefreshPlayerQuest()
        {
        }

        private void RefreshPlayerInventory()
        {

        }

        private void CheckLocationMonsters(Location location)
        {
            if (location.MonsterLivingHere == null)
            {
                EnableControlCombat(false);
                return;
            }

            EnableControlCombat(true);
            DisplayMonstersMsg(location.MonsterLivingHere);

            SpawnMonster(location.MonsterLivingHere);
        }

        private void SpawnMonster(Monster monsterLivingHere)
        {
            _currentMonster = World.MonsterByID(monsterLivingHere.ID);

            FillCombatCmb();
        }

        private void FillCombatCmb()
        {
            List<Item> items = new List<Item>();
            cboWeapons.DataSource = null;

            foreach (InventoryItem ii in _player.Inventory)
            {
                if (ii.Details.GetType().Name == "Weapon")
                {
                    Item item = World.ItemByID(ii.Details.ID);
                    items.Add(item);
                }
            }

            cboWeapons.DataSource = items;
            cboWeapons.DisplayMember = "Name";
        }

        private void DisplayMonstersMsg(Monster monster)
        {
            rtbMessages.Text += "You see a " + monster.Name;
        }

        private void EnableControlCombat(bool enabled = true)
        {
            btnUsePotion.Enabled = enabled;
            btnUseWeapon.Enabled = enabled;
            cboPotions.Enabled = enabled;
            cboWeapons.Enabled = enabled;
        }

        private void DisplayQuestMsg(Location newLocation)
        {
            rtbMessages.Text += "You completed the " + newLocation.QuestAvailableHere.Name + " quest!" + Environment.NewLine;
            rtbMessages.Text += "You received: " + newLocation.QuestAvailableHere.RewardExperiencePoints.ToString() + " xp and "
                + newLocation.QuestAvailableHere.RewardGold.ToString() + " gold and "
                + newLocation.QuestAvailableHere.RewardItem.Name + " item";
            _player.ExperiencePoints += newLocation.QuestAvailableHere.RewardExperiencePoints;
            _player.Gold += newLocation.QuestAvailableHere.RewardGold;
        }

        private void AddItemToPlayerIventory(Item rewardItem)
        {
            bool isNewItem = true;

            foreach (InventoryItem ii in _player.Inventory)
            {
                if (ii.Details.ID == rewardItem.ID)
                {
                    ii.Quantity++;
                    isNewItem = false;
                }
            }
            if (isNewItem)
                _player.Inventory.Add(new InventoryItem(rewardItem, 1));
        }

        private void RemovePlayerQuestItems(List<QuestCompletionItem> questCompletionItems)
        {
            foreach (QuestCompletionItem item in questCompletionItems)
            {
                foreach (InventoryItem inventoryItem in _player.Inventory)
                {
                    if (item.Details.ID == inventoryItem.Details.ID)
                    {
                        inventoryItem.Quantity = 0;
                        break;
                    }
                }
            }
        }

        private bool CheckQuestItemCompletion(List<QuestCompletionItem> questCompletionItems)
        {
            bool hasTheItem = false;

            foreach (QuestCompletionItem item in questCompletionItems)
            {
                foreach (InventoryItem inventoryItem in _player.Inventory)
                {
                    if (item.Details.ID == inventoryItem.Details.ID)
                    {
                        if (inventoryItem.Quantity == item.Quantity)
                        {
                            hasTheItem = true;
                            break;
                        }
                    }
                }
            }
            return hasTheItem;
        }

        private PlayerQuest PlayerHaveQuest(Quest questAvailable, List<PlayerQuest> playerQuests)
        => playerQuests.Find(quest => quest.Details.ID == questAvailable.ID);

        private bool PlayerHaveThisQuest(Quest questAvailable, List<PlayerQuest> playerQuests)
            => playerQuests.Any(q => q.Details.ID == questAvailable.ID);

        private bool LocationHasQuest(Location location) => location.QuestAvailableHere != null;

        private void RenewPlayerStats()
        {
            _player.CurrentHitPoints = _player.MaximumHitPoints;
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
        }

        private void DisplayLocationInfo(Location location)
        {
            rtbLocation.Text = location.Name + Environment.NewLine;
            rtbLocation.Text += location.Description + Environment.NewLine;

            rtbLocation.Text += (_mainQuest != null ? Environment.NewLine + "Main quest: " + _mainQuest.Details.Name.ToString() : String.Empty);

            rtbMessages.Text = String.Empty;
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

        private void btnUseWeapon_Click(object sender, EventArgs e)
        {
            if (_currentMonster == null)
                return;
            InitCombat();
        }

        private void InitCombat()
        {
            DisplayCombatMsg("You used " + cboWeapons.Text + " against " + _currentMonster.Name);

            Weapon selectedWeapon = (Weapon)cboWeapons.SelectedItem;

            int hitPoint = new Random().Next(selectedWeapon.MinimumDamage, selectedWeapon.MaximumDamage);
            if (hitPoint > 0)
            {
                DisplayCombatMsg("You hit " + hitPoint.ToString());

                _currentMonster.CurrentHitPoints = _currentMonster.MaximumHitPoints - hitPoint;

                if (_currentMonster.CurrentHitPoints <= 0)
                {
                    ReceiveLoot();
                    _currentMonster = null;

                    btnUseWeapon.Enabled = false;

                    DisplayCombatMsg("You beat the monster!" +
                        Environment.NewLine +
                        "You received: " +
                        Environment.NewLine

                        );
                }
            }
            else
                DisplayCombatMsg("You missed the attack.");
        }

        private void ReceiveLoot()
        {
            List<LootItem> loot = _currentMonster.LootTable;
            foreach (LootItem item in loot)
            {
                Random random = new Random();

                int r = random.Next(0, 100);

                if (r >= item.DropPercentage)
                    AddItemToPlayerIventory(World.ItemByID(item.Details.ID));
            }
        }

        private void DisplayCombatMsg(string msg)
        => rtbMessages.Text += Environment.NewLine + msg;

        private void btnUsePotion_Click(object sender, EventArgs e)
        {

        }
    }
}
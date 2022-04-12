using Engine;

namespace SuperAdventure
{
    public partial class SuperAdventure : Form
    {
        private Player _player;
        private Monster _currentMonster;
        private string _activeQuest, _completedQuest, _activeMonster;
        bool _alreadyDisplayed = false;

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
            HealPlayer();

            if (LocationHasQuest(newLocation))
                SetPlayerQuest(newLocation);

            if (LocationHasMonster(newLocation))
                setMonster(newLocation);
            else
            {
                _currentMonster = null;
                _activeMonster = string.Empty;
                EnableControlCombat(false);
            }

            DisplayLocationInfo(newLocation);
            RefreshPlayerStats();
            RefreshUI();
        }

        private bool LocationHasMonster(Location location)
         => location.MonsterLivingHere != null;

        private void SetPlayerQuest(Location location)
        {
            PlayerQuest playerQuest = PlayerHaveQuest(location.QuestAvailableHere, _player.Quests);

            if (playerQuest == null)
            {
                playerQuest = new PlayerQuest(location.QuestAvailableHere, false);
                _player.Quests.Add(playerQuest);

                string msg = string.Empty;

                msg += Environment.NewLine;
                msg += Environment.NewLine;
                msg += "Active quest: " + playerQuest.Details.Name;
                msg += Environment.NewLine;
                msg += playerQuest.Details.Description;

                _activeQuest = msg;
                _alreadyDisplayed = false;
            }
            else if (!playerQuest.IsCompleted)
                if (CheckQuestItemCompletion(location.QuestAvailableHere.QuestCompletionItems))
                    CompleteQuest(location, ref playerQuest);
        }

        private void DisplayLocationInfo(Location newLocation)
        {
            rtbLocation.Text = newLocation.Name.ToString();
            rtbLocation.Text += Environment.NewLine;
            rtbLocation.Text += newLocation.Description.ToString();

            string msgQuest = string.Empty;
            string msgMonster = string.Empty;

            rtbMessages.Text = string.Empty;

            if (!string.IsNullOrEmpty(_activeQuest))
                msgQuest += _activeQuest;

            if (!string.IsNullOrEmpty(_completedQuest))
            {
                msgQuest += _completedQuest;
                _alreadyDisplayed = true;
            }

            if (!string.IsNullOrEmpty(_activeMonster))
                msgMonster += _activeMonster;

            if (_alreadyDisplayed)
            {
                _alreadyDisplayed = false;
                _completedQuest = string.Empty;
            }
            rtbLocation.Text += msgQuest;
            rtbMessages.Text += msgMonster;
        }

        private void CompleteQuest(Location location, ref PlayerQuest playerQuest)
        {
            playerQuest.IsCompleted = true;
            RemovePlayerQuestItems(location.QuestAvailableHere.QuestCompletionItems);
            AddItemToPlayerIventory(location.QuestAvailableHere.RewardItem);

            string msg = string.Empty;

            msg = Environment.NewLine + Environment.NewLine;
            msg += "You completed the " + location.QuestAvailableHere.Name + " quest!" + Environment.NewLine;
            msg += "You received: " + location.QuestAvailableHere.RewardExperiencePoints.ToString() + " xp and "
                + location.QuestAvailableHere.RewardGold.ToString() + " gold and "
                + location.QuestAvailableHere.RewardItem.Name + " item";
            _player.ExperiencePoints += location.QuestAvailableHere.RewardExperiencePoints;
            _player.Gold += location.QuestAvailableHere.RewardGold;

            _completedQuest = msg;
            _activeQuest = string.Empty;
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

        private void setMonster(Location location)
        {
            EnableControlCombat(true);

            SpawnMonster(location.MonsterLivingHere);
            FillCombatCmb();

            _activeMonster += "You see a " + location.MonsterLivingHere.Name;

            if (location.MonsterLivingHere != null)
            {

            }
            else
            {

            }
        }

        private void SpawnMonster(Monster monsterLivingHere) => _currentMonster = World.MonsterByID(monsterLivingHere.ID);

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


        private void EnableControlCombat(bool enabled = true)
        {
            btnUsePotion.Enabled = enabled;
            btnUseWeapon.Enabled = enabled;
            cboPotions.Enabled = enabled;
            cboWeapons.Enabled = enabled;
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

        private void HealPlayer()
        {
            _player.CurrentHitPoints = _player.MaximumHitPoints;
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
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
                    string itemName = string.Empty;
                    string itemMsg;

                    btnUseWeapon.Enabled = false;

                    List<string> lstItem = ReceiveLoot();
                    _player.Gold += _currentMonster.RewardGold;
                    _player.ExperiencePoints += _currentMonster.RewardExperiencePoints;
                    UpdatePlayerStats();

                    if (lstItem != null)
                        foreach (string name in lstItem)
                            itemName += name + Environment.NewLine;

                    itemMsg = "You received: " + Environment.NewLine;
                    itemMsg += !string.IsNullOrEmpty(itemName) ? itemName : string.Empty;
                    itemMsg += _currentMonster.RewardGold.ToString() + " Gold";
                    itemMsg += Environment.NewLine + _currentMonster.RewardExperiencePoints.ToString() + " XP";

                    DisplayCombatMsg("You beat the monster!" +
                    Environment.NewLine +
                    itemMsg +
                    Environment.NewLine
                    );

                    _currentMonster = null;
                }
            }
            else
                DisplayCombatMsg("You missed the attack.");
        }

        private void UpdatePlayerStats()
        {
            lblExperience.Text = _player.ExperiencePoints.ToString();
            lblGold.Text = _player.Gold.ToString();
        }

        private List<string> ReceiveLoot()
        {
            List<string> lstItem = new List<string>();
            //foreach (LootItem item in _currentMonster.LootTable)
            //{
            //    if (new Random().Next(100 - item.DropPercentage, 100) >= item.DropPercentage)
            //    {
            //        Item it = World.ItemByID(item.Details.ID);
            //        AddItemToPlayerIventory(it);
            //        lstItem.Add(item.Details.Name);
            //    }
            //}
            Item it = World.ItemByID(2);
            AddItemToPlayerIventory(it);
            lstItem.Add("Rat tail");
            return lstItem;
        }

        private void DisplayCombatMsg(string msg)
        => rtbMessages.Text += Environment.NewLine + msg;

        private void btnUsePotion_Click(object sender, EventArgs e)
        {

        }
    }
}
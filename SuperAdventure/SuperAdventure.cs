using Engine;
using System.IO;
using System.Numerics;

namespace SuperAdventure
{
    public partial class SuperAdventure : Form
    {
        private Player _player;
        private Monster _currentMonster;
        private string _activeQuest, _completedQuest, _activeMonster;
        bool _alreadyDisplayed = false;
        private const string PLAYER_DATA_FILE_NAME = "PlayerData.xml";

        public SuperAdventure()
        {
            InitializeComponent();
            InitQuestsGrid();

            if (File.Exists(PLAYER_DATA_FILE_NAME))
            {
                _player = Player.CreatePlayerFromXmlString(File.ReadAllText(PLAYER_DATA_FILE_NAME));
                RefreshCombatCombo();
                setQuestGrid();
                RefreshItemsCombo();
            }
            else
                _player = Player.CreateDefaultPlayer();

            MoveTo(World.LocationByID(_player.CurrentLocation.ID));

            UpdatePlayerStats();            

            initInventoryGrid();
            RefreshInventoryGrid();
        }

        private void InitQuestsGrid()
        {
            dgvQuests.Columns.Add("ID", "ID");
            dgvQuests.Columns["ID"].Visible = false;

            dgvQuests.Columns.Add("Title", "Title");
            dgvQuests.Columns.Add("Completed", "Completed");
            dgvQuests.Columns.Add("Item", "Item needed");
        }

        private void initInventoryGrid()
        {
            dgvInventory.Columns.Add("Name", "Name");
            dgvInventory.Columns.Add("Qtde", "Qtde");
        }

        private void MoveTo(Location newLocation)
        {
            if (CheckIfItemIsRequired(newLocation))
            {
                rtbMessages.Text += "You must have a " + newLocation.ItemRequiredToEnter.Name + " to enter this location." + Environment.NewLine;
                return;
            }

            _player.CurrentLocation = newLocation;
            DisplayLocationInfo(newLocation);
            SetMoveButtons(newLocation);
            LookForQuests(newLocation);
            LookForMonsters(newLocation);
        }

        private bool CheckIfItemIsRequired(Location location)
        {
            if (location.ItemRequiredToEnter == null) return false;

            return !_player.Inventory.Exists(ii => ii.Details.ID == location.ItemRequiredToEnter.ID
                && ii.Quantity > 0);
        }

        private void LookForMonsters(Location newLocation)
        {
            if (LocationHasMonster(newLocation))
            {
                setMonster(newLocation.MonsterLivingHere);
                RefreshCombos();
            }
            else
                EnableControlCombat(false);
        }

        private void CompleteQuest(Location location, PlayerQuest playerQuest)
        {
            playerQuest.IsCompleted = true;
            RemovePlayerInventoryItens(location.QuestAvailableHere.QuestCompletionItems);
            AddItemToPlayerIventory(location.QuestAvailableHere.RewardItem);

            RefreshItemsCombo();

            string msg = string.Empty;

            msg = Environment.NewLine + Environment.NewLine;
            msg += "You completed the " + location.QuestAvailableHere.Name + " quest!" + Environment.NewLine;
            msg += "You received: " + location.QuestAvailableHere.RewardExperiencePoints.ToString() + " xp and "
                + location.QuestAvailableHere.RewardGold.ToString() + " gold and "
                + location.QuestAvailableHere.RewardItem.Name + " item";
            _player.Gold += location.QuestAvailableHere.RewardGold;
            _player.ExperiencePoints += location.QuestAvailableHere.RewardExperiencePoints;

            rtbLocation.Text = msg;
            _activeQuest = string.Empty;
        }

        private void RefreshItemsCombo()
        {
            List<HealingPotion> healingPotionList = new List<HealingPotion>();
            foreach (InventoryItem ii in _player.Inventory)
                if (ii.Details is HealingPotion)
                    healingPotionList.Add((HealingPotion)ii.Details);

            cboPotions.DisplayMember = "Name";
            cboPotions.ValueMember = "ID";
            cboPotions.DataSource = healingPotionList;
        }

        private void setQuestGrid()
        {
            if (_player.Quests.Count < 1) return;

            string itensQuest = string.Empty;

            dgvQuests.Rows.Clear();

            foreach (PlayerQuest pq in _player.Quests)
            {
                itensQuest = string.Empty;
                foreach (QuestCompletionItem qci in pq.Details.QuestCompletionItems)
                    itensQuest += qci.Quantity + " " + qci.Details.Name.ToString();

                itensQuest += "\n";
                dgvQuests.Rows.Add(pq.Details.ID,pq.Details.Name, (pq.IsCompleted) ? "Yes" : "No", itensQuest);
            }

           
        }

        private void LookForQuests(Location newLocation)
        {
            if (LocationHasQuest(newLocation))
            {
                if (PlayerHasThisQuest(newLocation.QuestAvailableHere, _player.Quests))
                {
                    if (!QuestAlreadyCompleted(newLocation.QuestAvailableHere))
                    {
                        if (PlayerHasTheItens(newLocation.QuestAvailableHere, _player.Inventory))
                        {
                            CompleteQuest(newLocation,_player.Quests.Find(pq => pq.Details.ID == newLocation.QuestAvailableHere.ID));
                            setQuestGrid();
                        }
                    }
                    else
                        setQuestGrid();
                }
                else //new quest
                    SetPlayerQuest(newLocation);
            }
        }

        private bool PlayerHasThisQuest(Quest questAvailable, List<PlayerQuest> playerQuests)
                    => playerQuests.Exists(q => q.Details.ID == questAvailable.ID);

        private bool QuestAlreadyCompleted(Quest quest) =>
            _player.Quests.Exists(pq => pq.Details.ID == quest.ID && pq.IsCompleted); 
        
        private bool PlayerHasTheItens(Quest quest, List<InventoryItem> playerItens)
        {
            foreach (InventoryItem ii in playerItens)            
                foreach (QuestCompletionItem qqi in quest.QuestCompletionItems)
                    if (quest.QuestCompletionItems.Exists(qqi => qqi.Details.ID == ii.Details.ID && qqi.Quantity == ii.Quantity)) return true;
            
            return false;
        }

        private bool LocationHasMonster(Location location)
         => location.MonsterLivingHere != null;
        private void SetPlayerQuest(Location location)
        {
            _player.Quests.Add(new PlayerQuest(location.QuestAvailableHere, false));
            //definir grid
            setQuestGrid();
        }
        private void DisplayLocationInfo(Location newLocation)
        {
            rtbLocation.Text = newLocation.Name.ToString();
            rtbLocation.Text += Environment.NewLine;
            rtbLocation.Text += newLocation.Description.ToString();

            string msgQuest = string.Empty;
            string msgMonster = string.Empty;

            //rtbMessages.Text = string.Empty;

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
        private void RefreshUI()
        {
            //FillCombatCmb();
            //TODO potions
        }

        private void setMonster(Monster monster)
        {
            _currentMonster = SpawnMonster(monster);
            DisplayMonsterInfo();
            
            EnableControlCombat(true);

            MonsterAttack(_currentMonster);
        }

        private void DisplayMonsterInfo()
        {
            rtbMessages.Text += "You see a " + _currentMonster.Name;
        }

        private void MonsterAttack(Monster monster)
        {
            tMonsterAttack.Interval = 6000;
            tMonsterAttack.Enabled = true;
        }

        private Monster SpawnMonster(Monster monsterLivingHere) => World.MonsterByID(monsterLivingHere.ID);

        private void RefreshCombos()
        {
            RefreshCombatCombo();
            RefreshItemCombo();
        }

        private void RefreshItemCombo()
        {
            return;
        }

        private void RefreshCombatCombo()
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

        private void RemovePlayerInventoryItens(List<QuestCompletionItem> questCompletionItems)
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
        private bool LocationHasQuest(Location location) => location.QuestAvailableHere != null;

        private void SetMoveButtons(Location location)
        {
            btnNorth.Visible = location.LocationToNorth != null;
            btnEast.Visible = location.LocationToEast != null;
            btnSouth.Visible = location.LocationToSouth != null;
            btnWest.Visible = location.LocationToWest != null;
        }

        private void btnNorth_Click(object sender, EventArgs e) => MoveTo(_player.CurrentLocation.LocationToNorth);

        private void btnEast_Click(object sender, EventArgs e) => MoveTo(_player.CurrentLocation.LocationToEast);

        private void btnSouth_Click(object sender, EventArgs e) => MoveTo(_player.CurrentLocation.LocationToSouth);

        private void btnWest_Click(object sender, EventArgs e) => MoveTo(_player.CurrentLocation.LocationToWest);

        private void btnUseWeapon_Click(object sender, EventArgs e)
        {
            DamageToMonster();                  

            //killed the monster
            if(_currentMonster == null)
            {
                RefreshCombos();
                RefreshInventoryGrid();
            }
        }

        private void RefreshInventoryGrid()
        {
            if(_player.Inventory.Count == 0) return;

            dgvInventory.Rows.Clear();

            foreach(InventoryItem ii in _player.Inventory)
                dgvInventory.Rows.Add(ii.Details.Name, ii.Quantity);
        }

        private void DamageToMonster()
        {
            DisplayCombatMsg("You used " + cboWeapons.Text + " against " + _currentMonster.Name);

            Weapon selectedWeapon = (Weapon)cboWeapons.SelectedItem;

            int hitPoint = new Random().Next(selectedWeapon.MinimumDamage, selectedWeapon.MaximumDamage);
            if (hitPoint > 0)
            {
                DisplayCombatMsg("You hit " + hitPoint.ToString());

                _currentMonster.CurrentHitPoints = _currentMonster.CurrentHitPoints - hitPoint;

                if (_currentMonster.CurrentHitPoints <= 0)
                {
                    tMonsterAttack.Enabled = false;

                    string itemName = string.Empty;
                    string itemMsg;

                    btnUseWeapon.Enabled = false;

                    List<string> lstItem = ReceiveLoot();
                    _player.Gold += _currentMonster.RewardGold;
                    _player.ExperiencePoints += _currentMonster.RewardExperiencePoints;

                    if (lstItem != null)
                        foreach (string name in lstItem)
                            itemName += name + Environment.NewLine;

                    itemMsg = "You received: " + Environment.NewLine;
                    itemMsg += !string.IsNullOrEmpty(itemName) ? itemName : string.Empty;
                    itemMsg += _currentMonster.RewardGold.ToString() + " Gold";
                    itemMsg += Environment.NewLine + _currentMonster.RewardExperiencePoints.ToString() + " XP";

                    DisplayCombatMsg(
                        "You beat the monster!" +
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
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            lblLevel.Text = _player.Level.ToString();
        }

        private List<string> ReceiveLoot()
        {
            List<string> lstItem = new List<string>();
            foreach (LootItem item in _currentMonster.LootTable)
            {
                if (new Random().Next(100 - item.DropPercentage, 100) >= item.DropPercentage)
                {
                    Item it = World.ItemByID(item.Details.ID);                   
                    AddItemToPlayerIventory(it);
                    lstItem.Add(item.Details.Name);
                }
            }
            return lstItem;
        }

        private void tMonsterAttack_Tick(object sender, EventArgs e)
        {
            Random random = new Random();
            int damage = random.Next(0, _currentMonster.MaximumDamage);

            if (damage > 0)
            {
                MonsterHitPlayer(damage);
                UpdatePlayerStats();
            }
        }

        private void MonsterHitPlayer(int damage)
        {
            rtbMessages.Text += Environment.NewLine;
            rtbMessages.Text += "You take " + damage.ToString() + " damage from the " + _currentMonster.Name;
            _player.CurrentHitPoints -= damage;

            if (_player.CurrentHitPoints <= 0)
                GameOver();
        }

        private void GameOver()
        {
            tMonsterAttack.Enabled = false;
            _player.CurrentHitPoints = 0;

            rtbMessages.Text += Environment.NewLine;
            rtbMessages.Text += "YOU DIED";
            setAllButtons(false);
        }

        private void setAllButtons(bool status)
        {
            btnUseWeapon.Enabled = status;
            btnUsePotion.Enabled = status;
            btnNorth.Enabled = status;
            btnEast.Enabled = status;
            btnSouth.Enabled = status;
            btnWest.Enabled = status;
        }

        private void rtbMessages_TextChanged(object sender, EventArgs e)
        {
            rtbMessages.SelectionStart = rtbMessages.Text.Length;
            rtbMessages.ScrollToCaret();
        }

        private void SuperAdventure_FormClosing(object sender, FormClosingEventArgs e)
            =>  File.WriteAllText(PLAYER_DATA_FILE_NAME,_player.ToXMLString());

        private void DisplayCombatMsg(string msg)
        => rtbMessages.Text += Environment.NewLine + msg;

        private void btnUsePotion_Click(object sender, EventArgs e)
        {
            if (cboPotions.SelectedIndex == -1)
                return;

            HealingPotion selectedPotion = (HealingPotion)cboPotions.SelectedItem;
            _player.CurrentHitPoints += selectedPotion.AmountToHeal;

            if (_player.CurrentHitPoints > _player.MaximumHitPoints)
                _player.CurrentHitPoints = _player.MaximumHitPoints;

            rtbMessages.Text += Environment.NewLine;
            rtbMessages.Text += "You healed himself with " + selectedPotion.AmountToHeal.ToString() + " points";

            UpdatePlayerStats();

            foreach (InventoryItem ii in _player.Inventory)
                if (ii.Details.ID == selectedPotion.ID && ii.Quantity > 0)
                    ii.Quantity--;
        }
    }
}
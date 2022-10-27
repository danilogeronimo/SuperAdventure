using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Xml;


namespace Engine
{
    public class Player : LivingCreature
    {
        public int Gold { get; set; }
        public int Level { get { return ((ExperiencePoints / 100) + 1); } }
        public int ExperiencePoints { get; set; }
        public List<InventoryItem> Inventory { get; set; }
        public List<PlayerQuest> Quests { get; set; }
        public Location CurrentLocation { get; set; }

        private Player(int currentHitPoints, int maximumHitPoints, int gold, int experiencePoints)
                :base(currentHitPoints, maximumHitPoints)
        {
            Gold = gold;
            ExperiencePoints = experiencePoints;
            Inventory = new List<InventoryItem>();
            Quests = new List<PlayerQuest>();
        }

        public static Player CreateDefaultPlayer()
        {
            Player player = new Player(10,10,20,0);
            player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));
            player.CurrentLocation = World.LocationByID(World.LOCATION_ID_HOME);

            return player;
        }

        public static Player CreatePlayerFromXmlString(string xmlPlayerData)
        {
            try
            {
                XmlDocument playerData = new XmlDocument();

                playerData.LoadXml(xmlPlayerData);

                Player player = RetrieveLoadedPlayerStats(playerData);
                player.CurrentLocation = World.LocationByID(RetrieveLoadedPlayerLocationID(playerData));
                player.Inventory = RetrieveLoadedPlayerInventory(playerData, player);
                player.Quests = RetrieveLoadedPlayerQuest(playerData, player);

                return player;
            }
            catch
            {
                return Player.CreateDefaultPlayer();
            }
        }

        private static List<PlayerQuest> RetrieveLoadedPlayerQuest(XmlDocument playerData, Player player)
        {
            List<PlayerQuest> playerQuest = new List<PlayerQuest>();

            foreach (XmlNode node in playerData.SelectNodes("/Player/PlayerQuests/PlayerQuest"))
            {
                int id = Convert.ToInt32(node.Attributes["ID"].Value);
                bool isCompleted = Convert.ToBoolean(node.Attributes["IsCompleted"].Value);
                playerQuest.Add(new PlayerQuest(World.QuestByID(id), isCompleted));                
            }

            return playerQuest;
        }

        private static List<InventoryItem> RetrieveLoadedPlayerInventory(XmlDocument playerData, Player player)
        {
            List<InventoryItem> inventoryItem = new List<InventoryItem>();

            foreach (XmlNode node in playerData.SelectNodes("/Player/InvetoryItems/InventoryItem"))
            {
                int id = Convert.ToInt32(node.Attributes["ID"].Value);
                int quantity = Convert.ToInt32(node.Attributes["Quantity"].Value);
                inventoryItem.Add(new InventoryItem(World.ItemByID(id), quantity));
            }
            return inventoryItem;
        }

        private static int RetrieveLoadedPlayerLocationID(XmlDocument playerData)
            => Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentLocation").InnerText);

        private static Player RetrieveLoadedPlayerStats(XmlDocument playerData)
        {
            int currentHitPoint = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentHitPoints").InnerText);
            int maximumHitPoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/MaximumHitPoints").InnerText);
            int gold = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/Gold").InnerText);
            int experiencePoints = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/ExperiencePoints").InnerText);

            return new Player(currentHitPoint,maximumHitPoints,gold,experiencePoints);
        }

        public string ToXMLString()
        {
            XmlDocument playerData = new XmlDocument();

            XmlNode player = playerData.CreateElement("Player");
            playerData.AppendChild(player);

            BuildStatsXml(playerData, player);
            BuildInventoryXml(playerData, player);
            BuildQuestXml(playerData, player);

            return playerData.InnerXml;
        }

        private void BuildQuestXml(XmlDocument playerData, XmlNode player)
        {
            XmlNode playerQuests = playerData.CreateElement("PlayerQuests");
            player.AppendChild(playerQuests);

            XmlNode teste = playerData.CreateElement("Teste");

            foreach (PlayerQuest pq in this.Quests)
            {
                XmlNode playerQuest = playerData.CreateElement("PlayerQuest");

                XmlAttribute playerQuestId = playerData.CreateAttribute("ID");
                playerQuestId.Value = pq.Details.ID.ToString();
                playerQuest.Attributes.Append(playerQuestId);

                XmlAttribute playerQuestIsCompleted = playerData.CreateAttribute("IsCompleted");
                playerQuestIsCompleted.Value = pq.IsCompleted.ToString();
                playerQuest.Attributes.Append(playerQuestIsCompleted);

                playerQuests.AppendChild(playerQuest);
            }

            //playerQuests.AppendChild(teste);  
        }

        private void BuildInventoryXml(XmlDocument playerData, XmlNode player)
        {
            XmlNode invetoryItems = playerData.CreateElement("InvetoryItems");
            player.AppendChild(invetoryItems);

            foreach (InventoryItem ii in this.Inventory)
            {
                XmlNode inventoryItem = playerData.CreateElement("InventoryItem");

                XmlAttribute idAttribute = playerData.CreateAttribute("ID");
                idAttribute.Value = ii.Details.ID.ToString();
                inventoryItem.Attributes.Append(idAttribute);

                XmlAttribute quantityAttribute = playerData.CreateAttribute("Quantity");
                quantityAttribute.Value = ii.Quantity.ToString();
                inventoryItem.Attributes.Append(quantityAttribute);

                invetoryItems.AppendChild(inventoryItem);
            }

        }

        private void BuildStatsXml(XmlDocument playerData, XmlNode player)
        {
            XmlNode stats = playerData.CreateElement("Stats");
            player.AppendChild(stats);

            XmlNode currentHitPoints = playerData.CreateElement("CurrentHitPoints");
            currentHitPoints.AppendChild(playerData.CreateTextNode(this.CurrentHitPoints.ToString()));
            stats.AppendChild(currentHitPoints);

            XmlNode maximumHitPoints = playerData.CreateElement("MaximumHitPoints");
            maximumHitPoints.AppendChild(playerData.CreateTextNode(this.MaximumHitPoints.ToString()));
            stats.AppendChild(maximumHitPoints);

            XmlNode gold = playerData.CreateElement("Gold");
            gold.AppendChild(playerData.CreateTextNode(this.Gold.ToString()));
            stats.AppendChild(gold);

            XmlNode experience = playerData.CreateElement("ExperiencePoints");
            experience.AppendChild(playerData.CreateTextNode(this.ExperiencePoints.ToString()));
            stats.AppendChild(experience);

            XmlNode currentLocation = playerData.CreateElement("CurrentLocation");
            currentLocation.AppendChild(playerData.CreateTextNode(this.CurrentLocation.ID.ToString()));
            stats.AppendChild(currentLocation);
        }
    }
}

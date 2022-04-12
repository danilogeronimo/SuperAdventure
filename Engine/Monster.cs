using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class Monster : LivingCreature
    {

        public int ID { get; set; }
        public string Name { get; set; }
        public int MaximumDamage { get; set; }
        public int RewardExperiencePoints { get; set; }
        public int RewardGold { get; set; }
        public List<LootItem> LootTable { get; set; }


        public Monster(int iD, string name, int maximumDamage, int rewardExperiencePoints, int rewardGold, int currentHitPoints, int maximumHitPoints) : base(currentHitPoints, maximumHitPoints)
        {
            ID = iD;
            Name = name;
            MaximumDamage = maximumDamage;
            RewardExperiencePoints = rewardExperiencePoints;
            RewardGold = rewardGold;
            CurrentHitPoints = currentHitPoints;
            MaximumHitPoints = maximumHitPoints;
            LootTable = new List<LootItem>();
        }

        public Monster(Monster monster) : base(monster.CurrentHitPoints, monster.MaximumHitPoints)
        {
            ID = monster.ID;
            Name = monster.Name;
            MaximumDamage = monster.MaximumDamage;
            RewardExperiencePoints = monster.RewardExperiencePoints;
            RewardGold = monster.RewardGold;
            CurrentHitPoints = monster.CurrentHitPoints;
            MaximumHitPoints = monster.MaximumHitPoints;
            LootTable = monster.LootTable;
        }
    }
}

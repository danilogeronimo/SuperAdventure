using Engine;

namespace SuperAdventure
{
    public partial class SuperAdventure : Form
    {
        private Player _player;
        public SuperAdventure()
        {
            InitializeComponent();
            InitializePlayer();
            InitializePlayerStats();
            InitializeLocation();
        }

        private static void InitializeLocation()
        {
            Location location = new Location(1, "Home", "This is your house.");
        }

        private void InitializePlayerStats()
        {
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            lblGold.Text = _player.Gold.ToString();
            lblExperience.Text = _player.ExperiencePoints.ToString();
            lblLevel.Text = _player.Level.ToString();
        }

        private void InitializePlayer()
        => _player = new Player(10, 10, 20, 0, 1);

    }
}
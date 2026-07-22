using Gameplay.Scripts.Controller;
using Gameplay.Scripts.Data;
using UnityEngine;

namespace Gameplay.Scripts.UI
{
    public class UIController : Singleton<UIController>
    {
        protected ChooseIngredientPanel chooseIngredientPanel;
        protected AvailableFoodPanel availableFoodPanel;
        protected FoodInProgressPanel foodInProgressPanel;
        protected FoodNeedToServePanel foodNeedToServePanel;
        protected RewindMapPopup rewindMapPopup;

        private void Start()
        {
            chooseIngredientPanel = ChooseIngredientPanel.Instance;
            availableFoodPanel    = AvailableFoodPanel.Instance;
            foodInProgressPanel   = FoodInProgressPanel.Instance;
            foodNeedToServePanel  = FoodNeedToServePanel.Instance;
            rewindMapPopup        = RewindMapPopup.Instance;
        }

        // ------------------------------------------------------------------ //
        //  Open – dùng để gán vào UnityEvent / Button.OnClick trong Inspector
        // ------------------------------------------------------------------ //

        public void OpenChooseIngredientPanel()  => chooseIngredientPanel.Show();
        public void OpenAvailableFoodPanel()     => availableFoodPanel.Show();
        public void OpenFoodInProgressPanel()    => foodInProgressPanel.Show();
        public void OpenFoodNeedToServePanel()   => foodNeedToServePanel.Show();

        /// <summary>
        /// Mở RewindMapPopup: load lịch sử food rồi hiện popup.
        /// </summary>
        public void OpenRewindMapPopup()         => rewindMapPopup.Open();

        // ------------------------------------------------------------------ //
        //  Close – đóng từng panel / popup
        // ------------------------------------------------------------------ //

        public void CloseChooseIngredientPanel() => chooseIngredientPanel.Hide();
        public void CloseAvailableFoodPanel()    => availableFoodPanel.Hide();
        public void CloseFoodInProgressPanel()   => foodInProgressPanel.Hide();
        public void CloseFoodNeedToServePanel()  => foodNeedToServePanel.Hide();
        public void CloseRewindMapPopup()        => rewindMapPopup.Close();

        // ------------------------------------------------------------------ //
        //  Game lifecycle
        // ------------------------------------------------------------------ //

        public void StartGame(LevelData levelData)
        {
            GameController.Instance.StartGame(levelData);
        }
    }
}
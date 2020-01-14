using Caliburn.Micro;
using PoeTradeHub.TradeAPI.Models;

namespace PoeTradeHub.UI.Models
{
    public class LeagueInfoModel : PropertyChangedBase
    {
        private readonly LeagueInfo _data;
        private bool _isSelected;

        public LeagueInfoModel(LeagueInfo data)
        {
            _data = data;
            IsSelected = false;
        }

        public string Id => _data.Id;

        public string Text => _data.Text;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                NotifyOfPropertyChange(nameof(IsSelected));
            }
        }
    }
}

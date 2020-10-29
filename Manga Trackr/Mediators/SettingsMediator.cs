using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Manga_Trackr.Mediators
{
    public class SettingsMediator
    {
        #region Members

        private static SettingsMediator _instance;
        private SettingsWindow _window;

        #endregion

        #region Properties

        public static SettingsMediator Instance
        {
            get { return _instance; }
        }

        #endregion

        #region Constructor

        static SettingsMediator()
        {
            _instance = new SettingsMediator();
        }

        private SettingsMediator()
        {

        }

        #endregion

        public void RegisterWindow(SettingsWindow window)
        {
            _window = window;
        }

        public void Close()
        {
            _window.Close();
            _window = null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IdleDetector.Tools.Detectors;

namespace IdleDetector.Main
{
    internal class Core
    {
        MousePosChangedDetector MouseMovementDetector { get; set; }
        ScreenChangedDetector ScreenChangedDetector { get; set; }
        ProcessChangedDetector ProcessDetector { get; set; }

        private bool IsScreenIdle { get; set; }
        private bool IsMouseIdle { get; set; }
        private bool IsBlacklistedProcessRunning { get; set; }

        public Core()
        {
            this.MouseMovementDetector = new MousePosChangedDetector();
            this.ScreenChangedDetector = new ScreenChangedDetector();
            this.ProcessDetector = new ProcessChangedDetector();

            this.MouseMovementDetector.OnMouseStatusChanged += OnMouseStateChanged;
            this.ScreenChangedDetector.OnScreenStatusChanged += OnScreenStateChanged;
            this.ProcessDetector.OnBlacklistedProcessChanged += OnBlacklistedProcessChanged;

            ProcessDetector.Initialize();
        }

        private void OnBlacklistedProcessChanged(object sender, bool value)
        {
            IsBlacklistedProcessRunning = value;
            if (IsBlacklistedProcessRunning)
            {
                //
            }
            Task.Run(()=>MessageBox.Show(IsBlacklistedProcessRunning.ToString()));
        }

        private void OnScreenStateChanged(object sender, bool value)
        {
            IsScreenIdle = value;
            Task.Run(() => MessageBox.Show(IsScreenIdle.ToString()));
        }

        private void OnMouseStateChanged(object sender, bool value)
        {
            IsMouseIdle = value;
            Task.Run(() => MessageBox.Show(IsMouseIdle.ToString()));
        }

    }
}

using System;
using System.Drawing;
using System.Windows.Forms;

namespace IdleDetector.Tools.Detectors
{
    internal class MousePosChangedDetector
    {
        public EventHandler<bool> OnMouseStatusChanged;

        private System.Timers.Timer MouseMovementDetectorTimer;
        private Point LastPoint { get; set; }
        private int MouseIdleTime { get; set; }
        private bool IsMouseIdle { get; set; } = false;

        private void CheckCursorMovement(object sender, EventArgs e)
        {
            Point currentPoint = new Point(Cursor.Position.X, Cursor.Position.Y);
            if (LastPoint == currentPoint)
            {
                //Idle
                MouseIdleTime++;
            }
            else
            {
                //Moved the mouse
                LastPoint = currentPoint;
                MouseIdleTime = 0;
            }

            if((MouseIdleTime >= 30) && (IsMouseIdle == false))
            {
                IsMouseIdle = true;
                OnStatusChanged();
            }
            else if((MouseIdleTime < 30) && (IsMouseIdle == true))
            {
                IsMouseIdle = false;
                OnStatusChanged();
            }
        }

        private void OnStatusChanged()
        {
            EventHandler<bool> handleMouseChanged = OnMouseStatusChanged;
            if (handleMouseChanged != null)
            {
                handleMouseChanged(this, IsMouseIdle);
            }
        }

        public MousePosChangedDetector()
        {
            LastPoint = new Point(Cursor.Position.X, Cursor.Position.Y);
            MouseMovementDetectorTimer = new System.Timers.Timer();
            MouseMovementDetectorTimer.Interval = 1000;
            MouseMovementDetectorTimer.Elapsed += CheckCursorMovement;
            MouseMovementDetectorTimer.AutoReset = true;
            MouseMovementDetectorTimer.Enabled = true;
        }
    }
}

using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deep_Learning_Demo.Forms
{
    public class csXtraFormEX : XtraForm
    {
        public bool IsFormLoad;
        public csDevMessage MessageHelper;

        public csXtraFormEX()
        {
            MessageHelper = new csDevMessage(this);
            this.IconOptions.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        }

        public bool IsFormExit()
        {
            if (this.Visible ||
                this.IsDisposed ||
                this.Disposing)
            {
                return true;
            }
            else return false;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // csXtraFormEX
            // 
            this.ClientSize = new System.Drawing.Size(298, 270);
            this.IconOptions.ShowIcon = false;
            this.Name = "csXtraFormEX";
            this.ResumeLayout(false);

        }
    }
}

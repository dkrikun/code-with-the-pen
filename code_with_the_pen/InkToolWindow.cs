namespace code_with_the_pen
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;

    [Guid("ca2053af-3cc2-4243-ab5e-05c7ccd5e6f2")]
    public class InkToolWindow : ToolWindowPane
    {
        public InkToolWindow() : base(null)
        {
            this.Caption = "InkToolWindow";
            this.Content = new InkToolWindowControl();
        }
    }
}

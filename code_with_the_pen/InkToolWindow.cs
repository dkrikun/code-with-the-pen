namespace code_with_the_pen
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;

    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("ca2053af-3cc2-4243-ab5e-05c7ccd5e6f2")]
    public class InkToolWindow : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InkToolWindow"/> class.
        /// </summary>
        public InkToolWindow() : base(null)
        {
            this.Caption = "InkToolWindow";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new InkToolWindowControl();
        }
    }
}

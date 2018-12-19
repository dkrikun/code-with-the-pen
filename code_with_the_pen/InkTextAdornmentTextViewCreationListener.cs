using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace code_with_the_pen
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class InkTextAdornmentTextViewCreationListener : IWpfTextViewCreationListener
    {
        #pragma warning disable 649, 169
        [Export(typeof(AdornmentLayerDefinition))]
        [Name("InkTextAdornment")]
        [Order(After = PredefinedAdornmentLayers.Selection, Before = PredefinedAdornmentLayers.Text)]
        private AdornmentLayerDefinition editorAdornmentLayer;
        #pragma warning restore 649, 169

        public void TextViewCreated(IWpfTextView textView)
        {
            new InkTextAdornment(textView);
        }
    }
}

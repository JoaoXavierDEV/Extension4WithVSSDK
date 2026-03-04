using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Extension4WithVSSDK
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    public sealed class ErrorListMonitorActivator : IWpfTextViewCreationListener
    {
        public void TextViewCreated(IWpfTextView textView)
        {
            ErrorListMonitorService.Instance.Start();
        }
    }
}

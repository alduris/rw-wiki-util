using System;
using WikiUtil.Remix.UI;

namespace WikiUtil.Remix
{
    internal class DirTab(OptionInterface owner) : Tab(owner, "Working Folder")
    {
        public override void Initialize()
        {
            _ = new OpDirPicker(this);
        }

        public override void Update()
        {
        }
    }
}

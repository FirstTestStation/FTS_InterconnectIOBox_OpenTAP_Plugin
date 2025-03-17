using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using OpenTap;

namespace InterconnectIOBox
{
    [Display("MyResultListener", Description: "Insert a description here", Group: "FTS_Selftest")]
    public class MyResultListener : ResultListener
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion
        public MyResultListener()
        {
            // ToDo: Set default values for properties / settings.
            Name = "MyResultListener";
        }

        public override void Open()
        {
            base.Open();
            //Add resource open code.
        }

        public override void Close()
        {
            //Add resource close code.
            base.Close();
        }
    }
}

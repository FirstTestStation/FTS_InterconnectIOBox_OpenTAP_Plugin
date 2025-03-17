using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using OpenTap;

namespace InterconnectIOBox
{
    [Display("InterconnectIO", Description: "Multi-functions Interconnect IO Box", Group: "First_TestStation")]
    public class InterconnectIO : ScpiInstrument
    {

        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion
        public InterconnectIO()
        {
            Name = "InterconnectIO";
            // ToDo: Set default values for properties / settings.
            // Set default values for properties / settings.
            // VisaAddress = "Simulate";

        }

        public override void Open()
        {
            base.Open();
            // TODO:  Open the connection to the instrument here
            // Use this to ensure the correct instrument is being connected to.
            if (!IdnString.Contains("InterconnectIO"))
            {
                Log.Error("This instrument driver does not support the connected instrument.");
                throw new ArgumentException("Wrong instrument type.");
            }
        }




        public override void Close()
        {
            // TODO:  Shut down the connection to the instrument here.
            base.Close();
        }

        // Unique to this class.
        public void DoNothing()
        {
            OnActivity();   // Causes the GUI to indicate progress.
            Log.Info("InterconnectIO called.");
        }

    }

}

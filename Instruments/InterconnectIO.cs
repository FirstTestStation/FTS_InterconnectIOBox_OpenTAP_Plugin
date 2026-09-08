using System;
using OpenTap;

namespace InterconnectIOBox.Instruments
{
    [Display("FTS_Interconnect", Description: "Multi-function Interconnect IO Box.", Group: "First_TestStation")]
    public class InterconnectIO : ScpiInstrument
    {
        #region Settings
        // ToDo: Add property here for each parameter the end user should be able to change
        #endregion

        public InterconnectIO()
        {
            Name = "InterconnectIO";
            // ToDo: Set default values for properties / settings.
            // VisaAddress = "Simulate";
        }

        public override void Open()
        {
            base.Open();

            // Ensure the correct instrument is being connected to.
            if (!IdnString.Contains("Interconnect"))
            {
                Log.Error("This instrument driver does not support the connected instrument.");
                throw new ArgumentException("Wrong instrument type.");
            }
        }

        public override void Close()
        {
            // TODO: Shut down the connection to the instrument here.
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
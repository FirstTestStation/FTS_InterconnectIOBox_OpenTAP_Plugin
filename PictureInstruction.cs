using System;
using System.ComponentModel;
using OpenTap; // Add this using directive for DisplayAttribute and related attributes

namespace InterconnectIOBox
{




    [Display("Instructions Picture", Groups: new[] {  "InterconnectIO", "Fixture/DUT" },
        Description: "Pop-up to operator to display picture instruction")]
    public class OpenTapPictureExample : ResultTestStep
    {
      

        enum ResponseEnum
        {
            [Display("Yes, Completed!")]
            Yes,
            [Display("No, Something is wrong, abort!")]
            No
        }

        [Display("Confirm configuration")]
        class PictureDialogRequest
        {
            public PictureDialogRequest(string question)
            {
                Question = question;
            }

            /// <summary>
            /// The layout of a picture can be controlled using the Layout attribute, just like other UserInput members
            /// </summary>
            [Layout(LayoutMode.FullRow)]
            [Display("Picture", Order: 1)]
            public Picture Picture { get; set; }

            [Layout(LayoutMode.FullRow, rowHeight: 2)]
            [Browsable(true)]
            [Display("Message", Order: 2)]
            public string Question { get; }

            [Layout(LayoutMode.FloatBottom | LayoutMode.FullRow)]
            [Submit]
            public ResponseEnum Response { get; set; } // Renamed from 'response' to 'Response'

        }
        /// <summary>
        /// This method executes the logic of the test step.
        /// </summary>
        public override void Run()
        {
            string verdictString;

            // Create the dialog request object, populating it with step properties.
            var request = new PictureDialogRequest(Instruction)
            {
                Picture = Picture,
            };

            // Display the dialog to the operator and wait for a response.
            UserInput.Request(request);

            // Check the operator's response.
            if (request.Response == ResponseEnum.No)
            {
                // Operator chose 'No', indicating failure or abort.
                Log.Error("Operator aborted test due to configuration issue. Aborting test plan.");
                UpgradeVerdict(Verdict.Error);
                verdictString = Verdict.Error.ToString();

            }
            else // (request.Response == ResponseEnum.Yes)
            {
                // Operator chose 'Yes', confirming success.
                Log.Info("Operator confirmed instructions completed.");

                // Set the verdict to Pass (green '✓'), fixing the 'NotSet' issue.
                UpgradeVerdict(Verdict.Pass);
                verdictString = Verdict.Pass.ToString();
            }



                // Publish the result record
                var result = new TestResult<string>
                {
                    ParamName = $"Picture",
                    StepName = Name,
                    Verdict = verdictString,
                    Units = "Check"
                };

                // This line publishes the result to OpenTAP's log and database
                PublishResult(result);



        }








        public string Instruction { get; set; } = "Instructions To be displayed in bottom of picture.";

        /// <summary>
        /// Instantiate an OpenTAP picture with some default picture
        /// These can be controlled by other test step properties if they should be configurable, or they can be hardcoded values
        /// </summary>
        public Picture Picture { get; } = new Picture()
        {
            Source = "Picture\\Fixture.png",
            Description = "Operator Instructions."
        };

        /// <summary>
        /// Control the source of the picture with a regular test step property
        /// </summary>
        [Display("Source", "The source of the picture. This can be a URL or a file path.", "Picture", Order: 2,
            Collapsed: true)]
        [FilePath(FilePathAttribute.BehaviorChoice.Open)]
        public string PictureSource
        {
            get => Picture.Source;
            set => Picture.Source = value;
        }

        /// <summary>
        /// Control the description of the picture with a regular test step property
        /// </summary>
        [Display("Description", "A description of the picture. " +
                                "This can be helpful to set if the picture cannot be loaded for some reason, " +
                                "or if the test plan is not running in a GUI environment.",
            "Picture", Order: 3, Collapsed: true)]

        public string PictureDescription
        {
            get => Picture.Description;
            set => Picture.Description = value;
        }
    }
}
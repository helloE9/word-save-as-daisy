using Daisy.SaveAsDAISY.Conversion;
using Daisy.SaveAsDAISY.Conversion.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using static Daisy.SaveAsDAISY.ConversionProgress;
using MSword = Microsoft.Office.Interop.Word;

namespace Daisy.SaveAsDAISY {
    /// <summary>
    /// Conversion events handler using graphical interface (windows dialog)
    /// </summary>
	public class GraphicalEventsHandler : IConversionEventsHandler
	{

        private System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(
                    "DaisyAddinLib.resources.Labels",
                    Assembly.GetExecutingAssembly()
            );

        #region Conversion progress dialog
        public ConversionProgress ProgressDialog { get; set; }
        public Thread DialogThread { get; set; } = null;

        public void TryInitializeProgress(string message, int maximum = 1, int step = 1)
        {
            if(ProgressDialog != null && !ProgressDialog.IsDisposed)
            {
                if ((DialogThread == null || !DialogThread.IsAlive))
                {
                    DialogThread = new Thread(
                        () => {
                            ProgressDialog.ShowDialog();
                            ProgressDialog.InitializeProgress(message, maximum, step);
                        });
                    DialogThread.Start();
                    while (!ProgressDialog.Visible) ;
                }
                else
                {
                    ProgressDialog.InitializeProgress(message, maximum, step);
                }
            }
            
        }

        private void TryShowMessage(string message, bool isProgress = false) {
            if(ProgressDialog != null && !ProgressDialog.IsDisposed)
            {
                if ((DialogThread == null || !DialogThread.IsAlive))
                {
                    DialogThread = new Thread(
                        () => {
                            ProgressDialog.ShowDialog();
                            ProgressDialog.AddMessage(message, isProgress);
                            if (isProgress) ProgressDialog.Progress();
                        });
                    DialogThread.Start();
                    while (!ProgressDialog.Visible) ;
                }
                else // Is already started and dialog is visible
                {
                    ProgressDialog.AddMessage(message, isProgress);
                    if (isProgress) ProgressDialog.Progress();
                }

            }
            
        }

        private void TryClosingDialog() {
#if DEBUG
            // keep open the dialog for debugging purpose
#else
            if(DialogThread != null && DialogThread.IsAlive) {
                Thread.Sleep(2000);
                ProgressDialog.Close();
                DialogThread.Join();
            }
#endif
        }
        #endregion

        public GraphicalEventsHandler() {

        }

        public void LinkToProgressDialog(ref ConversionProgress progressDialog, int maximumValue = 2) {
            this.ProgressDialog = progressDialog;
        }



        public System.Resources.ResourceManager Labels {
            get { return this.resourceManager; }
        }

        #region Preprocessing
        public void onDocumentPreprocessingStart(string inputPath) {
            // Intialize progress bar for preprocessing (7 steps)
            TryInitializeProgress("Preprocessing " + inputPath, 7);

        }

        public void onPreprocessingCancel() {
            TryShowMessage("Preprocessing canceled ");
            TryClosingDialog();
        }

        public void onPreprocessingError(string inputPath, string errors) {
            TryClosingDialog();
            MessageBox.Show(errors, "Preprocessing errors", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void onPreprocessingSuccess() {
            TryShowMessage("Preprocessing done", true);
        }

        public bool AskForTrackConfirmation() {
            return MessageBox.Show(Labels.GetString("TrackConfirmation"), "SaveAsDAISY", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes;
        }

        public DialogResult documentMustBeRenamed(StringValidator authorizedNamePattern) {
            string BoxText = authorizedNamePattern.UnauthorizedValueMessage +
                       "\r\n" +
                       "\r\nDo you want to save this document under a new name ?" +
                       "\r\nThe document with the original name will not be deleted." +
                       "\r\n" +
                       "\r\n(Click Yes to save the document under a new name and use the new one, " +
                           "No to continue with the current document, " +
                           "or Cancel to abort the conversion)";
            return MessageBox.Show(BoxText, "Unauthorized characters in the document filename", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
        }

        public bool userIsRenamingDocument(ref object preprocessedObject) {
            object missing = Type.Missing;
            MSword.Dialog dlg = ((MSword.Document)preprocessedObject).Application.Dialogs[MSword.WdWordDialog.wdDialogFileSaveAs];
            int saveResult = dlg.Show(ref missing);
            return saveResult == -1; // ok pressed, see https://docs.microsoft.com/fr-fr/dotnet/api/microsoft.office.interop.word.dialog.show?view=word-pia#Microsoft_Office_Interop_Word_Dialog_Show_System_Object__
        }

        public bool AskForTranslatingSubdocuments() {
            DialogResult dialogResult = MessageBox.Show("Do you want to translate the current document along with sub documents?", "SaveAsDAISY", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            return dialogResult == DialogResult.Yes;
        }
        #endregion

        public void OnStop(string message)
		{
			OnStop(message,"SaveAsDAISY");
		}

		public void OnError(string errorMessage)
		{
            TryShowMessage("An error occured during conversion : \r\n" + errorMessage, true);
            TryClosingDialog();
            MessageBox.Show(errorMessage, "An error occured during conversion", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		public void OnStop(string message, string title)
		{
            TryClosingDialog();
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Stop);
		}

        #region Conversion to dtbook
        public void onDocumentListConversionStart(List<DocumentParameters> documentLists, ConversionParameters conversion) {
            TryInitializeProgress("Starting documents list conversion", documentLists.Count + (conversion.PostProcessor != null ? 1 : 0));
            
        }

        public void onDocumentConversionStart(DocumentParameters document, ConversionParameters conversion) {
            TryInitializeProgress("Converting document " + document.InputPath, conversion.PostProcessor != null ? 2 : 1);

        }

        public void onDocumentListConversionSuccess(List<DocumentParameters> documentLists, ConversionParameters conversion) {
            TryClosingDialog();
        }

        public void onDocumentConversionSuccess(DocumentParameters document, ConversionParameters conversion) {
            TryShowMessage("Successful conversion of " + document.InputPath + " to " + document.OutputPath, true);
            
            
        }
        #endregion

        #region Post processing
        public void onPostProcessingStart(ConversionParameters conversion) {
            TryInitializeProgress("Starting pipeline processing", conversion.PostProcessor.StepsCount + 1);
            
            //conversion.PostProcessor.setPipelineErrorListener((string message) => {
            //    if (message != null) {
            //        ProgressDialog.AddMessage(message, true);
            //    }
            //});
            //conversion.PostProcessor.setPipelineOutputListener((string message) => {
            //    if (message != null) {
            //        ProgressDialog.AddMessage(message,false);
            //    }
            //});
            //conversion.PostProcessor.setPipelineProgressListener((string message) => {
            //    if (message != null) {
            //        ProgressDialog.AddMessage(message, true);
            //    }
            //});
        }

        

        public void onPostProcessingSuccess(ConversionParameters conversion) {
            TryShowMessage("Successfully processed or converted dtbook, result stored in " + conversion.OutputPath, true);
            
            TryClosingDialog();
        }

        #endregion

        public void onConversionCanceled() {
            TryShowMessage("Canceling conversion");
            TryClosingDialog();
        }

        public void onProgressMessageReceived(object sender, EventArgs e) {
            TryShowMessage(((DaisyEventArgs)e).Message, true);
        }

        public void onFeedbackMessageReceived(object sender, EventArgs e) {
            TryShowMessage(((DaisyEventArgs)e).Message);
        }

        public void onFeedbackValidationMessageReceived(object sender, EventArgs e) {
            TryShowMessage(((DaisyEventArgs)e).Message);
        }

        public void OnSuccessMasterSubValidation(string message) {
            MasterSubValidation infoBox = new MasterSubValidation(message, "Success");
            infoBox.ShowDialog();
        }

        public void OnUnknownError(string error) {
            TryClosingDialog();
            MessageBox.Show(error, "Unknown error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void OnUnknownError(string title, string details) {
            TryClosingDialog();
            InfoBox infoBox = new InfoBox(title, details, Labels);
            infoBox.ShowDialog();
        }

        public void OnValidationErrors(List<ValidationError> errors, string outputFile) {
            Validation validationDialog = new Validation(
                "FailedLabel",
                string.Join(
                        "\r\n",
                        errors.Select(
                            error => error.ToString()
                        ).ToArray()
                    ), outputFile, 
                Labels);
            validationDialog.ShowDialog();
        }

        public void OnLostElements(string outputFile, ArrayList elements) {
            Fidility fidilityDialog = new Fidility("FeedbackLabel", elements, outputFile, Labels);
            fidilityDialog.ShowDialog();
        }

        public bool IsContinueDTBookGenerationOnLostElements() {
            DialogResult continueDTBookGenerationResult = MessageBox.Show("Do you want to create audio file", "SaveAsDAISY", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return continueDTBookGenerationResult == DialogResult.Yes;
        }

        public void OnSuccess() {
            TryClosingDialog();
            MessageBox.Show(Labels.GetString("SucessLabel"), "SaveAsDAISY - Success", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
        }

        public void OnMasterSubValidationError(string error) {
            MasterSubValidation infoBox = new MasterSubValidation(error, "Validation");
            infoBox.ShowDialog();
        }
        
    }
}